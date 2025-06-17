# System Patterns for ConversationService

> **Note:** As of the latest migration (Phases 1-9), ConversationService now uses a fully modular, best-practices folder and namespace structure. All legacy folders have been removed, all files are in their correct locations, and documentation/diagrams are up to date. The current focus is on feature enhancements and performance optimization.

## Architecture Overview
ConversationService follows a clean, layered architecture pattern within a microservices ecosystem:

- **API Layer**: Controllers handling HTTP requests and responses
  - ConversationController for conversation management
  - ChatController for real-time chat interactions
  - FolderController for folder organization
  - NoteController for note management
  - FeedbackController for feedback collection
  - DocumentController for document upload and management
- **Service Layer**: Domain-specific services containing business logic
  - ConversationService for conversation management
  - ChatService for AI model interactions
  - ChatHistoryManager for history retrieval and persistence
  - FolderService for folder organization
  - NoteService for note management
  - FeedbackService for feedback handling
  - RagService for document retrieval and indexing
  - DocumentManagementService for document lifecycle management
  - DocumentProcessingService for text extraction and processing
- **Infrastructure Layer**: Technical implementations and external service integrations
  - RedisCacheService for low-level Redis operations
  - CacheManager for high-level caching abstraction
  - InferenceEngineConnector for AI model integration
  - RagInfrastructure for embedding and vector operations
  - DocumentStorage for secure file system operations
- **Data Access Layer**: Repository pattern via IUnitOfWork from shared DB layer

## RAG System Architecture
The RAG (Retrieval-Augmented Generation) system follows a clean architecture pattern:

### Infrastructure Layer (`Infrastructure/Rag/`)
- **Embedding**
  - `ITextEmbeddingGeneration`: Interface for text embedding generation
  - `InferenceEngineTextEmbeddingGeneration`: Inference Engine-based implementation
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
  - `QueryCleaner`: Query preprocessing utilities

## Document Processing Architecture

### Infrastructure Layer (`Infrastructure/Document/`)
- **Storage**
  - `IDocumentStorage`: Interface for document storage operations
  - `FileSystemDocumentStorage`: Implementation for file system storage
- **Options**
  - `DocumentManagementOptions`: Configuration for document management
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

## Design Patterns
- **Repository Pattern**: Abstracts data access through repositories from shared DB layer
- **Unit of Work**: Manages transactions and repository coordination via IUnitOfWork
- **Dependency Injection**: Comprehensive service registration in ServiceExtensions.cs
- **Observer Pattern**: Server-sent events for streaming AI responses
- **Strategy Pattern**: Different strategies for cache retrieval and fallback
- **Factory Pattern**: Creation of chat histories and response objects
- **Decorator Pattern**: Enhanced services with caching behavior
- **Adapter Pattern**: InferenceEngineConnector adapting to the InferenceEngine client
- **Cache-Aside Pattern**: GetOrSetAsync with database fallback strategy
- **Circuit Breaker Pattern**: Timeout and retry logic for cache operations
- **Chain of Responsibility**: Document processor selection based on file type
- **Template Method**: Common document processing flow with format-specific implementations

## Component Relationships
```
Controllers → Services → Repositories/Connectors → Database/External Services
       ↓              ↓                  ↓
 Validators      Cache Manager    Document Storage
```

- **Controllers**: Handle HTTP requests, validate inputs, manage authentication
- **Validators**: Ensure request data integrity through FluentValidation
- **Services**: Implement business logic, coordinate with repositories and caching
- **ChatHistoryManager**: Manages conversation history with caching integration
- **CacheManager**: Provides caching abstraction with fallback mechanisms
- **RedisCacheService**: Offers low-level Redis operations with error handling
- **Repositories**: Access database via the shared DB layer
- **InferenceEngineConnector**: Integrates with the Inference Engine service via ngrok endpoint
- **DocumentStorage**: Manages secure file storage operations
- **DocumentProcessors**: Handle format-specific text extraction

## Service Organization
- **ConversationService**: Manages conversation CRUD, search, and organization
  - CreateConversationAsync
  - GetConversationsAsync
  - SearchConversationsAsync
  - UpdateConversationAsync
  - DeleteConversationAsync
  - MoveConversationToFolderAsync
- **ChatService**: Handles chat interactions and streaming responses
  - GetModelResponse (non-streaming)
  - GetStreamedModelResponse (streaming)
  - Internal processing methods for response handling
- **ChatHistoryManager**: Manages conversation history
  - GetChatHistoryWithCachingAsync
  - SaveChatInteractionAsync
  - SaveStreamedChatInteractionAsync
  - InvalidateChatHistoryCacheAsync
- **FolderService**: Manages folder CRUD operations and organization
- **NoteService**: Manages note CRUD operations
- **FeedbackService**: Handles feedback collection and management
- **DocumentManagementService**: Manages document lifecycle
  - UploadDocumentAsync
  - GetDocumentAsync
  - DeleteDocumentAsync
  - GetConversationDocumentsAsync
