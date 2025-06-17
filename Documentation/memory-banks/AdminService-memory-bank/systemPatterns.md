# System Patterns for AdminService

## Architecture Overview
AdminService follows a clean, layered architecture pattern within a microservices ecosystem:

- **API Layer**: Controllers handling HTTP requests and responses, organized by domain:
  - UserOperationsController for user management
  - AIModelOperationsController for model administration 
  - TagOperationsController for tag management
  - InferenceOperationsController for Ollama integration
- **Service Layer**: Business logic encapsulated in domain-specific services
- **Validation Layer**: FluentValidation for comprehensive request validation
- **Data Access Layer**: Repository pattern via IUnitOfWork for data operations
- **Integration Layer**: OllamaConnector with IOllamaApiClient for Ollama integration
- **Caching Layer**: Redis-based caching with JSON serialization for distributed state

## Design Patterns
- **Repository Pattern**: Abstracts data access through repository interfaces from Ollama_DB_layer
- **Unit of Work**: Manages transactions and repository coordination through IUnitOfWork
- **Dependency Injection**: Comprehensive service registration in ServiceExtensions.cs
- **Validator Pattern**: FluentValidation with specialized validators for each request type
- **Service Pattern**: Business logic encapsulated in domain-specific services
- **Connector Pattern**: OllamaConnector abstracts external service integration
- **Progress Report Pattern**: IAsyncEnumerable with IProgress for model installation progress
- **Try/Catch Pattern**: Consistent error handling with contextual logging
- **Builder Pattern**: WebApplication.CreateBuilder for application configuration
- **Cache-Aside Pattern**: Caching implemented for frequently accessed data
- **Circuit Breaker Pattern**: Resilience policies for external service calls
- **Retry Pattern**: Automatic retry with exponential backoff for transient failures

## Component Relationships
```
Controllers → Services → Repositories/Connectors → Database/External Services
        ↓           ↓
   Validators    Cache Manager
```

- **Controllers**: Depend on service interfaces and validators
- **Services**: Implement business logic and coordination
- **Repositories**: Provided by Ollama_DB_layer for data access
- **Connectors**: OllamaConnector integrates with OllamaSharp
- **Validators**: Ensure request data integrity through FluentValidation
- **Cache Manager**: Provides abstracted access to Redis cache
- **All Components**: Registered via dependency injection in ServiceExtensions.cs

## Service Organization
- **UserOperationsService**: Full user lifecycle and role management
- **AIModelOperationsService**: Model administration and tag assignments
- **TagsOperationsService**: Tag creation and management
- **InferenceOperationsService**: Ollama integration for model operations

## Configuration Management
- **ServiceExtensions.cs**: Organized extension methods for service registration:
  - AddDatabaseAndIdentity: Database context and Identity configuration
  - AddJwtAuthentication: JWT configuration (currently commented out)
  - AddRepositories: Repository registration from Ollama_DB_layer
  - AddApplicationServices: Service and validator registration
  - ConfigureCors: CORS policy configuration
  - AddCaching: Redis cache configuration with settings and retry policies
  - ConfigureSwagger: Swagger with JWT support
- **appsettings.json**: Environment-specific configurations:
  - Database connection string (SQL Server)
  - Redis connection (Upstash)
  - JWT settings for authentication
  - RedisCaching settings for timeouts and retries

## API Design
- **RESTful Endpoints**: Organized by domain with consistent URL structure:
  - /api/Admin/UserOperations/...
  - /api/Admin/AIModelOperations/...
  - /api/Admin/TagOperations/...
  - /api/Admin/InferenceOperations/...
- **HTTP Methods**: Appropriate use based on operation:
  - GET: Retrieval operations
  - POST: Creation operations
  - PUT: Full updates
  - PATCH: Partial updates
  - DELETE: Removal operations
- **Status Codes**: Consistent response codes:
  - 200/201: Success responses
  - 400: Bad request (validation failures)
  - 404: Not found (resource not located)
  - 500: Server errors
- **Request/Response Models**: Strongly-typed DTOs for operations
- **Pagination**: Implemented for collection endpoints with page size/number

## Validation Strategy
- **FluentValidation**: Used throughout the application
- **Request-Specific Validators**: Custom validators for each request type
- **Conditional Validation**: Using When() for context-specific validation
- **Validator Registration**: Organized in ServiceExtensions.cs
- **Validation Processing**: Controller-level validation before business logic
- **Error Responses**: Consistent format for validation failures

## Error Handling
- **Try/Catch Blocks**: In controllers with specific exception handling
- **Status Code Mapping**: Appropriate HTTP status codes based on exception type:
  - KeyNotFoundException → 404 Not Found
  - ArgumentException → 400 Bad Request
  - InvalidOperationException → 400 Bad Request
  - General exceptions → 500 Internal Server Error
- **Contextual Logging**: ILogger<T> for detailed error information
- **Consistent Response Format**: Structured error messages
- **Custom Exception Types**: Specialized exceptions for different error scenarios
- **Resilience Policies**: Retry and Circuit Breaker patterns for external services

## Streaming Implementation
- **Server-Sent Events**: For model installation progress
- **IAsyncEnumerable**: For streaming progress updates
- **IProgress<T>**: For progress reporting to controllers
- **Response.BodyWriter**: For direct streaming to clients
- **Content-Type: text/event-stream**: Proper streaming configuration

## Integration Patterns
- **OllamaConnector**: Abstracts integration with OllamaSharp
- **IOllamaApiClient**: External client for Ollama API
- **Progress Reporting**: Structured updates for long-running operations
- **Model Conversion**: Mapping between API and domain models
- **Service Discovery**: Dynamic URL configuration via RabbitMQ messaging
- **Distributed Configuration**: Shared configuration via Redis caching

## Caching Strategy
- **Cache-Aside Pattern**: First check cache, then source, update cache
- **Distributed Cache**: Redis for cross-service shared data
- **JSON Serialization**: Consistent serialization across services
- **Timeout Management**: Configurable timeouts for remote Redis operations
- **Exception Handling**: Custom exceptions for cache operations with fallbacks
- **Retry Policies**: Automatic retries for transient failures
- **Cache Keys**: Centralized definition of cache keys
- **Cache Manager**: Higher-level abstraction for caching operations
- **Cache Duration**: Configurable expiration with defaults 