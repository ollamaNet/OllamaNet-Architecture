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