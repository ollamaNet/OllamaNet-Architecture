using System.Text.RegularExpressions;

namespace ConversationService.Services.Rag.Helpers
{
    public class QueryCleaner
    {
        public static string CleanQueryAndExtractKeywords(string userQuery)
        {
            string input = userQuery;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Normalize
            input = input.Trim().ToLowerInvariant();

            // Remove punctuation
            input = Regex.Replace(input, @"[^\w\s]", "");

            // Tokenize and remove stopwords
            var stopWords = new HashSet<string>
            {
                "what", "is", "the", "in", "of", "and", "do", "i", "how", "to",
                "does", "a", "an", "on", "with", "for", "can", "you", "please"
            };

            var keywords = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !stopWords.Contains(word))
                .ToList();

            return string.Join(" ", keywords);
        }
    }
} 