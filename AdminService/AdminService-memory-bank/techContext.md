# Technical Context for AdminService

## Core Technologies
- **.NET 9.0**: Latest .NET platform for the application
- **ASP.NET Core**: Web API framework for RESTful endpoints
- **Entity Framework Core**: ORM for database operations
- **SQL Server**: Primary database (via Database ASP.NET)
- **OllamaSharp**: Client library for Ollama API integration
- **FluentValidation**: Comprehensive request validation framework
- **StackExchange.Redis**: Redis client for caching (minimal usage)
- **Swagger/OpenAPI**: API documentation and interactive testing
- **Serilog**: Structured logging framework

## Key Dependencies
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication (configured but commented out)
- **Microsoft.AspNetCore.Identity**: User identity management
- **FluentValidation.AspNetCore**: Request validation library
- **Swashbuckle.AspNetCore**: Swagger integration for API documentation
- **Scalar.AspNetCore**: API schema generation tool
- **StackExchange.Redis**: Redis client for caching capabilities
- **OllamaSharp**: Ollama API client for model operations
- **Ollama_DB_layer**: Database access layer with repositories and entities
- **MediatR**: Mediator pattern implementation for pipeline behaviors

## Folder Structure
```
AdminService/
│
├── Controllers/                # API endpoints grouped by domain
│   ├── User/                  # User management endpoints
│   │   └── UserOperationsController.cs
│   ├── AIModel/              # AI model management endpoints
│   │   └── AIModelOperationsController.cs
│   ├── Tag/                  # Tag management endpoints
│   │   └── TagOperationsController.cs
│   ├── Inference/            # Inference engine integration endpoints
│   │   └── InferenceOperationsController.cs
│   └── Validators/           # FluentValidation classes for requests
│       ├── User/
│       ├── AIModel/
│       ├── Tag/
│       └── Inference/
├── Services/                  # Business logic grouped by domain
│   ├── User/                 # User management domain
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Mappers/
│   │   └── Implementation/
│   │       └── UserService.cs, IUserService.cs
│   ├── AIModel/             # AI model management domain
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── Implementation/
│   │       └── ModelService.cs, IModelService.cs
│   ├── Tag/                 # Tag management domain
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── Implementation/
│   │       └── TagService.cs, ITagService.cs
│   ├── Inference/          # Inference engine integration domain
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── Implementation/
│   │       └── InferenceService.cs, IInferenceService.cs
│   └── Shared/             # Shared interfaces or base classes
├── Infrastructure/         # Cross-cutting concerns
│   ├── Caching/           # Redis caching implementation
│   │   ├── CacheManager.cs
│   │   ├── RedisCacheService.cs
│   │   ├── CacheKeys.cs
│   │   └── Options/
│   │       └── RedisCacheOptions.cs
│   ├── Authentication/    # JWT auth implementation (commented)
│   │   ├── JwtHandler.cs
│   │   ├── AuthOptions.cs
│   │   └── Extensions/
│   │       └── AuthenticationExtensions.cs
│   ├── Configuration/     # Configuration management
│   │   ├── ConfigurationKeys.cs
│   │   ├── AppSettings.cs
│   │   └── Options/
│   │       ├── InferenceEngineOptions.cs
│   │       ├── UserManagementOptions.cs
│   │       └── ModelManagementOptions.cs
│   ├── Integration/       # External service connectors
│   │   └── InferenceEngine/
│   │       ├── InferenceEngineConnector.cs
│   │       └── IInferenceEngineConnector.cs
│   ├── Messaging/         # Reserved for future message queue implementation
│   └── Logging/           # Comprehensive logging implementation
│       ├── LoggerService.cs
│       ├── ILoggerService.cs
│       ├── LoggingBehavior.cs
│       ├── Models/
│       │   ├── LogEntry.cs
│       │   └── AuditRecord.cs
│       └── Options/
│           └── LoggingOptions.cs
├── Docs/                  # Documentation and system design
│   ├── architecture/
│   └── api/
└── AdminService-memory-bank/  # Project context and documentation
```

