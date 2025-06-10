using ConversationService.Services.Document.Processors.Base;

namespace ConversationService.Services.Document.DTOs.Responses;

/// <summary>
/// Response DTO for document processing operations
/// </summary>
public class ProcessingResponse
{
    /// <summary>
    /// ID of the processed document
    /// </summary>
    public string AttachmentId { get; set; } = string.Empty;

    /// <summary>
    /// Processing status of the document
    /// </summary>
    public DocumentProcessingStatus Status { get; set; }

    /// <summary>
    /// Extracted text content from the document
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Metadata extracted from the document
    /// </summary>
    public ProcessingMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Number of chunks the text was split into
    /// </summary>
    public int ChunkCount { get; set; }

    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
} 