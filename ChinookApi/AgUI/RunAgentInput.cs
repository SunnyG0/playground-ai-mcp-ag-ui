using System.Text.Json.Serialization;

namespace ChinookApi.AgUI;

/// <summary>
/// Represents the input payload for an AG-UI agent run.
/// This is the AG-UI protocol "RunAgentInput" object sent by the client.
/// </summary>
public sealed class RunAgentInput
{
    [JsonPropertyName("threadId")]
    public string ThreadId { get; set; } = string.Empty;

    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonPropertyName("parentRunId")]
    public string? ParentRunId { get; set; }

    [JsonPropertyName("messages")]
    public List<AgUIMessage> Messages { get; set; } = [];

    [JsonPropertyName("state")]
    public object? State { get; set; }

    [JsonPropertyName("tools")]
    public List<AgUITool> Tools { get; set; } = [];

    [JsonPropertyName("context")]
    public List<AgUIContext>? Context { get; set; }
}

/// <summary>
/// An AG-UI message in the conversation history.
/// </summary>
public sealed class AgUIMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("toolCallId")]
    public string? ToolCallId { get; set; }

    [JsonPropertyName("toolCalls")]
    public List<AgUIToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// A tool call included in an assistant message.
/// </summary>
public sealed class AgUIToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public AgUIToolCallFunction Function { get; set; } = new();
}

/// <summary>
/// The function info inside a tool call.
/// </summary>
public sealed class AgUIToolCallFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// A tool definition passed by the client in the run input.
/// </summary>
public sealed class AgUITool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }
}

/// <summary>
/// Context element passed with the run input.
/// </summary>
public sealed class AgUIContext
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
