namespace ConversationService.Infrastructure.Document.Storage;

/// <summary>
/// Base exception for document-related operations
/// </summary>
public class DocumentException : Exception
{
    /// <summary>
    /// ID of the document that caused the exception
    /// </summary>
    public string? DocumentId { get; }

    public DocumentException(string message) : base(message)
    {
    }

    public DocumentException(string message, string documentId) : base(message)
    {
        DocumentId = documentId;
    }

    public DocumentException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DocumentException(string message, string documentId, Exception innerException) : base(message, innerException)
    {
        DocumentId = documentId;
    }
}

/// <summary>
/// Exception thrown when document storage operations fail
/// </summary>
public class DocumentStorageException : DocumentException
{
    /// <summary>
    /// The storage operation that failed
    /// </summary>
    public string Operation { get; }

    public DocumentStorageException(string message, string operation) 
        : base(message)
    {
        Operation = operation;
    }

    public DocumentStorageException(string message, string operation, string documentId) 
        : base(message, documentId)
    {
        Operation = operation;
    }

    public DocumentStorageException(string message, string operation, Exception innerException) 
        : base(message, innerException)
    {
        Operation = operation;
    }

    public DocumentStorageException(string message, string operation, string documentId, Exception innerException) 
        : base(message, documentId, innerException)
    {
        Operation = operation;
    }
}

/// <summary>
/// Exception thrown when document validation fails
/// </summary>
public class DocumentValidationException : DocumentException
{
    public DocumentValidationException(string message) : base(message)
    {
    }

    public DocumentValidationException(string message, string documentId) : base(message, documentId)
    {
    }

    public DocumentValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DocumentValidationException(string message, string documentId, Exception innerException) 
        : base(message, documentId, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when document is not found
/// </summary>
public class DocumentNotFoundException : DocumentException
{
    public DocumentNotFoundException(string message) : base(message)
    {
    }

    public DocumentNotFoundException(string message, string documentId) : base(message, documentId)
    {
    }

    public DocumentNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DocumentNotFoundException(string message, string documentId, Exception innerException) 
        : base(message, documentId, innerException)
    {
    }
} 