# Technical Context for ConversationService

> **Note:** As of the latest migration (Phases 1-9), ConversationService now uses a fully modular, best-practices folder and namespace structure. All legacy folders have been removed, all files are in their correct locations, and documentation/diagrams are up to date. The current focus is on feature enhancements and performance optimization.

## Core Technologies
- **.NET 9.0**: Modern .NET platform for building the service
- **ASP.NET Core**: Web API framework for REST endpoints and streaming responses
- **Entity Framework Core**: ORM for database operations through the shared Ollama_DB_layer
- **SQL Server**: Primary relational database for data persistence (at db19911.public.databaseasp.net)
- **Redis**: Distributed caching for performance optimization using Upstash
- **OllamaSharp**: Client library for interacting with Ollama AI models
- **Semantic Kernel**: Microsoft's framework for chat completion capabilities
- **FluentValidation**: Request validation framework for input validation
- **Swagger/OpenAPI**: API documentation and interactive testing
- **Pinecone**: Vector database for RAG system document storage and retrieval
- **PdfPig**: PDF text extraction for document processing

## Key Dependencies
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication implementation
- **Microsoft.AspNetCore.Identity**: User identity and role management
- **Microsoft.EntityFrameworkCore**: Data access and ORM functionality
- **Microsoft.Extensions.Caching**: Caching abstractions and implementations
- **Microsoft.SemanticKernel**: AI chat completion frameworks
- **StackExchange.Redis**: Redis client for distributed caching
- **OllamaSharp**: Ollama API client for AI model interactions
- **FluentValidation.AspNetCore**: Request validation framework
- **Swashbuckle.AspNetCore**: Swagger integration for API documentation
- **Ollama_DB_layer**: Shared database access layer with repositories and entities
- **Pinecone**: Vector database client for RAG operations
- **UglyToad.PdfPig**: PDF text extraction and processing

## Key Components

### Controllers
- **ConversationController**: Manages conversation lifecycle (create, read, update, delete, search)
- **ChatController**: Handles chat interactions with streaming support
- **FolderController**: Manages folder organization for conversations
- **NoteController**: Handles note creation and management
- **FeedbackController**: Manages user feedback on AI responses

### Services
- **ConversationService**: Business logic for conversation management
- **ChatService**: Handles AI model interactions and response processing
- **ChatHistoryManager**: Manages chat history retrieval, caching, and persistence
- **FolderService**: Manages folder operations and hierarchies
- **NoteService**: Handles note operations
- **FeedbackService**: Processes and stores user feedback
- **RagIndexingService**: Handles document processing and indexing for RAG
- **RagRetrievalService**: Manages context retrieval for RAG-enhanced responses

### RAG Components
- **Infrastructure Layer**
  - **OllamaTextEmbeddingGeneration**: Generates text embeddings using Ollama
  - **PineconeService**: Manages vector database operations
  - **RagOptions**: Configuration for RAG system behavior
  - **PineconeOptions**: Configuration for Pinecone integration
- **Service Layer**
  - **RagIndexingService**: Processes and indexes documents
  - **RagRetrievalService**: Retrieves relevant context for queries
  - **DocumentChunk**: DTO for document segments
  - **QueryCleaner**: Helper for query preprocessing

### Caching Components
- **RedisCacheService**: Low-level Redis operations implementation with error handling
- **CacheManager**: High-level caching abstraction with fallback mechanisms
- **RedisCacheSettings**: Configuration for Redis connection and domain-specific TTL values
- **CacheKeys**: Centralized cache key management with specific key formats by domain
- **CacheExceptions**: Specialized exception types for different cache failure scenarios

### Connectors
- **OllamaConnector**: Abstraction for Ollama AI service integration
- **IOllamaApiClient**: Interface for the Ollama API client, currently connecting to Ollama instance via ngrok

### Validators
- **OpenConversationRequestValidator**: Validates conversation creation requests
- **UpdateConversationRequestValidator**: Validates conversation update requests
- **ChatRequestValidator**: Validates chat message requests
- **CreateFolderRequestValidator**: Validates folder creation requests
- **AddFeedbackRequestValidator**: Validates feedback submission requests

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

## Streaming Implementation
Real-time chat responses are implemented using Server-Sent Events (SSE):

- **Content-Type**: text/event-stream for SSE format
- **IAsyncEnumerable**: Asynchronous streaming via OllamaConnector.GetStreamedChatMessageContentsAsync
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

## Development Environment
- **IDE**: Visual Studio or VS Code
- **Local Database**: SQL Server (local or containerized)
- **Redis**: Upstash cloud service or local Redis instance
- **API Testing**: Swagger UI, Postman, and ConversationService.http
- **Logging**: Console and file-based logging during development

## External Services
- **Ollama AI Service**: AI model inference engine via ngrok endpoint
  - Current endpoint: https://704e-35-196-162-195.ngrok-free.app
- **SQL Server Database**: Data persistence at db19911.public.databaseasp.net
- **Redis Cache**: Distributed caching via Upstash at content-ghoul-42217.upstash.io

## Configuration Management
- **appsettings.json**: Main configuration for:
  - Database connection string
  - Redis connection string
  - JWT settings
  - RedisCacheSettings with domain-specific TTLs
- **ServiceExtensions.cs**: Centralized service registration
- **Environment-Specific Settings**: Development vs Production configs 