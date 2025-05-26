# Technical Context for OllamaNet

## Technology Stack Overview

### Core Technologies
- **.NET 9.0**: Modern .NET platform for building all services
- **ASP.NET Core**: Web API framework for RESTful endpoints and streaming responses
- **Entity Framework Core**: ORM for database operations through shared Ollama_DB_layer
- **SQL Server**: Primary relational database for data persistence (db19911.public.databaseasp.net)
- **Redis**: Distributed caching for performance optimization using Upstash (content-ghoul-42217.upstash.io)
- **Ocelot**: API Gateway implementation for request routing and load balancing
- **OllamaSharp**: Client library for interacting with Ollama AI models
- **Semantic Kernel**: Microsoft's framework for chat completion capabilities
- **FluentValidation**: Request validation framework for input validation
- **Swagger/OpenAPI**: API documentation and interactive testing

## Microservice-Specific Technologies

### Gateway Service
- **Ocelot**: Core routing and gateway capabilities
- **JWT Bearer Authentication**: Token validation middleware
- **Rate Limiting**: Configurable limits by endpoint
- **Configuration Management**: Split into service-specific files
- **CORS Configuration**: For frontend application access

### ConversationService
- **Server-Sent Events**: Real-time streaming responses
- **IAsyncEnumerable**: Asynchronous streaming via OllamaConnector
- **Redis Cache**: Multi-level caching architecture with fallback
- **OllamaSharp**: Client for AI model interactions
- **Entity Framework Core**: Data access via Ollama_DB_layer
- **FluentValidation**: Request model validation
- **JWT Authentication**: Security implementation with roles

### AuthService
- **ASP.NET Identity**: User management framework
- **JWT Authentication**: Token generation and validation
- **Refresh Tokens**: Persistent session management
- **Password Management**: Reset and change functionality
- **Role-Based Authorization**: Access control policies
- **Entity Framework Core**: Identity storage and access

### AdminService
- **OllamaSharp**: Client for Ollama API integration
- **Entity Framework Core**: Data access via Ollama_DB_layer
- **FluentValidation**: Administrative request validation
- **Server-Sent Events**: Progress streaming for long operations
- **Repository Pattern**: Data access abstraction
- **JWT Authentication**: Security (configured but commented out)

### ExploreService
- **Redis Cache**: Performance optimization for model metadata
- **Entity Framework Core**: Data access via Ollama_DB_layer
- **JWT Authentication**: Secure access to endpoints
- **Repository Pattern**: Data access abstraction
- **Pagination**: Efficient data retrieval

## Key Dependencies

### NuGet Packages
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication implementation
- **Microsoft.AspNetCore.Identity**: User identity and role management
- **Microsoft.EntityFrameworkCore**: Data access and ORM functionality
- **Microsoft.Extensions.Caching.StackExchangeRedis**: Redis caching support
- **Microsoft.SemanticKernel**: AI chat completion frameworks
- **StackExchange.Redis**: Redis client for distributed caching
- **OllamaSharp**: Ollama API client for AI model interactions
- **FluentValidation.AspNetCore**: Request validation framework
- **Swashbuckle.AspNetCore**: Swagger integration for API documentation
- **Ocelot**: API Gateway framework
- **Ollama_DB_layer**: Shared database access layer with repositories and entities

## Shared Configuration Patterns

