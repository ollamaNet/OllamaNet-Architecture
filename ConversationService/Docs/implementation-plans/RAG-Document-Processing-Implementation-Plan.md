# RAG Document Processing Implementation Plan

## Overview

This implementation plan outlines the phased development of the RAG Document Processing feature. Each phase is designed to be implemented sequentially, with clear deliverables and dependencies.

## Phase 1: Core Infrastructure (Week 1)

### 1.1 Project Structure Setup
```
ConversationService/
├── Services/
│   └── Document/
│       ├── DTOs/
│       │   ├── Requests/
│       │   └── Responses/
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

### 1.3 Configuration
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

## Phase 2: Document Storage (Week 1-2)

### 2.1 File System Storage Implementation
1. Implement `FileSystemDocumentStorage`
   - Secure file path generation
   - File operations (save, get, delete)
   - Error handling

### 2.2 Document Management Service
1. Implement `DocumentManagementService`
   - File upload handling
   - Storage coordination
   - Metadata management

### 2.3 Exception Types
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

## Phase 3: Document Processing (Week 2-3)

### 3.1 Document Processors
```csharp
public interface IDocumentProcessor
{
    bool SupportsContentType(string contentType);
    Task<string> ExtractTextAsync(Stream fileStream);
    Task<ProcessingMetadata> GetMetadataAsync(Stream fileStream);
}
```

Implement processors for:
1. PDF (PdfPig)
2. Text (Direct)
3. Word (OpenXml)
4. Markdown (Direct)

### 3.2 Processing Service
```csharp
public interface IDocumentProcessingService
{
    Task<ProcessingResult> ProcessDocumentAsync(string attachmentId);
    Task<string> ExtractTextAsync(string filePath);
    Task<IEnumerable<TextChunk>> ChunkTextAsync(string text);
}
```

## Phase 4: API Development (Week 3)

### 4.1 API Endpoints
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

### 4.2 Request/Response Models
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

## Phase 5: RAG Integration (Week 4)

### 5.1 Document Processing Pipeline
1. Implement chunking strategy
   - Fixed size chunks
   - Overlap handling
   - Metadata extraction

2. Vector Storage Integration
   - Chunk embedding
   - Metadata storage
   - Index management

### 5.2 RAG Service Updates
1. Update `RagIndexingService`
   ```csharp
   public interface IRagIndexingService
   {
       Task IndexDocumentAsync(string documentId, IEnumerable<TextChunk> chunks);
       Task DeleteDocumentAsync(string documentId);
   }
   ```

2. Update `RagRetrievalService`
   ```csharp
   public interface IRagRetrievalService
   {
       Task<IEnumerable<RelevantChunk>> GetRelevantChunksAsync(string query, string conversationId);
   }
   ```

## Phase 6: Security (Week 4-5)

### 6.1 Upload Security
1. Content validation
   - File size checks
   - MIME type validation
   - Path traversal prevention

2. Storage security
   - Secure file paths
   - Access control
   - Cleanup policies

### 6.2 Access Control
1. Document ownership
2. Conversation access
3. API authorization

## Phase 7: Monitoring (Week 5)

### 7.1 Performance Monitoring
1. Document metrics
   - Upload time
   - Processing duration
   - Storage usage

2. RAG metrics
   - Indexing time
   - Retrieval performance
   - Context relevance

### 7.2 Error Tracking
1. Upload errors
2. Processing failures
3. Access violations

## Dependencies

### External Packages
- PdfPig
- DocumentFormat.OpenXml
- Required middleware

### Internal Dependencies
- RAG services
- UnitOfWork
- Authentication

## Deployment Configuration

### Storage Settings
```json
{
  "Storage": {
    "BasePath": "uploads/documents",
    "MaxFileSize": "10MB",
    "AllowedTypes": ["pdf", "txt", "docx", "md"]
  }
}
```

### Processing Settings
```json
{
  "Processing": {
    "ChunkSize": 500,
    "ChunkOverlap": 50,
    "MaxProcessingTime": "5m"
  }
}
```

### Security Settings
```json
{
  "Security": {
    "MaxUploadSize": "10MB",
    "ScanTimeout": "30s",
    "AllowedMimeTypes": [
      "application/pdf",
      "text/plain",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "text/markdown"
    ]
  }
}
``` 