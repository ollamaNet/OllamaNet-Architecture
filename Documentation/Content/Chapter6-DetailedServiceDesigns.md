# Chapter 6: Detailed Service Designs - Documentation Implementation

## Purpose
This document outlines the implementation plan for Chapter 6 of the OllamaNet documentation, focusing on the detailed design of each microservice in the system. Each service will be comprehensively documented with a consistent structure while highlighting their unique features and implementation details.

## Implementation Approach

The implementation will follow these steps for each service:

1. Review all service-specific documentation in memory banks
2. Analyze code structure and implementation details
3. Extract API design and data models
4. Create standardized diagrams for components and workflows
5. Document service-specific features and integration points
6. Ensure consistent terminology with glossary entries

## Service Documentation Structure

Each service section will follow this consistent structure:

1. **Purpose & Responsibility**: Overview of the service's role in the system
2. **API Design**: Endpoints, operations, and communication patterns
3. **Data Model**: Key entities and relationships specific to the service
4. **Sequence Diagrams**: Key operational flows and interactions
5. **Service-specific Components**: Unique features and implementations
6. **Integration Points**: How the service connects with other components

## AdminService Documentation

### AdminService: Purpose & Responsibility

AdminService serves as the central control point for platform administration, providing comprehensive administrative capabilities for managing all aspects of the OllamaNet platform. Its core responsibilities include:

- **User Administration**: Managing user accounts, roles, and permissions
- **Model Management**: Administration of AI models, including metadata and categorization
- **Tag Management**: Creating and maintaining tags for model organization
- **Inference Operations**: Managing model installation and configuration through Ollama integration

The service implements a clean, domain-driven architecture with clear separation of concerns across controllers, services, and infrastructure components.

### AdminService: API Design

The AdminService exposes a RESTful API organized into domain-specific controllers:

**User Operations Endpoints**
- `GET /api/Admin/UserOperations/Users`: Retrieve paginated list of users
- `GET /api/Admin/UserOperations/Users/{id}`: Get user details
- `POST /api/Admin/UserOperations/Users`: Create new user
- `PUT /api/Admin/UserOperations/Users/{id}`: Update user information
- `PATCH /api/Admin/UserOperations/Users/{id}/Status`: Change user status
- `POST /api/Admin/UserOperations/Roles`: Create new role
- `POST /api/Admin/UserOperations/Users/{id}/Roles`: Assign role to user

**AI Model Operations Endpoints**
- `GET /api/Admin/AIModelOperations/Models`: Retrieve paginated list of models
- `GET /api/Admin/AIModelOperations/Models/{id}`: Get model details
- `POST /api/Admin/AIModelOperations/Models`: Create new model
- `PUT /api/Admin/AIModelOperations/Models/{id}`: Update model information
- `DELETE /api/Admin/AIModelOperations/Models/{id}`: Delete model
- `POST /api/Admin/AIModelOperations/Models/{id}/Tags`: Assign tag to model
- `DELETE /api/Admin/AIModelOperations/Models/{id}/Tags/{tagId}`: Remove tag from model

**Tag Operations Endpoints**
- `GET /api/Admin/TagOperations/Tags`: Retrieve all tags
- `GET /api/Admin/TagOperations/Tags/{id}`: Get tag details
- `POST /api/Admin/TagOperations/Tags`: Create new tag
- `PUT /api/Admin/TagOperations/Tags/{id}`: Update tag
- `DELETE /api/Admin/TagOperations/Tags/{id}`: Delete tag

**Inference Operations Endpoints**
- `GET /api/Admin/InferenceOperations/Models`: Get available models from Ollama
- `POST /api/Admin/InferenceOperations/Models/{modelName}/Pull`: Install model with progress reporting
- `DELETE /api/Admin/InferenceOperations/Models/{modelName}`: Delete model from Ollama

All endpoints implement:
- Comprehensive request validation using FluentValidation
- Consistent error responses with appropriate HTTP status codes
- Authentication and authorization requirements (JWT-based)
- Swagger documentation for API discovery

### AdminService: Data Model

The AdminService works with these key entities:

**User Management Entities**
- **ApplicationUser**: Core user entity with identity information
- **Role**: User role for authorization purposes
- **UserRole**: Many-to-many relationship between users and roles
- **UserProfile**: Extended user information and preferences

