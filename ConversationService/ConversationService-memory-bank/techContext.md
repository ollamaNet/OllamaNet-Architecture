# Technical Context for ConversationService

> **Note:** As of the latest migration (Phases 1-9), ConversationService now uses a fully modular, best-practices folder and namespace structure. All legacy folders have been removed, all files are in their correct locations, and documentation/diagrams are up to date. The current focus is on feature enhancements and performance optimization.

## Core Technologies
- **.NET 9.0**: Modern .NET platform for building the service
- **ASP.NET Core**: Web API framework for REST endpoints and streaming responses
- **Entity Framework Core**: ORM for database operations through the shared Ollama_DB_layer
- **SQL Server**: Primary relational database for data persistence (at db19911.public.databaseasp.net)
- **Redis**: Distributed caching for performance optimization using Upstash
- **InferenceEngine Client**: Client library for interacting with Inference Engine AI models
- **Semantic Kernel**: Microsoft's framework for chat completion capabilities
- **FluentValidation**: Request validation framework for input validation
- **Swagger/OpenAPI**: API documentation and interactive testing
- **Pinecone**: Vector database for RAG system document storage and retrieval
- **PdfPig**: PDF text extraction for document processing
- **DocumentFormat.OpenXml**: Processing Word documents for text extraction
- **RabbitMQ.Client**: RabbitMQ messaging client
- **Polly**: Resilience and transient fault handling

## Key Dependencies
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication implementation
- **Microsoft.AspNetCore.Identity**: User identity and role management
- **Microsoft.EntityFrameworkCore**: Data access and ORM functionality
- **Microsoft.Extensions.Caching**: Caching abstractions and implementations
- **Microsoft.SemanticKernel**: AI chat completion frameworks
- **StackExchange.Redis**: Redis client for distributed caching
- **InferenceEngine Client**: Inference Engine API client for AI model interactions
- **FluentValidation.AspNetCore**: Request validation framework
- **Swashbuckle.AspNetCore**: Swagger integration for API documentation
- **Ollama_DB_layer**: Shared database access layer with repositories and entities
- **Pinecone**: Vector database client for RAG operations
- **UglyToad.PdfPig**: PDF text extraction and processing
- **DocumentFormat.OpenXml**: Word document processing and text extraction
- **RabbitMQ**: Message broker for service discovery

## Key Components

### Controllers
- **ConversationController**: Manages conversation lifecycle (create, read, update, delete, search)
- **ChatController**: Handles chat interactions with streaming support
- **FolderController**: Manages folder organization for conversations
- **NoteController**: Handles note creation and management
- **FeedbackController**: Manages user feedback on AI responses
- **DocumentController**: Handles document upload, retrieval, and management

### Services
- **ConversationService**: Business logic for conversation management
- **ChatService**: Handles AI model interactions and response processing
- **ChatHistoryManager**: Manages chat history retrieval, caching, and persistence
- **FolderService**: Manages folder operations and hierarchies
- **NoteService**: Handles note operations
- **FeedbackService**: Processes and stores user feedback
- **DocumentManagementService**: Manages document lifecycle and storage
- **DocumentProcessingService**: Handles document text extraction and processing
- **RagIndexingService**: Handles document processing and indexing for RAG
- **RagRetrievalService**: Manages context retrieval for RAG-enhanced responses

### RAG Components
- **Infrastructure Layer**
  - **InferenceEngineTextEmbeddingGeneration**: Generates text embeddings using Inference Engine
  - **PineconeService**: Manages vector database operations
  - **RagOptions**: Configuration for RAG system behavior
  - **PineconeOptions**: Configuration for Pinecone integration
- **Service Layer**
  - **RagIndexingService**: Processes and indexes documents
  - **RagRetrievalService**: Retrieves relevant context for queries
  - **DocumentChunk**: DTO for document segments
  - **QueryCleaner**: Helper for query preprocessing

### Document Processing Components
- **Infrastructure Layer**
  - **IDocumentStorage**: Interface for document storage operations
  - **FileSystemDocumentStorage**: File system storage implementation
  - **DocumentManagementOptions**: Configuration for document management
  - **DocumentException**: Base exception for document operations
- **Service Layer**
  - **IDocumentManagementService**: Document lifecycle management interface
  - **DocumentManagementService**: Implementation for document lifecycle
  - **IDocumentProcessingService**: Document processing interface
  - **DocumentProcessingService**: Implementation for text extraction
  - **IDocumentProcessor**: Base interface for document processors
  - **PdfDocumentProcessor**: PDF-specific text extraction
  - **TextDocumentProcessor**: Text file processing
  - **WordDocumentProcessor**: Word document processing
  - **MarkdownDocumentProcessor**: Markdown processing
  - **ProcessingResponse**: Result DTO with metrics and metadata

### Caching Components
- **RedisCacheService**: Low-level Redis operations implementation with error handling
- **CacheManager**: High-level caching abstraction with fallback mechanisms
- **RedisCacheSettings**: Configuration for Redis connection and domain-specific TTL values
- **CacheKeys**: Centralized cache key management with specific key formats by domain
- **CacheExceptions**: Specialized exception types for different cache failure scenarios

