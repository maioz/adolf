using System.Net;
namespace PolitoGPT;

public class Bot
{
    readonly Polito _polito;
    readonly OpenAI _openAi;
    readonly Completion _completion;
    private int _lastAlertID;

    public Bot(string openaiToken, string politzCookie, CompletionType completionType, string lastAlertID)
    {
        _polito = new(politzCookie);
        _openAi = new(openaiToken);

        _completion = GetCompletionType(completionType);

        _lastAlertID = Int32.Parse(lastAlertID);
    }

    Completion GetCompletionType(CompletionType type)
    {
        if(type == CompletionType.Davinci)
            return new Davinci(_openAi);

        if(type == CompletionType.Gpt35)
            return new Gpt35(_openAi);

        throw new Exception($"completion type not implemented: {type}");
    }


    async Task ProcessNotification(Notification notification)
    {
        var postId = notification.PostId;
        var alertId = notification.AlertId;
        var type = notification.Type;

        Log($"Getting value for post {postId}, alert {alertId}");

        var conversation = await _polito.GetConversation(postId, type);

        var xfToken = conversation.Token;
        var threadId = conversation.ThreadId;

        if(conversation.IsEmpty)
        {
            Log("Conversation is empty, skiping");
            return;
        }

        string anwserValue = await GetAnwserValue(conversation);

        if(string.IsNullOrEmpty(anwserValue))
        {
            Log("Skiping, response is empty");
            return;
        }

        Log($"Replying thread {threadId}, user {notification.UserName}.");
        await _polito.Reply(threadId, xfToken, anwserValue);

        Log($"Mention {alertId} finished succefully!");

    }

    async Task<string> GetAnwserValue(Conversation conversation)
    {
        var post = conversation.OriginalPost;

        var content = post.Content.ToLower();

        var images = new string[] { "--imagem", "--image", "--img", "--foto" };

        if(images.Any(content.Contains) && false)
        {
            foreach(var item in images)
                content = content.Replace(item, string.Empty);

            content = content.Replace("chatgpt", string.Empty)
                .Trim();

            return await GetAnwserImage(content, post.Html);
        }
        else
        {
            return await GetAnwserCompletion(conversation);
        }
    }

    async Task<string> GetAnwserCompletion(Conversation conversation)
    {
        var post = conversation.OriginalPost;

        Log($"Completion: {_completion.Name}. Getting chat for message: {post.Content}");

        var messageArray = conversation.Posts.Reverse().ToArray();

        var answer = await _completion.GetCompletion(messageArray);

        if(string.IsNullOrEmpty(answer))
            return string.Empty;

        return post.Html += $"<p>{answer}</p>";
    }

    async Task<string> GetAnwserImage(string content, string postHtmlDecode)
    {
        Log($"Getting image for message: {content}");

        try
        {
            var image = await _openAi.GenerateImage(content);
            var url = image.data.First().url;

            return postHtmlDecode + $"<img src=\"{url}\" />";

        }
        catch(HttpRequestException ex)
            when(ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return postHtmlDecode + $"<p>O texto contem termos inadequados</p>";
        }
    }

    static void Log(string messagem) => Console.WriteLine(messagem);

    public async Task Run()
    {
        Log($"Starting bot");
        Log($"Completion model selected: {_completion.Name}");

        while(true)
        {
            var allNotifications = Array.Empty<Notification>();

            try
            {
                var not = await _polito.GetNotifications();

                if(_lastAlertID == 0)
                {
                    _lastAlertID = not.Select(x => x.AlertIdInt)
                        .OrderByDescending(x => x)
                        .FirstOrDefault();
                }

                allNotifications = not.Where(n => n.AlertIdInt > _lastAlertID)
                    .ToArray();

                _lastAlertID = not.Select(x => x.AlertIdInt)
                            .OrderByDescending(x => x)
                            .FirstOrDefault();

                Log($"{allNotifications.Length} notifications found");
            }
            catch(Exception ex)
            {
                Log(ex.Message);
            }


            foreach(var notification in allNotifications)
            {
                try
                {
                    ProcessNotification(notification).Wait();
                }
                catch(Exception ex)
                {
                    Log(ex.Message);
                }
            }

            Thread.Sleep(10_000);
        }

    }
}