### appsettings.json Structure
Each service follows a consistent configuration pattern:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db19911.public.databaseasp.net;Database=Ollama_DB;User Id=xxx;Password=xxx;TrustServerCertificate=True;"
  },
  "Redis": {
    "ConnectionString": "content-ghoul-42217.upstash.io:42217,password=xxx,ssl=True,abortConnect=False"
  },
  "JwtSettings": {
    "SecretKey": "xxx",
    "Issuer": "OllamaNetAuth",
    "Audience": "OllamaNetClients",
    "ExpirationMinutes": 43200
  },
  "RedisCacheSettings": {
    "ConnectionString": "content-ghoul-42217.upstash.io:42217,password=xxx,ssl=True,abortConnect=False",
    "DefaultTTLMinutes": 60,
    "ModelInfoTTLMinutes": 1440,
    "TagsTTLMinutes": 1440,
    "SearchResultsTTLMinutes": 30,
    "MaxRetryAttempts": 3,
    "RetryDelayMilliseconds": 100,
    "RetryDelayMultiplier": 5,
    "OperationTimeoutMilliseconds": 2000
  },
  "OllamaApiSettings": {
    "BaseUrl": "https://704e-35-196-162-195.ngrok-free.app"
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  }
}
```

### ServiceExtensions.cs Pattern
Each service implements extension methods for registration:

```csharp
public static class ServiceExtensions
{
    public static IServiceCollection AddDatabaseAndIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        // Database and Identity registration
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT authentication setup
    }
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Repository registration
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Service registration
    }
    
    public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        // CORS configuration
    }
    
    public static IServiceCollection ConfigureCache(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis cache configuration
    }
    
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        // Swagger documentation setup
    }
}
```

## Caching Implementation

The OllamaNet platform implements a sophisticated Redis-based caching architecture:

### Core Caching Components
- **RedisCacheSettings**: Configuration for Redis connection and domain-specific TTL values
- **CacheKeys**: Centralized cache key management with specific key formats by domain
- **RedisCacheService**: Low-level Redis operations implementation with error handling
- **CacheManager**: High-level caching abstraction with fallback mechanisms
- **CacheExceptions**: Specialized exception types for different cache failure scenarios

### Key Caching Patterns
- **Cache-Aside Pattern**: GetOrSetAsync methods with database fallback
- **TTL Management**: Domain-specific expiration times:
  - Default: 60 minutes
  - Model Info: 1440 minutes (24 hours)
  - Tags: 1440 minutes (24 hours)
  - Search Results: 30 minutes
- **Retry Logic**: Configured with exponential backoff:
  - MaxRetryAttempts: 3
  - RetryDelayMilliseconds: 100
  - RetryDelayMultiplier: 5
- **Exception Handling**: Specialized exceptions with graceful degradation
- **Performance Monitoring**: Stopwatch for operation timing

## Streaming Implementation

The ConversationService implements real-time streaming using Server-Sent Events:

- **Content-Type**: text/event-stream for SSE format
- **IAsyncEnumerable**: Asynchronous streaming via OllamaConnector
- **Response.BodyWriter**: Direct writing to the response stream
- **JSON Serialization**: Response objects serialized to JSON for streaming
- **Error Handling**: Status code responses within the streaming context
- **Background Processing**: Task.Run for post-streaming operations
- **Cancellation Support**: EnumeratorCancellation attribute for proper cancellation

## Authentication & Authorization

All services implement a consistent authentication approach:

- **JWT Authentication**: Token-based authentication via JwtBearer
- **Token Duration**: 30-day token lifetime (43200 minutes)
- **User Identification**: UserId from JWT token claims or X-User-Id header
- **Role-Based Authorization**: Admin and User role policies
- **Token Validation**: Comprehensive validation in JwtBearerOptions:
  - ValidateIssuerSigningKey: true
  - ValidateIssuer: true
  - ValidateAudience: true
  - ValidateLifetime: true
- **HTTPS Requirements**: Enforced in pipeline
- **CORS Configuration**: Configured for frontend application

## API Design Patterns

All services follow RESTful API conventions:

- **Resource-Based Endpoints**: /{resource} pattern
- **HTTP Method Usage**: GET, POST, PUT, DELETE for appropriate operations
- **Status Codes**: Consistent mapping from operations and exceptions
- **Validation**: FluentValidation for request models
- **Documentation**: Swagger with ProducesResponseType attributes
- **Paging**: Consistent paging parameters (page, pageSize)
- **Filtering**: Query parameters for resource filtering
- **Error Responses**: Consistent error format

## Data Access Patterns

All services use a shared database approach:

- **Shared Database**: Single SQL Server instance
- **Ollama_DB_layer**: Common data access layer with:
  - Entity definitions
  - Repository interfaces
  - Repository implementations
  - Database context
- **Unit of Work**: Transaction management
- **Repository Pattern**: Data access abstraction
- **Entity Framework Core**: ORM for database operations

## External Services

- **Ollama AI Service**: AI model inference engine via ngrok endpoint
  - Current endpoint: https://704e-35-196-162-195.ngrok-free.app
- **SQL Server Database**: Data persistence at db19911.public.databaseasp.net
- **Redis Cache**: Distributed caching via Upstash at content-ghoul-42217.upstash.io

## Development Environment

- **IDE**: Visual Studio or VS Code
- **Local Database**: SQL Server (local or containerized)
- **Redis**: Upstash cloud service or local Redis instance
- **API Testing**: Swagger UI, Postman, and .http files
- **Logging**: Console and file-based logging during development
- **Containerization**: Docker support (incomplete) 