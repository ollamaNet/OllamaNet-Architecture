# AdminService System Design Plan

## 1. Folder Structure (Current)

```
AdminService/
│
├── Controllers/                # API endpoints (User, AIModel, Tag, Inference, Health)
│   └── Validators/             # FluentValidation classes for requests
├── Services/                   # All domain services grouped here
│   ├── UserOperations/         # User management domain logic
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── UserOperationsService.cs, IUserOperationsService.cs
│   ├── AIModelOperations/      # AI model administration domain logic
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── AIModelOperationsService.cs, IAIModelOperationsService.cs
│   ├── TagsOperations/         # Tag management domain logic
│   │   ├── DTOs/
│   │   └── TagsOperationsService.cs, ITagsOperationsService.cs
│   └── InferenceOperations/    # Inference engine operations domain logic
│       ├── DTOs/
│       └── InferenceOperationsService.cs, IInferenceOperationsService.cs
├── Infrastructure/             # Cross-cutting concerns
│   ├── Caching/                # Redis, MemoryCache, etc.
│   │   ├── CacheManager.cs
│   │   ├── RedisCacheService.cs
│   │   ├── RedisCacheSettings.cs
│   │   ├── CacheKeys.cs
│   │   └── Exceptions/
│   ├── Configuration/          # Dynamic configuration management
│   │   ├── InferenceEngineConfiguration.cs
│   │   ├── IInferenceEngineConfiguration.cs
│   │   └── Options/
│   ├── Extensions/             # Extension methods for service registration
│   │   └── ServiceExtensions.cs
│   ├── Integration/            # External connectors
│   │   └── InferenceEngine/
│   │       ├── InferenceEngineConnector.cs
│   │       └── IInferenceEngineConnector.cs
│   ├── Messaging/              # Message broker integration
│   │   ├── Consumers/
│   │   │   └── InferenceUrlConsumer.cs
│   │   ├── Models/
│   │   │   └── InferenceUrlUpdateMessage.cs
│   │   └── Resilience/
│   │       └── RabbitMQResiliencePolicies.cs
│   └── Validation/             # Validation utilities
│       ├── IUrlValidator.cs
│       └── UrlValidator.cs
├── Docs/                       # Documentation and system design
│   ├── Features/               # Feature-specific documentation
│   └── architecture/           # Architecture documentation
├── AdminService-memory-bank/   # Project context and documentation
├── MIGRATION_PROGRESS/         # Migration tracking
├── Properties/
├── Program.cs
├── ServiceExtensions.cs
└── AdminService.csproj
```

---

## 2. Folder & File Responsibilities