- **DocumentProcessingService**: Handles document processing
  - ProcessDocumentAsync
  - ExtractTextAsync
  - ChunkTextAsync

## Configuration Management
- **ServiceExtensions.cs**: Extension methods for service registration:
  - AddDatabaseAndIdentity: Database context and identity setup
  - AddJwtAuthentication: JWT authentication configuration
  - AddRepositories: Repository registration
  - AddApplicationServices: Service and connector registration
  - ConfigureCors: CORS policy setup
  - ConfigureCache: Redis cache configuration
  - ConfigureSwagger: Swagger documentation setup
  - AddDocumentServices: Document processors and storage
- **appsettings.json**: Environment-specific configuration for:
  - Database connection (SQL Server)
  - Redis connection (Upstash)
  - JWT settings with 30-day duration
  - RedisCacheSettings with domain-specific TTLs
  - DocumentManagement settings (file size, types, paths)
  - Logging configuration

## API Design
- **RESTful Endpoints**: Organized by resource type with clear URLs
- **HTTP Methods**: Proper use of GET, POST, PUT, DELETE
- **Streaming Endpoint**: SSE for real-time AI responses
- **Authentication**: JWT tokens for secure access
- **Authorization**: Role-based with [Authorize] attribute
- **ProducesResponseType**: Explicit response type documentation
- **Swagger Documentation**: API documentation with status codes
- **Status Codes**: Consistent HTTP status codes
- **File Upload**: Multipart form-data for document uploads

## Caching Strategy
- **Multi-level Caching**: Redis-based distributed caching via Upstash
- **Cache Keys**: Centralized key management in CacheKeys.cs:
  - Conversation-specific keys: ConversationList, ConversationInfo, ConversationMessages
  - Chat-specific keys: ChatHistory
- **TTL Management**: Domain-specific expiration times in RedisCacheSettings
  - Default: 60 minutes
  - Model Info: 24 hours
  - Tags: 24 hours
  - Search Results: 30 minutes
- **Cache Invalidation**: Targeted invalidation on data mutations
- **Fallback Mechanism**: Database retrieval on cache miss
- **Exception Handling**: Specialized exception hierarchy in CacheExceptions.cs
- **Retry Logic**: Configurable retry parameters with exponential backoff
- **Performance Monitoring**: Stopwatch for operation timing

## Error Handling
- **Controller-Level Try/Catch**: Specific exception handling with status codes
- **Logging**: Comprehensive logging with Stopwatch for performance timing
- **HTTP Status Codes**: Mapped from exception types:
  - 400: Bad Request (validation failures, ArgumentException)
  - 401: Unauthorized (missing authentication)
  - 404: Not Found (KeyNotFoundException, InvalidOperationException)
  - 500: Internal Server Error (general exceptions)
- **Custom Exceptions**: Domain-specific exception types
- **Validation Errors**: Structured response format
- **Cache Exceptions**: Specialized hierarchy for different cache failures
- **Document Exceptions**: Specialized hierarchy for document operations

## Streaming Implementation
- **Server-Sent Events**: Real-time streaming for chat responses
- **Content-Type**: text/event-stream for proper SSE implementation
- **IAsyncEnumerable**: In InferenceEngineConnector.GetStreamedChatMessageContentsAsync
- **Response.BodyWriter**: Direct streaming to HTTP response
- **Cancellation Support**: EnumeratorCancellation for proper cancellation
- **JSON Serialization**: Response objects serialized with consistent format
- **Background Processing**: Task.Run for post-streaming operations

## Data Consistency
- **Unit of Work Pattern**: For atomic operations via IUnitOfWork
- **Cache Synchronization**: Invalidation on data changes
- **Cache-Aside Pattern**: Consistent data retrieval strategy
- **Input Validation**: FluentValidation before persistence
- **Error Handling**: Transaction management
- **Contextual Logging**: Operation context in all data operations

## Cross-Cutting Concerns
- **Logging**: Contextual logging with operation metadata and timing
- **Authentication**: JWT-based user identification
- **Authorization**: Role-based access control with policies
- **Caching**: Redis-based performance optimization
- **Validation**: Request validation with FluentValidation
- **Exception Handling**: Controller-level with appropriate status codes
- **Performance Monitoring**: Stopwatch for timing metrics
- **Document Security**: Content validation and secure storage

## Security Patterns
- **JWT Authentication**: Token validation with comprehensive checks
- **User Identification**: Claims-based identity and X-User-Id header
- **Role-based Authorization**: Admin and User policies
- **Input Validation**: FluentValidation for request validation
- **HTTPS Enforcement**: In middleware pipeline
- **CORS Configuration**: Specific origin allowance (localhost:5173)
- **Token Security**: 30-day lifetime with full validation
- **Document Security**:
  - Content type validation
  - File size validation
  - Secure file paths
  - Access control

## Deployment Patterns
- Kubernetes-based deployment
- Containerized services
- Redis cluster for caching
- Health monitoring
- Auto-scaling
- Load balancing

## Integration Patterns
- REST API for service-to-service communication
- Redis for distributed caching
- SQL Server for data persistence
- Inference Engine Service for model inference
- API Gateway for request routing and authentication

