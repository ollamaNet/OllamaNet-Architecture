using ConversationService.Services.Document.DTOs;
using ConversationService.Services.Document.DTOs.Requests;
using ConversationService.Services.Document.DTOs.Responses;
using Ollama_DB_layer.Entities;

namespace ConversationService.Services.Document.Mappers;

/// <summary>
/// Mapper for converting between Attachment entity and DTOs
/// </summary>
public static class AttachmentMapper
{
    /// <summary>
    /// Maps an Attachment entity to an AttachmentResponse DTO
    /// </summary>
    public static AttachmentResponse ToResponse(this Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        return new AttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt,
            ConversationId = attachment.Conversation_Id,
            ProcessingStatus = DocumentProcessingStatus.Completed
        };
    }

    /// <summary>
    /// Creates an Attachment entity from an upload request and file path
    /// </summary>
    public static Attachment ToEntity(this UploadDocumentRequest request, string filePath)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return new Attachment
        {
            Id = Guid.NewGuid().ToString(),
            FileName = request.File.FileName,
            FilePath = filePath,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            Conversation_Id = request.ConversationId,
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }
} 