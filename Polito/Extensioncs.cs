using System.Text;
using System.Text.RegularExpressions;

namespace PolitoGPT;

public static class RegexExtensions
{
    public const RegexOptions RegexGlobalOptions =
        RegexOptions.Multiline |
        RegexOptions.Singleline |
        RegexOptions.IgnoreCase;


    public static MatchCollection Matchs(this string str, string pattern)
    {
        return Regex.Matches(str, pattern, RegexGlobalOptions, TimeSpan.FromSeconds(5));
    }

    public static Match Match(this string str, string pattern)
    {
        return str.Matchs(pattern).First();
    }
}


public static class StringBuilderExtensions
{
    public static StringBuilder PrependLine(this StringBuilder builder, string value)
    {
        return builder.Prepend($"{value}\n");
    }

    public static StringBuilder Prepend(this StringBuilder builder, string value)
    {
        return builder.Insert(0, value);
    }
}

