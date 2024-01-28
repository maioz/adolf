using System.Text;

namespace PolitoGPT;

public class Davinci : Completion
{
    public override string Name => nameof(Davinci);

    public Davinci(OpenAI openai) : base(openai)
    {
    }

    async Task<StringBuilder> Get(StringBuilder chat)
    {
        var response = new StringBuilder();

        for(int i = 0; i < 4; i++)
        {
            var result = await _openAI.GetCompletion(chat.ToString());

            var value = result.GetFirstChoice();

            response.Append(value.Text);

            if(value.FinishReason != "length")
                break;

            chat.Append(value.Text);
        }

        return response;

    }

    public override async Task<string> GetCompletion(Post[] posts)
    {
        var chat = PostsToChat(posts);

        var response = await Get(chat);

        var finalValue = response.ToString().Trim();

        if(string.IsNullOrEmpty(finalValue))
            return string.Empty;

        return FormatAnwser(finalValue);
    }

    private static StringBuilder PostsToChat(Post[] posts)
    {
        var chat = new StringBuilder();

        chat.Append($"Seu nome é {Polito.UserName}.\n\n");

        foreach(var item in posts)
            chat.Append($"{item.Username}: {item.Content}.\n");

        chat.Append($"{Polito.UserName}:");
        return chat;
    }
}

