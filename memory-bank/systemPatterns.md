# System Architecture Patterns for OllamaNet

## Microservices Architecture Overview

The OllamaNet platform employs a microservices architecture with the following key components:

1. **Gateway Service**: API gateway using Ocelot for unified access to all microservices
2. **ConversationService**: Manages conversations and real-time chat interactions
3. **AuthService**: Handles authentication and user management
4. **AdminService**: Provides comprehensive administration capabilities
5. **ExploreService**: Enables discovery and browsing of available AI models
6. **Shared Database Layer**: Common data access components for all services

### Service Components in Detail

#### Gateway Service
- **Core Functionality**: Routes client requests to appropriate backend microservices
- **Key Components**: 
  - Ocelot routing engine with modular configuration
  - Authentication middleware for JWT validation
  - Rate limiting implementation
  - Request routing to appropriate services
- **Configuration**: Split into service-specific files for maintainability
- **Integration Points**: All backend microservices

#### ConversationService
- **Core Functionality**: Manages conversations, real-time chat, and streaming
- **Key Components**:
  - ConversationController for conversation management
  - ChatController for real-time chat with streaming
  - FolderController for organization
  - NoteController for annotations
  - FeedbackController for user feedback
  - ChatHistoryManager with caching integration
  - OllamaConnector for AI model integration
- **Patterns**: Streaming responses via Server-Sent Events, Cache-Aside pattern
- **Specialized Features**: IAsyncEnumerable streaming, Redis caching

#### AuthService
- **Core Functionality**: User authentication, authorization, profile management
- **Key Components**:
  - AccountController for authentication
  - UserController for user management
  - RefreshTokenManager for persistent sessions
  - Role-based authorization system
- **Patterns**: JWT authentication with refresh tokens, Claims-based identity
- **Specialized Features**: Password reset flows, role management

#### AdminService
- **Core Functionality**: Platform administration and management
- **Key Components**:
  - UserManagementController for user administration
  - ModelManagementController for AI model management
  - TagManagementController for organizational tagging
  - ModelOperationsController for inference operations
- **Patterns**: Administrative operations, progress streaming
- **Specialized Features**: Streaming progress for long-running operations

#### ExploreService
- **Core Functionality**: Model discovery and browsing
- **Key Components**:
  - ModelController for model discovery
  - TagController for tag browsing
- **Patterns**: Cache-Aside for performance optimization
- **Specialized Features**: Efficient caching of model metadata

## Common Design Patterns Across Services

### Architecture Patterns
- **Clean Architecture**: Controllers → Services → Repositories → Database
- **Layered Design**: API, Service, Data Access, and Integration layers
- **API Gateway Pattern**: Unified entry point with routing
- **Repository Pattern**: Data access abstraction across all services
- **Unit of Work**: Transaction management for data operations
- **Cache-Aside Pattern**: Performance optimization with fallback

### Implementation Patterns
- **Dependency Injection**: Constructor-based DI throughout all services
- **Service Registration**: Centralized in ServiceExtensions.cs files
- **Options Pattern**: Configuration binding to strongly-typed classes
- **Controller Pattern**: RESTful APIs with consistent HTTP methods
- **Validator Pattern**: FluentValidation for request validation
- **Factory Pattern**: Creation of complex objects

### Caching Strategy
All services implement a consistent Redis-based caching strategy:

- **RedisCacheService**: Low-level Redis operations with error handling
- **CacheManager**: High-level abstraction with fallback mechanisms
- **CacheKeys**: Centralized key management with domain-specific formats:
  - Resource-specific: "resource:type:{0}" 
  - List-specific: "resource:list:param:{0}"
- **TTL Management**: Domain-specific expiration times
- **Retry Logic**: Configurable with exponential backoff
- **Exception Handling**: Specialized exception hierarchy
- **Fallback Strategy**: Graceful degradation to database access

