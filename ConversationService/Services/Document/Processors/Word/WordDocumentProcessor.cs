using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ConversationService.Services.Document.Processors.Base;
using System.Text;

namespace ConversationService.Services.Document.Processors.Word;

/// <summary>
/// Document processor for Word documents using OpenXml
/// </summary>
public class WordDocumentProcessor : IDocumentProcessor
{
    private static readonly string[] SupportedTypes = new[]
    {
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
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

        // Create a memory stream to avoid disposing the original stream
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var text = new StringBuilder();
        using (var wordDocument = WordprocessingDocument.Open(memoryStream, false))
        {
            var body = wordDocument.MainDocumentPart?.Document.Body;
            if (body != null)
            {
                // Extract text from paragraphs
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    text.AppendLine(paragraph.InnerText);
                }
            }
        }

        return text.ToString();
    }

    /// <inheritdoc/>
    public async Task<ProcessingMetadata> GetMetadataAsync(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        // Create a memory stream to avoid disposing the original stream
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var wordDocument = WordprocessingDocument.Open(memoryStream, false);
        var coreProps = wordDocument.PackageProperties;

        var metadata = new ProcessingMetadata
        {
            Title = coreProps?.Title,
            Author = coreProps?.Creator,
            CreatedAt = coreProps?.Created,
            ModifiedAt = coreProps?.Modified
        };

        // Count pages (approximate based on page breaks)
        var body = wordDocument.MainDocumentPart?.Document.Body;
        if (body != null)
        {
            var pageBreaks = body.Descendants<LastRenderedPageBreak>().Count();
            metadata.TotalPages = pageBreaks + 1; // Add 1 for the last page

            // Add Word-specific metadata
            metadata.AdditionalMetadata["ParagraphCount"] = 
                body.Elements<Paragraph>().Count().ToString();
            metadata.AdditionalMetadata["Revision"] = 
                coreProps?.Revision?.ToString() ?? "1";
            if (!string.IsNullOrEmpty(coreProps?.Category))
                metadata.AdditionalMetadata["Category"] = coreProps.Category;
        }

        return metadata;
    }
} 