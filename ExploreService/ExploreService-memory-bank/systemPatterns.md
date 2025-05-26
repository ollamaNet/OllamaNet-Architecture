# System Patterns for ExploreService

## Architecture Overview
ExploreService follows a clean, layered architecture pattern within a microservices ecosystem:

- **API Layer**: Controllers handling HTTP requests and responses (ExploreController)
- **Service Layer**: Business logic encapsulation (IExploreService implementation)
- **Data Access Layer**: Repository pattern for data operations (via IUnitOfWork)
- **Caching Layer**: Sophisticated Redis-backed caching with resilience patterns

## Design Patterns
- **Repository Pattern**: Abstracts data access through repository interfaces (IAIModelRepository, ITagRepository, etc.)
- **Unit of Work**: Manages transactions and repository coordination (IUnitOfWork)
- **Dependency Injection**: For loose coupling and testability
- **Caching Strategy**: Two-tier caching approach with ICacheManager and IRedisCacheService
- **Circuit Breaker Pattern**: Graceful fallback when cache operations fail
- **Retry Pattern**: Implemented in cache operations with exponential backoff
- **Mapper Pattern**: Clean separation between domain entities and DTOs (ModelMapper, TagMapper)
- **Exception Handling**: Specialized exception hierarchy for domain and infrastructure concerns

## Component Relationships
```
Controllers → Services → Repositories → Database
           ↓
    CacheManager → RedisCacheService → Redis
```

- Controllers depend on service interfaces (IExploreService)
- Services implement business logic and coordinate between repositories and cache
- CacheManager provides high-level caching abstraction with fallback strategies
- RedisCacheService provides low-level Redis operations with timeout/retry handling
- Repositories provide data access abstractions through the UnitOfWork

## Configuration Management
- Settings organized in appsettings.json
- Strongly-typed settings with IOptions<RedisCacheSettings>
- Environment-specific configurations for development and production
- Services registered via extension methods for clean startup organization

## API Design
- RESTful endpoints following REST principles
- OpenAPI/Swagger documentation for API discovery
- Consistent response patterns
- Structured error handling with custom exceptions

## Caching Strategy
- Cache-aside pattern with GetOrSetAsync for all data operations
- Configurable expiration policies per data type
- Fallback to data provider on cache failures
- Sophisticated error handling for all cache scenarios:
  - Connection failures
  - Timeouts with configurable thresholds
  - Serialization errors
  - General cache operations

## Exception Handling
- Hierarchical exception structure:
  - Base ExploreServiceException
  - Specialized exceptions for not-found scenarios (ModelNotFoundException, TagNotFoundException)
  - Data access exceptions (DataRetrievalException)
  - Cache-specific exception hierarchy with CacheException base
- Consistent exception conversion from infrastructure to domain
- Detailed logging at appropriate levels

## Cross-Cutting Concerns
- Comprehensive logging with appropriate log levels
- Performance monitoring with Stopwatch for critical operations
- CORS configuration for frontend integration
- Detailed caching policy defined in RedisCacheSettings 