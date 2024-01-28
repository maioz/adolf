using System.Text.Json.Serialization;

namespace PolitoGPT;

public class ImageRequest
{
    [JsonPropertyName("n")]
    public int N { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("size")]
    public string Size { get; set; }
}
