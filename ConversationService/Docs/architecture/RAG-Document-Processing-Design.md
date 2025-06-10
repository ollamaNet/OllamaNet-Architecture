# RAG Document Processing Feature Design

## Feature Overview

The RAG Document Processing feature enables users to upload documents to conversations, process them for text extraction, and use the content to enhance AI responses through Retrieval-Augmented Generation (RAG).

## Directory Structure

```
ConversationService/
├── Controllers/
│   └── DocumentController.cs
├── Services/
│   └── Document/
│       ├── DTOs/
│       │   ├── Requests/
│       │   │   ├── UploadDocumentRequest.cs
│       │   │   └── ProcessDocumentRequest.cs
│       │   └── Responses/
│       │       ├── AttachmentResponse.cs
│       │       └── ProcessingResponse.cs
│       ├── Interfaces/
│       │   ├── IDocumentManagementService.cs
│       │   └── IDocumentProcessingService.cs
│       ├── Implementation/
│       │   ├── DocumentManagementService.cs
│       │   └── DocumentProcessingService.cs
│       ├── Processors/
│       │   ├── IDocumentProcessor.cs
│       │   ├── PdfDocumentProcessor.cs
│       │   ├── TextDocumentProcessor.cs
│       │   ├── WordDocumentProcessor.cs
│       │   └── MarkdownDocumentProcessor.cs
│       └── Validators/
│           └── DocumentRequestValidator.cs
├── Infrastructure/
│   └── Document/
│       ├── Storage/
│       │   ├── IDocumentStorage.cs
│       │   └── FileSystemDocumentStorage.cs
│       └── Options/
│           └── DocumentManagementOptions.cs
└── Tests/
    └── Document/
        ├── Unit/
        │   └── DocumentProcessingTests.cs
        └── Integration/
            └── DocumentManagementTests.cs
```

## Component Details

### Controllers

#### `DocumentController.cs`
- Handles HTTP requests for document operations
- Endpoints:
  - POST /api/documents (Upload)
  - DELETE /api/documents/{id}
  - GET /api/documents/{id}
  - GET /api/documents/conversation/{conversationId}
- Implements authorization and validation

### Services

#### Document Management
- **IDocumentManagementService**
  - Contract for document CRUD operations
  - Handles file storage coordination
  - Manages document metadata

- **DocumentManagementService**
  - Implements IDocumentManagementService
  - Coordinates between storage and database
  - Handles document lifecycle

#### Document Processing
- **IDocumentProcessingService**
  - Contract for document processing operations
  - Defines text extraction methods
  - Manages processing pipeline

- **DocumentProcessingService**
  - Implements IDocumentProcessingService
  - Coordinates document processors
  - Handles text extraction and chunking

#### Document Processors
- **IDocumentProcessor**
  ```csharp
  public interface IDocumentProcessor
  {
      bool SupportsContentType(string contentType);
      Task<string> ExtractTextAsync(Stream fileStream);
      Task<ProcessingMetadata> GetMetadataAsync(Stream fileStream);
  }
  ```

- **Specific Processors**
  - PdfDocumentProcessor (PdfPig)
  - TextDocumentProcessor (Direct)
  - WordDocumentProcessor (OpenXml)
  - MarkdownDocumentProcessor (Direct)

### DTOs

#### Requests
- **UploadDocumentRequest**
  ```csharp
  public class UploadDocumentRequest
  {
      public IFormFile File { get; set; }
      public string ConversationId { get; set; }
      public Dictionary<string, string> Metadata { get; set; }
  }
  ```

- **ProcessDocumentRequest**
  ```csharp
  public class ProcessDocumentRequest
  {
      public string AttachmentId { get; set; }
      public ProcessingOptions Options { get; set; }
  }
  ```

#### Responses
- **AttachmentResponse**
  ```csharp
  public class AttachmentResponse
  {
      public string Id { get; set; }
      public string FileName { get; set; }
      public string ContentType { get; set; }
      public long FileSize { get; set; }
      public DateTime UploadedAt { get; set; }
      public string ConversationId { get; set; }
      public Dictionary<string, string> Metadata { get; set; }
  }
  ```

### Infrastructure

#### Storage
- **IDocumentStorage**
  ```csharp
  public interface IDocumentStorage
  {
      Task<string> SaveAsync(Stream fileStream, string fileName);
      Task<Stream> GetAsync(string filePath);
      Task DeleteAsync(string filePath);
      Task<bool> ExistsAsync(string filePath);
  }
  ```

- **FileSystemDocumentStorage**
  - Implements IDocumentStorage
  - Manages file system operations
  - Handles path generation and security

#### Options
- **DocumentManagementOptions**
  ```csharp
  public class DocumentManagementOptions
  {
      public long MaxFileSizeBytes { get; set; }
      public string[] AllowedContentTypes { get; set; }
      public string StoragePath { get; set; }
      public int ChunkSize { get; set; }
      public int ChunkOverlap { get; set; }
  }
  ```

## Integration Points

### 1. Database Integration
- Uses UnitOfWork pattern with IAttachmentRepository
- Manages document metadata in Attachments table
- Links documents to conversations

### 2. RAG Integration
- Processes documents for RAG indexing
- Provides text chunks for vector storage
- Integrates with existing RAG services

### 3. Storage Integration
- Manages secure file storage
- Handles file paths and access
- Implements cleanup procedures

## Security Measures

1. **Upload Security**
   - Content type validation
   - File size limits
   - Antivirus integration point
   - Path traversal prevention

2. **Access Control**
   - Authorization checks
   - Conversation ownership validation
   - Secure file access

## Error Handling

### Exception Hierarchy
```csharp
public class DocumentException : Exception
{
    public string DocumentId { get; }
}

public class DocumentProcessingException : DocumentException
{
    public string ProcessingStage { get; }
}

public class DocumentStorageException : DocumentException
{
    public string StorageOperation { get; }
}

public class UnsupportedDocumentTypeException : DocumentException
{
    public string ContentType { get; }
}
```

## Validation Rules

1. **Upload Validation**
   - File size within limits
   - Allowed content types
   - Valid conversation ID
   - File not empty

2. **Processing Validation**
   - Valid attachment ID
   - Supported document type
   - File exists
   - Processing options valid

## Monitoring Points

1. **Performance Metrics**
   - Upload time
   - Processing duration
   - Storage operations
   - RAG indexing time

2. **Error Metrics**
   - Failed uploads
   - Processing errors
   - Storage failures
   - Invalid requests

## Dependencies

1. **External Libraries**
   - PdfPig for PDF processing
   - DocumentFormat.OpenXml for Word documents
   - Required NuGet packages

2. **Internal Dependencies**
   - RAG services
   - UnitOfWork and repositories
   - Authentication services 