using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ConversationService.Infrastructure.Document.Options;
using ConversationService.Infrastructure.Document.Storage;
using ConversationService.Services.Document.DTOs;
using ConversationService.Services.Document.DTOs.Responses;
using ConversationService.Services.Document.Interfaces;
using ConversationService.Services.Document.Processors.Base;
using ConversationService.Services.Rag.DTOs;
using ConversationService.Services.Rag.Interfaces;
using Ollama_DB_layer.UOW;
using ConversationService.Infrastructure.Document.Exceptions;
using ConversationServices.Services.ChatService.DTOs;

namespace ConversationService.Services.Document.Implementation;

/// <summary>
/// Service for processing documents and extracting their content
/// </summary>
public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDocumentStorage _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRagIndexingService _ragIndexingService;
    private readonly IEnumerable<IDocumentProcessor> _processors;
    private readonly DocumentManagementOptions _options;
    private readonly ILogger<DocumentProcessingService> _logger;

    public DocumentProcessingService(
        IDocumentStorage storage,
        IUnitOfWork unitOfWork,
        IRagIndexingService ragIndexingService,
        IEnumerable<IDocumentProcessor> processors,
        IOptions<DocumentManagementOptions> options,
        ILogger<DocumentProcessingService> logger)
    {
        _storage = storage;
        _unitOfWork = unitOfWork;
        _ragIndexingService = ragIndexingService;
        _processors = processors;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ProcessingResponse> ProcessDocumentAsync(string attachmentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(attachmentId);
        _logger.LogInformation("Starting document processing for attachment {AttachmentId}", attachmentId);
        
        var stopwatch = Stopwatch.StartNew();

        // Get attachment
        var attachment = await _unitOfWork.AttachmentRepo.GetByIdAsync(attachmentId);
        if (attachment == null)
        {
            _logger.LogWarning("Document not found: {AttachmentId}", attachmentId);
            throw new Infrastructure.Document.Exceptions.DocumentNotFoundException("Document not found", attachmentId);
        }

        try
        {
            // Update status to processing
            //attachment.ProcessingStatus = DocumentProcessingStatus.Processing;
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Document status set to Processing: {AttachmentId}", attachmentId);

            // Get file content
            _logger.LogDebug("Loading document from storage: {FilePath}", attachment.FilePath);
            using var fileStream = await _storage.GetAsync(attachment.FilePath);

            // Find appropriate processor
            var processor = _processors.FirstOrDefault(p => p.SupportsContentType(attachment.ContentType ?? ""));
            if (processor == null)
            {
                _logger.LogWarning("No processor found for document type: {ContentType}", attachment.ContentType ?? "unknown");
                throw new UnsupportedDocumentTypeException("No processor found for document type", attachmentId)
                {
                    ContentType = attachment.ContentType ?? "unknown"
                };
            }
            _logger.LogInformation("Using {ProcessorType} for document {AttachmentId}", processor.GetType().Name, attachmentId);

            // Extract text and metadata
            var textExtractionTimer = Stopwatch.StartNew();
            var text = await processor.ExtractTextAsync(fileStream);
            textExtractionTimer.Stop();
            _logger.LogInformation("Text extraction completed in {ElapsedMs}ms for document {AttachmentId}", 
                textExtractionTimer.ElapsedMilliseconds, attachmentId);

            fileStream.Position = 0;
            var metadataTimer = Stopwatch.StartNew();
            var metadata = await processor.GetMetadataAsync(fileStream);
            metadataTimer.Stop();
            _logger.LogInformation("Metadata extraction completed in {ElapsedMs}ms for document {AttachmentId}",
                metadataTimer.ElapsedMilliseconds, attachmentId);

            // Create chunks
            _logger.LogDebug("Chunking text with size={ChunkSize}, overlap={ChunkOverlap}", _options.ChunkSize, _options.ChunkOverlap);
            var chunks = ChunkText(text, _options.ChunkSize, _options.ChunkOverlap);
            _logger.LogInformation("Created {ChunkCount} chunks for document {AttachmentId}", chunks.Count, attachmentId);

            // Index each chunk in RAG system
            var indexingTimer = Stopwatch.StartNew();
            var indexedChunkCount = 0;
            foreach (var chunk in chunks)
            {
                await _ragIndexingService.IndexDocumentAsync(new PromptRequest
                {
                    ConversationId = attachment.Conversation_Id,
                    Content = chunk.Text,
                    Metadata = new Dictionary<string, string>
                    {
                        ["ChunkIndex"] = chunk.IndexOnPage.ToString(),
                        ["DocumentId"] = attachmentId,
                        ["FileName"] = attachment.FileName,
                        ["ContentType"] = attachment.ContentType ?? "unknown"
                    }
                });
                indexedChunkCount++;
            }
            indexingTimer.Stop();
            _logger.LogInformation("Indexed {ChunkCount} chunks in {ElapsedMs}ms for document {AttachmentId}",
                indexedChunkCount, indexingTimer.ElapsedMilliseconds, attachmentId);

            // Update attachment with metadata and status
            //attachment.ProcessingStatus = DocumentProcessingStatus.Completed;
            //attachment.Metadata = metadata.AdditionalMetadata;
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Document processing completed for {AttachmentId}", attachmentId);

            stopwatch.Stop();
            _logger.LogInformation("Total processing time: {ElapsedMs}ms for document {AttachmentId}",
                stopwatch.ElapsedMilliseconds, attachmentId);

            return new ProcessingResponse
            {
                AttachmentId = attachmentId,
                Status = DocumentProcessingStatus.Completed,
                ExtractedText = text,
                Metadata = metadata,
                ChunkCount = chunks.Count,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            // Update status to failed
            //attachment.ProcessingStatus = DocumentProcessingStatus.Failed;
            await _unitOfWork.SaveChangesAsync();

            stopwatch.Stop();
            _logger.LogError(ex, "Document processing failed after {ElapsedMs}ms for {AttachmentId}: {ErrorMessage}",
                stopwatch.ElapsedMilliseconds, attachmentId, ex.Message);

            throw new DocumentProcessingException("Failed to process document", ex)
            {
                DocumentId = attachmentId
            };
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExtractTextAsync(string attachmentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(attachmentId);
        _logger.LogInformation("Extracting text for document {AttachmentId}", attachmentId);

        var attachment = await _unitOfWork.AttachmentRepo.GetByIdAsync(attachmentId);
        if (attachment == null)
        {
            _logger.LogWarning("Document not found: {AttachmentId}", attachmentId);
            throw new Infrastructure.Document.Exceptions.DocumentNotFoundException("Document not found", attachmentId);
        }

        try
        {
            using var fileStream = await _storage.GetAsync(attachment.FilePath);
            var processor = _processors.FirstOrDefault(p => p.SupportsContentType(attachment.ContentType ?? ""));
            
            if (processor == null)
            {
                _logger.LogWarning("No processor found for document type: {ContentType}", attachment.ContentType ?? "unknown");
                throw new UnsupportedDocumentTypeException("No processor found for document type", attachmentId)
                {
                    ContentType = attachment.ContentType ?? "unknown"
                };
            }

            var timer = Stopwatch.StartNew();
            var text = await processor.ExtractTextAsync(fileStream);
            timer.Stop();
            
            _logger.LogInformation("Text extraction completed in {ElapsedMs}ms for document {AttachmentId}",
                timer.ElapsedMilliseconds, attachmentId);
                
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Text extraction failed for document {AttachmentId}: {ErrorMessage}",
                attachmentId, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public List<DocumentChunk> ChunkText(string text, int chunkSize = 500, int overlap = 50)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(overlap, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(overlap, chunkSize);

        _logger.LogDebug("Chunking text of length {TextLength} with size={ChunkSize}, overlap={Overlap}",
            text.Length, chunkSize, overlap);

        var chunks = new List<DocumentChunk>();
        var words = text.Split(' ');
        var wordsCount = words.Length;

        if (wordsCount <= chunkSize)
        {
            _logger.LogDebug("Text fits in a single chunk (words: {WordCount})", wordsCount);
            chunks.Add(new DocumentChunk
            {
                IndexOnPage = 0,
                Text = text
            });
            return chunks;
        }

        var index = 0;
        var position = 0;

        while (position < wordsCount)
        {
            var remainingWords = wordsCount - position;
            var currentChunkSize = Math.Min(chunkSize, remainingWords);

            var chunkWords = words.Skip(position).Take(currentChunkSize);
            var chunkText = string.Join(" ", chunkWords);

            chunks.Add(new DocumentChunk
            {
                IndexOnPage = index++,
                Text = chunkText
            });

            position += chunkSize - overlap;
        }

        _logger.LogDebug("Created {ChunkCount} chunks from text", chunks.Count);
        return chunks;
    }
} 