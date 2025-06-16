# System Patterns for AdminService

## Architecture Overview
AdminService follows a clean, layered architecture pattern within a microservices ecosystem:

- **API Layer**: Controllers handling HTTP requests and responses, organized by domain:
  - Controllers/User/UserOperationsController for user management
  - Controllers/AIModel/AIModelOperationsController for model administration 
  - Controllers/Tag/TagOperationsController for tag management
  - Controllers/Inference/InferenceOperationsController for Ollama integration
  - Controllers/Validators organized by domain (User, AIModel, Tag, Inference)
- **Service Layer**: Business logic encapsulated in domain-specific services organized by domain
- **Infrastructure Layer**: Cross-cutting concerns including caching, logging, configuration, and integration
- **Validation Layer**: FluentValidation for comprehensive request validation
- **Data Access Layer**: Repository pattern via IUnitOfWork for data operations

## Design Patterns
- **Repository Pattern**: Abstracts data access through repository interfaces from Ollama_DB_layer
- **Unit of Work**: Manages transactions and repository coordination through IUnitOfWork
- **Dependency Injection**: Comprehensive service registration in ServiceExtensions.cs
- **Validator Pattern**: FluentValidation with specialized validators for each request type
- **Service Pattern**: Business logic encapsulated in domain-specific services
- **Options Pattern**: Configuration options classes for all configurable values
- **Connector Pattern**: InferenceEngineConnector abstracts external service integration
- **Progress Report Pattern**: IAsyncEnumerable with IProgress for model installation progress
- **Global Exception Handler**: Centralized error handling with exception type mapping
- **MediatR Pipeline Behavior**: For cross-cutting concerns like logging (planned)

## Component Relationships
```
Controllers → Services → Repositories/Connectors → Database/External Services
        ↓
   Validators
        ↓
Infrastructure (Caching, Logging, Configuration, Integration)
```

- **Controllers**: Depend on service interfaces and validators
- **Services**: Implement business logic and coordination
- **Repositories**: Provided by Ollama_DB_layer for data access
- **Connectors**: InferenceEngineConnector integrates with OllamaSharp
- **Validators**: Ensure request data integrity through FluentValidation
- **Infrastructure**: Provides cross-cutting concerns like caching, logging, and configuration
- **All Components**: Registered via dependency injection in ServiceExtensions.cs

## Service Organization
- **User Domain**: User management services and DTOs
  - Services/User/Implementation/UserService.cs
  - Services/User/DTOs/Requests and Responses
  - Services/User/Mappers
- **AIModel Domain**: Model management services and DTOs
  - Services/AIModel/Implementation/ModelService.cs
  - Services/AIModel/DTOs/Requests and Responses
  - Services/AIModel/Mappers
- **Tag Domain**: Tag management services and DTOs
  - Services/Tag/Implementation/TagService.cs
  - Services/Tag/DTOs/Requests and Responses
  - Services/Tag/Mappers
- **Inference Domain**: Ollama integration services and DTOs
  - Services/Inference/Implementation/InferenceService.cs
  - Services/Inference/DTOs/Requests and Responses
  - Services/Inference/Mappers
- **Shared**: Common interfaces and base classes

## Infrastructure Organization
- **Caching**: Redis implementation and cache key management
  - Infrastructure/Caching/CacheManager.cs
  - Infrastructure/Caching/RedisCacheService.cs
  - Infrastructure/Caching/CacheKeys.cs
  - Infrastructure/Caching/Options/RedisCacheOptions.cs
- **Authentication**: JWT implementation (commented)
  - Infrastructure/Authentication/JwtHandler.cs
  - Infrastructure/Authentication/AuthOptions.cs
  - Infrastructure/Authentication/Extensions/AuthenticationExtensions.cs
- **Configuration**: Options pattern implementation
  - Infrastructure/Configuration/ConfigurationKeys.cs
  - Infrastructure/Configuration/AppSettings.cs
  - Infrastructure/Configuration/Options/InferenceEngineOptions.cs
  - Infrastructure/Configuration/Options/UserManagementOptions.cs
  - Infrastructure/Configuration/Options/ModelManagementOptions.cs
