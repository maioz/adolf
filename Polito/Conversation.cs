namespace PolitoGPT;

internal class Conversation
{
    public Conversation()
    {
        Posts = Array.Empty<Post>();
    }
    public string Token { get; set; }
    public string ThreadId { get; set; }
    public Post[] Posts { get; set; }
    public Post OriginalPost => Posts.First();
    public bool IsEmpty => Posts.Length == 0 || String.IsNullOrEmpty(OriginalPost.Content.Trim());
}