**Model Management Entities**
- **AIModel**: Core model entity with metadata and configuration
- **ModelVersion**: Version information for models
- **Tag**: Classification and organization tags
- **ModelTag**: Many-to-many relationship between models and tags

**Key Relationships**
- Users can have multiple roles (many-to-many)
- Models can have multiple tags (many-to-many)
- Models can have multiple versions (one-to-many)

The service implements the Repository pattern via IUnitOfWork for data access, with specialized repositories for each entity type.

### AdminService: Sequence Diagrams

**Model Management Flow**
``` mermaid
[Client] -> [AIModelOperationsController]: Create Model Request
[AIModelOperationsController] -> [FluentValidation]: Validate Request
[FluentValidation] --> [AIModelOperationsController]: Validation Result
[AIModelOperationsController] -> [AIModelOperationsService]: CreateModelAsync
[AIModelOperationsService] -> [CacheManager]: Check Cache
[CacheManager] --> [AIModelOperationsService]: Cache Miss
[AIModelOperationsService] -> [UnitOfWork]: Create Model
[UnitOfWork] -> [DbContext]: SaveChanges
[DbContext] --> [UnitOfWork]: Success
[UnitOfWork] --> [AIModelOperationsService]: Model Created
[AIModelOperationsService] -> [CacheManager]: Invalidate Cache
[AIModelOperationsService] --> [AIModelOperationsController]: Model Response
[AIModelOperationsController] --> [Client]: HTTP 201 Created
```

**Model Installation Flow**
``` mermaid 
[Client] -> [InferenceOperationsController]: Pull Model Request
[InferenceOperationsController] -> [InferenceOperationsService]: PullModelAsync
[InferenceOperationsService] -> [InferenceEngineConnector]: PullModelAsync
[InferenceEngineConnector] -> [Ollama API]: Pull Model
[Ollama API] --> [InferenceEngineConnector]: Stream Progress
[InferenceEngineConnector] --> [InferenceOperationsService]: IAsyncEnumerable<Progress>
[InferenceOperationsService] --> [InferenceOperationsController]: Progress Stream
[InferenceOperationsController] --> [Client]: Server-Sent Events with progress
[Ollama API] --> [InferenceEngineConnector]: Completion
[InferenceEngineConnector] --> [InferenceOperationsService]: Completion
[InferenceOperationsService] --> [InferenceOperationsController]: Completion
[InferenceOperationsController] --> [Client]: Completion Event
```


### AdminService: Service-specific Components

**Model Management Features**
- Comprehensive model metadata management
- Tag-based organization and categorization
- Integration with Ollama for model installation

**User Management Features**
- Role-based access control
- User status management (active, suspended, etc.)
- Profile information management

**Integration with InferenceService**
- Dynamic InferenceEngine URL configuration via RabbitMQ
- Streaming progress for long-running operations
- Redis-based caching of model information

**Caching Architecture**
- Two-tier caching with CacheManager and RedisCacheService
- Domain-specific cache invalidation strategies
- Resilience patterns for cache failures

**Service Discovery Implementation**
- RabbitMQ consumer for Inference URL updates
- Dynamic configuration update without restart
- Redis persistence for configuration durability

## AuthService Documentation

### AuthService: Purpose & Responsibility

AuthService handles all aspects of user authentication and authorization in the OllamaNet platform. Its core responsibilities include:

- **Authentication**: User login and token issuance
- **Authorization**: Role-based access control
- **User Registration**: New user account creation
- **Password Management**: Change, reset, and recovery workflows
- **Token Management**: JWT token generation, validation, and refresh

The service provides a secure foundation for the entire platform, ensuring that users can only access resources appropriate for their roles and that authentication state is properly maintained across the distributed system.

### AuthService: API Design

The AuthService exposes a RESTful API through the AuthController:

**Authentication Endpoints**
- `POST /api/Auth/Login`: Authenticate user and issue tokens
- `POST /api/Auth/Register`: Register new user account
- `POST /api/Auth/Refresh`: Refresh authentication token
- `POST /api/Auth/Logout`: Invalidate refresh token

**Password Management Endpoints**
- `POST /api/Auth/ChangePassword`: Change user password
- `POST /api/Auth/ForgotPassword`: Initiate password reset process
- `POST /api/Auth/ResetPassword`: Complete password reset with token

