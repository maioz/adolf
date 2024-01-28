using PolitoGPT;

var apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
var cookie = Environment.GetEnvironmentVariable("POLITZ_COOKIE");
var completionType = Environment.GetEnvironmentVariable("COMPLETION_TYPE");
var lastAlertID = Environment.GetEnvironmentVariable("LAST_ALERT_ID") ?? "0";

if(string.IsNullOrEmpty(apiKey))
    throw new Exception("OpenAI key not provided");

if(string.IsNullOrEmpty(cookie))
    throw new Exception("Politz cookie not provided");

completionType ??= nameof(CompletionType.Gpt35);

var valid = Enum.TryParse<CompletionType>(completionType, true, out var type);

if(!valid)
    throw new Exception($"Completion type {completionType} not supported");

var bot = new Bot(apiKey, cookie, type, lastAlertID);

await bot.Run();