## Key Components

### Controllers Layer
- **User Domain**:
  - **UserOperationsController**: User management endpoints
  - Domain-specific validators in Controllers/Validators/User/
- **AIModel Domain**:
  - **AIModelOperationsController**: AI model administration endpoints
  - Domain-specific validators in Controllers/Validators/AIModel/
- **Tag Domain**:
  - **TagOperationsController**: Tag management endpoints
  - Domain-specific validators in Controllers/Validators/Tag/
- **Inference Domain**:
  - **InferenceOperationsController**: Ollama integration endpoints
  - Domain-specific validators in Controllers/Validators/Inference/

### Services Layer
- **User Domain**:
  - **UserService (IUserService)**: User and role management business logic
  - Domain-specific DTOs and mappers
- **AIModel Domain**:
  - **ModelService (IModelService)**: AI model administration business logic
  - Domain-specific DTOs and mappers
- **Tag Domain**:
  - **TagService (ITagService)**: Tag management business logic
  - Domain-specific DTOs and mappers
- **Inference Domain**:
  - **InferenceService (IInferenceService)**: Ollama integration business logic
  - Domain-specific DTOs and mappers
- **Shared**: Common interfaces and base classes

### Infrastructure Layer
- **Caching**:
  - **CacheManager**: Cache management implementation
  - **RedisCacheService**: Redis-based caching service
  - **CacheKeys**: Centralized cache key definitions
  - **RedisCacheOptions**: Configuration options for caching
- **Authentication** (commented):
  - **JwtHandler**: JWT token generation and validation
  - **AuthOptions**: Authentication configuration options
  - **AuthenticationExtensions**: DI extensions for authentication
- **Configuration**:
  - **ConfigurationKeys**: Centralized configuration key constants
  - **AppSettings**: Configuration wrapper
  - **Options Classes**: Strongly-typed configuration options
- **Integration**:
  - **InferenceEngineConnector**: Interface with Ollama inference engine
  - **IInferenceEngineConnector**: Interface for inference engine operations
- **Logging**:
  - **LoggerService**: Comprehensive logging implementation
  - **ILoggerService**: Logging interface
  - **LoggingBehavior**: MediatR pipeline behavior for logging
  - **LogEntry/AuditRecord**: Logging data models
  - **LoggingOptions**: Logging configuration options
- **Error Handling**:
  - **GlobalExceptionHandler**: Centralized exception handling
  - **Custom Exceptions**: Domain-specific exception types

## API Structure
All endpoints follow the pattern `/api/Admin/[domain]` with appropriate HTTP methods:

### User Operations
- `GET /api/Admin/User`: List users (with pagination and optional role filtering)
- `GET /api/Admin/User/{id}`: Get user by ID
- `GET /api/Admin/User/search`: Search users by term and optional role
- `PATCH /api/Admin/User/{id}/role`: Change user role
- `PATCH /api/Admin/User/{id}/status`: Toggle user active status
- `DELETE /api/Admin/User/{id}`: Hard delete user
- `PATCH /api/Admin/User/{id}/soft-delete`: Soft delete user
- `POST /api/Admin/User/{id}/lock`: Lock user with specified duration
- `POST /api/Admin/User/{id}/unlock`: Unlock user account
- `GET /api/Admin/User/roles`: Get available roles
- `POST /api/Admin/User/roles`: Create new role
- `DELETE /api/Admin/User/roles/{id}`: Delete role

### AI Model Operations
- `GET /api/Admin/AIModel/{modelId}`: Get model by ID
- `POST /api/Admin/AIModel`: Create model (with or without Ollama integration)
- `PUT /api/Admin/AIModel`: Update model details
- `POST /api/Admin/AIModel/tags/add`: Add tags to model
- `POST /api/Admin/AIModel/tags/remove`: Remove tags from model
- `DELETE /api/Admin/AIModel/{modelId}`: Hard delete model
- `DELETE /api/Admin/AIModel/{modelId}/softdelete`: Soft delete model

