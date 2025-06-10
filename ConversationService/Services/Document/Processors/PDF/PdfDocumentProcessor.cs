using UglyToad.PdfPig;
using ConversationService.Services.Document.Processors.Base;
using System.Text;

namespace ConversationService.Services.Document.Processors.PDF;

/// <summary>
/// Document processor for PDF files using PdfPig library
/// </summary>
public class PdfDocumentProcessor : IDocumentProcessor
{
    private static readonly string[] SupportedTypes = new[]
    {
        "application/pdf"
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
        using (var pdfDocument = PdfDocument.Open(memoryStream))
        {
            foreach (var page in pdfDocument.GetPages())
            {
                text.AppendLine(page.Text);
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

        using var pdfDocument = PdfDocument.Open(memoryStream);

        var metadata = new ProcessingMetadata
        {
            TotalPages = pdfDocument.NumberOfPages
        };

        if (pdfDocument.Information != null)
        {
            metadata.Title = pdfDocument.Information.Title;
            metadata.Author = pdfDocument.Information.Author;
            metadata.CreatedAt = pdfDocument.Information.GetCreatedDateTimeOffset()?.DateTime;
            metadata.ModifiedAt = pdfDocument.Information.GetModifiedDateTimeOffset()?.DateTime;

            // Add additional PDF-specific metadata
            if (!string.IsNullOrEmpty(pdfDocument.Information.Creator))
                metadata.AdditionalMetadata["Creator"] = pdfDocument.Information.Creator;
            if (!string.IsNullOrEmpty(pdfDocument.Information.Producer))
                metadata.AdditionalMetadata["Producer"] = pdfDocument.Information.Producer;
        }

        return metadata;
    }
} 