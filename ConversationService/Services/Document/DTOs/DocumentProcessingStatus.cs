namespace ConversationService.Services.Document.DTOs;

/// <summary>
/// Represents the processing status of a document
/// </summary>
public enum DocumentProcessingStatus
{
    /// <summary>
    /// Document has been uploaded but not processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Document is currently being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Document has been successfully processed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Document processing failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Document is invalid or corrupted
    /// </summary>
    Invalid = 4
} 