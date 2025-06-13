using System.Text.RegularExpressions;

namespace ConversationService.Infrastructure.Messaging.Validators;

public interface IUrlValidator
{
    bool IsValid(string url);
}

public class UrlValidator : IUrlValidator
{
    private static readonly Regex UrlRegex = new Regex(
        @"^https://[a-zA-Z0-9][-a-zA-Z0-9]*\.ngrok-free\.app/?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool IsValid(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
            
        return UrlRegex.IsMatch(url);
    }
} 