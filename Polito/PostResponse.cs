namespace PolitoGPT;

public class Html
{
    public string content { get; set; }
}

public class PostResponse
{
    public string status { get; set; }
    public Html html { get; set; }
    public string quote { get; set; }
    public string quoteHtml { get; set; }
    public Visitor visitor { get; set; }
}

public class Visitor
{
    public string conversations_unread { get; set; }
    public string alerts_unviewed { get; set; }
    public string total_unread { get; set; }
}