**User Management Endpoints**
- `GET /api/Auth/Profile`: Get current user profile
- `PUT /api/Auth/Profile`: Update user profile
- `GET /api/Auth/Roles`: Get available roles (admin only)

**Security Features**
- JWT-based authentication with HTTP-only cookie for refresh token
- One-time use refresh tokens with rotation on each use
- Token revocation on logout
- Password complexity enforcement
- Rate limiting for security-sensitive operations

All endpoints implement comprehensive request validation and appropriate HTTP status codes for different scenarios.

### AuthService: Data Model

The AuthService works with these key entities:

**Core Identity Entities**
- **ApplicationUser**: User identity information
- **Role**: Authorization role
- **UserRole**: Many-to-many relationship between users and roles
- **RefreshToken**: Token for authentication renewal

**Key Relationships**
- Each user can have multiple roles (many-to-many)
- Each user can have multiple refresh tokens (one-to-many)

**Token Model**
- JWTs contain claims for user ID, username, roles, and expiration
- Refresh tokens include creation timestamp, expiration, and revocation information

The service uses ASP.NET Identity for user management and a custom JWT implementation for token handling.

### AuthService: Sequence Diagrams

**Authentication Flow**
```mermiad
[Client] -> [AuthController]: Login Request
[AuthController] -> [AuthService]: Login(username, password)
[AuthService] -> [UserManager]: FindByNameAsync + CheckPasswordAsync
[UserManager] --> [AuthService]: User Object
[AuthService] -> [JWTManager]: GenerateJwtToken
[JWTManager] --> [AuthService]: JWT
[AuthService] -> [RefreshTokenRepo]: Create Refresh Token
[RefreshTokenRepo] --> [AuthService]: Token Created
[AuthService] --> [AuthController]: Auth Response
[AuthController] -> [Response]: Set HTTP-only Cookie
[AuthController] --> [Client]: JWT + Success
```

**Token Refresh Flow**
``` mermaid 
[Client] -> [AuthController]: Refresh Request + Cookie
[AuthController] -> [AuthService]: RefreshToken(token)
[AuthService] -> [RefreshTokenRepo]: Find Token
[RefreshTokenRepo] --> [AuthService]: Token
[AuthService]: Validate Token (not expired, not revoked)
[AuthService] -> [RefreshTokenRepo]: Revoke Old Token
[AuthService] -> [JWTManager]: Generate New JWT
[JWTManager] --> [AuthService]: New JWT
[AuthService] -> [RefreshTokenRepo]: Create New Refresh Token
[RefreshTokenRepo] --> [AuthService]: New Token Created
[AuthService] --> [AuthController]: New Auth Response
[AuthController] -> [Response]: Set New HTTP-only Cookie
[AuthController] --> [Client]: New JWT + Success
```


### AuthService: Service-specific Components

**JWT Implementation**
- JWT token generation using asymmetric encryption
- Custom claim management for authorization
- Token validation with security best practices

**Password Policies**
- Configurable password complexity requirements
- Password hashing with modern algorithms
- Automatic migration of older hash formats

**Refresh Token Rotation**
- One-time use refresh tokens
- Automatic token rotation on successful refresh
- Token revocation on logout
- Token expiration management

**Secure Cookie Handling**
- HTTP-only cookies for refresh tokens
- Secure flag for HTTPS-only transmission
- Same-site policies for CSRF protection

**Email Notifications**
- Registration confirmation emails
- Password reset emails with secure links
- Activity notification emails

## ExploreService Documentation

### ExploreService: Purpose & Responsibility

ExploreService enables users to discover and evaluate AI models available in the OllamaNet platform. Its core responsibilities include:

- **Model Discovery**: Browsing and searching available AI models
- **Model Evaluation**: Retrieving detailed model information
- **Category Navigation**: Finding models by tags and categories
- **Performance Optimization**: Caching frequently accessed model data

The service acts as a discovery layer between users and the model catalog, providing efficient ways to find the most appropriate models for specific use cases.

### ExploreService: API Design

The ExploreService exposes a RESTful API through the ExploreController:

