using System.Text;
using System.Text.Json;
using ChinookApi.Agent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace ChinookApi.Controllers;

[ApiController]
public class AgentController(
    IChatClient chatClient,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ControllerBase
{
    private const string SystemPrompt =
        "You are a helpful music catalog assistant for the Chinook database. " +
        "You can search for artists, albums, tracks, playlists, genres, and customer purchase history. " +
        "Use the available tools to look up information and provide clear, concise answers.";

    [HttpPost("/agent")]
    public async Task RunAgentAsync([FromBody] RunAgentInput input, CancellationToken cancellationToken)
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        var threadId = input.ThreadId ?? Guid.NewGuid().ToString();
        var runId = input.RunId ?? Guid.NewGuid().ToString();

        await WriteEventAsync(new { type = "RUN_STARTED", thread_id = threadId, run_id = runId }, cancellationToken);

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

            var options = new ChatOptions { Tools = [.. mcpTools] };

            while (true)
            {
                var currentMessageId = Guid.NewGuid().ToString();
                var textStarted = false;
                var toolCallMap = new Dictionary<string, FunctionCallContent>();
                var allContents = new List<AIContent>();

                await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
                {
                    foreach (var content in update.Contents)
                    {
                        allContents.Add(content);

                        if (content is TextContent textContent && !string.IsNullOrEmpty(textContent.Text))
                        {
                            if (!textStarted)
                            {
                                await WriteEventAsync(
                                    new { type = "TEXT_MESSAGE_START", message_id = currentMessageId, role = "assistant" },
                                    cancellationToken);
                                textStarted = true;
                            }

                            await WriteEventAsync(
                                new { type = "TEXT_MESSAGE_CONTENT", message_id = currentMessageId, delta = textContent.Text },
                                cancellationToken);
                        }
                        else if (content is FunctionCallContent functionCall && functionCall.CallId is not null)
                        {
                            toolCallMap[functionCall.CallId] = functionCall;
                        }
                    }
                }

                if (textStarted)
                {
                    await WriteEventAsync(
                        new { type = "TEXT_MESSAGE_END", message_id = currentMessageId },
                        cancellationToken);
                }

                messages.Add(new ChatMessage(ChatRole.Assistant, allContents));

                if (toolCallMap.Count == 0)
                    break;

                foreach (var (callId, toolCall) in toolCallMap)
                {
                    var argsJson = toolCall.Arguments is not null
                        ? JsonSerializer.Serialize(toolCall.Arguments)
                        : "{}";

                    await WriteEventAsync(
                        new { type = "TOOL_CALL_START", tool_call_id = callId, tool_call_message_id = currentMessageId, tool_name = toolCall.Name },
                        cancellationToken);
                    await WriteEventAsync(
                        new { type = "TOOL_CALL_ARGS_DELTA", tool_call_id = callId, delta = argsJson },
                        cancellationToken);
                    await WriteEventAsync(
                        new { type = "TOOL_CALL_END", tool_call_id = callId },
                        cancellationToken);

                    var callResult = await mcpClient.CallToolAsync(
                        toolCall.Name,
                        toolCall.Arguments?.ToDictionary(k => k.Key, v => v.Value),
                        cancellationToken: cancellationToken);

                    var resultText = string.Join("\n", callResult.Content
                        .OfType<TextContentBlock>()
                        .Select(c => c.Text));

                    messages.Add(new ChatMessage(ChatRole.Tool,
                        [new FunctionResultContent(callId, resultText)]));
                }
            }
        }
        catch (Exception ex)
        {
            await WriteEventAsync(
                new { type = "RUN_ERROR", message = ex.Message },
                cancellationToken);
            return;
        }

        await WriteEventAsync(
            new { type = "RUN_FINISHED", thread_id = threadId, run_id = runId },
            cancellationToken);
    }

    private static List<ChatMessage> BuildMessages(List<AgentMessage> agentMessages)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt)
        };

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