- **Integration**: External service connectors
  - Infrastructure/Integration/InferenceEngine/InferenceEngineConnector.cs
  - Infrastructure/Integration/InferenceEngine/IInferenceEngineConnector.cs
- **Logging**: Comprehensive logging implementation
  - Infrastructure/Logging/LoggerService.cs
  - Infrastructure/Logging/ILoggerService.cs
  - Infrastructure/Logging/LoggingBehavior.cs
  - Infrastructure/Logging/Models/LogEntry.cs
  - Infrastructure/Logging/Models/AuditRecord.cs
  - Infrastructure/Logging/Options/LoggingOptions.cs
- **Error Handling**: Global exception handling
  - Infrastructure/ErrorHandling/GlobalExceptionHandler.cs
  - Infrastructure/ErrorHandling/Exceptions/ValidationException.cs
  - Infrastructure/ErrorHandling/Exceptions/NotFoundException.cs
  - Infrastructure/ErrorHandling/Exceptions/BusinessException.cs

## Configuration Management
- **Options Pattern**: Strongly-typed configuration classes:
  - InferenceEngineOptions for Ollama integration settings
  - UserManagementOptions for user management settings
  - ModelManagementOptions for model management settings
  - RedisCacheOptions for caching settings
  - LoggingOptions for logging configuration
- **ConfigurationKeys**: Centralized configuration key constants
- **appsettings.json**: Environment-specific configurations:
  - Database connection string (SQL Server)
  - Redis connection (Upstash)
  - JWT settings for authentication
  - Inference engine settings
  - User management settings
  - Model management settings
  - Caching durations
  - Logging configuration

## API Design
- **RESTful Endpoints**: Organized by domain with consistent URL structure:
  - /api/Admin/User/...
  - /api/Admin/AIModel/...
  - /api/Admin/Tag/...
  - /api/Admin/Inference/...
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
- **Domain-Specific Validators**: Organized by domain (User, AIModel, Tag, Inference)
- **Conditional Validation**: Using When() for context-specific validation
- **Validator Registration**: Organized in ServiceExtensions.cs
- **Validation Processing**: Controller-level validation before business logic
- **Error Responses**: Consistent format for validation failures

## Error Handling
- **Global Exception Handler**: Centralized error handling
- **Exception Type Mapping**: Appropriate HTTP status codes based on exception type:
  - ValidationException → 400 Bad Request
  - NotFoundException → 404 Not Found
  - BusinessException → 400 Bad Request
  - General exceptions → 500 Internal Server Error
- **Contextual Logging**: ILoggerService for detailed error information
- **Consistent Response Format**: Structured error messages

## Caching Strategy
- **User Domain**: Cache user profiles (5 min TTL), user roles (15 min TTL)
- **AI Model Domain**: Cache model metadata (10 min TTL), model tags (15 min TTL)
- **Tag Domain**: Cache all tags (30 min TTL), tag relationships (15 min TTL)
- **Inference Domain**: No caching for inference operations
- **Cache Implementation**: ICacheService interface with Redis implementation
- **Cache Keys**: Centralized in CacheKeys.cs
- **Cache Invalidation**: Automatic invalidation on data changes

## Streaming Implementation
- **Server-Sent Events**: For model installation progress
- **IAsyncEnumerable**: For streaming progress updates
- **IProgress<T>**: For progress reporting to controllers
- **Response.BodyWriter**: For direct streaming to clients
- **Content-Type: text/event-stream**: Proper streaming configuration

## Logging Strategy
- **Logging Levels**: Trace, Debug, Information, Warning, Error, Critical
- **Structured Logging**: Using ILoggerService interface
- **Log Enrichment**: Request correlation IDs, user information, operation context
- **Audit Logging**: Track administrative actions with user, timestamp, action, and resources
- **Implementation**: Serilog with configurable sinks
- **MediatR Logging Behavior**: Pipeline behavior for request/response logging

## Integration Patterns
- **InferenceEngineConnector**: Abstracts integration with OllamaSharp
- **IOllamaApiClient**: External client for Ollama API
- **Progress Reporting**: Structured updates for long-running operations
- **Model Conversion**: Mapping between API and domain models 