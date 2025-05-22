# System Patterns for ConversationService

## Architecture Overview
ConversationService follows a clean, layered architecture pattern within a microservices ecosystem:

- **API Layer**: Controllers handling HTTP requests and responses
  - ConversationController for conversation management
  - ChatController for real-time chat interactions
  - FolderController for folder organization
  - NoteController for note management
  - FeedbackController for feedback collection
- **Service Layer**: Domain-specific services containing business logic
  - ConversationService for conversation management
  - ChatService for AI model interactions
  - ChatHistoryManager for history retrieval and persistence
  - FolderService for folder organization
  - NoteService for note management
  - FeedbackService for feedback handling
- **Caching Layer**: Redis-based caching for performance optimization
  - RedisCacheService for low-level Redis operations
  - CacheManager for high-level caching abstraction
  - Cache-specific exception handling
- **Data Access Layer**: Repository pattern via IUnitOfWork from shared Ollama_DB_layer
- **Integration Layer**: OllamaConnector for AI model integration

## Design Patterns
- **Repository Pattern**: Abstracts data access through repositories from Ollama_DB_layer
- **Unit of Work**: Manages transactions and repository coordination via IUnitOfWork
- **Dependency Injection**: Comprehensive service registration in ServiceExtensions.cs
- **Observer Pattern**: Server-sent events for streaming AI responses
- **Strategy Pattern**: Different strategies for cache retrieval and fallback
- **Factory Pattern**: Creation of chat histories and response objects
- **Decorator Pattern**: Enhanced services with caching behavior
- **Adapter Pattern**: OllamaConnector adapting to the OllamaSharp client
- **Cache-Aside Pattern**: GetOrSetAsync with database fallback strategy
- **Circuit Breaker Pattern**: Timeout and retry logic for cache operations

## Component Relationships
```
Controllers → Services → Repositories/Connectors → Database/External Services
       ↓              ↓
 Validators      Cache Manager
```

- **Controllers**: Handle HTTP requests, validate inputs, manage authentication
- **Validators**: Ensure request data integrity through FluentValidation
- **Services**: Implement business logic, coordinate with repositories and caching
- **ChatHistoryManager**: Manages conversation history with caching integration
- **CacheManager**: Provides caching abstraction with fallback mechanisms
- **RedisCacheService**: Offers low-level Redis operations with error handling
- **Repositories**: Access database via the shared Ollama_DB_layer
- **OllamaConnector**: Integrates with the Ollama AI service via ngrok endpoint

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

## Configuration Management
- **ServiceExtensions.cs**: Extension methods for service registration:
  - AddDatabaseAndIdentity: Database context and identity setup
  - AddJwtAuthentication: JWT authentication configuration
  - AddRepositories: Repository registration
  - AddApplicationServices: Service and connector registration
  - ConfigureCors: CORS policy setup
  - ConfigureCache: Redis cache configuration
  - ConfigureSwagger: Swagger documentation setup
- **appsettings.json**: Environment-specific configuration for:
  - Database connection (SQL Server)
  - Redis connection (Upstash)
  - JWT settings with 30-day duration
  - RedisCacheSettings with domain-specific TTLs
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

## Streaming Implementation
- **Server-Sent Events**: Real-time streaming for chat responses
- **Content-Type**: text/event-stream for proper SSE implementation
- **IAsyncEnumerable**: In OllamaConnector.GetStreamedChatMessageContentsAsync
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

## Security Patterns
- **JWT Authentication**: Token validation with comprehensive checks
- **User Identification**: Claims-based identity and X-User-Id header
- **Role-based Authorization**: Admin and User policies
- **Input Validation**: FluentValidation for request validation
- **HTTPS Enforcement**: In middleware pipeline
- **CORS Configuration**: Specific origin allowance (localhost:5173)
- **Token Security**: 30-day lifetime with full validation

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
- Ollama Service for model inference
- API Gateway for request routing and authentication 