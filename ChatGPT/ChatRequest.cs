using System.Text.Json.Serialization;

namespace PolitoGPT;

public class ChatRequest
{
    [JsonPropertyName("max_tokens")]

    public int MaxTokens { get; internal set; }

    [JsonPropertyName("model")]
    public string Model { get; internal set; }

    [JsonPropertyName("messages")]
    public Message[] Messages { get; internal set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; internal set; }


    [JsonPropertyName("content")]
    public string Content { get; internal set; }
}

