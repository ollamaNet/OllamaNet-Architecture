# Document Management and RAG Integration Implementation Plan

## Overview

This implementation plan outlines the phased implementation of document management with the RAG (Retrieval-Augmented Generation) system in the ConversationService. The plan is divided into distinct phases to ensure systematic development and integration.

## Phase 1: Core Infrastructure Setup

### 1.1 Project Structure
```
ConversationService/
├── Services/
│   └── Document/
│       ├── DTOs/
│       ├── Interfaces/
│       ├── Implementation/
│       └── Processors/
└── Infrastructure/
    └── Document/
        ├── Storage/
        └── Options/
```

### 1.2 Base Interfaces
```csharp
public interface IDocumentManagementService
{
    Task<AttachmentResponse> UploadDocumentAsync(IFormFile file, string conversationId);
    Task<bool> DeleteDocumentAsync(string attachmentId);
    Task<AttachmentResponse> GetDocumentAsync(string attachmentId);
    Task<IEnumerable<AttachmentResponse>> GetConversationDocumentsAsync(string conversationId);
}

public interface IDocumentStorage
{
    Task<string> SaveAsync(Stream fileStream, string fileName);
    Task<Stream> GetAsync(string filePath);
    Task DeleteAsync(string filePath);
    Task<bool> ExistsAsync(string filePath);
}
```

### 1.3 Configuration Setup
```json
{
  "DocumentManagement": {
    "MaxFileSizeBytes": 10485760,
    "AllowedContentTypes": [
      "application/pdf",
      "text/plain",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "text/markdown"
    ],
    "StoragePath": "uploads/documents",
    "ChunkSize": 500,
    "ChunkOverlap": 50
  }
}
```

## Phase 2: Document Storage Implementation

### 2.1 File System Storage
1. Implement `FileSystemDocumentStorage`
   - Secure file path generation
   - File system operations
   - Error handling

### 2.2 Document Management Service
1. Implement `DocumentManagementService`
   - File upload handling
   - Storage coordination
   - Basic metadata management

### 2.3 Exception Handling
```csharp
public class DocumentException : Exception
{
    public string DocumentId { get; }
}

public class DocumentStorageException : DocumentException
{
    public string StorageOperation { get; }
}
```

## Phase 3: Document Processing Pipeline

### 3.1 Document Processors
1. Base Interface
```csharp
public interface IDocumentProcessor
{
    bool SupportsContentType(string contentType);
    Task<string> ExtractTextAsync(Stream fileStream);
    Task<ProcessingMetadata> GetMetadataAsync(Stream fileStream);
}
```

2. Implement Processors:
   - PdfDocumentProcessor (PdfPig)
   - TextDocumentProcessor
   - WordDocumentProcessor (OpenXml)
   - MarkdownDocumentProcessor

### 3.2 Processing Service
```csharp
public interface IDocumentProcessingService
{
    Task<ProcessingResult> ProcessDocumentAsync(string attachmentId);
    Task<string> ExtractTextAsync(string filePath);
    Task<IEnumerable<TextChunk>> ChunkTextAsync(string text);
}
```

## Phase 4: API Layer Implementation

### 4.1 Controller Implementation
```csharp
[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    [HttpPost]
    public Task<ActionResult<AttachmentResponse>> UploadDocument([FromForm] UploadDocumentRequest request);

    [HttpDelete("{id}")]
    public Task<ActionResult> DeleteDocument(string id);

    [HttpGet("{id}")]
    public Task<ActionResult<AttachmentResponse>> GetDocument(string id);

    [HttpGet("conversation/{conversationId}")]
    public Task<ActionResult<IEnumerable<AttachmentResponse>>> GetConversationDocuments(string conversationId);
}
```

### 4.2 Request/Response DTOs
```csharp
public class UploadDocumentRequest
{
    public IFormFile File { get; set; }
    public string ConversationId { get; set; }
}

public class AttachmentResponse
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string ConversationId { get; set; }
}
```

## Phase 5: RAG Integration

### 5.1 Document Chunking
1. Implement chunking strategy
2. Add metadata extraction
3. Implement chunk storage

### 5.2 RAG Service Integration
1. Update `RagIndexingService`
   - Add document chunk processing
   - Implement vector storage
   - Add metadata indexing

2. Update `RagRetrievalService`
   - Enhance context retrieval
   - Add document source tracking
   - Implement relevance scoring

## Phase 6: Security Implementation

### 6.1 Upload Security
1. Content type validation
2. File size validation
3. Path traversal prevention
4. Antivirus integration point

### 6.2 Access Control
1. Document ownership validation
2. Conversation access checks
3. Storage security

## Phase 7: Monitoring and Logging

### 7.1 Performance Monitoring
1. Document processing metrics
2. Storage usage tracking
3. RAG performance metrics

### 7.2 Error Logging
1. Upload events
2. Processing errors
3. Access attempts
4. System errors

## Dependencies

### External Libraries
- PdfPig for PDF processing
- DocumentFormat.OpenXml for Word documents
- Required NuGet packages

### Internal Dependencies
- RAG services
- UnitOfWork and repositories
- Authentication services

## Deployment Considerations

1. Storage Configuration
   - File system paths
   - Permissions setup
   - Cleanup policies

2. Performance Settings
   - File size limits
   - Processing timeouts
   - Chunking parameters

3. Security Settings
   - Allowed content types
   - Maximum file sizes
   - Access control rules 