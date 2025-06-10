using ConversationService.Services.Document.DTOs.Responses;
using ConversationService.Services.Rag.DTOs;

namespace ConversationService.Services.Document.Interfaces;

/// <summary>
/// Service interface for document processing operations
/// </summary>
public interface IDocumentProcessingService
{
    /// <summary>
    /// Processes a document by extracting its text content and metadata
    /// </summary>
    /// <param name="attachmentId">ID of the document to process</param>
    /// <returns>Processing result containing extracted text and metadata</returns>
    Task<ProcessingResponse> ProcessDocumentAsync(string attachmentId);

    /// <summary>
    /// Extracts text content from a document
    /// </summary>
    /// <param name="attachmentId">ID of the document to extract text from</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextAsync(string attachmentId);

    /// <summary>
    /// Chunks text into smaller segments for RAG processing
    /// </summary>
    /// <param name="text">Text to chunk</param>
    /// <param name="chunkSize">Size of each chunk in words</param>
    /// <param name="overlap">Number of words to overlap between chunks</param>
    /// <returns>List of text chunks</returns>
    List<DocumentChunk> ChunkText(string text, int chunkSize = 500, int overlap = 50);
} 