**Model Discovery Endpoints**
- `GET /api/v1/explore/models`: Retrieve paginated list of models
- `GET /api/v1/explore/models/{id}`: Get detailed model information

**Tag Navigation Endpoints**
- `GET /api/v1/explore/tags`: List all available tags
- `GET /api/v1/explore/tags/{tagId}/models`: Get models with specific tag

All endpoints implement:
- Response caching with configurable TTL
- Pagination for large result sets
- Optional filtering parameters
- Detailed error responses

### ExploreService: Data Model

The ExploreService works with these key entities:

**Model Catalog Entities**
- **AIModel**: Core model entity with metadata
- **Tag**: Classification and organization tag
- **ModelTag**: Many-to-many relationship between models and tags

**Response Models**
- **ModelInfoResponse**: Detailed model information
- **ModelCard**: Summary model information for listings
- **GetTagsResponse**: Tag information for UI rendering

The service uses dedicated mapper classes to transform between entities and response DTOs.

### ExploreService: Sequence Diagrams

**Model Discovery Flow**
``` mermaid 
[Client] -> [ExploreController]: GET Models Request
[ExploreController] -> [ExploreService]: GetModelsAsync
[ExploreService] -> [CacheManager]: GetOrSetAsync(ModelsList)
[CacheManager] -> [RedisCacheService]: GetAsync
[RedisCacheService] --> [CacheManager]: Cache Miss
[CacheManager] -> [UnitOfWork]: GetModelsAsync
[UnitOfWork] -> [ModelRepository]: GetPaginatedAsync
[ModelRepository] --> [UnitOfWork]: Models
[UnitOfWork] --> [CacheManager]: Models
[CacheManager] -> [ModelMapper]: MapToResponse
[ModelMapper] --> [CacheManager]: Response DTOs
[CacheManager] -> [RedisCacheService]: SetAsync
[CacheManager] --> [ExploreService]: Response DTOs
[ExploreService] --> [ExploreController]: Models Response
[ExploreController] --> [Client]: JSON Response
```

**Tag-Based Model Search Flow**
``` mermaid 
[Client] -> [ExploreController]: GET Models by Tag
[ExploreController] -> [ExploreService]: GetModelsByTagAsync
[ExploreService] -> [CacheManager]: GetOrSetAsync(TagModels)
[CacheManager] -> [RedisCacheService]: GetAsync
[RedisCacheService] --> [CacheManager]: Cache Miss
[CacheManager] -> [UnitOfWork]: GetModelsByTagAsync
[UnitOfWork] -> [TagRepository]: GetByIdAsync
[TagRepository] --> [UnitOfWork]: Tag (or null)
[UnitOfWork] -> [ModelRepository]: GetByTagIdAsync
[ModelRepository] --> [UnitOfWork]: Models
[UnitOfWork] --> [CacheManager]: Models
[CacheManager] -> [ModelMapper]: MapToResponse
[ModelMapper] --> [CacheManager]: Response DTOs
[CacheManager] -> [RedisCacheService]: SetAsync
[CacheManager] --> [ExploreService]: Response DTOs
[ExploreService] --> [ExploreController]: Models Response
[ExploreController] --> [Client]: JSON Response
```

### ExploreService: Service-specific Components

**Caching Strategy**
- Sophisticated two-tier caching architecture:
  - CacheManager for high-level caching abstraction
  - RedisCacheService for low-level Redis operations
- Cache-aside pattern with GetOrSetAsync for all data operations
- Graceful fallback to database on cache failures
- Configurable expiration policies per data type
- Performance tracking with Stopwatch

**Exception Management**
- Hierarchical exception structure:
  - Base ExploreServiceException
  - Specialized exceptions (ModelNotFoundException, TagNotFoundException)
  - Cache-specific exceptions (CacheException, ConnectionException)
- Consistent exception handling with proper HTTP status codes
- Detailed logging for troubleshooting

**Performance Optimization**
- Aggressive caching of frequently accessed data
- Efficient pagination implementation
- Optimized database queries
- Connection pooling for database access

## ConversationService Documentation

### ConversationService: Purpose & Responsibility

ConversationService manages all aspects of user conversations with AI models in the OllamaNet platform. Its core responsibilities include:

