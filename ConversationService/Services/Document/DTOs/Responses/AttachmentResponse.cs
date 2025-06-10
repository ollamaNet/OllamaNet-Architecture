using ConversationService.Services.Document.DTOs;

namespace ConversationService.Services.Document.DTOs.Responses;

/// <summary>
/// Response DTO for document/attachment operations
/// </summary>
public class AttachmentResponse
{
    /// <summary>
    /// Unique identifier of the attachment
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Original filename of the document
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the document
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// When the document was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// ID of the conversation this document belongs to
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata associated with the document
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Processing status of the document
    /// </summary>
    public DocumentProcessingStatus ProcessingStatus { get; set; }
} 