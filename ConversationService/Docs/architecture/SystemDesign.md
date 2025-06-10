# ConversationService System Design Plan

## 1. Folder Structure (Current, Post-Migration)

```
ConversationService/
│
├── Controllers/                # API endpoints (Chat, Conversation, Note, Folder, Feedback, Document)
│   └── Validators/             # FluentValidation classes for requests
├── Services/                   # All domain services grouped here
│   ├── Chat/                   # Chat domain logic
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── ChatService.cs, IChatService.cs, HistoryManager.cs
│   ├── Conversation/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── ConversationService.cs, IConversationService.cs
│   ├── Note/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── NoteService.cs, INoteService.cs
│   ├── Folder/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── FolderService.cs, IFolderService.cs
│   ├── Feedback/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── FeedbackService.cs, IFeedbackService.cs
│   ├── Document/               # Document management and processing
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Implementation/
│   │   ├── Interfaces/
│   │   ├── Mappers/
│   │   └── Processors/
│   │       ├── Base/
│   │       ├── PDF/
│   │       ├── Text/
│   │       └── Word/
│   ├── Rag/                    # RAG service layer components
│   │   ├── DTOs/
│   │   │   └── DocumentChunk.cs
│   │   ├── Interfaces/
│   │   │   ├── IRagIndexingService.cs
│   │   │   └── IRagRetrievalService.cs
│   │   ├── Helpers/
│   │   │   └── QueryCleaner.cs
│   │   └── Implementation/
│   │       ├── RagIndexingService.cs
│   │       └── RagRetrievalService.cs
│   └── Shared/                 # (Optional) Shared logic, interfaces, or base classes
├── Infrastructure/             # Cross-cutting concerns
│   ├── Caching/                # Redis, MemoryCache, etc.
│   │   ├── CacheManager.cs
│   │   ├── RedisCacheService.cs
│   │   ├── RedisCacheSettings.cs
│   │   ├── CacheKeys.cs
│   │   └── Exceptions/
│   ├── Document/               # Document storage infrastructure
│   │   ├── Storage/
│   │   ├── Options/
│   │   └── Exceptions/
│   ├── Logging/                # Logging abstractions/services (future)
│   ├── Email/                  # Email sending infrastructure (future)
│   ├── Integration/            # External connectors
│   │   ├── OllamaConnector.cs
│   │   └── IOllamaConnector.cs
│   └── Rag/                    # RAG infrastructure components
│       ├── Embedding/
│       │   ├── ITextEmbeddingGeneration.cs
│       │   └── OllamaTextEmbeddingGeneration.cs
│       ├── VectorDb/
│       │   └── PineconeService.cs
│       └── Options/
│           ├── PineconeOptions.cs
│           └── RagOptions.cs
├── diagrams/                   # Architecture, class, and flow diagrams
├── Docs/                       # Documentation and system design
├── ConversationService-memory-bank/ # Project context and documentation
├── Properties/
├── obj/
├── bin/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── ServiceExtensions.cs
└── ConversationService.csproj
```

---

## 2. Folder & File Responsibilities

