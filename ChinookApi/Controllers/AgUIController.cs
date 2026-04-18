using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ChinookApi.AgUI;

namespace ChinookApi.Controllers;

/// <summary>
/// AG-UI protocol endpoint.  Accepts a POST request with a <see cref="RunAgentInput"/>
/// body and streams AG-UI lifecycle + message events back as Server-Sent Events (SSE).
///
/// Compatible with CopilotKit and other AG-UI–enabled frontends.
/// </summary>
[ApiController]
[Route("agui")]
public class AgUIController(ChinookAgentFactory agentFactory) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// POST /agui
    /// Runs the Chinook AI agent for a single turn and streams the response as SSE.
    /// </summary>
    [HttpPost]
    public async Task RunAsync([FromBody] RunAgentInput input, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";

        await using var writer = new StreamWriter(Response.Body, leaveOpen: true);

        // ── 1. RUN_STARTED ────────────────────────────────────────────────
        await WriteEventAsync(writer, new RunStartedEvent { ThreadId = input.ThreadId, RunId = input.RunId }, cancellationToken);

        var messageId = Guid.NewGuid().ToString("N");
        Exception? runError = null;

        try
        {
            // ── 2. STEP_STARTED ───────────────────────────────────────────
            await WriteEventAsync(writer, new StepStartedEvent { StepName = "agent" }, cancellationToken);

            // Map AG-UI messages to Microsoft.Extensions.AI ChatMessage objects
            var chatMessages = MapMessages(input.Messages);

            // Create the Chinook agent for this run
            AIAgent agent = agentFactory.Create();

            // ── 3. TEXT_MESSAGE_START ─────────────────────────────────────
            await WriteEventAsync(writer, new TextMessageStartEvent { MessageId = messageId, Role = "assistant" }, cancellationToken);

            // ── 4. TEXT_MESSAGE_CONTENT (streamed chunks) ─────────────────
            await foreach (var update in agent.RunStreamingAsync(chatMessages, cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    await WriteEventAsync(writer, new TextMessageContentEvent { MessageId = messageId, Delta = update.Text }, cancellationToken);
                }
            }

            // ── 5. TEXT_MESSAGE_END ───────────────────────────────────────
            await WriteEventAsync(writer, new TextMessageEndEvent { MessageId = messageId }, cancellationToken);

            // ── 6. STEP_FINISHED ──────────────────────────────────────────
            await WriteEventAsync(writer, new StepFinishedEvent { StepName = "agent" }, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Client disconnected – no need to emit an error event.
            return;
        }
        catch (Exception ex)
        {
            runError = ex;
            await WriteEventAsync(writer, new RunErrorEvent { Message = ex.Message }, cancellationToken);
        }

        // ── 7. RUN_FINISHED ───────────────────────────────────────────────
        await WriteEventAsync(
            writer,
            new RunFinishedEvent
            {
                ThreadId = input.ThreadId,
                RunId = input.RunId,
                Outcome = runError is null ? "success" : "error",
            },
            cancellationToken);

        await writer.FlushAsync(cancellationToken);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IEnumerable<ChatMessage> MapMessages(IEnumerable<AgUIMessage> agUIMessages)
    {
        foreach (var msg in agUIMessages)
        {
            var role = msg.Role?.ToLowerInvariant() switch
            {
                "user" => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                "system" => ChatRole.System,
                _ => ChatRole.User,
            };

            yield return new ChatMessage(role, msg.Content ?? string.Empty);
        }
    }

    private static async Task WriteEventAsync(StreamWriter writer, AgUIEvent evt, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(evt, evt.GetType(), JsonOptions);
        await writer.WriteAsync($"data: {json}\n\n".AsMemory(), ct);
        await writer.FlushAsync(ct);
    }
}
