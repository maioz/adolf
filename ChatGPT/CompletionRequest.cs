using System.Text.Json.Serialization;

namespace PolitoGPT;

public class CompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
}

public class CompletionResponse
{
    [JsonPropertyName("choices")]
    public List<ChatGPTChoice> Choices { get; set; }
    public string GetFirstChoiceText() => Choices.First().Text;

    public ChatGPTChoice GetFirstChoice() => Choices.FirstOrDefault();
}

public class ChatGPTChoice
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}