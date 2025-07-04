# AdminService Project Brief

## Project Overview
AdminService is a critical microservice component of the OllamaNet platform that provides comprehensive administrative capabilities. It serves as the central control point for platform administrators to manage all aspects of the system, including users, AI models, tags, and inference operations. This service is designed with a clean architecture, domain-driven design, robust validation, and RESTful API principles to ensure secure and efficient platform administration.

## Core Functionality
- **User Management**: Create, read, update, delete users; manage roles and permissions; control account status and security
- **AI Model Management**: Add, update, delete models; manage metadata; assign and remove tags for categorization
- **Tag Management**: Create, update, and delete tags for organizing models and content
- **Inference Operations**: Install, uninstall, and manage AI models through Ollama integration with progress streaming

## Technical Stack
- **ASP.NET Core Web API (.NET 9.0)**: Core framework for RESTful API development
- **Entity Framework Core**: ORM for data access through Ollama_DB_layer
- **SQL Server**: Primary database for persistence (hosted on Database ASP.NET)
- **Redis**: Caching capability using Upstash (configured with domain-specific TTLs)
- **OllamaSharp**: Client library for Ollama integration
- **FluentValidation**: Comprehensive request validation framework
- **Swagger/OpenAPI**: Interactive API documentation and testing
- **JWT Authentication**: Security framework (configured but currently commented out)
- **Serilog**: Structured logging framework for comprehensive logging
- **MediatR**: Mediator pattern implementation for pipeline behaviors

## Architecture
The service follows a domain-driven design with clear separation of concerns:

- **Controllers Layer**: Domain-specific controllers for user, model, tag, and inference operations
- **Services Layer**: Business logic organized by domain with proper DTOs and mappers
- **Infrastructure Layer**: Cross-cutting concerns including caching, logging, configuration, and integration
- **Validation Layer**: Domain-specific validators using FluentValidation
- **Data Access Layer**: Repository pattern via IUnitOfWork for data operations

## Project Scope
AdminService is a key component in the OllamaNet microservices architecture, providing the administrative backbone for platform management. It's designed to be consumed by administrative frontends, automation tools, and other backend services that require administrative capabilities. The service separates administrative concerns from end-user functionality, ensuring proper access control and specialized operations for platform management.

## Integration Points
- **Ollama_DB_layer**: Database access layer with repositories and entity definitions
- **InferenceEngineConnector**: Integration with the Ollama inference engine API
- **Frontend Application**: Web UI consuming the AdminService API (via CORS policy)
- **Identity System**: User authentication and authorization framework
- **Redis Cache**: Domain-specific caching for performance optimization
- **Other Microservices**: Potential integration with other platform components

## Key Requirements
- **Comprehensive Administration**: Complete control of all platform aspects
- **Secure Operations**: Role-based access control for administrative functions
- **Robust Validation**: Comprehensive validation of all incoming requests
- **RESTful API Design**: Consistent patterns across all administrative operations
- **Efficient Data Access**: Optimized database operations via repositories
- **Streaming Capability**: Real-time progress reporting for long-running operations
- **Error Handling**: Global exception handling with appropriate status codes
- **Ollama Integration**: Seamless connection to the Ollama AI model engine
- **Domain Separation**: Clear organization of code by domain
- **Configuration Management**: No hard-coded values, using options pattern

## Migration Plan
The service is undergoing a significant architectural restructuring to improve organization, maintainability, and scalability. The migration is organized into seven phases:

1. **Preparation and Analysis**: Set up foundation and analyze existing code
2. **Infrastructure Layer Setup**: Establish cross-cutting concerns
3. **Domain Services Layer**: Implement domain-specific services
4. **Controllers and Validators**: Reorganize by domain
5. **Error Handling and Validation**: Implement consistent approach
6. **Integration and Testing**: Integrate all components and test
7. **Documentation and Cleanup**: Finalize documentation and remove obsolete components 