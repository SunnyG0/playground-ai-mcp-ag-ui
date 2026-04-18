using System.Text.Json.Serialization;

namespace ChinookApi.AgUI;

/// <summary>
/// Base class for all AG-UI protocol events.
/// </summary>
public abstract class AgUIEvent
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

/// <summary>RUN_STARTED — emitted once at the beginning of every agent run.</summary>
public sealed class RunStartedEvent : AgUIEvent
{
    public override string Type => "RUN_STARTED";

    [JsonPropertyName("threadId")]
    public string ThreadId { get; set; } = string.Empty;

    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;
}

/// <summary>STEP_STARTED — emitted when a logical processing step begins.</summary>
public sealed class StepStartedEvent : AgUIEvent
{
    public override string Type => "STEP_STARTED";

    [JsonPropertyName("stepName")]
    public string StepName { get; set; } = string.Empty;
}

/// <summary>TEXT_MESSAGE_START — marks the beginning of a streamed assistant message.</summary>
public sealed class TextMessageStartEvent : AgUIEvent
{
    public override string Type => "TEXT_MESSAGE_START";

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = "assistant";
}

/// <summary>TEXT_MESSAGE_CONTENT — carries an incremental text chunk.</summary>
public sealed class TextMessageContentEvent : AgUIEvent
{
    public override string Type => "TEXT_MESSAGE_CONTENT";

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("delta")]
    public string Delta { get; set; } = string.Empty;
}

/// <summary>TEXT_MESSAGE_END — signals that the current assistant message is complete.</summary>
public sealed class TextMessageEndEvent : AgUIEvent
{
    public override string Type => "TEXT_MESSAGE_END";

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;
}

/// <summary>STEP_FINISHED — emitted when a logical processing step ends.</summary>
public sealed class StepFinishedEvent : AgUIEvent
{
    public override string Type => "STEP_FINISHED";

    [JsonPropertyName("stepName")]
    public string StepName { get; set; } = string.Empty;
}

/// <summary>RUN_FINISHED — signals the end of the agent run.</summary>
public sealed class RunFinishedEvent : AgUIEvent
{
    public override string Type => "RUN_FINISHED";

    [JsonPropertyName("threadId")]
    public string ThreadId { get; set; } = string.Empty;

    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = "success";
}

/// <summary>RUN_ERROR — signals that the agent run encountered an unrecoverable error.</summary>
public sealed class RunErrorEvent : AgUIEvent
{
    public override string Type => "RUN_ERROR";

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