### Connectors
- **InferenceEngineConnector**: Abstraction for Inference Engine service integration
- **IInferenceEngineApiClient**: Interface for the Inference Engine API client, currently connecting to Inference Engine instance via ngrok

### Validators
- **OpenConversationRequestValidator**: Validates conversation creation requests
- **UpdateConversationRequestValidator**: Validates conversation update requests
- **ChatRequestValidator**: Validates chat message requests
- **CreateFolderRequestValidator**: Validates folder creation requests
- **AddFeedbackRequestValidator**: Validates feedback submission requests
- **DocumentRequestValidator**: Validates document upload requests

## API Structure
All endpoints follow RESTful conventions with appropriate HTTP methods:

### Conversation Endpoints
- `POST /api/conversations`: Create a new conversation
- `GET /api/conversations`: Get all conversations for the user (paginated)
- `GET /api/conversations/{conversationId}`: Get a specific conversation by ID
- `GET /api/conversations/search`: Search conversations by term
- `GET /api/conversations/{conversationId}/messages`: Get all messages for a conversation
- `PUT /api/conversations/{conversationId}`: Update a conversation
- `DELETE /api/conversations/{conversationId}`: Delete a conversation
- `PUT /api/conversations/{conversationId}/move`: Move conversation to a different folder

### Chat Endpoints
- `POST /api/chats`: Send a chat message (non-streaming)
- `POST /api/chats/stream`: Send a chat message (streaming response)

### Folder Endpoints
- `POST /api/folders`: Create a new folder
- `GET /api/folders`: Get all folders for the user
- `GET /api/folders/{folderId}`: Get a specific folder by ID
- `PUT /api/folders/{folderId}`: Update a folder
- `DELETE /api/folders/{folderId}`: Delete a folder
- `GET /api/folders/{folderId}/conversations`: Get all conversations in a folder

### Note Endpoints
- `POST /api/notes`: Create a new note
- `GET /api/notes/{noteId}`: Get a specific note by ID
- `PUT /api/notes/{noteId}`: Update a note
- `DELETE /api/notes/{noteId}`: Delete a note
- `GET /api/notes/conversation/{conversationId}`: Get all notes for a conversation

### Feedback Endpoints
- `POST /api/feedback`: Submit feedback for an AI response
- `GET /api/feedback/user`: Get all feedback from the current user
- `PUT /api/feedback/{feedbackId}`: Update feedback
- `DELETE /api/feedback/{feedbackId}`: Delete feedback

### Document Endpoints
- `POST /api/documents`: Upload a document to a conversation
- `GET /api/documents/{id}`: Get document metadata by ID
- `DELETE /api/documents/{id}`: Delete a document
- `GET /api/documents/conversation/{conversationId}`: Get all documents for a conversation

## Caching Implementation
The service implements a comprehensive Redis-based caching strategy using Upstash:

- **Distributed Caching**: Redis hosted on Upstash (content-ghoul-42217.upstash.io)
- **Domain-Specific TTL**: Configured expiration times in RedisCacheSettings:
  - Default: 60 minutes
  - Model Info: 1440 minutes (24 hours)
  - Tags: 1440 minutes (24 hours)
  - Search Results: 30 minutes
- **Cache Key Strategy**: Structured key formats in CacheKeys.cs by domain:
  - ConversationList: "conversations:user:{0}"
  - ConversationInfo: "conversation:info:{0}"
  - ChatHistory: "chat:history:{0}"
- **Exception Handling**: Specialized exceptions in CacheExceptions.cs:
  - CacheConnectionException
  - CacheSerializationException
  - CacheTimeoutException
  - CacheKeyNotFoundException
  - CacheOperationException
- **Retry Logic**: Configured in RedisCacheSettings:
  - MaxRetryAttempts: 3
  - RetryDelayMilliseconds: 100 
  - RetryDelayMultiplier: 5
- **Operation Timeout**: 2000 milliseconds (configurable)
- **Cache-Aside Pattern**: GetOrSetAsync methods with database fallback

## Document Processing Implementation
Document processing and RAG integration are implemented with:

- **Storage Strategy**: File system storage with secure paths
- **Processing Pipeline**: Format-specific processors for different document types
- **Chunking Strategy**: Text segmentation with configurable size and overlap
- **Metadata Extraction**: Format-specific metadata from document files
- **Security Measures**:
  - Content type validation
  - File size limits
  - Secure file paths
  - Access control
- **Performance Monitoring**:
  - Processing time tracking
  - Text extraction metrics
  - Chunking metrics
  - RAG indexing time

## Streaming Implementation
Real-time chat responses are implemented using Server-Sent Events (SSE):

