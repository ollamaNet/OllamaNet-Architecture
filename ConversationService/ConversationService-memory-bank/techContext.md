# Technical Context for ConversationService

> **Note:** As of the latest migration (Phases 1-9), ConversationService now uses a fully modular, best-practices folder and namespace structure. All legacy folders have been removed, all files are in their correct locations, and documentation/diagrams are up to date. The current focus is on feature enhancements and performance optimization.

## Core Technologies
- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for data access
- **SQL Server**: Relational database
- **Redis (Upstash)**: Distributed caching
- **Pinecone**: Vector database for RAG
- **Semantic Kernel**: AI orchestration
- **RabbitMQ**: Message broker for service discovery
- **Polly**: Resilience and transient-fault-handling

## Key Dependencies
- **FluentValidation**: Request validation
- **AutoMapper**: Object mapping
- **Newtonsoft.Json**: JSON serialization
- **Microsoft.Extensions.Caching.StackExchangeRedis**: Redis client
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication
- **Microsoft.EntityFrameworkCore.SqlServer**: SQL Server provider
- **RabbitMQ.Client**: RabbitMQ client
- **Polly**: Resilience policies
- **iTextSharp**: PDF processing
- **DocumentFormat.OpenXml**: Word document processing
- **Markdig**: Markdown processing

## Key Components

### Controllers
- **ConversationController**: Manages conversations
  - `GET /api/conversations`: List conversations
  - `GET /api/conversations/{id}`: Get conversation
  - `POST /api/conversations`: Create conversation
  - `PUT /api/conversations/{id}`: Update conversation
  - `DELETE /api/conversations/{id}`: Delete conversation
  - `GET /api/conversations/search`: Search conversations
  - `PUT /api/conversations/{id}/folder`: Move to folder

- **ChatController**: Handles chat interactions
  - `POST /api/chat`: Get model response
  - `POST /api/chat/stream`: Stream model response
  - `GET /api/chat/history/{conversationId}`: Get chat history

- **FolderController**: Manages folder organization
  - `GET /api/folders`: List folders
  - `GET /api/folders/{id}`: Get folder
  - `POST /api/folders`: Create folder
  - `PUT /api/folders/{id}`: Update folder
  - `DELETE /api/folders/{id}`: Delete folder

- **NoteController**: Manages conversation notes
  - `GET /api/notes/{conversationId}`: Get notes
  - `POST /api/notes`: Create note
  - `PUT /api/notes/{id}`: Update note
  - `DELETE /api/notes/{id}`: Delete note

- **FeedbackController**: Handles feedback collection
  - `POST /api/feedback`: Submit feedback
  - `GET /api/feedback/{conversationId}`: Get feedback

- **DocumentController**: Manages document operations
  - `POST /api/documents/upload`: Upload document
  - `GET /api/documents/{id}`: Get document
  - `DELETE /api/documents/{id}`: Delete document
  - `GET /api/documents/conversation/{conversationId}`: List documents

### Services
- **ConversationService**: Manages conversation lifecycle
  - CreateConversationAsync
  - GetConversationsAsync
  - GetConversationByIdAsync
  - UpdateConversationAsync
  - DeleteConversationAsync
  - SearchConversationsAsync
  - MoveConversationToFolderAsync

- **ChatService**: Handles chat interactions
  - GetModelResponseAsync
  - GetStreamedModelResponseAsync
  - GetChatHistoryAsync
  - SaveChatInteractionAsync

- **FolderService**: Manages folder organization
  - CreateFolderAsync
  - GetFoldersAsync
  - GetFolderByIdAsync
  - UpdateFolderAsync
  - DeleteFolderAsync

- **NoteService**: Manages conversation notes
  - CreateNoteAsync
  - GetNotesByConversationIdAsync
  - UpdateNoteAsync
  - DeleteNoteAsync

- **FeedbackService**: Handles feedback collection
  - SubmitFeedbackAsync
  - GetFeedbackByConversationIdAsync

