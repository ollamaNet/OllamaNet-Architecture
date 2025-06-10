using ConversationService.Services.Document.DTOs.Requests;
using ConversationService.Services.Document.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace ConversationService.Services.Document.Interfaces;

/// <summary>
/// Service interface for document management operations
/// </summary>
public interface IDocumentManagementService
{
    /// <summary>
    /// Uploads a new document and associates it with a conversation
    /// </summary>
    /// <param name="request">Upload request containing file and conversation details</param>
    /// <returns>Response containing the uploaded document details</returns>
    Task<AttachmentResponse> UploadDocumentAsync(UploadDocumentRequest request);

    /// <summary>
    /// Deletes a document by its ID
    /// </summary>
    /// <param name="attachmentId">ID of the document to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    Task<bool> DeleteDocumentAsync(string attachmentId);

    /// <summary>
    /// Retrieves a document by its ID
    /// </summary>
    /// <param name="attachmentId">ID of the document to retrieve</param>
    /// <returns>Document details if found</returns>
    Task<AttachmentResponse> GetDocumentAsync(string attachmentId);

    /// <summary>
    /// Lists all documents associated with a conversation
    /// </summary>
    /// <param name="conversationId">ID of the conversation</param>
    /// <returns>List of documents in the conversation</returns>
    Task<IEnumerable<AttachmentResponse>> GetConversationDocumentsAsync(string conversationId);

    /// <summary>
    /// Downloads a document's content
    /// </summary>
    /// <param name="attachmentId">ID of the document to download</param>
    /// <returns>Document stream if found</returns>
    Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(string attachmentId);
} 