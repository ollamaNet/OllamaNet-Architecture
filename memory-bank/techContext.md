# Technical Context

## Technology Stack

### Core Technologies
- **.NET Core** - Primary development framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM and database access
- **SQL Server** - Primary database
- **Redis** - Distributed caching
- **RabbitMQ** - Message queue for asynchronous communication

### Development Tools
- Visual Studio 2022
- Docker
- Redis CLI
- Postman
- Git

### Authentication & Authorization
- **JWT** - Token-based authentication
- **Identity Framework** - User management

### Required Services
1. Redis
   - Token caching
   - Rate limiting
   - Session management
   - Configuration: appsettings.json

2. Ocelot Gateway
   - Request routing
   - Load balancing
   - Configuration: ocelot.json

3. Authentication Service
   - JWT token generation
   - User management
   - Token validation

### Configuration Files
1. appsettings.json
   ```json
   {
     "TokenCaching": {
       "Redis": {
         "ConnectionString": "localhost:6379",
         "Database": 1
       },
       "Options": {
         "SlidingExpiration": "00:30:00",
         "AbsoluteExpiration": "01:00:00",
         "CachePrefix": "token:"
       }
     },
     "RateLimiting": {
       "Redis": {
         "ConnectionString": "localhost:6379",
         "Database": 0
       },
       "GlobalRules": [
         {
           "Endpoint": "*",
           "Period": "1s",
           "Limit": 100
         }
       ]
     }
   }
   ```

2. ocelot.json
   ```json
   {
     "GlobalConfiguration": {
       "BaseUrl": "https://localhost:7270",
       "RateLimitOptions": {
         "ClientIdHeader": "ClientId",
         "QuotaExceededMessage": "Rate limit exceeded",
         "HttpStatusCode": 429
       }
     },
     "Routes": [
       {
         "UpstreamPathTemplate": "/gateway/{service}/{everything}",
         "DownstreamPathTemplate": "/api/{service}/{everything}",
         "DownstreamScheme": "https",
         "RateLimitOptions": {
           "EnableRateLimiting": true
         }
       }
     ]
   }
   ```

## Technical Constraints

### Authentication
- JWT token validation at Gateway
- Redis-based token caching
- Token blacklisting support
- Role-based authorization

### Rate Limiting
- Distributed rate limiting using Redis
- Configurable limits per endpoint
- Client-based rate limiting
- IP-based rate limiting

### Caching
- Redis-based token caching
- Sliding expiration for tokens
- Absolute expiration as fallback
- Cache invalidation strategies

### Security
- HTTPS required
- JWT token validation
- Rate limiting
- Request validation
- CORS configuration

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="7.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.6.122" />
<PackageReference Include="Ocelot" Version="18.0.0" />
<PackageReference Include="AspNetCoreRateLimit" Version="4.0.1" />
```

### External Services
- Redis Server
- Authentication Service
- Microservices (Auth, Admin, Explore, Conversation)

## Development Guidelines

### Code Organization
- Middleware for authentication
- Middleware for rate limiting
- Services for token validation
- Services for Redis caching
- Configuration classes
- Extension methods for setup

### Best Practices
- Async/await throughout
- Proper error handling
- Comprehensive logging
- Unit testing
- Documentation
- Configuration validation

### Performance Considerations
- Redis connection pooling
- Token cache optimization
- Rate limit counter optimization
- Request/response transformation
- Connection management

## Monitoring and Logging

### Metrics to Track
- Authentication success/failure rates
- Token cache hit/miss ratios
- Rate limit violations
- Request latency
- Redis connection status
- Service health

### Logging Strategy
- Structured logging
- Correlation IDs
- Request tracing
- Error logging
- Performance metrics
- Security events

## Caching Architecture

The application uses a multi-layered Redis caching architecture for performance optimization:

### Core Caching Components

1. **RedisCacheSettings**: Configuration class managing connection strings and cache duration settings
2. **CacheKeys**: Static class defining standardized cache key formats
3. **RedisCacheService**: Low-level Redis operations handling with robust error management
4. **CacheManager**: High-level abstraction providing cache-or-compute pattern with fallback strategies
5. **Exception Handling**: Specialized cache exceptions converting to service-appropriate exceptions

### Caching Pattern

The caching implementation follows these principles:

- **Cache-Aside Pattern**: Used throughout services with GetOrSetAsync for transparent caching
- **Error Resilience**: Graceful fallback to database when Redis is unavailable
- **Timeout Handling**: Operations have configurable timeouts to prevent cascading failures
- **Structured Logging**: Comprehensive logging at appropriate levels for observability

### Implementation Guide

A detailed implementation guide is available at `docs/RedisCache_Implementation_Guide.md` to maintain consistency across services.

## Error Handling Strategy

The application implements a layered exception handling approach:

1. **Cache-Level Exceptions**: CacheException hierarchy with specific types for different failure modes
2. **Service-Level Exceptions**: Service-specific exceptions providing business context
3. **Exception Conversion**: ExceptionConverter utility translating between layers
4. **Controller-Level Handling**: HTTP-appropriate status codes based on exception types

## Logging Strategy

The application uses structured logging with the following level convention:

- **Debug**: Detailed diagnostic information (cache operations, timing)
- **Information**: Normal application flow (method entry/exit, cache hits/misses)
- **Warning**: Potential issues that don't impact functionality (cache fallbacks)
- **Error**: Runtime errors affecting functionality (database failures, timeout issues)

## Other Technical Details

[Keep existing content about other technical aspects of the system] 