- **Conversation Management**: Creating, organizing, and retrieving conversations
- **Chat Interaction**: Processing user messages and obtaining AI model responses
- **Message Persistence**: Storing and retrieving conversation history
- **Folder Organization**: Managing conversation folders for organization
- **RAG Implementation**: Enhancing responses with document-based context
- **Document Management**: Uploading and processing documents for RAG

The service serves as the primary interaction point between users and AI models, ensuring a responsive, context-aware conversation experience.

### ConversationService: API Design

The ConversationService exposes multiple controllers for domain-specific functionality:

**Conversation Management Endpoints**
- `POST /api/Conversation/Open`: Create a new conversation
- `GET /api/Conversation`: List user conversations (paginated)
- `GET /api/Conversation/Search`: Search conversations
- `GET /api/Conversation/{id}`: Get conversation details
- `GET /api/Conversation/{id}/Messages`: Get conversation messages
- `PUT /api/Conversation/{id}`: Update conversation
- `DELETE /api/Conversation/{id}`: Delete conversation
- `POST /api/Conversation/{id}/Move`: Move conversation to folder

**Chat Endpoints**
- `POST /api/Chat/GetResponse`: Get AI response (non-streaming)
- `POST /api/Chat/GetStreamedResponse`: Get streaming AI response
- `GET /api/Chat/History/{conversationId}`: Get chat history

**Folder Endpoints**
- `POST /api/Folder`: Create folder
- `GET /api/Folder`: Get user folders
- `PUT /api/Folder/{id}`: Update folder
- `DELETE /api/Folder/{id}`: Delete folder

**Note Endpoints**
- `POST /api/Note`: Create note
- `GET /api/Note/{conversationId}`: Get notes for conversation
- `PUT /api/Note/{id}`: Update note
- `DELETE /api/Note/{id}`: Delete note

**Document Management Endpoints**
- `POST /api/Document/Upload/{conversationId}`: Upload document
- `GET /api/Document/{id}`: Get document
- `GET /api/Document/Conversation/{conversationId}`: List conversation documents
- `DELETE /api/Document/{id}`: Delete document

**Feedback Endpoints**
- `POST /api/Feedback`: Add feedback
- `PUT /api/Feedback/{id}`: Update feedback
- `GET /api/Feedback/{conversationId}`: Get feedback for conversation

All endpoints implement validation, proper HTTP status codes, and authorization requirements.

### ConversationService: Data Model

The ConversationService works with these key entities:

**Conversation Entities**
- **Conversation**: Core conversation entity with metadata
- **MessageHistory**: Individual messages within a conversation
- **Folder**: Organization container for conversations
- **Note**: User notes associated with conversations
- **Feedback**: User feedback on AI responses

**Document Entities**
- **Document**: Uploaded document metadata
- **DocumentChunk**: Document segments for RAG processing
- **VectorEmbedding**: Vector representations for semantic search

**Key Relationships**
- Each conversation belongs to a user
- Conversations can be organized in folders
- Messages belong to conversations
- Documents are associated with conversations
- Document chunks come from documents

### ConversationService: Sequence Diagrams

**Chat Message Flow with RAG**

``` mermaid 
[Client] -> [ChatController]: POST GetStreamedResponse
[ChatController] -> [ChatService]: GetStreamedModelResponse
[ChatService] -> [HistoryManager]: GetChatHistoryWithCachingAsync
[HistoryManager] --> [ChatService]: Previous Messages
[ChatService] -> [RagRetrievalService]: GetContextForQuery
[RagRetrievalService] -> [VectorSearch]: FindSimilarChunks
[VectorSearch] --> [RagRetrievalService]: Relevant Chunks
[RagRetrievalService] -> [RagRetrievalService]: FormatContextForInsertion
[RagRetrievalService] --> [ChatService]: Enhanced Context
[ChatService] -> [InferenceEngineConnector]: GetStreamingCompletion
[InferenceEngineConnector] -> [Inference API]: Streaming Request
[Inference API] --> [InferenceEngineConnector]: Token Stream
[InferenceEngineConnector] --> [ChatService]: Token Stream
[ChatService] --> [ChatController]: IAsyncEnumerable<string>
[ChatController] --> [Client]: Server-Sent Events
[ChatService] -> [HistoryManager]: SaveStreamedChatInteractionAsync
[HistoryManager] -> [UnitOfWork]: SaveChanges
```