- **DocumentManagementService**: Manages document lifecycle
  - UploadDocumentAsync
  - GetDocumentAsync
  - DeleteDocumentAsync
  - GetConversationDocumentsAsync

- **DocumentProcessingService**: Processes documents
  - ProcessDocumentAsync
  - ExtractTextAsync
  - ChunkTextAsync

- **RagIndexingService**: Handles document indexing
  - IndexDocumentAsync
  - DeleteDocumentIndexAsync

- **RagRetrievalService**: Retrieves relevant context
  - RetrieveContextAsync
  - CleanQueryAsync

### RAG Components
- **InferenceEngineTextEmbeddingGeneration**: Generates embeddings
  - GenerateEmbeddingsAsync

- **PineconeService**: Vector database operations
  - UpsertVectorsAsync
  - QueryAsync
  - DeleteAsync

- **RagOptions**: Configuration settings
  - ChunkSize
  - ChunkOverlap
  - TopK
  - Namespace

- **PineconeOptions**: Pinecone configuration
  - ApiKey
  - Environment
  - IndexName

- **DocumentChunk**: Represents document chunk
  - Id
  - Text
  - Metadata
  - Embedding

- **QueryCleaner**: Preprocesses queries
  - CleanQueryAsync

### Document Processing Components
- **IDocumentStorage**: Storage interface
  - SaveDocumentAsync
  - GetDocumentAsync
  - DeleteDocumentAsync

- **FileSystemDocumentStorage**: File system implementation
  - SaveDocumentAsync
  - GetDocumentAsync
  - DeleteDocumentAsync

- **IDocumentProcessor**: Processor interface
  - ProcessAsync
  - ExtractTextAsync

- **PdfDocumentProcessor**: PDF processor
  - ProcessAsync
  - ExtractTextAsync

- **TextDocumentProcessor**: Text processor
  - ProcessAsync
  - ExtractTextAsync

- **WordDocumentProcessor**: Word processor
  - ProcessAsync
  - ExtractTextAsync

- **MarkdownDocumentProcessor**: Markdown processor
  - ProcessAsync
  - ExtractTextAsync

### Caching Components
- **RedisCacheService**: Low-level Redis operations
  - GetAsync
  - SetAsync
  - RemoveAsync
  - GetOrSetAsync

- **CacheManager**: High-level caching abstraction
  - GetOrSetAsync
  - InvalidateAsync

- **RedisCacheSettings**: Configuration settings
  - ConnectionString
  - InstanceName
  - DefaultTTL
  - RetryCount
  - RetryDelay
  - Timeout

- **CacheKeys**: Centralized key management
  - ConversationList
  - ConversationInfo
  - ChatHistory
  - ModelInfo
  - Tags
  - SearchResults

- **CacheExceptions**: Specialized exceptions
  - CacheConnectionException
  - CacheSerializationException
  - CacheTimeoutException
  - CacheKeyNotFoundException
  - CacheOperationException

### Connectors
- **InferenceEngineConnector**: AI model integration
  - GetChatMessageContentsAsync
  - GetStreamedChatMessageContentsAsync
  - GenerateEmbeddingsAsync

### Validators
- **CreateConversationRequestValidator**: Validates creation
- **UpdateConversationRequestValidator**: Validates updates
- **ChatRequestValidator**: Validates chat requests
- **CreateFolderRequestValidator**: Validates folder creation
- **UpdateFolderRequestValidator**: Validates folder updates
- **CreateNoteRequestValidator**: Validates note creation
- **UpdateNoteRequestValidator**: Validates note updates
- **FeedbackRequestValidator**: Validates feedback
- **UploadDocumentRequestValidator**: Validates uploads

## API Structure

### Conversation Endpoints

```
GET /api/conversations
GET /api/conversations/{id}
POST /api/conversations
PUT /api/conversations/{id}
DELETE /api/conversations/{id}
GET /api/conversations/search?query={query}
PUT /api/conversations/{id}/folder
```

