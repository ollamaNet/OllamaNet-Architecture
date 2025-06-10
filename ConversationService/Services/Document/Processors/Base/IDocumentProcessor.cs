namespace ConversationService.Services.Document.Processors.Base;

/// <summary>
/// Metadata extracted from a document during processing
/// </summary>
public class ProcessingMetadata
{
    /// <summary>
    /// Total number of pages or sections in the document
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Document title if available
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Document author if available
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Creation date if available
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last modified date if available
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Additional metadata specific to the document type
    /// </summary>
    public Dictionary<string, string> AdditionalMetadata { get; set; } = new();
}

/// <summary>
/// Interface for document processors that handle specific document types
/// </summary>
public interface IDocumentProcessor
{
    /// <summary>
    /// Checks if this processor supports the given content type
    /// </summary>
    /// <param name="contentType">MIME type of the document</param>
    /// <returns>True if supported, false otherwise</returns>
    bool SupportsContentType(string contentType);

    /// <summary>
    /// Extracts text content from a document
    /// </summary>
    /// <param name="fileStream">Document content stream</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextAsync(Stream fileStream);

    /// <summary>
    /// Extracts metadata from a document
    /// </summary>
    /// <param name="fileStream">Document content stream</param>
    /// <returns>Extracted metadata</returns>
    Task<ProcessingMetadata> GetMetadataAsync(Stream fileStream);
} 