using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using System.Linq;
using System.Text.RegularExpressions;

namespace PolitoGPT;

public class Polito
{
    private const int ConversationSize = 3;
    public const string UserName = "Soyboy_da_IGN";
    private const string UserBbCodePattern = @"\[USER.+?\]@(.+?)\[\/USER\]";
    private const string BbCodePattern = @"\[.+?\](.+?)\[\/.+?]";
    private HttpClient _http;

    public Polito(string cookie)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
        };

        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://forum.onipotentes.com/"),
        };

        _http.DefaultRequestHeaders.TryAddWithoutValidation("authority", "forum.politz.com.br");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, text/javascript, */*; q=0.01");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,pt;q=0.8");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("cookie", cookie);
        _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"105\", \"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"105\"");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Linux\"");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-dest", "empty");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "cors");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "same-origin");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");
    }

    static NotificationType TypeFromString(string type)
    {
        return Enum.Parse<NotificationType>(type, true);
    }

    internal async Task<Notification[]> GetNotifications()
    {
        var response = await _http.GetAsync("account/alerts?skip_mark_read=1&skip_summarize=1&show_only=unread");

        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        var cleanHtml = ClearHtml(html);

        var pattern = "<li\\sdata-alert-id=\\\"([0-9]+)\\\".+?<a\\shref=\\\"/members.+?username.+?data-user-id=\\\"([0-9]+)\\\".+?>(.+?)<\\/a>.+?(mencionou|citou).+?<a href=\\\"/posts/([0-9]+)/\\\".+?</li>";

        var matchs = cleanHtml.Matchs(pattern);

        var alerts = matchs.Select(m => new Notification()
        {
            AlertId = m.Groups[1].Value,
            UserId = m.Groups[2].Value,
            UserName = m.Groups[3].Value,
            Type = TypeFromString(m.Groups[4].Value),
            PostId = m.Groups[5].Value,
        });

        return alerts.ToArray();
    }

    internal async Task<Conversation> GetConversation(string postId, NotificationType type)
    {
        var threadId = await GetThreadIdForPost(postId);
        var response = await _http.GetAsync($"threads/{threadId}/reply?quote={postId}");

        response.EnsureSuccessStatusCode();

        var cleanHtml = ClearHtml(await response.Content.ReadAsStringAsync());

        string token = GetToken(cleanHtml);

        var posts = await GetConversationRecursively(postId, token, ConversationSize);

        return new Conversation()
        {
            Token = token,
            ThreadId = threadId,
            Posts = ResponseToPosts(posts)
        };
    }

    private static Post[] ResponseToPosts(List<PostResponse> response)
    {
        var posts = response.Select(p =>
        {
            var (user, value) = GetValueFromQuote(p.quote);

            return new Post()
            {
                Content = FormatFinalContent(value),
                Html = p.quoteHtml,
                Username = user
            };
        });

        return posts.ToArray();
    }

    private static string FormatFinalContent(string content)
    {
        content = content.Trim();
        content = Regex.Replace(content, UserBbCodePattern, "$1", RegexExtensions.RegexGlobalOptions);
        content = Regex.Replace(content, BbCodePattern, "$1", RegexExtensions.RegexGlobalOptions);

        return content;
    }

    private async Task<List<PostResponse>> GetConversationRecursively(
        string postId,
        string token,
        int counter,
        List<PostResponse>? posts = default)
    {
        posts ??= new();

        if(counter-- <= 0)
            return posts;

        var response = await _http.GetAsync($"posts/{postId}/");

        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        var cleanHtml = ClearHtml(html);

        var post = await GetPost(postId, token);

        posts.Add(post);

        var postContentRegex = $@"data-content=""post-{postId}"".+?<\/article>";
        var postMatch = cleanHtml.Match(postContentRegex);
        var postHtml = postMatch.Groups[0].Value;

        var quotePostRegex = @"<blockquote.+?data-source=""post:\s(?'quotePostId'[0-9]+).+?"".+?<a.+?>(?'userName'.+?):<\/a>.+?\/blockquote>";

        var match = postHtml.Matchs(quotePostRegex)
            .FirstOrDefault(m => m.Success);

        if(match != null)
        {
            var quotePostId = match.Groups["quotePostId"].Value;
            await GetConversationRecursively(quotePostId, token, counter, posts);
        }

        return posts;
    }

    private static (string userName, string value) GetValueFromQuote(string quote)
    {
        var match = quote.Match(@"\[QUOTE=""(.+?),.+?\](.+?)\[\/QUOTE\]");

        var userName = match.Groups[1].Value;
        var value = match.Groups[2].Value;

        value = value.Replace("\n", String.Empty);

        return (userName, value);
    }

    private static string GetToken(string cleanHtml)
    {
        var tokenRegex = "<input type=\"hidden\" name=\"_xfToken\" value=\"(.+?)\"";
        var token = cleanHtml.Match(tokenRegex).Groups[1].Value;
        return token;
    }

    private async Task<PostResponse> GetPost(string postId, string xfToken)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("_xfToken", xfToken),
            new KeyValuePair<string, string>("_xfResponseType", "json")
        });

        var response = await _http.PostAsync($"posts/{postId}/quote", content);
        response.EnsureSuccessStatusCode();

        var post = await response.Content.ReadFromJsonAsync<PostResponse>()
            ?? throw new Exception("Invalid post response with 20X");

        return post;
    }

    internal async Task Reply(string threadId, string xfToken, string answer)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("message_html", answer),
            new KeyValuePair<string, string>("_xfToken", xfToken),
            new KeyValuePair<string, string>("_xfResponseType", "json")
        });

        var response = await _http.PostAsync($"/threads/{threadId}/add-reply", content);

        response.EnsureSuccessStatusCode();
    }

    private static string ClearHtml(string html)
    {
        return html.Replace("\\t", string.Empty)
            .Replace("\\n", string.Empty)
            .Replace("\\\"", "\"");
    }

    private async Task<string> GetThreadIdForPost(string postId)
    {
        var response = await _http.GetAsync($"posts/{postId}/");

        response.EnsureSuccessStatusCode();

        var regex = "threads\\/(.+?)\\/";
        var uri = response.RequestMessage?.RequestUri?.AbsolutePath ?? string.Empty;

        var match = uri.Match(regex);

        return match.Groups[1].Value;
    }

    internal async Task MarkAlertAsRead(string alertId, string xfToken)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("_xfToken", xfToken),
            new KeyValuePair<string, string>("_xfResponseType", "json")
        });

        var response = await _http.PostAsync($"account/alert/{alertId}/read", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAlertAsUnread(string alertId, string xfToken)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("_xfToken", xfToken),
            new KeyValuePair<string, string>("_xfResponseType", "json")
        });

        var response = await _http.PostAsync($"account/alert/{alertId}/unread", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> IsOnline()
    {
        var request = await _http.GetAsync("");
        return request.IsSuccessStatusCode;
    }
}