# Technical Context for AdminService

## Core Technologies
- **.NET 9.0**: Latest .NET platform for the application
- **ASP.NET Core**: Web API framework for RESTful endpoints
- **Entity Framework Core**: ORM for database operations
- **SQL Server**: Primary database (via Database ASP.NET)
- **OllamaSharp**: Client library for Ollama API integration
- **FluentValidation**: Comprehensive request validation framework
- **StackExchange.Redis**: Redis client for distributed caching and configuration
- **RabbitMQ**: Message broker for service discovery and configuration updates
- **Swagger/OpenAPI**: API documentation and interactive testing

## Key Dependencies
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication (configured but commented out)
- **Microsoft.AspNetCore.Identity**: User identity management
- **FluentValidation.AspNetCore**: Request validation library
- **Swashbuckle.AspNetCore**: Swagger integration for API documentation
- **Scalar.AspNetCore**: API schema generation tool
- **StackExchange.Redis**: Redis client for caching capabilities
- **OllamaSharp**: Ollama API client for model operations
- **Ollama_DB_layer**: Database access layer with repositories and entities
- **System.Text.Json**: JSON serialization for cache values

## Key Components

### Controllers
- **UserOperationsController**: Comprehensive user management endpoints
  - User CRUD operations
  - Role assignment
  - Status management (activation, locking)
  - Role creation and management
- **AIModelOperationsController**: AI model administration endpoints
  - Model CRUD operations
  - Tag assignment and removal
  - Model search and filtering
- **TagOperationsController**: Tag management endpoints
  - Tag CRUD operations
  - Tag retrieval and search
- **InferenceOperationsController**: Ollama integration endpoints
  - Model installation with progress streaming
  - Model information retrieval
  - Model management operations
- **AdminController**: (commented out) Overlapping admin functionality

### Services
- **UserOperationsService**: User and role management business logic
- **AIModelOperationsService**: AI model administration business logic
- **TagsOperationsService**: Tag management business logic
- **InferenceOperationsService**: Ollama integration business logic
- **AdminService**: (legacy) General admin operations

### Validators
- Comprehensive validation classes implementing AbstractValidator<T>:
  - User operation validators (CreateUser, UpdateProfile, etc.)
  - Model operation validators (CreateModel, UpdateModel, etc.)
  - Tag operation validators
  - Search and filter validators

### Connectors
- **OllamaConnector**: Interface with the Ollama inference engine
  - Model information retrieval
  - Model installation with progress reporting
  - Model removal operations

## API Structure
All endpoints follow the pattern `/api/Admin/[controller]` with appropriate HTTP methods:

### User Operations
- `GET /api/Admin/UserOperations`: List users (with pagination and optional role filtering)
- `GET /api/Admin/UserOperations/{id}`: Get user by ID
- `GET /api/Admin/UserOperations/search`: Search users by term and optional role
- `PATCH /api/Admin/UserOperations/{id}/role`: Change user role
- `PATCH /api/Admin/UserOperations/{id}/status`: Toggle user active status
- `DELETE /api/Admin/UserOperations/{id}`: Hard delete user
- `PATCH /api/Admin/UserOperations/{id}/soft-delete`: Soft delete user
- `POST /api/Admin/UserOperations/{id}/lock`: Lock user with specified duration
- `POST /api/Admin/UserOperations/{id}/unlock`: Unlock user account
- `GET /api/Admin/UserOperations/roles`: Get available roles
- `POST /api/Admin/UserOperations/roles`: Create new role
- `DELETE /api/Admin/UserOperations/roles/{id}`: Delete role

### AI Model Operations
- `GET /api/Admin/AIModelOperations/{modelId}`: Get model by ID
- `POST /api/Admin/AIModelOperations`: Create model (with or without Ollama integration)
- `PUT /api/Admin/AIModelOperations`: Update model details
- `POST /api/Admin/AIModelOperations/tags/add`: Add tags to model
- `POST /api/Admin/AIModelOperations/tags/remove`: Remove tags from model
- `DELETE /api/Admin/AIModelOperations/{modelId}`: Hard delete model
- `DELETE /api/Admin/AIModelOperations/{modelId}/softdelete`: Soft delete model

### Tag Operations
- `GET /api/Admin/TagOperations/{id}`: Get tag by ID
- `POST /api/Admin/TagOperations`: Create new tag
- `PUT /api/Admin/TagOperations`: Update existing tag
- `DELETE /api/Admin/TagOperations/{id}`: Delete tag

### Inference Operations
- `GET /api/Admin/InferenceOperations/models/info`: Get model information by name
- `GET /api/Admin/InferenceOperations/models`: Get installed models (with pagination)
- `POST /api/Admin/InferenceOperations/models/pull`: Pull/install model with progress streaming
- `DELETE /api/Admin/InferenceOperations/models`: Uninstall model

## Validation Approach
- **Request Model Validation**: All request models validated with FluentValidation
- **Controller-Level Validation**: Validation performed before processing requests
- **Conditional Validation**: Using When() for context-specific validation rules
- **Custom Validators**: Specialized validators for each request type
- **Validation Registration**: Validators registered in dependency injection
- **Consistent Error Responses**: Standardized format for validation failures

## Error Handling Strategy
- **Controller-Level Try/Catch**: Wrapped operations with appropriate error handling
- **Exception Type Mapping**: 
  - KeyNotFoundException → 404 Not Found
  - ArgumentException → 400 Bad Request
  - InvalidOperationException → 400 Bad Request
  - General exceptions → 500 Internal Server Error
- **Contextual Logging**: Detailed logging with ILogger<T>
- **Structured Responses**: Consistent error response format

## Streaming Implementation
The model installation endpoint supports Server-Sent Events for progress reporting:
- **Content-Type**: text/event-stream
- **Cache-Control**: no-cache
- **Connection**: keep-alive
- **IAsyncEnumerable**: For asynchronous streaming of progress updates
- **IProgress<T>**: For progress reporting to controllers
- **Response.BodyWriter**: For direct streaming to HTTP response

## Development Environment
- **IDE**: Visual Studio or VS Code
- **Runtime**: .NET 9.0 SDK
- **Database**: SQL Server (remote or local)
- **Cache**: Redis instance (Upstash or local)
- **API Testing**: Swagger UI or AdminService.http
- **Ollama**: Local Ollama instance for testing model operations

## External Services
- **Database ASP.NET**: Hosted SQL Server instance at db19911.public.databaseasp.net
- **Upstash**: Redis cloud service at gentle-reindeer-38808.upstash.io
- **CloudAMQP**: RabbitMQ cloud service at toucan.lmq.cloudamqp.com
- **Ollama**: AI model serving system (default: http://localhost:11434, configurable via service discovery)

## Configuration
- **Connection Strings**: Database and Redis connections in appsettings.json
- **JWT Settings**: Authentication configuration (defined but commented out in code)
- **CORS Settings**: Frontend origin configuration for http://localhost:5173
- **RedisCaching**: Timeout, retry, and expiration settings for Redis operations
- **RabbitMQ**: Message broker configuration for service discovery
- **Logging**: Standard ASP.NET Core logging configuration

## Security Considerations
- JWT authentication structure in place but commented out in Program.cs
- Role-based authorization policies defined (Admin, User)
- Input validation for all requests via FluentValidation
- Password reset and account management capabilities
- User locking and unlocking functionality
- SQL Server with TLS/SSL encryption enabled 