- **Controllers/**: Only handle HTTP, validation, and delegate to services.
  - **Validators/**: All FluentValidation classes for request validation.
- **Services/**: All business logic, grouped by domain.
  - **Chat/**, **Conversation/**, **Note/**, **Folder/**, **Feedback/**: Each encapsulates business logic for a domain.
    - **DTOs/**: Data Transfer Objects for each feature.
    - **Mappers/**: Feature-specific mappers (all mappers are now feature-specific and reside in their respective domain folders).
  - **Document/**: Document management, processing, and storage integration.
    - **DTOs/**: Document-related data transfer objects.
    - **Implementation/**: Document service implementations.
    - **Interfaces/**: Document service interfaces.
    - **Processors/**: Document format-specific text extraction processors.
  - **Shared/**: (Optional) Shared logic, interfaces, or base classes used by multiple domains.
- **Infrastructure/**: Cross-cutting concerns.
  - **Caching/**: `CacheManager.cs`, `RedisCacheService.cs`, `RedisCacheSettings.cs`, `CacheKeys.cs`, cache exceptions.
  - **Document/**: Document storage, options, and exceptions.
  - **Logging/**: (future) Logging abstractions, adapters, or providers.
  - **Email/**: (future) SMTP, SendGrid, or notification services.
  - **Integration/**: `OllamaConnector.cs`, `IOllamaConnector.cs`, any other external system connectors.
- **diagrams/**: PlantUML or other diagrams for architecture, flows, and classes.
- **Docs/**: All documentation, including this system design plan.
- **ConversationService-memory-bank/**: Project context, memory, and documentation.

---

## 3. Best Practices

- **Naming**: Use clear, descriptive, and consistent names for all files and folders.
- **DTOs**: Always grouped by feature for clarity and maintainability.
- **Services**: All business logic, one service per domain, grouped under Services/.
- **Dependency Injection**: All services, helpers, and infrastructure registered via DI.
- **Single Responsibility Principle**: Each class/folder has one clear purpose.
- **Options Pattern**: For all configuration (e.g., caching, integration, document management).
- **Documentation**: Keep Docs/ and memory-bank/ up to date with every major change.
- **Reserve Folders**: For future cross-cutting concerns (Caching, Logging, Email, etc.), even if not yet implemented.

---

## 4. Migration Note
- The migration to the new modular structure is complete. All legacy folders (Cache, Connectors, ChatService, ConversationService, NoteService, FolderService, FeedbackService, Mappers, Helpers) have been removed. All files are now in their correct locations, and namespaces are consistent.
- All mappers are now feature-specific and reside in their respective domain folders.
- This structure is current as of Phase 9 (Cleanup).

---

## 5. Legacy Code/Deprecation
- If any files, classes, or features are identified as legacy or planned for deprecation, mark them clearly and avoid refactoring them unless necessary.
- If not, assume all code is current and should be organized for future maintainability.

---

## 6. Audit: Current State Snapshot (Post-Migration)

**Domain Folders and Files:**
- Services/Chat/: ChatService.cs, IChatService.cs, HistoryManager.cs, DTOs/, Mappers/
- Services/Conversation/: ConversationService.cs, IConversationService.cs, DTOs/, Mappers/
- Services/Note/: NoteService.cs, INoteService.cs, DTOs/, Mappers/
- Services/Folder/: FolderService.cs, IFolderService.cs, DTOs/, Mappers/
- Services/Feedback/: FeedbackService.cs, IFeedbackService.cs, DTOs/, Mappers/
- Services/Document/: IDocumentManagementService.cs, DocumentManagementService.cs, IDocumentProcessingService.cs, DocumentProcessingService.cs, DTOs/, Processors/

**Shared, Infrastructure, and Helper Files:**
- Infrastructure/Caching/: CacheManager.cs, RedisCacheService.cs, RedisCacheSettings.cs, CacheKeys.cs, Exceptions/
- Infrastructure/Document/: IDocumentStorage.cs, FileSystemDocumentStorage.cs, Options/, Exceptions/
- Infrastructure/Integration/: OllamaConnector.cs, IOllamaConnector.cs
- Infrastructure/Logging/: (placeholder for future use)
- Infrastructure/Email/: (placeholder for future use)
- ServiceExtensions.cs, Program.cs, RedisCache_Implementation_Guide.md, .cursorrules

**Controllers and Validators:**
- Controllers/: ChatController.cs, ConversationController.cs, NoteController.cs, FolderController.cs, FeedbackController.cs, DocumentController.cs, Validators/

**Legacy/Deprecated Files:**
- No files explicitly marked as deprecated or legacy in the current structure.

---

**This snapshot provides the current state after migration to the new structure.**

**This plan ensures ConversationService remains simple, modular, and ready for future growth, following best practices for modern .NET microservices.**

## 7. RAG System Architecture

The RAG (Retrieval-Augmented Generation) system follows a clean architecture pattern with clear separation between infrastructure and service layers:

### Infrastructure Layer (`Infrastructure/Rag/`)
- **Embedding**
  - `ITextEmbeddingGeneration`: Interface for text embedding generation
  - `OllamaTextEmbeddingGeneration`: Ollama-based implementation
- **Vector Database**
  - `IPineconeService`: Interface for vector database operations
  - `PineconeService`: Pinecone implementation for vector storage and retrieval
- **Configuration**
  - `RagOptions`: RAG system configuration
  - `PineconeOptions`: Pinecone-specific settings

### Service Layer (`Services/Rag/`)
- **Interfaces**
  - `IRagIndexingService`: Document indexing operations
  - `IRagRetrievalService`: Context retrieval operations
- **Implementation**
  - `RagIndexingService`: Document processing and indexing
  - `RagRetrievalService`: Query processing and context retrieval
- **DTOs**
  - `DocumentChunk`: Document chunk representation
- **Helpers**
  - `QueryCleaner`: Query preprocessing utilities (temporarily disabled for evaluation)

### Key Features
- Clean separation between infrastructure and business logic
- Dependency injection for all components
- Clear interface boundaries
- Configuration-driven behavior
- Proper error handling and logging
- Integration with chat service for enhanced responses

### Current Status
- Core RAG functionality implemented and operational
- Query cleaning functionality temporarily disabled for evaluation
- Integration with chat service complete
- Vector storage using Pinecone
- Text embedding via Ollama
- Document processing with proper chunking
- Document metadata support for enhanced context retrieval

## 8. Document Processing Architecture

The Document Processing system follows a modular, extensible architecture with separate infrastructure and service layers:

### Infrastructure Layer (`Infrastructure/Document/`)
- **Storage**
  - `IDocumentStorage`: Interface for document storage operations
  - `FileSystemDocumentStorage`: Implementation for file system storage
- **Options**
  - `DocumentManagementOptions`: Configuration for document management including:
    - Allowed file types
    - Maximum file size
    - Storage paths
    - Security settings
- **Exceptions**
  - `DocumentException`: Base exception for document operations
  - `DocumentStorageException`: Storage-specific exceptions
  - `DocumentProcessingException`: Processing-specific exceptions
  - `UnsupportedDocumentTypeException`: Format validation exceptions

### Service Layer (`Services/Document/`)
- **Interfaces**
  - `IDocumentManagementService`: Document lifecycle management
  - `IDocumentProcessingService`: Document processing operations
- **Implementation**
  - `DocumentManagementService`: Handles document storage and metadata
  - `DocumentProcessingService`: Manages text extraction and processing
- **DTOs**
  - `Requests/UploadDocumentRequest`: Document upload parameters
  - `Responses/AttachmentResponse`: Document metadata response
  - `Responses/ProcessingResponse`: Processing result with metrics
- **Processors**
  - `Base/IDocumentProcessor`: Base interface for all processors
  - `PDF/PdfDocumentProcessor`: PDF-specific text extraction
  - `Text/TextDocumentProcessor`: Plain text processing
  - `Word/WordDocumentProcessor`: Word document processing
  - `Markdown/MarkdownDocumentProcessor`: Markdown processing

### Key Features
- Extensible processor architecture for multiple document formats
- Clean separation of concerns with interface-based design
- Format-specific text extraction strategies
- Performance monitoring with detailed metrics
- Comprehensive error handling and logging
- Secure file storage with access control
- Integration with RAG system for context-enhanced conversations
- Text chunking with configurable size and overlap

### Current Status
- Document upload API implemented and operational
- Multiple format processors functioning (PDF, Text, Word, Markdown)
- RAG integration complete with document chunking
- Security implementation with content validation
- Performance monitoring with processing metrics
- Error handling and logging integrated