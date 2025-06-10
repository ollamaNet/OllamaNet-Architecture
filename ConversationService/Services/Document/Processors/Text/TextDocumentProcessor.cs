using ConversationService.Services.Document.Processors.Base;

namespace ConversationService.Services.Document.Processors.Text;

/// <summary>
/// Document processor for plain text files
/// </summary>
public class TextDocumentProcessor : IDocumentProcessor
{
    private static readonly string[] SupportedTypes = new[]
    {
        "text/plain",
        "text/markdown" // Also handle markdown as plain text
    };

    /// <inheritdoc/>
    public bool SupportsContentType(string contentType)
    {
        return SupportedTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<string> ExtractTextAsync(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using var reader = new StreamReader(fileStream, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    /// <inheritdoc/>
    public async Task<ProcessingMetadata> GetMetadataAsync(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        // For text files, we'll count lines and characters
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        var metadata = new ProcessingMetadata
        {
            TotalPages = 1, // Text files are considered single-page
            CreatedAt = null, // Text files don't have built-in metadata
            ModifiedAt = null
        };

        // Add text-specific metadata
        metadata.AdditionalMetadata["LineCount"] = content.Split('\n').Length.ToString();
        metadata.AdditionalMetadata["CharacterCount"] = content.Length.ToString();
        metadata.AdditionalMetadata["WordCount"] = content.Split(new[] { ' ', '\n', '\r', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries).Length.ToString();

        return metadata;
    }
} 