### Chat Endpoints

```
POST /api/chat
POST /api/chat/stream
GET /api/chat/history/{conversationId}
```

### Folder Endpoints

```
GET /api/folders
GET /api/folders/{id}
POST /api/folders
PUT /api/folders/{id}
DELETE /api/folders/{id}
```

### Note Endpoints

```
GET /api/notes/{conversationId}
POST /api/notes
PUT /api/notes/{id}
DELETE /api/notes/{id}
```

### Feedback Endpoints

```
POST /api/feedback
GET /api/feedback/{conversationId}
```

### Document Endpoints

```
POST /api/documents/upload
GET /api/documents/{id}
DELETE /api/documents/{id}
GET /api/documents/conversation/{conversationId}
```

## Caching Implementation

The caching implementation uses Redis (Upstash) with a comprehensive strategy:

### Domain-Specific TTL

Different expiration times based on data type:
- Conversation List: 60 minutes
- Conversation Info: 60 minutes
- Chat History: 60 minutes
- Model Info: 1440 minutes (24 hours)
- Tags: 1440 minutes (24 hours)
- Search Results: 30 minutes

### Cache Key Strategy

Structured key format with prefixes and identifiers:
- `conversation:list:{userId}`
- `conversation:info:{conversationId}`
- `conversation:messages:{conversationId}`
- `chat:history:{conversationId}`
- `model:info:{modelId}`
- `tags:all`
- `search:results:{query}`

### Exception Handling

Specialized exceptions for different failure scenarios:
- `CacheConnectionException`: Connection failures
- `CacheSerializationException`: Serialization issues
- `CacheTimeoutException`: Operation timeouts
- `CacheKeyNotFoundException`: Missing keys
- `CacheOperationException`: General failures

### Retry Logic

Configurable retry parameters with exponential backoff:
- RetryCount: 3
- RetryDelay: 100ms (doubled each retry)

### Timeout

Configurable timeout to prevent hanging operations:
- Timeout: 500ms

### Cache-Aside Pattern

Implemented in `GetOrSetAsync` method:
1. Try to get from cache
2. If not found, get from source
3. Store in cache
4. Return result

## Document Processing Implementation

### Storage Strategy

- File system storage with secure paths
- Path format: `{basePath}/{userId}/{conversationId}/{fileName}`
- Content type validation
- Size limits (configurable)

### Processing Pipeline

1. Upload document
2. Validate content type and size
3. Save to storage
4. Extract text based on document type
5. Chunk text
6. Generate embeddings
7. Index in vector database

### Chunking

- Fixed-size chunks with overlap
- Default chunk size: 1000 characters
- Default overlap: 200 characters
- Paragraph-aware chunking

### Metadata Extraction

- Document title
- Creation date
- Last modified date
- Author (when available)
- Page count (for PDFs)

### Security

- Content type validation
- Size limits
- Secure file paths
- Access control based on user ID

### Performance Monitoring

- Stopwatch for timing operations
- Logging at each processing stage
- Metrics for processing time

## Streaming Implementation

The streaming implementation uses Server-Sent Events (SSE) with a sophisticated approach:

### Server-Sent Events

- Content-Type: text/event-stream
- Connection: keep-alive
- Cache-Control: no-cache

### IAsyncEnumerable

- Used in InferenceEngineConnector.GetStreamedChatMessageContentsAsync
- Enables token-by-token streaming
- Supports cancellation via EnumeratorCancellation

### JSON Serialization

- Each token wrapped in a JSON object
- Format: `{"token": "text", "isComplete": false}`
- Final message: `{"token": "", "isComplete": true}`

### Error Handling

- Try/catch around streaming operations
- Error messages sent as events
- Connection closed on error

### Background Processing

- Task.Run for post-streaming operations
- Saves chat history after streaming completes
- Updates conversation metadata

