using ConversationService.Infrastructure.Document.Options;
using ConversationService.Infrastructure.Document.Storage;
using ConversationService.Services.Document.DTOs;
using ConversationService.Services.Document.DTOs.Requests;
using ConversationService.Services.Document.DTOs.Responses;
using ConversationService.Services.Document.Interfaces;
using ConversationService.Services.Document.Mappers;
using Microsoft.Extensions.Options;
using Ollama_DB_layer.UOW;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using Ollama_DB_layer.Entities;

namespace ConversationService.Services.Document.Implementation;

/// <summary>
/// Implementation of document management service
/// </summary>
public class DocumentManagementService : IDocumentManagementService
{
    private readonly IDocumentStorage _storage;
    private readonly DocumentManagementOptions _options;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentManagementService(
        IDocumentStorage storage,
        IOptions<DocumentManagementOptions> options,
        IUnitOfWork unitOfWork)
    {
        _storage = storage;
        _options = options.Value;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<AttachmentResponse> UploadDocumentAsync(UploadDocumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.File);
        ArgumentException.ThrowIfNullOrEmpty(request.ConversationId);

        // Security validation
        ValidateFile(request.File);

        try
        {
            // Save file to storage
            string filePath;
            using (var stream = request.File.OpenReadStream())
            {
                filePath = await _storage.SaveAsync(stream, request.File.FileName);
            }

            // Create attachment record
            var attachment = new Attachment
            {
                FileName = request.File.FileName,
                FilePath = filePath,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                UploadedAt = DateTime.UtcNow,
                Conversation_Id = request.ConversationId,
                //ProcessingStatus = DocumentProcessingStatus.Pending
            };

            await _unitOfWork.AttachmentRepo.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();

            return new AttachmentResponse
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt,
                ConversationId = attachment.Conversation_Id,
                //ProcessingStatus = attachment.ProcessingStatus
            };
        }
        catch (Exception ex)
        {
            throw new DocumentStorageException("Failed to upload document", ex.ToString());
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteDocumentAsync(string attachmentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(attachmentId);

        var attachment = await _unitOfWork.AttachmentRepo.GetByIdAsync(attachmentId);
        if (attachment == null)
            return false;

        try
        {
            // Delete from storage
            if (!string.IsNullOrEmpty(attachment.FilePath) && await _storage.ExistsAsync(attachment.FilePath))
            {
                await _storage.DeleteAsync(attachment.FilePath);
            }

            // Delete from database
            await _unitOfWork.AttachmentRepo.DeleteAsync(attachmentId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new DocumentStorageException("Failed to delete document", ex.ToString());
        }
    }

    /// <inheritdoc/>
    public async Task<AttachmentResponse> GetDocumentAsync(string attachmentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(attachmentId);

        var attachment = await _unitOfWork.AttachmentRepo.GetByIdAsync(attachmentId);
        if (attachment == null)
            return null;

        return new AttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt,
            ConversationId = attachment.Conversation_Id,
            //ProcessingStatus = attachment.ProcessingStatus
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AttachmentResponse>> GetConversationDocumentsAsync(string conversationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(conversationId);

        var attachments = await _unitOfWork.AttachmentRepo.GetAttachmentsByConversationIdAsync(conversationId);
        
        return attachments.Select(a => new AttachmentResponse
        {
            Id = a.Id,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSize = a.FileSize,
            UploadedAt = a.UploadedAt,
            ConversationId = a.Conversation_Id,
            //ProcessingStatus = a.ProcessingStatus
        });
    }

    /// <inheritdoc/>
    public async Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(string attachmentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(attachmentId);

        var attachment = await _unitOfWork.AttachmentRepo.GetByIdAsync(attachmentId);
        if (attachment == null)
            throw new DocumentNotFoundException("Document not found", attachmentId);

        var content = await _storage.GetAsync(attachment.FilePath);
        return (content, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
    }





    /// <summary>
    /// Validates a file for security and compliance with allowed types and size
    /// </summary>
    private void ValidateFile(IFormFile file)
    {
        // Check file size
        if (file.Length <= 0)
        {
            throw new ArgumentException("File is empty");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            throw new ArgumentException($"File size exceeds the maximum allowed size of {_options.MaxFileSizeBytes / (1024 * 1024)} MB");
        }

        // Check content type
        if (!_options.AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException($"Content type '{file.ContentType}' is not allowed");
        }

        // Validate filename (prevent path traversal and special characters)
        string fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrEmpty(fileName) || fileName != file.FileName)
        {
            throw new SecurityException("Invalid filename");
        }

        // Additional validation for special characters that could be used in path traversal
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new SecurityException("Filename contains invalid characters");
        }

        // Detect potential path traversal attempts
        if (file.FileName.Contains("..") || file.FileName.Contains('/') || file.FileName.Contains('\\'))
        {
            throw new SecurityException("Potential path traversal attempt detected");
        }
    }
} 