## Architectural Patterns

- **Microservices Architecture**: ConversationService is one component in a larger microservices ecosystem
- **Repository Pattern**: Data access abstracted through repositories
- **Unit of Work Pattern**: Transaction management for database operations
- **Dependency Injection**: Used throughout the application for loose coupling
- **Options Pattern**: Configuration handled via IOptions
- **Service Discovery Pattern**: Using RabbitMQ for dynamic InferenceEngine URL updates
- **Circuit Breaker Pattern**: Using Polly for resilient RabbitMQ connections
- **Publisher-Subscriber Pattern**: For service discovery message distribution
- **Caching Pattern**: Redis used for caching and persistent storage

## Design Patterns

- **Adapter Pattern**: InferenceEngineConnector adapting to the InferenceEngine client
- **Factory Pattern**: Creation of services and repositories
- **Strategy Pattern**: For different document processors
- **Observer Pattern**: For InferenceEngine URL change notifications
- **Command Pattern**: For handling requests
- **Builder Pattern**: For constructing complex objects
- **Singleton Pattern**: For services that should have a single instance

## Messaging Patterns

- **Request-Response**: For synchronous API calls
- **Event-Based Communication**: For asynchronous operations
- **Message Queue**: RabbitMQ for service discovery and configuration updates
- **Publish-Subscribe**: Using RabbitMQ topic exchange for URL updates

## Integration Patterns

- **API Gateway**: All requests route through an API gateway
- **Service Registry**: Using RabbitMQ and Redis for service endpoint discovery
- **Message Broker**: RabbitMQ for communication between services
- **Circuit Breaker**: Using Polly for resilient external service calls

## Concurrency Patterns

- **Async/Await**: For non-blocking I/O operations
- **Thread Pool**: For background processing
- **Task-based Asynchronous Pattern (TAP)**: For asynchronous operations
- **IAsyncEnumerable**: In InferenceEngineConnector.GetStreamedChatMessageContentsAsync

## Key Components

- **Controllers**: REST API endpoints
- **Services**: Business logic
- **Repositories**: Data access
- **DTOs**: Data transfer objects
- **Validators**: Request validation
- **Mappers**: Object mapping
- **InferenceEngineConnector**: Integrates with the Inference Engine service
- **InferenceEngineConfiguration**: Manages dynamic URL configuration
- **RabbitMQ Consumer**: Listens for configuration updates

## Key Layers

1. **Presentation Layer**: Controllers
2. **Service Layer**: Services, DTOs, Mappers
3. **Data Access Layer**: Repositories, Entity Framework Core
4. **Integration Layer**: External service connectors
5. **Configuration Layer**: Dynamic configuration management
6. **Messaging Layer**: RabbitMQ consumers and producers

## Resilience Patterns

- **Retry Pattern**: Using Polly for retrying failed operations
- **Circuit Breaker Pattern**: Using Polly for handling faults
- **Fallback Pattern**: Fallback to configuration when Redis is unavailable
- **Timeout Pattern**: Configurable timeouts for external service calls
- **Bulkhead Pattern**: Isolating failures to prevent cascading

## Service Registration

The application uses organized service registration in `ServiceExtensions.cs`:

- **AddDatabaseAndIdentity**: Database and identity services
- **AddJwtAuthentication**: Authentication and authorization
- **AddRepositories**: Data access repositories
- **AddApplicationServices**: Service and connector registration
- **AddInferenceEngineConfiguration**: Service discovery and configuration
- **ConfigureCors**: CORS policies
- **ConfigureCache**: Redis cache
- **ConfigureSwagger**: API documentation

## Configuration Management

The application uses a multi-layered configuration approach:

1. **Static Configuration**: appsettings.json for initial settings
2. **Dynamic Configuration**: InferenceEngineConfiguration for runtime updates
3. **Persistent Configuration**: Redis for configuration persistence
4. **Event-Based Updates**: RabbitMQ for real-time configuration changes
5. **Resilient Fallbacks**: Graceful degradation when services are unavailable

## Service Discovery Implementation

The service discovery mechanism consists of:

1. **InferenceEngineConfiguration**: Central service for URL management
   - Provides current URL via GetBaseUrl()
   - Updates URL via UpdateBaseUrl(string)
   - Notifies subscribers via BaseUrlChanged event
   - Persists URL in Redis cache

2. **RabbitMQ Messaging**:
   - Topic exchange: "service-discovery"
   - Queue: "inference-url-updates"
   - Routing key: "inference.url.changed"
   - Message format: InferenceUrlUpdateMessage

3. **Resilience with Polly**:
   - Retry policy for connection attempts
   - Circuit breaker for handling outages
   - Graceful degradation when services unavailable

4. **Consumer Architecture**:
   - BackgroundService for long-running processing
   - Async message handling
   - Reconnection logic for RabbitMQ

This service discovery mechanism allows the Inference Engine URL to be updated dynamically at runtime without requiring application restart, which is particularly useful with services using dynamic URLs like ngrok. 