namespace PolitoGPT;

public class Gpt35 : Completion
{
    public Gpt35(OpenAI openai) : base(openai)
    {
    }

    public override string Name => nameof(Gpt35);

    private static string GetRoleFromUsername(string username)
    {
        return username.Equals(Polito.UserName, StringComparison.OrdinalIgnoreCase)
            ? "assistant"
            : "user";
    }

    public override async Task<string> GetCompletion(Post[] posts)
    {
        var mesages = posts.Select(p => new Message()
        {
            Content = p.Content,
            Role = GetRoleFromUsername(p.Username)
        }).ToArray();

        var response = await _openAI.GetChat(mesages);
        var value = response.GetFirstChoiceText();
        return FormatAnwser(value);
    }
}