- **Content-Type**: text/event-stream for SSE format
- **IAsyncEnumerable**: Asynchronous streaming via InferenceEngineConnector.GetStreamedChatMessageContentsAsync
- **Response.BodyWriter**: Direct writing to the response stream in ChatController.StreamMessage
- **JSON Serialization**: Response objects serialized to JSON for streaming
- **Error Handling**: Status code responses within the streaming context
- **Background Processing**: Task.Run for post-streaming conversation saving
- **Cancellation Support**: EnumeratorCancellation attribute for proper cancellation

## Authentication & Authorization
- **JWT Authentication**: Token-based authentication via JwtBearer with 30-day duration
- **User Identification**: UserId from JWT token claims or X-User-Id header
- **Role-Based Authorization**: Admin and User role policies defined
- **Token Validation**: Comprehensive validation in JwtBearerOptions:
  - ValidateIssuerSigningKey: true
  - ValidateIssuer: true
  - ValidateAudience: true
  - ValidateLifetime: true
- **HTTPS Requirements**: Enforced in pipeline
- **Cross-Origin Resource Sharing**: Configured for localhost:5173
- **Resource Authorization**: Ownership validation for documents and conversations

## Error Handling Strategy
- **Exception Types**: Specialized exceptions for different scenarios
- **HTTP Status Codes**: Mapped from exception types:
  - 400: Bad Request for validation failures
  - 401: Unauthorized for authentication failures
  - 404: Not Found for missing resources
  - 500: Server Error for unhandled exceptions
- **Logging**: Structured logging with Stopwatch for performance timing
- **Transient Error Handling**: Retry logic for cache and external service calls
- **Error Response Format**: Consistent JSON structure with error details
- **Document Processing Errors**: Specialized handling for document-specific issues

## Development Environment
- **IDE**: Visual Studio or VS Code
- **Local Database**: SQL Server (local or containerized)
- **Redis**: Upstash cloud service or local Redis instance
- **API Testing**: Swagger UI, Postman, and ConversationService.http
- **Logging**: Console and file-based logging during development
- **Document Storage**: Local file system with configurable paths

## External Services
- **Inference Engine Service**: AI model inference engine via ngrok endpoint
  - Current endpoint: https://704e-35-196-162-195.ngrok-free.app
- **SQL Server Database**: Data persistence at db19911.public.databaseasp.net
- **Redis Cache**: Distributed caching via Upstash at content-ghoul-42217.upstash.io
- **Pinecone**: Vector database for RAG at pinecone.io
- **RabbitMQ**: Message broker for service discovery

## Configuration Management
- **appsettings.json**: Main configuration for:
  - Database connection string
  - Redis connection string
  - JWT settings
  - RedisCacheSettings with domain-specific TTLs
  - DocumentManagement settings (allowed types, size limits, paths)
  - RagOptions settings (chunk size, retrieval settings)
- **ServiceExtensions.cs**: Centralized service registration
- **Environment-Specific Settings**: Development vs Production configs

## Service Discovery with RabbitMQ
1. **Components**:
   - **InferenceEngineConfiguration**: Service for URL management
   - **InferenceUrlConsumer**: RabbitMQ message consumer
   - **RabbitMQResiliencePolicies**: Resilience patterns for connections
   - **UrlValidator**: Validation for incoming URLs

2. **Messaging Flow**:
   - Admin service publishes URL updates to RabbitMQ exchange
   - ConversationService subscribes to updates via dedicated queue
   - Updates are validated, applied, and persisted to Redis
   - Services using the URL are notified via observer pattern

3. **Resilience**:
   - Retry patterns for connection failures
   - Circuit breaker for RabbitMQ outages
   - Fallback to configuration values when Redis unavailable
   - Graceful reconnection logic

4. **Configuration**:
   ```json
   "RabbitMQ": {
     "HostName": "toucan.lmq.cloudamqp.com",
     "Port": 5672,
     "UserName": "ftyqicrl",
     "Password": "****",
     "VirtualHost": "ftyqicrl",
     "Exchange": "service-discovery",
     "InferenceUrlQueue": "inference-url-updates",
     "InferenceUrlRoutingKey": "inference.url.changed"
   }
   ```

## Service Dependencies

The ConversationService depends on:
1. **Database Service**: SQL Server for data persistence
2. **InferenceEngine Service**: For language model inference
3. **Redis**: For caching and configuration persistence
4. **RabbitMQ**: For service discovery and messaging
5. **File Storage**: For document storage
6. **Identity Service**: For authentication (optional, can use local)

## Configuration Management

1. **appsettings.json**: Static configuration
2. **Environment Variables**: Overrides for deployment
3. **User Secrets**: Local development secrets
4. **Dynamic Configuration**: Runtime updates via service discovery
5. **Feature Flags**: Toggles for features in development

## Development Workflow

1. **Local Development**: Local services or containerized dependencies
2. **CI/CD Pipeline**: Automated builds and deployments
3. **Testing Strategy**: Unit, integration, and end-to-end tests
4. **Documentation**: OpenAPI, readme files, and diagrams
5. **Code Quality**: Linting, code reviews, and static analysis 