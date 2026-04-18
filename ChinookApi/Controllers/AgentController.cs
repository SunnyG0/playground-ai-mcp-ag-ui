using System.Text;
using System.Text.Json;
using ChinookApi.Agent;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace ChinookApi.Controllers;

[ApiController]
public class AgentController(
    AIAgent agent,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("/agent")]
    public async Task RunAgentAsync([FromBody] RunAgentInput input, CancellationToken cancellationToken)
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        var threadId = string.IsNullOrWhiteSpace(input.ThreadId) ? Guid.NewGuid().ToString("N") : input.ThreadId;
        var runId = string.IsNullOrWhiteSpace(input.RunId) ? Guid.NewGuid().ToString("N") : input.RunId;

        await WriteEventAsync(new { type = "RUN_STARTED", threadId, runId }, cancellationToken);

        try
        {
            var mcpUrl = configuration["Agent:McpServerUrl"] ?? "http://localhost:5100/mcp";
            var httpClient = httpClientFactory.CreateClient("mcp");
            httpClient.BaseAddress = new Uri(mcpUrl);

            var transport = new HttpClientTransport(
                new HttpClientTransportOptions { Endpoint = new Uri(mcpUrl) },
                httpClient);

            await using var mcpClient = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
            var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);

            var messages = BuildMessages(input.Messages);

            var runOptions = new ChatClientAgentRunOptions
            {
                ChatOptions = new ChatOptions { Tools = [.. mcpTools] }
            };

            var session = await agent.CreateSessionAsync(cancellationToken);

            string? currentMessageId = null;

            await foreach (var update in agent.RunStreamingAsync(messages, session, runOptions, cancellationToken)
                                              .AsChatResponseUpdatesAsync())
            {
                // Assign a fallback message ID when the provider doesn't supply one
                if (string.IsNullOrWhiteSpace(update.MessageId))
                    update.MessageId = currentMessageId ?? Guid.NewGuid().ToString("N");

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent && !string.IsNullOrEmpty(textContent.Text))
                    {
                        if (!string.Equals(currentMessageId, update.MessageId, StringComparison.Ordinal))
                        {
                            if (currentMessageId is not null)
                            {
                                await WriteEventAsync(
                                    new { type = "TEXT_MESSAGE_END", messageId = currentMessageId },
                                    cancellationToken);
                            }

                            currentMessageId = update.MessageId;
                            var role = update.Role?.Value ?? "assistant";
                            await WriteEventAsync(
                                new { type = "TEXT_MESSAGE_START", messageId = currentMessageId, role },
                                cancellationToken);
                        }

                        await WriteEventAsync(
                            new { type = "TEXT_MESSAGE_CONTENT", messageId = currentMessageId, delta = textContent.Text },
                            cancellationToken);
                    }
                    else if (content is FunctionCallContent functionCall && functionCall.CallId is not null)
                    {
                        var argsJson = functionCall.Arguments is not null
                            ? JsonSerializer.Serialize(functionCall.Arguments)
                            : "{}";

                        await WriteEventAsync(
                            new { type = "TOOL_CALL_START", toolCallId = functionCall.CallId, toolCallName = functionCall.Name, parentMessageId = update.MessageId },
                            cancellationToken);
                        await WriteEventAsync(
                            new { type = "TOOL_CALL_ARGS", toolCallId = functionCall.CallId, delta = argsJson },
                            cancellationToken);
                        await WriteEventAsync(
                            new { type = "TOOL_CALL_END", toolCallId = functionCall.CallId },
                            cancellationToken);
                    }
                    else if (content is FunctionResultContent functionResult)
                    {
                        var resultText = functionResult.Result switch
                        {
                            string s => s,
                            null => "",
                            var r => JsonSerializer.Serialize(r)
                        };

                        await WriteEventAsync(
                            new { type = "TOOL_CALL_RESULT", messageId = update.MessageId, toolCallId = functionResult.CallId, content = resultText, role = "tool" },
                            cancellationToken);
                    }
                }
            }

            if (currentMessageId is not null)
            {
                await WriteEventAsync(
                    new { type = "TEXT_MESSAGE_END", messageId = currentMessageId },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await WriteEventAsync(
                new { type = "RUN_ERROR", message = ex.Message },
                cancellationToken);
            return;
        }

        await WriteEventAsync(new { type = "RUN_FINISHED", threadId, runId }, cancellationToken);
    }

    private static List<ChatMessage> BuildMessages(List<AgentMessage> agentMessages)
    {
        var messages = new List<ChatMessage>();

        foreach (var msg in agentMessages)
        {
            var role = msg.Role switch
            {
                "assistant" => ChatRole.Assistant,
                "tool" => ChatRole.Tool,
                "system" => ChatRole.System,
                _ => ChatRole.User
            };

            if (role == ChatRole.Tool && msg.ToolCallId is not null)
            {
                messages.Add(new ChatMessage(ChatRole.Tool,
                    [new FunctionResultContent(msg.ToolCallId, msg.Content ?? string.Empty)]));
            }
            else if (role == ChatRole.Assistant && msg.ToolCalls?.Count > 0)
            {
                var contents = new List<AIContent>();
                if (!string.IsNullOrEmpty(msg.Content))
                    contents.Add(new TextContent(msg.Content));

                foreach (var tc in msg.ToolCalls)
                {
                    IDictionary<string, object?> args;
                    try
                    {
                        args = JsonSerializer.Deserialize<Dictionary<string, object?>>(tc.Function.Arguments) ?? new Dictionary<string, object?>();
                    }
                    catch (JsonException)
                    {
                        args = new Dictionary<string, object?>();
                    }
                    contents.Add(new FunctionCallContent(tc.Id, tc.Function.Name, args));
                }

                messages.Add(new ChatMessage(role, contents));
            }
            else
            {
                messages.Add(new ChatMessage(role, msg.Content ?? string.Empty));
            }
        }

        return messages;
    }

    private async Task WriteEventAsync(object data, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes($"data: {json}\n\n");
        await Response.Body.WriteAsync(bytes, cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
