# ExploreService Project Brief

## Project Overview
ExploreService is a microservice component of the OllamaNet system that provides exploration capabilities for AI models. It allows users to browse, search, and get detailed information about available AI models and their associated tags.

## Core Functionality
- Retrieve paginated lists of available AI models
- Get detailed information about specific models
- Browse all available tags
- Find models associated with specific tags

## Technical Stack
- ASP.NET Core Web API (.NET 9.0)
- Entity Framework Core for data access
- SQL Server for persistence
- Redis for caching
- JWT authentication for security
- Swagger/OpenAPI for API documentation

## Project Scope
This service is part of a larger microservices architecture for the OllamaNet platform, which appears to be a system for managing and interacting with AI models.

## Integration Points
- Database layer (Ollama_DB_layer)
- Frontend application (referenced in CORS policy)
- Redis cache for performance optimization
- Authentication system for secure access

## Key Requirements
- High performance model discovery
- Efficient caching of frequently accessed data
- Comprehensive API documentation
- Proper error handling and logging 