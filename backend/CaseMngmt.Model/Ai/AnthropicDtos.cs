using System.Text.Json;
using System.Text.Json.Serialization;

namespace CaseMngmt.Models.Ai
{
    public class AnthropicRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("messages")]
        public List<AnthropicMessage> Messages { get; set; } = new();

        [JsonPropertyName("tools")]
        public List<AnthropicTool>? Tools { get; set; }

        [JsonPropertyName("tool_choice")]
        public AnthropicToolChoice? ToolChoice { get; set; }
    }

    public class AnthropicMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        // string for plain text messages, or a List<object> of content blocks
        // (e.g. image/document + text) for multi-modal (vision) requests.
        [JsonPropertyName("content")]
        public object Content { get; set; } = string.Empty;
    }

    public class AnthropicTool
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("input_schema")]
        public object InputSchema { get; set; } = new();
    }

    public class AnthropicToolChoice
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "tool";

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class AnthropicResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("content")]
        public List<AnthropicContentBlock> Content { get; set; } = new();

        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }

        [JsonPropertyName("usage")]
        public AnthropicUsage? Usage { get; set; }
    }

    public class AnthropicContentBlock
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("input")]
        public JsonElement Input { get; set; }
    }

    public class AnthropicUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}