### Tag Operations
- `GET /api/Admin/Tag/{id}`: Get tag by ID
- `POST /api/Admin/Tag`: Create new tag
- `PUT /api/Admin/Tag`: Update existing tag
- `DELETE /api/Admin/Tag/{id}`: Delete tag

### Inference Operations
- `GET /api/Admin/Inference/models/info`: Get model information by name
- `GET /api/Admin/Inference/models`: Get installed models (with pagination)
- `POST /api/Admin/Inference/models/pull`: Pull/install model with progress streaming
- `DELETE /api/Admin/Inference/models`: Uninstall model

## Caching Strategy

### User Domain
- Cache user profiles: 5 minutes TTL
- Cache user roles: 15 minutes TTL
- No caching for user operations (create, update, delete)

### AI Model Domain
- Cache model metadata: 10 minutes TTL
- Cache model tags: 15 minutes TTL
- No caching for model operations (create, update, delete)

### Tag Domain
- Cache all tags: 30 minutes TTL
- Cache tag relationships: 15 minutes TTL
- No caching for tag operations (create, update, delete)

### Inference Domain
- No caching for any inference operations (as per requirements)

## Validation Approach
- **Domain-Specific Validators**: Organized by domain in Controllers/Validators/
- **FluentValidation**: All request models validated with FluentValidation
- **Controller-Level Validation**: Validation performed before processing requests
- **Conditional Validation**: Using When() for context-specific validation rules
- **Validation Registration**: Validators registered in dependency injection
- **Consistent Error Responses**: Standardized format for validation failures

## Error Handling Strategy
- **Global Exception Handler**: Centralized exception handling
- **Exception Type Mapping**: 
  - ValidationException → 400 Bad Request
  - NotFoundException → 404 Not Found
  - BusinessException → 400 Bad Request
  - General exceptions → 500 Internal Server Error
- **Contextual Logging**: Detailed logging with ILoggerService
- **Structured Responses**: Consistent error response format

## Logging Strategy
- **Logging Levels**:
  - **Trace**: Detailed debugging information (development only)
  - **Debug**: Debugging information useful during development
  - **Information**: General operational information
  - **Warning**: Non-critical issues that might need attention
  - **Error**: Error conditions that impact specific operations
  - **Critical**: Critical failures requiring immediate attention
- **Structured Logging**: Using ILoggerService interface
- **Log Enrichment**: Request correlation IDs, user information, operation context
- **Audit Logging**: Track all administrative actions
- **Implementation**: Serilog with configurable sinks
- **MediatR Logging Behavior**: Pipeline behavior for request/response logging

## Streaming Implementation
The model installation endpoint supports Server-Sent Events for progress reporting:
- **Content-Type**: text/event-stream
- **Cache-Control**: no-cache
- **Connection**: keep-alive
- **IAsyncEnumerable**: For asynchronous streaming of progress updates
- **IProgress<T>**: For progress reporting to controllers
- **Response.BodyWriter**: For direct streaming to HTTP response

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

## Development Environment
- **IDE**: Visual Studio or VS Code
- **Runtime**: .NET 9.0 SDK
- **Database**: SQL Server (remote or local)
- **Cache**: Redis instance (Upstash or local)
- **API Testing**: Swagger UI or AdminService.http
- **Ollama**: Local Ollama instance for testing model operations

## External Services
- **Database ASP.NET**: Hosted SQL Server instance at db19911.public.databaseasp.net
- **Upstash**: Redis cloud service at content-ghoul-42217.upstash.io
- **Ollama**: AI model serving system (default: http://localhost:11434)

## Security Considerations
- JWT authentication structure in place but commented out in Program.cs
- Role-based authorization policies defined (Admin, User)
- Input validation for all requests via FluentValidation
- Password reset and account management capabilities
- User locking and unlocking functionality
- SQL Server with TLS/SSL encryption enabled 