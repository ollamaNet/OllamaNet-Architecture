# RAG Document Processing Components

## Core Components

### 1. Document Controller
- Location: `Controllers/Document/DocumentController.cs`
- Purpose: Handle HTTP requests for document operations
- Endpoints:
  ```
  POST   /api/documents
  DELETE /api/documents/{id}
  GET    /api/documents/{id}
  GET    /api/documents/conversation/{id}
  ```

### 2. Document Management Service
- Location: `Services/Document/Implementation/DocumentManagementService.cs`
- Purpose: Manage document lifecycle and storage
- Key Methods:
  ```csharp
  Task<AttachmentResponse> UploadAsync(UploadDocumentRequest request)
  Task<bool> DeleteAsync(string id)
  Task<AttachmentResponse> GetAsync(string id)
  Task<IEnumerable<AttachmentResponse>> GetByConversationAsync(string conversationId)
  ```

### 3. Document Processing Service
- Location: `Services/Document/Implementation/DocumentProcessingService.cs`
- Purpose: Handle document text extraction and processing
- Key Methods:
  ```csharp
  Task<ProcessingResponse> ProcessAsync(string attachmentId)
  Task<string> ExtractTextAsync(Stream fileStream, string contentType)
  Task<IEnumerable<TextChunk>> ChunkTextAsync(string text)
  ```

### 4. Document Storage
- Location: `Infrastructure/Document/Storage/FileSystemDocumentStorage.cs`
- Purpose: Handle secure file storage operations
- Key Methods:
  ```csharp
  Task<string> SaveAsync(Stream fileStream, string fileName)
  Task<Stream> GetAsync(string filePath)
  Task DeleteAsync(string filePath)
  ```

## Document Processors

### Base Interface
```csharp
public interface IDocumentProcessor
{
    bool SupportsContentType(string contentType);
    Task<string> ExtractTextAsync(Stream fileStream);
    Task<ProcessingMetadata> GetMetadataAsync(Stream fileStream);
}
```

### Implementations
1. PDF Processor
   - Uses PdfPig library
   - Handles PDF text extraction
   - Extracts metadata

2. Text Processor
   - Direct text reading
   - Basic metadata extraction
   - UTF-8 encoding support

3. Word Processor
   - Uses OpenXml SDK
   - Handles DOCX format
   - Extracts document properties

4. Markdown Processor
   - Direct text reading
   - Markdown parsing
   - Metadata extraction

## Data Transfer Objects

### Request DTOs
```csharp
public class UploadDocumentRequest
{
    public IFormFile File { get; set; }
    public string ConversationId { get; set; }
}

public class ProcessDocumentRequest
{
    public string AttachmentId { get; set; }
    public ProcessingOptions Options { get; set; }
}
```

### Response DTOs
```csharp
public class AttachmentResponse
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string ConversationId { get; set; }
}

public class ProcessingResponse
{
    public string AttachmentId { get; set; }
    public ProcessingStatus Status { get; set; }
    public string ErrorMessage { get; set; }
    public ProcessingMetadata Metadata { get; set; }
}
```

## Configuration

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

## Exception Handling

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
``` 