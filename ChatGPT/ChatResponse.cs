namespace PolitoGPT;

public class Choice
{
    public ResponseMessage message { get; set; }
    public string finish_reason { get; set; }
    public int index { get; set; }
}

public class ResponseMessage
{
    public string role { get; set; }
    public string content { get; set; }
}

public class ChatResponse
{
    public ChatResponse()
    {
        choices = new();
    }
    public string id { get; set; }
    public string @object { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public Usage usage { get; set; }
    public List<Choice> choices { get; set; }

    internal string GetFirstChoiceText()
    {
        return choices.First().message.content;
    }
}

public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}