### Streaming Implementation
ConversationService implements real-time streaming responses:

- **Server-Sent Events (SSE)**: Content-Type: text/event-stream
- **IAsyncEnumerable**: Asynchronous streaming from AI models
- **Response.BodyWriter**: Direct streaming to HTTP response
- **Cancellation Support**: EnumeratorCancellation attribute
- **Background Processing**: Post-streaming operations

### Data Access Pattern
- **Shared Database Approach**: Single database instance with shared schema
- **Repository Abstraction**: IRepositories from Ollama_DB_layer
- **Unit of Work**: IUnitOfWork for transaction management
- **Entity Framework Core**: ORM for data access operations

### Authentication & Authorization Pattern
- **JWT Authentication**: Token-based with validation
- **Role-Based Access**: User and Admin roles
- **Claims-Based Identity**: User identification from claims
- **Token Refresh**: Persistent sessions with refresh tokens
- **Resource Ownership**: Validation of user access to resources

### Error Handling Pattern
- **Controller-Level Try/Catch**: With appropriate status codes
- **HTTP Status Mapping**: 400, 401, 404, 500 based on exception type
- **Validation Error Format**: Consistent structure with details
- **Logging Strategy**: Contextual with operation metadata
- **Performance Monitoring**: Stopwatch for timing metrics

## Component Relationships

### Service Dependencies
```
Client Applications → Gateway → [AuthService, ConversationService, AdminService, ExploreService]
                                        ↓
                                 Shared Database Layer
                                        ↓
                                  SQL Server Database
```

### Shared Resources
- **Authentication**: JWT validation common across services
- **Database**: Shared SQL Server instance at db19911.public.databaseasp.net
- **Cache**: Redis cache at content-ghoul-42217.upstash.io (Upstash)
- **Ollama**: AI model inference via ngrok endpoints
- **Logging**: Common logging patterns and severity levels

### Communication Patterns
- **Synchronous REST**: Service-to-service API calls
- **Streaming Responses**: Server-Sent Events for real-time data
- **Database Synchronization**: Shared database for data consistency
- **Cache Coordination**: Redis for distributed caching

## Technology-Specific Patterns

### ASP.NET Core Patterns
- **Middleware Pipeline**: Authentication, Authorization, CORS
- **Service Registration**: Extension methods for DI setup
- **Options Configuration**: Strongly-typed settings classes
- **Controller Attribute Routing**: Consistent URL structure
- **Filters**: For cross-cutting concerns
- **Model Binding**: Request deserialization and validation

### Entity Framework Core Patterns
- **Repository Abstraction**: IRepository<T> for data access
- **Context Configuration**: Fluent API for entity mapping
- **Lazy Loading**: For efficient relationship loading
- **Query Projection**: DTOs for optimized queries
- **Transaction Management**: Via IUnitOfWork

### Caching Patterns
- **Distributed Caching**: Redis-based with Upstash
- **Cache Key Management**: Consistent naming convention
- **TTL Strategy**: Domain-specific expiration times
- **Multi-level Cache**: Redis with memory fallback
- **Cache Invalidation**: On data modification events
- **Cache-or-Compute**: GetOrSetAsync pattern

### Validation Patterns
- **FluentValidation**: For request model validation
- **Validator Registration**: Extension methods for DI setup
- **Conditional Validation**: Business rules enforcement
- **Validation Response**: Consistent error format
- **Custom Validators**: For complex validation scenarios

### Security Patterns
- **JWT Authentication**: Token validation and generation
- **Claims-Based Identity**: User identification
- **Role-Based Authorization**: Access control policies
- **Password Hashing**: Secure credential storage
- **HTTPS Enforcement**: Secure communications
- **Cross-Origin Resource Sharing**: Controlled access

# System Architecture Patterns

## Microservices Architecture

### Service Components
1. **API Gateway**
   - Entry point for all client requests
   - JWT token validation and caching
   - Rate limiting implementation
   - Request routing using Ocelot
   - Redis-based token caching
   - Distributed rate limiting

