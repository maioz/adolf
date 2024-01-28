using System.Text.RegularExpressions;
using System.Threading;

namespace PolitoGPT;

public abstract class Completion
{
    protected OpenAI _openAI;

    public abstract string Name { get; }

    public Completion(OpenAI openai)
    {
        _openAI = openai;
    }

    public abstract Task<string> GetCompletion(Post[] messages);

    protected static string FormatAnwser(string answerHtml)
    {
        answerHtml = Regex.Replace(answerHtml, $"{Polito.UserName}:", string.Empty);
        answerHtml = Regex.Replace(answerHtml, "^\n\n", string.Empty);
        answerHtml = Regex.Replace(answerHtml, "^\n", string.Empty);
        return answerHtml.Replace("\n", "<br>").Trim();
    }
}

