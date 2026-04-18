using System.Text.Json.Serialization;

namespace ChinookApi.Agent;

public record RunAgentInput(
    [property: JsonPropertyName("threadId")] string? ThreadId,
    [property: JsonPropertyName("runId")] string? RunId,
    [property: JsonPropertyName("messages")] List<AgentMessage> Messages,
    [property: JsonPropertyName("tools")] List<object>? Tools = null,
    [property: JsonPropertyName("state")] object? State = null,
    [property: JsonPropertyName("forwardedProps")] object? ForwardedProps = null);

public record AgentMessage(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("toolCalls")] List<AgentToolCall>? ToolCalls = null,
    [property: JsonPropertyName("toolCallId")] string? ToolCallId = null,
    [property: JsonPropertyName("name")] string? Name = null);

public record AgentToolCall(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("function")] AgentFunction Function);

public record AgentFunction(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("arguments")] string Arguments);