**Conversation Management Flow**
``` mermaid 
[Client] -> [ConversationController]: POST Open Conversation
[ConversationController] -> [ConversationService]: CreateConversationAsync
[ConversationService] -> [UnitOfWork]: CreateAsync
[UnitOfWork] -> [ConversationRepository]: Insert
[ConversationRepository] -> [DbContext]: SaveChanges
[DbContext] --> [ConversationRepository]: Success
[ConversationRepository] --> [UnitOfWork]: Conversation
[UnitOfWork] --> [ConversationService]: Conversation
[ConversationService] --> [ConversationController]: Response
[ConversationController] --> [Client]: HTTP 201 Created
```


**Document Processing Flow**
```mermaid 
[Client] -> [DocumentController]: Upload Document
[DocumentController] -> [DocumentService]: UploadDocumentAsync
[DocumentService] -> [DocumentStorage]: SaveFileAsync
[DocumentStorage] --> [DocumentService]: File Path
[DocumentService] -> [DocumentProcessor]: ProcessDocumentAsync
[DocumentProcessor] -> [DocumentProcessor]: ExtractTextAsync
[DocumentProcessor] -> [DocumentProcessor]: ChunkTextAsync
[DocumentProcessor] --> [DocumentService]: Document Chunks
[DocumentService] -> [RagIndexingService]: IndexDocumentAsync
[RagIndexingService] -> [TextEmbeddingGeneration]: GenerateEmbeddingsAsync
[TextEmbeddingGeneration] --> [RagIndexingService]: Embeddings
[RagIndexingService] -> [VectorStorage]: StoreVectorsAsync
[VectorStorage] --> [RagIndexingService]: Success
[RagIndexingService] --> [DocumentService]: Indexing Complete
[DocumentService] -> [UnitOfWork]: SaveDocumentMetadata
[UnitOfWork] --> [DocumentService]: Success
[DocumentService] --> [DocumentController]: Document Response
[DocumentController] --> [Client]: HTTP 201 Created
```



### ConversationService: Service-specific Components

**RAG Implementation**
- Text chunking for document processing
- Vector embeddings via Ollama embedding models
- Semantic search for context retrieval
- Dynamic context insertion into prompts

**Streaming Response Implementation**
- Server-Sent Events for streaming AI responses
- IAsyncEnumerable for asynchronous streaming
- Optimized token processing pipeline

**Caching Strategy**
- Two-tier caching with CacheManager and RedisCacheService
- Message history caching for faster responses
- Cache invalidation on conversation updates

**Document Processing Pipeline**
- Multi-format document support (PDF, DOCX, TXT)
- Extraction pipeline with format-specific processors
- Chunking strategies for optimal retrieval

**InferenceEngine Integration**
- Dynamic service discovery via RabbitMQ
- Resilience patterns for API calls
- Connection management for optimal performance

**Folder Organization**
- Hierarchical conversation management
- Efficient folder-based querying
- User-specific folder structures

## InferenceService Documentation

### InferenceService: Purpose & Responsibility

InferenceService (also known as Spicy Avocado) provides AI model inference capabilities by exposing Ollama models as accessible APIs. Its unique approach enables running models from cloud notebooks and making them available to other services. Its core responsibilities include:

- **Model Inference**: Processing text generation requests
- **Model Hosting**: Running Ollama models locally
- **API Exposure**: Making local services available via secure tunnels
- **Service Discovery**: Publishing API endpoints to other services

This service is implemented as a Jupyter notebook that orchestrates Ollama and ngrok to enable flexible AI model deployment from virtually any cloud notebook environment.

### InferenceService: API Design

The InferenceService exposes Ollama's API through an ngrok tunnel:

**Inference Endpoints**
- `POST /api/generate`: Generate text from a prompt
- `POST /api/chat`: Chat completion with message history
- `POST /api/embeddings`: Generate text embeddings

**Model Management Endpoints**
- `GET /api/tags`: List available models
- `POST /api/pull`: Pull a new model

**These endpoints are the standard Ollama API endpoints, exposed through ngrok.**

### InferenceService: Data Model

The InferenceService uses these key data structures:

