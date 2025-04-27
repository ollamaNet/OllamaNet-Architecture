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