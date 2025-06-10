using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConversationService.Services.Document.DTOs.Requests;
using ConversationService.Services.Document.DTOs.Responses;
using ConversationService.Services.Document.Interfaces;
using ConversationService.Infrastructure.Document.Storage;

namespace ConversationService.Controllers.Document;

/// <summary>
/// Controller for document management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentManagementService _documentManagementService;
    private readonly IDocumentProcessingService _documentProcessingService;

    public DocumentController(
        IDocumentManagementService documentManagementService,
        IDocumentProcessingService documentProcessingService)
    {
        _documentManagementService = documentManagementService;
        _documentProcessingService = documentProcessingService;
    }

    /// <summary>
    /// Uploads a new document and associates it with a conversation
    /// </summary>
    /// <param name="request">Upload request containing file and conversation details</param>
    /// <returns>Response containing the uploaded document details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AttachmentResponse>> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        try
        {
            var response = await _documentManagementService.UploadDocumentAsync(request);
            
            // Start processing asynchronously
            _ = _documentProcessingService.ProcessDocumentAsync(response.Id);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }




    /// <summary>
    /// Deletes a document by its ID
    /// </summary>
    /// <param name="id">ID of the document to delete</param>
    /// <returns>Success or failure status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteDocument(string id)
    {
        var result = await _documentManagementService.DeleteDocumentAsync(id);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Gets a document by its ID
    /// </summary>
    /// <param name="id">ID of the document to retrieve</param>
    /// <returns>Document details if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AttachmentResponse>> GetDocument(string id)
    {
        try
        {
            var document = await _documentManagementService.GetDocumentAsync(id);
            return Ok(document);
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Lists all documents in a conversation
    /// </summary>
    /// <param name="conversationId">ID of the conversation</param>
    /// <returns>List of documents in the conversation</returns>
    [HttpGet("conversation/{conversationId}")]
    [ProducesResponseType(typeof(IEnumerable<AttachmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetConversationDocuments(string conversationId)
    {
        var documents = await _documentManagementService.GetConversationDocumentsAsync(conversationId);
        return Ok(documents);
    }

    /// <summary>
    /// Downloads a document's content
    /// </summary>
    /// <param name="id">ID of the document to download</param>
    /// <returns>Document file stream</returns>
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DownloadDocument(string id)
    {
        try
        {
            var (content, contentType, fileName) = await _documentManagementService.DownloadDocumentAsync(id);
            return File(content, contentType, fileName);
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Manually triggers document processing
    /// </summary>
    /// <param name="id">ID of the document to process</param>
    /// <returns>Processing result</returns>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProcessingResponse>> ProcessDocument(string id)
    {
        try
        {
            var result = await _documentProcessingService.ProcessDocumentAsync(id);
            return Ok(result);
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
        catch (Infrastructure.Document.Exceptions.UnsupportedDocumentTypeException ex)
        {
            return BadRequest($"Unsupported document type: {ex.ContentType}");
        }
    }
} 