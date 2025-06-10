# RAG Document Processing Flow

## Document Upload Flow

```mermaid
sequenceDiagram
    participant Client
    participant API as DocumentController
    participant Manager as DocumentManagementService
    participant Storage as FileSystemDocumentStorage
    participant DB as Database

    Client->>API: Upload Document
    API->>Manager: Process Upload Request
    Manager->>Storage: Save File
    Storage-->>Manager: File Path
    Manager->>DB: Save Metadata
    DB-->>Manager: Attachment ID
    Manager-->>API: Attachment Response
    API-->>Client: Success Response
```

## Document Processing Flow

```mermaid
sequenceDiagram
    participant Manager as DocumentManagementService
    participant Processor as DocumentProcessingService
    participant Storage as FileSystemDocumentStorage
    participant Extractor as DocumentProcessor
    participant RAG as RagIndexingService

    Manager->>Processor: Process Document
    Processor->>Storage: Get File
    Storage-->>Processor: File Stream
    Processor->>Extractor: Extract Text
    Extractor-->>Processor: Raw Text
    Processor->>Processor: Chunk Text
    Processor->>RAG: Index Chunks
    RAG-->>Processor: Indexing Complete
    Processor-->>Manager: Processing Complete
```

## RAG Integration Flow

```mermaid
sequenceDiagram
    participant Chat as ChatService
    participant RAG as RagRetrievalService
    participant Vector as VectorDB
    participant LLM as OllamaConnector

    Chat->>RAG: Get Context
    RAG->>Vector: Search Similar
    Vector-->>RAG: Relevant Chunks
    RAG->>RAG: Process Results
    RAG-->>Chat: Context
    Chat->>LLM: Generate Response
    LLM-->>Chat: Enhanced Response
```

## Process Steps

### 1. Document Upload
1. Client sends document with conversation ID
2. Validate request (size, type, etc.)
3. Generate secure file path
4. Save file to storage
5. Create attachment record
6. Return attachment metadata

### 2. Document Processing
1. Retrieve document from storage
2. Identify appropriate processor
3. Extract text content
4. Extract metadata
5. Chunk text for RAG
6. Index chunks in vector database

### 3. RAG Integration
1. Receive chat query
2. Search vector database
3. Retrieve relevant chunks
4. Process and rank results
5. Include context in prompt
6. Generate enhanced response

## Error Handling

### Upload Errors
- File too large
- Invalid content type
- Storage failure
- Database error

### Processing Errors
- File not found
- Extraction failure
- Processing timeout
- Indexing failure

### RAG Errors
- Search failure
- Context processing error
- Response generation error

## Security Checks

### Upload Security
1. Validate file size
2. Validate content type
3. Scan for malware
4. Check user permissions

### Processing Security
1. Verify file access
2. Validate processing request
3. Check resource limits

### RAG Security
1. Validate context access
2. Check conversation ownership
3. Verify API permissions 