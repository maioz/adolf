using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PolitoGPT;

public class OpenAI
{
    private HttpClient _http;
    private string _apiKey;
    private readonly JsonSerializerOptions _defaultJsonoptions;

    public OpenAI(string apiKey)
    {
        _apiKey = apiKey;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
        };

        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/"),
            Timeout = TimeSpan.FromMinutes(2)
        };

        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        _defaultJsonoptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    }

    public async Task<CompletionResponse> GetCompletion(string message)
    {
        var request = new CompletionRequest()
        {
            MaxTokens = 500,
            Model = "text-davinci-003",
            Prompt = message,
        };

        var response = await _http.PostAsJsonAsync("completions", request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CompletionResponse>(_defaultJsonoptions);

        return result;
    }

    public async Task<ChatResponse> GetChat(Message[] messages)
    {
        messages = messages.Prepend(new(){
            Role = "system",
            Content = "Você é uma pessoa real e não um assistente virtual. Você responde as perguntas de forma clara e objetiva. Não introduza você mesmo."
        }).ToArray();
        
        var request = new ChatRequest()
        {
            MaxTokens = 500,
            Model = "gpt-4-1106-preview",
            Messages = messages
        };

        var response = await _http.PostAsJsonAsync("chat/completions", request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>();

        return result;
    }

    internal async Task<ImageResponse> GenerateImage(string message)
    {
        //https://api.openai.com/v1/images/generations
        var request = new ImageRequest()
        {
            N = 1,
            Size = "1024x1024",
            Prompt = message,
        };

        var response = await _http.PostAsJsonAsync("images/generations", request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ImageResponse>();
    }

    internal async Task GetUsage(DateTime dateTime1, DateTime dateTime2)
    {
        var uri = "https://api.openai.com/dashboard/billing/usage?end_date=2023-08-01&start_date=2023-07-01";
        var response = await _http.GetAsync(uri);
    }
}