namespace ConversationService.Infrastructure.Document.Storage;

/// <summary>
/// Interface for document storage operations
/// </summary>
public interface IDocumentStorage
{
    /// <summary>
    /// Saves a document stream to storage
    /// </summary>
    /// <param name="fileStream">The document content stream</param>
    /// <param name="fileName">Original file name</param>
    /// <returns>Storage path of the saved file</returns>
    Task<string> SaveAsync(Stream fileStream, string fileName);

    /// <summary>
    /// Retrieves a document from storage
    /// </summary>
    /// <param name="filePath">Storage path of the file</param>
    /// <returns>Stream containing the document content</returns>
    Task<Stream> GetAsync(string filePath);

    /// <summary>
    /// Deletes a document from storage
    /// </summary>
    /// <param name="filePath">Storage path of the file to delete</param>
    Task DeleteAsync(string filePath);

    /// <summary>
    /// Checks if a document exists in storage
    /// </summary>
    /// <param name="filePath">Storage path to check</param>
    /// <returns>True if the file exists, false otherwise</returns>
    Task<bool> ExistsAsync(string filePath);
} 