- **Controllers/**: Only handle HTTP, validation, and delegate to services.
  - **Validators/**: All FluentValidation classes for request validation.
- **Services/**: All business logic, grouped by domain.
  - **UserOperations/**: User and role management business logic.
    - **DTOs/**: Data Transfer Objects for user operations.
    - **Mappers/**: User-specific mappers.
  - **AIModelOperations/**: AI model administration business logic.
    - **DTOs/**: Data Transfer Objects for model operations.
    - **Mappers/**: Model-specific mappers.
  - **TagsOperations/**: Tag management business logic.
    - **DTOs/**: Data Transfer Objects for tag operations.
  - **InferenceOperations/**: Inference engine operations business logic.
    - **DTOs/**: Data Transfer Objects for inference operations.
- **Infrastructure/**: Cross-cutting concerns.
  - **Caching/**: `CacheManager.cs`, `RedisCacheService.cs`, `RedisCacheSettings.cs`, `CacheKeys.cs`, cache exceptions.
  - **Configuration/**: `InferenceEngineConfiguration.cs` for dynamic configuration management.
  - **Extensions/**: Service registration and extension methods.
  - **Integration/**: `InferenceEngineConnector.cs`, `IInferenceEngineConnector.cs` for Ollama integration.
  - **Messaging/**: RabbitMQ integration for service discovery.
    - **Consumers/**: Message consumers for different topics.
    - **Models/**: Message data models.
    - **Resilience/**: Resilience patterns for messaging.
  - **Validation/**: Validation utilities for URLs and other data.
- **Docs/**: All documentation, including this system design plan.
  - **Features/**: Feature-specific documentation.
  - **architecture/**: Architecture documentation.
- **AdminService-memory-bank/**: Project context, memory, and documentation.
- **MIGRATION_PROGRESS/**: Migration tracking and progress documentation.

---

## 3. Best Practices

- **Naming**: Use clear, descriptive, and consistent names for all files and folders.
- **DTOs**: Always grouped by feature for clarity and maintainability.
- **Services**: All business logic, one service per domain, grouped under Services/.
- **Dependency Injection**: All services, helpers, and infrastructure registered via DI.
- **Single Responsibility Principle**: Each class/folder has one clear purpose.
- **Options Pattern**: For all configuration (e.g., caching, integration).
- **Documentation**: Keep Docs/ and memory-bank/ up to date with every major change.
- **Resilience Patterns**: Use circuit breaker, retry, and fallback patterns for external dependencies.
- **Message-Based Communication**: Use RabbitMQ for loosely coupled service communication.
- **Dynamic Configuration**: Support runtime configuration updates via messaging.
- **Validation**: Use FluentValidation for comprehensive request validation.
- **Exception Handling**: Use try/catch with contextual logging for robust error handling.
- **RESTful API Design**: Use consistent patterns across all administrative operations.

---

## 4. Current State Snapshot

**Domain Folders and Files:**
- Services/UserOperations/: UserOperationsService.cs, IUserOperationsService.cs, DTOs/, Mappers/
- Services/AIModelOperations/: AIModelOperationsService.cs, IAIModelOperationsService.cs, DTOs/, Mappers/
- Services/TagsOperations/: TagsOperationsService.cs, ITagsOperationsService.cs, DTOs/
- Services/InferenceOperations/: InferenceOperationsService.cs, IInferenceOperationsService.cs, DTOs/

**Shared, Infrastructure, and Helper Files:**
- Infrastructure/Caching/: CacheManager.cs, RedisCacheService.cs, RedisCacheSettings.cs, CacheKeys.cs, Exceptions/
- Infrastructure/Configuration/: InferenceEngineConfiguration.cs, IInferenceEngineConfiguration.cs, Options/
- Infrastructure/Extensions/: ServiceExtensions.cs
- Infrastructure/Integration/: InferenceEngineConnector.cs, IInferenceEngineConnector.cs
- Infrastructure/Messaging/: RabbitMQ integration for service discovery
- Infrastructure/Validation/: IUrlValidator.cs, UrlValidator.cs

**Controllers and Validators:**
- Controllers/: UserOperationsController.cs, AIModelOperationsController.cs, TagOperationsController.cs, InferenceOperationsController.cs, HealthController.cs, Validators/

---

## 5. Service Discovery Architecture

The Service Discovery system follows a message-based architecture with RabbitMQ as the communication backbone:

### Infrastructure Layer

#### Configuration (`Infrastructure/Configuration/`)
- **InferenceEngineConfiguration**
  - Centralized configuration service for managing the InferenceEngine URL
  - Provides methods for retrieving and updating the URL
  - Uses Redis for persistent storage of URL
  - Emits events when URL is updated
  - Implements fault tolerance for Redis unavailability

#### Messaging (`Infrastructure/Messaging/`)
- **Consumers**
  - `InferenceUrlConsumer`: Background service that connects to RabbitMQ and processes URL update messages
- **Models**
  - `InferenceUrlUpdateMessage`: Message model for URL updates with timestamp and metadata
- **Resilience**
  - `RabbitMQResiliencePolicies`: Retry and circuit breaker patterns for RabbitMQ connections

#### Validation (`Infrastructure/Validation/`)
- **UrlValidator**
  - Validates URLs for security and format correctness

#### Integration (`Infrastructure/Integration/`)
- **InferenceEngineConnector**
  - Updated connector that uses the dynamic configuration service
  - Subscribes to URL changes and updates at runtime
  - Implements resilience patterns for external calls

### Key Features
- Dynamic configuration updates without service restart
- Message-based communication for loose coupling
- Redis persistence for configuration durability across restarts
- Resilience patterns for messaging and external services
- URL validation for security
- Event-based notification for configuration changes
- Graceful fallback mechanisms when services are unavailable

### Flow Diagram
```
┌──────────────┐     ┌───────────┐     ┌───────────────────┐
│ Admin Service│────>│  RabbitMQ │────>│nferenceUrlConsumer│
└──────────────┘     └───────────┘     └────────┬──────────┘
                                                │
                                                ▼
                                     ┌─────────────────────┐
                                     │InferenceEngineConfig│
                                     └────────┬────────────┘
                                              │
                          ┌───────────────────┼───────────────────┐
                          ▼                   ▼                   ▼
               ┌──────────────────┐  ┌─────────────┐    ┌───────────────┐
               │InferenceConnector│  │ Redis Cache │    │OtherDependents│
               └──────────────────┘  └─────────────┘    └───────────────┘
```

---

## 6. Caching Architecture

The Caching system follows a layered architecture with Redis as the primary caching mechanism:

### Infrastructure Layer (`Infrastructure/Caching/`)

- **Interfaces**
  - `ICacheManager`: High-level interface for cache operations
  - `IRedisCacheService`: Low-level interface for Redis operations

- **Implementation**
  - `CacheManager`: Implementation of ICacheManager with caching logic
  - `RedisCacheService`: Implementation of IRedisCacheService with Redis operations

- **Configuration**
  - `RedisCacheSettings`: Configuration for Redis connection and timeouts

- **Constants**
  - `CacheKeys`: Centralized cache key management

- **Exceptions**
  - `CacheException`: Base exception for cache operations
  - `CacheConnectionException`: Connection-specific exceptions
  - `CacheSerializationException`: Serialization-specific exceptions

### Key Features
- JSON serialization for all cached values
- Configurable timeouts for Redis operations
- Retry policies for transient failures
- Circuit breaker for Redis unavailability
- Centralized cache key management
- Comprehensive error handling
- GetOrSet pattern for efficient caching
- Cache invalidation capabilities

---

## 7. Integration Architecture

The Integration system provides a clean abstraction for external service communication:

### Infrastructure Layer (`Infrastructure/Integration/`)

#### InferenceEngine
- **Interfaces**
  - `IInferenceEngineConnector`: Interface for inference engine operations

- **Implementation**
  - `InferenceEngineConnector`: Implementation of IInferenceEngineConnector
    - Wraps OllamaSharp client for Ollama API integration
    - Subscribes to URL changes from InferenceEngineConfiguration
    - Implements resilience patterns for external calls
    - Provides methods for model management and inference operations

### Key Features
- Dynamic URL configuration via service discovery
- Resilience patterns for external service calls
- Clean abstraction of external service details
- Comprehensive error handling and logging
- Event-based reconfiguration without restart

---

## 8. API Layer Architecture

The API Layer follows RESTful principles with comprehensive validation:

### Controllers (`Controllers/`)

- **UserOperationsController**: User management endpoints
  - User CRUD operations
  - Role assignment and management
  - Status management (activation, locking)

- **AIModelOperationsController**: AI model administration endpoints
  - Model CRUD operations
  - Tag assignment and removal
  - Model search and filtering

- **TagOperationsController**: Tag management endpoints
  - Tag CRUD operations
  - Tag retrieval and search

- **InferenceOperationsController**: Inference engine integration endpoints
  - Model installation with progress streaming
  - Model information retrieval
  - Model management operations

- **HealthController**: Health check endpoints
  - Service health status
  - Dependency health status

### Validators (`Controllers/Validators/`)

- **UserAdminValidators**: Validators for user operations
  - CreateUserRequestValidator
  - UpdateUserProfileRequestValidator
  - ChangeUserStatusRequestValidator
  - AssignRoleRequestValidator

- **AIModelOperationsValidator**: Validators for model operations
  - CreateModelRequestValidator
  - UpdateModelRequestValidator
  - ModelTagOperationRequestValidator
  - SearchModelRequestValidator

- **CreateRoleRequestValidator**: Validator for role creation

### Key Features
- RESTful API design with appropriate HTTP methods
- Comprehensive request validation with FluentValidation
- Consistent error responses with appropriate status codes
- Swagger documentation with JWT support
- Streaming capability for long-running operations
- CORS configuration for frontend integration

---

**This plan ensures AdminService remains simple, modular, and ready for future growth, following best practices for modern .NET microservices.**