namespace ConversationService.Infrastructure.Document.Exceptions;

/// <summary>
/// Base exception for document-related errors
/// </summary>
public class DocumentException : Exception
{
    /// <summary>
    /// ID of the document that caused the error
    /// </summary>
    public string DocumentId { get; init; } = string.Empty;

    public DocumentException(string message, string documentId) 
        : base(message)
    {
        DocumentId = documentId;
    }

    public DocumentException(string message, string documentId, Exception innerException) 
        : base(message, innerException)
    {
        DocumentId = documentId;
    }
}

/// <summary>
/// Exception thrown when document processing fails
/// </summary>
public class DocumentProcessingException : DocumentException
{
    public DocumentProcessingException(string message, string documentId) 
        : base(message, documentId)
    {
    }

    public DocumentProcessingException(string message, Exception innerException) 
        : base(message, string.Empty, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a document is not found
/// </summary>
public class DocumentNotFoundException : DocumentException
{
    public DocumentNotFoundException(string message, string documentId) 
        : base(message, documentId)
    {
    }
}

/// <summary>
/// Exception thrown when a document type is not supported
/// </summary>
public class UnsupportedDocumentTypeException : DocumentException
{
    /// <summary>
    /// MIME type that was not supported
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    public UnsupportedDocumentTypeException(string message, string documentId) 
        : base(message, documentId)
    {
    }
} 