### Cancellation Support

- CancellationToken passed throughout chain
- EnumeratorCancellation attribute
- Proper cleanup on cancellation

## Authentication and Authorization

### JWT Authentication

- Bearer token authentication
- 30-day token lifetime
- Comprehensive validation checks
- Claims-based identity

### Role-Based Authorization

- Admin and User roles
- [Authorize] attribute with role policies
- Resource ownership validation

### Token Validation

- Issuer validation
- Audience validation
- Lifetime validation
- Signature validation

### HTTPS Enforcement

- Configured in middleware pipeline
- Redirects HTTP to HTTPS
- HSTS headers

### CORS Configuration

- Specific origin allowance (localhost:5173)
- Allowed methods: GET, POST, PUT, DELETE
- Allowed headers: Content-Type, Authorization

### Resource Authorization

- User ID from claims
- Resource ownership checks
- Admin override for all resources

## Error Handling

### Exception Types

- ArgumentException: Invalid arguments
- KeyNotFoundException: Resource not found
- InvalidOperationException: Invalid state
- DocumentProcessingException: Document issues
- CacheExceptions: Cache failures
- ValidationException: Validation failures

### HTTP Status Codes

- 400: Bad Request (validation failures)
- 401: Unauthorized (missing authentication)
- 404: Not Found (resource not found)
- 500: Internal Server Error (general exceptions)

### Logging

- Structured logging with context
- Exception details with stack traces
- Performance timing with Stopwatch
- Log levels based on severity

### Transient Error Handling

- Retry for transient failures
- Circuit breaker for persistent failures
- Fallback mechanisms

### Response Format

- Consistent error response format
- Error code
- Error message
- Error details (when appropriate)
- Example: `{"error": {"code": "NotFound", "message": "Conversation not found"}}`

## Development Environment

### Local Setup

- Visual Studio 2022
- .NET 9.0 SDK
- SQL Server (local or Docker)
- Redis (local or Upstash)
- RabbitMQ (local or Docker)

### External Services

- Inference Engine: AI model service
- SQL Server: Database
- Redis (Upstash): Caching
- Pinecone: Vector database
- RabbitMQ: Message broker

### Configuration

- appsettings.json: Base configuration
- appsettings.Development.json: Development overrides
- User Secrets: Sensitive information
- Environment Variables: Deployment-specific

### Service Registration

- ServiceExtensions.cs: Extension methods
- Startup.cs: Middleware and services

## Service Discovery

### RabbitMQ-Based URL Management

The service discovery mechanism uses RabbitMQ for dynamic configuration updates:

1. **InferenceEngineConfiguration**: Service for URL management
   - Provides current URL to services
   - Updates URL based on messages
   - Persists URL to Redis

2. **Messaging Flow**:
   - Exchange: "service-discovery"
   - Queue: "inference-url-updates"
   - Routing key: "inference.url.changed"
   - Message format: InferenceUrlUpdateMessage

3. **Resilience Patterns**:
   - Retry for connection issues
   - Circuit breaker for outages
   - Fallback to static configuration

4. **Configuration**:
   - RabbitMQ connection settings
   - Exchange and queue declarations
   - Consumer configuration

## Service Dependencies

- **Database**: SQL Server for data persistence
- **InferenceEngine**: AI model service
- **Redis**: Distributed caching
- **RabbitMQ**: Message broker
- **File Storage**: Document storage
- **Identity Service**: Authentication

## Configuration Management

- **Static Configuration**: appsettings.json
- **Environment Variables**: Deployment-specific
- **User Secrets**: Sensitive information
- **Dynamic Configuration**: Runtime updates
- **Feature Flags**: Conditional features

## Development Workflow

- **Local Development**: Visual Studio or VS Code
- **CI/CD**: GitHub Actions
- **Testing**: Unit, Integration, E2E
- **Documentation**: Swagger, README, Architecture docs
- **Code Quality**: StyleCop, SonarQube