2. **Authentication Service**
   - User authentication and authorization
   - JWT token generation
   - User profile management
   - Token validation endpoint

3. **Chat Services**
   - Real-time chat processing
   - Conversation management
   - Message handling
   - Synchronous communication with LLM Inference Service

4. **Explore Service**
   - Model discovery and search
   - Available models listing
   - Synchronous communication with LLM Inference Service
   - Redis caching for performance optimization
   - Cache invalidation strategies

5. **Admin Services**
   - System administration
   - Audit logging
   - Background task management
   - Asynchronous communication with LLM Inference Service

6. **LLM Inference Service**
   - Hosts and manages LLMs
   - Handles model inference requests
   - Model lifecycle management

### Communication Patterns

#### Synchronous Communication
- Real-time chat operations
- Authentication requests
- Model discovery and search
- User profile operations
- Direct API calls between services

#### Asynchronous Communication (Message Queue)
- Model installation/deletion
- System configuration updates
- Audit logging
- Background tasks
- Cross-service notifications
- Usage statistics collection

### Infrastructure Components
1. **Service Registry**
   - Service discovery
   - Load balancing
   - Health monitoring

2. **Config Server**
   - Centralized configuration
   - Environment-specific settings

3. **Message Queue**
   - Asynchronous communication
   - Task queuing
   - Event distribution

4. **Distributed Cache (Redis)**
   - Token caching
   - Rate limiting counters
   - Performance optimization
   - Session management
   - Data caching
   - Cache invalidation
   - Health monitoring
   - Connection management

5. **Monitoring**
   - System health monitoring
   - Performance metrics
   - Logging and tracing
   - Cache hit/miss ratios
   - Redis connection status
   - Rate limit monitoring
   - Authentication metrics

### Database Architecture
- **Shared Database Approach**
  - Single database instance for all services
  - Unified data access layer
  - Shared schema for common entities
  - Service-specific schemas where needed
  - Centralized data consistency
  - Simplified transaction management
  - Reduced operational complexity

### Database Dependencies
- All Services → Shared Database
  - Authentication Service
  - Chat Services
  - Explore Service
  - Admin Services
  - LLM Inference Service

## Design Patterns in Use

1. **API Gateway Pattern**
   - Single entry point
   - Request routing
   - Authentication/Authorization
   - Rate limiting
   - Token caching

2. **Service Discovery Pattern**
   - Dynamic service registration
   - Load balancing
   - Health checks

3. **Circuit Breaker Pattern**
   - Fault tolerance
   - Service resilience
   - Graceful degradation

4. **CQRS Pattern**
   - Separate read/write models
   - Optimized queries
   - Event sourcing

5. **Event-Driven Architecture**
   - Asynchronous communication
   - Loose coupling
   - Scalability

6. **Cache-Aside Pattern**
   - Lazy loading
   - Cache invalidation
   - Write-through caching
   - Read-through caching

## Component Relationships

### Direct Dependencies
- API Gateway → Authentication Service
- API Gateway → Chat Services
- API Gateway → Explore Service
- API Gateway → Admin Services
- Chat Services → LLM Inference Service
- Explore Service → LLM Inference Service
- Explore Service → Redis Cache

### Message Queue Dependencies
- Admin Services → Message Queue
- Background Tasks → Message Queue
- Audit Service → Message Queue
- Model Management → Message Queue

## Scalability Patterns
- Horizontal scaling
- Load balancing
- Caching strategies
- Database sharding
- Redis clustering
- Distributed rate limiting

## Security Patterns
- JWT authentication
- Role-based authorization
- Data encryption
- Secure communication
- Cache security
- Rate limiting
- Token blacklisting

## Caching Patterns

The system employs a consistent caching strategy across microservices:

### Core Pattern: Cache-Aside with Fallback

