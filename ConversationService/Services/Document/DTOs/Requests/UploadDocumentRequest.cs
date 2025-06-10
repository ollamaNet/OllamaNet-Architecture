using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ConversationService.Services.Document.DTOs.Requests;

/// <summary>
/// Request DTO for document upload
/// </summary>
public class UploadDocumentRequest
{
    /// <summary>
    /// The file to be uploaded
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// ID of the conversation to attach the document to
    /// </summary> 
    [Required]
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Optional metadata for the document
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
} 