**Request Formats**
- **GenerateRequest**: Prompt and parameters for text generation
- **ChatRequest**: Message history and parameters for chat completion
- **EmbeddingRequest**: Text for embedding generation

**Response Formats**
- **GenerateResponse**: Generated text and metadata
- **ChatResponse**: Assistant message and metadata
- **EmbeddingResponse**: Vector embeddings

**Service Discovery Messages**
- **InferenceUrlUpdateMessage**: URL and metadata for service registration

The service does not maintain persistent data structures beyond what Ollama provides, operating primarily as a stateless API gateway.

### InferenceService: Sequence Diagrams

**Service Startup and Registration Flow**
```mermaid 
[Jupyter Notebook] -> [Notebook]: Install dependencies
[Notebook] -> [Subprocess]: Start Ollama
[Subprocess] --> [Notebook]: Ollama running
[Notebook] -> [Subprocess]: Start ngrok
[Subprocess] --> [Notebook]: ngrok running
[Notebook] -> [ngrok API]: Get public URL
[ngrok API] --> [Notebook]: HTTPS URL
[Notebook] -> [RabbitMQ]: Connect
[Notebook] -> [RabbitMQ]: Publish URL update
[RabbitMQ] -> [AdminService]: Deliver message
[RabbitMQ] -> [ConversationService]: Deliver message
[Notebook] -> [Notebook]: Display URL
```



**Inference Request Flow**

``` mermaid 
[ConversationService] -> [InferenceConnector]: GetCompletion
[InferenceConnector] -> [ngrok]: POST /api/chat
[ngrok] -> [Ollama]: Forward request
[Ollama] -> [LLM]: Process prompt
[LLM] --> [Ollama]: Generated text
[Ollama] --> [ngrok]: Response
[ngrok] --> [InferenceConnector]: Response
[InferenceConnector] --> [ConversationService]: Completion
```



**Streaming Inference Flow**
``` mermaid 
[ConversationService] -> [InferenceConnector]: GetStreamingCompletion
[InferenceConnector] -> [ngrok]: POST /api/chat (stream=true)
[ngrok] -> [Ollama]: Forward request
[Ollama] -> [LLM]: Process prompt
[LLM] --> [Ollama]: Token stream
[Ollama] --> [ngrok]: Token stream
[ngrok] --> [InferenceConnector]: Token stream
[InferenceConnector] --> [ConversationService]: IAsyncEnumerable<string>
```


### InferenceService: Service-specific Components

**Jupyter Notebook Architecture**
- Self-contained deployment in a single notebook
- Process management for Ollama and ngrok
- Error handling and status reporting
- Interactive execution capability

**ngrok Tunneling Implementation**
- Secure HTTPS tunnel to local services
- Dynamic URL generation
- Public endpoint for service access
- TLS termination for secure communication

**Ollama Integration**
- Local model serving through Ollama
- Model pulling and configuration
- Standard API exposure
- Efficient request processing

**RabbitMQ Service Discovery**
- Topic exchange for service announcements
- Message format with URL and metadata
- Consumers in dependent services
- Dynamic service configuration updates

**Unique Implementation Aspects**
- Notebook-first approach for flexibility
- No persistent storage requirements
- Minimal configuration needs
- Deployment possible on various cloud notebook platforms

## Glossary

- **AdminService**: Service responsible for platform administration, including user management and model administration
- **AuthService**: Service responsible for authentication and authorization, ensuring secure access to the platform
- **ExploreService**: Service responsible for model discovery and browsing, enabling users to find appropriate models
- **ConversationService**: Service responsible for chat and conversation management, including message persistence and organization
- **InferenceService**: Service responsible for AI model inference, exposing Ollama models via API endpoints
- **RAG**: Retrieval-Augmented Generation, a technique for enhancing LLM responses with relevant context from documents
- **Ollama**: Open-source framework for running LLMs locally with an API interface
- **JWT**: JSON Web Token, used for secure authentication between services with claims-based information
- **ngrok**: Tool used to expose local services to the internet through secure tunnels
- **API Endpoint**: Specific URL path where an API service can be accessed to perform operations
- **Service Component**: Discrete functional unit within a microservice that handles specific responsibilities
- **Message Queue**: System for asynchronous service-to-service communication, enabling loose coupling
- **Vector Search**: Technique for finding semantically similar content using vector embeddings


