# Technical Context for ExploreService

## Core Technologies
- **.NET 9.0**: Latest .NET platform for building the API
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for database operations
- **SQL Server**: Primary database (deployed on Database ASP.NET)
- **Redis**: Distributed caching (deployed on Upstash)
- **Swagger/OpenAPI**: API documentation and testing

## Key Dependencies
- **FluentValidation.AspNetCore**: For request validation
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication
- **StackExchange.Redis**: Redis client library for cache operations
- **Swashbuckle.AspNetCore**: Swagger integration for API documentation
- **Scalar.AspNetCore**: GraphQL/API schema generation
- **OllamaNet_Components.ServiceDefaults**: Shared service components
- **Ollama_DB_layer**: Database access layer with repositories and entities

## Cache Implementation
- **Two-tier architecture**:
  - ICacheManager: High-level abstraction for caching operations
  - IRedisCacheService: Low-level Redis operations
- **Caching strategies**:
  - Cache-aside pattern via GetOrSetAsync
  - Automatic fallback to data source on cache failures
  - Retry mechanism with exponential backoff
  - Configurable timeouts for all operations
- **Cache keys**:
  - Standardized key format in CacheKeys class
  - Keys for models, tags, and search results
- **Expiration policies**:
  - Default: 60 minutes
  - Model info: 24 hours (1440 minutes)
  - Tags: 24 hours (1440 minutes)
  - Search: 30 minutes

## Exception Handling
- **Domain exceptions**:
  - ExploreServiceException (base)
  - ModelNotFoundException
  - TagNotFoundException
  - DataRetrievalException
- **Infrastructure exceptions**:
  - CacheException (base)
  - CacheConnectionException
  - CacheTimeoutException
  - CacheSerializationException
  - CacheKeyNotFoundException
  - CacheOperationException
- **Exception conversion**:
  - ExceptionConverter for mapping infrastructure to domain exceptions

## Performance Monitoring
- **Stopwatch** for measuring operation durations
- **Logging** with appropriate levels:
  - Information: Standard operations
  - Debug: Detailed operation information
  - Warning: Recoverable errors
  - Error: Critical issues
- **Cache hit/miss tracking** via logs

## Development Environment
- **IDE**: Visual Studio or VS Code
- **Runtime**: .NET 9.0 SDK
- **Local Database**: SQL Server (configurable)
- **Local Cache**: Redis instance (configurable)
- **API Testing**: Swagger UI or ExploreService.http

## External Services
- **Database ASP.NET**: Hosted SQL Server instance
- **Upstash**: Redis cloud service for distributed caching

## Configuration
- **Connection Strings**: Database and Redis connections in appsettings.json
- **JWT Settings**: Authentication configuration
- **Redis Cache Settings**:
  - ConnectionString: Upstash Redis connection
  - InstanceName: "ConversationService"
  - DefaultExpirationMinutes: 60
  - ModelInfoExpirationMinutes: 1440
  - TagExpirationMinutes: 1440
  - SearchExpirationMinutes: 30
  - OperationTimeoutMilliseconds: 2000
  - MaxRetryAttempts: 3
  - RetryDelayMilliseconds: 100
  - RetryDelayMultiplier: 5

## API Endpoints
- **GET /api/v1/explore/models**: Paginated model listing
- **GET /api/v1/explore/models/{id}**: Detailed model information
- **GET /api/v1/explore/tags**: List all tags
- **GET /api/v1/explore/tags/{tagId}/models**: Models by tag

## Deployment
- Deployed using WebDeploy
- Running as an ASP.NET Core web service
- HTTPS required for production
- Service configured for both Development and Production environments

## Security Considerations
- JWT authentication for API security
- Connection strings and secrets stored in appsettings.json (should be secured in production)
- HTTPS required in production
- Proper exception handling to avoid leaking sensitive information
- Extensive logging with appropriate levels