Each service uses the cache-aside pattern through a standardized CacheManager that:
1. Attempts to fetch data from Redis cache
2. On cache miss, retrieves from the database and populates cache
3. On cache failure, gracefully falls back to database access
4. Implements retry policies for transient errors

### Exception Handling Pattern

A uniform exception translation pattern is used:
1. Cache-level exceptions are caught and mapped to service-appropriate exceptions
2. Service methods have consistent try/catch blocks
3. The ExceptionConverter utility standardizes error messages and context

### Key Management Pattern

Cache keys follow a consistent naming convention:
- Namespace by service: `servicename:resource:id`
- Include pagination for lists: `servicename:resource:list:page:1:size:20`
- Include query parameters for filtered results: `servicename:resource:list:filter:param:value`

### Cache Invalidation Pattern

Cache entries use a combination of:
- TTL-based expiration for all entries
- Explicit invalidation when data is modified
- Keys grouped by resource type for selective clearing

## Logging Patterns

[Existing content about logging patterns]

# System Patterns

## Architecture

The OllamaNet Components system follows a microservices architecture with the following key services:

1. **ConversationService**: Manages conversations and chat interactions
2. **AuthService**: Handles authentication and user management
3. **AdminService**: Provides administrative functionality
4. **ExploreService**: Offers model exploration and discovery
5. **Gateway**: API Gateway for routing requests

Each service is independently deployable and follows similar patterns for consistency.

## Design Patterns

### Repository Pattern
- Used for data access across all services
- Abstracts database operations behind interfaces
- Enables unit testing through mocking
- Centralized query logic

### Unit of Work
- Manages transactions and database context
- Ensures atomicity of operations
- Coordinates across multiple repositories

### Dependency Injection
- Constructor-based DI throughout
- Service registration in ServiceExtensions classes
- Scoped lifetimes for request-bound services
- Singleton for stateless services

### Validation Pattern
- FluentValidation library used across all services
- Validator classes separated by request type
- Hierarchical validation with complex rule sets
- Conditional validation based on provided fields
- Range validation for numeric parameters
- Specialized validators registered through DI
- Validators follow the same naming convention: `{RequestType}Validator`

## Data Flow

### Request Processing
1. Gateway routes request to appropriate service
2. Controller receives and performs initial authorization
3. Request validated by corresponding validator
4. Service layer processes business logic
5. Repository layer handles data access
6. Response mapped and returned

### Caching Strategy
1. Cache checked before database access
2. Cache updated after successful database operations
3. Cache invalidated on updates/deletes
4. TTL set based on data volatility

## Error Handling

### Exception Framework
- Centralized exception handling
- Custom exceptions for business logic
- Exception middleware for HTTP responses
- Detailed logging for debugging

### Validation Errors
- Returned as 400 Bad Request
- Includes detailed error messages
- Validation happens before business logic
- Clear error codes and descriptions

## Communication

### Synchronous APIs
- REST API for service-to-service communication
- Consistent endpoint structure
- Uniform response format

### Asynchronous Messaging
- Event-based communication for decoupled operations
- Message queue for reliability

## Security

### Authentication
- JWT-based authentication
- Token refresh mechanism
- Claims-based authorization

### Authorization
- Role-based access control
- Resource-based permissions

## Performance Optimization

### Caching
- Redis for distributed caching
- Optimized cache key generation
- Selective caching based on usage patterns

### Query Optimization
- Efficient LINQ queries
- Pagination for large result sets
- Indexes on frequently queried fields

## Monitoring and Logging

### Logging Strategy
- Structured logging with severity levels
- Request correlation IDs
- Performance metrics

### Health Checks
- Service status endpoints
- Dependency health monitoring

## Containerization

### Docker
- Each service has a Dockerfile
- Optimized for layer caching
- Environment variable configuration

### Orchestration
- Kubernetes or Docker Compose
- Service discovery
- Load balancing 