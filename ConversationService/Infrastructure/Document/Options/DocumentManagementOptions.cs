namespace ConversationService.Infrastructure.Document.Options;

/// <summary>
/// Configuration options for document management functionality
/// </summary>
public class DocumentManagementOptions
{
    /// <summary>
    /// Maximum allowed file size in bytes
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB default

    /// <summary>
    /// List of allowed content types for document uploads
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = new[]
    {
        "application/pdf",
        "text/plain",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/markdown"
    };

    /// <summary>
    /// Base path for document storage
    /// </summary>
    public string StoragePath { get; set; } = "uploads/documents";

    /// <summary>
    /// Size of text chunks for RAG processing
    /// </summary>
    public int ChunkSize { get; set; } = 500;

    /// <summary>
    /// Number of overlapping tokens between chunks
    /// </summary>
    public int ChunkOverlap { get; set; } = 50;

    /// <summary>
    /// Maximum processing time for a single document in seconds
    /// </summary>
    public int MaxProcessingTimeSeconds { get; set; } = 300; // 5 minutes default
} 