# OllamaNet Components - Project Brief

## Project Overview
OllamaNet Components is a comprehensive .NET-based microservices architecture that enables seamless integration with Ollama, an open-source large language model framework. The platform provides a complete solution for AI model management, conversation capabilities, user authentication, administration, and model exploration through a collection of specialized microservices working together in a distributed system.

## Core Components
- **Gateway Service**: API gateway using Ocelot for unified access to all microservices with routing, authentication, and load balancing
- **Conversation Service**: Manages all aspects of user conversations with AI models including real-time chat and streaming responses
- **Auth Service**: Handles user authentication, authorization, and profile management with JWT-based security
- **Admin Service**: Provides comprehensive administrative capabilities for platform management including user, model, and tag administration
- **Explore Service**: Enables discovery and browsing of available AI models and their associated tags
- **Ollama_DB_layer**: Shared database access layer used by all services for consistent data operations
- **Service Defaults**: Common configuration and middleware for service standardization

## Project Goals
1. Create a modular, maintainable platform for interacting with Ollama AI models
2. Provide seamless conversation capabilities with real-time streaming responses
3. Enable secure user authentication and resource management
4. Offer comprehensive administration tools for platform management
5. Support efficient discovery and exploration of available AI models
6. Implement robust caching and performance optimization across all services

## Key Requirements
1. **Modular Architecture**: Independent, deployable microservices with clear responsibilities
2. **Real-time Interaction**: Streaming chat responses with low latency
3. **Comprehensive Administration**: Complete platform management capabilities
4. **Secure Authentication**: Role-based access control with JWT tokens
5. **Efficient Caching**: Redis-based distributed caching for performance optimization
6. **Model Discovery**: Easy browsing and searching of available AI models
7. **Conversation Management**: Organization, persistence, and retrieval of chat interactions
8. **Scalability**: Design for horizontal scaling and high availability

## Success Criteria
[To be defined with project owner]

## Timeline
[To be defined with project owner]

## Stakeholders
[To be defined with project owner]

## Technical Stack
- **.NET 9.0**: Modern .NET platform for building all services
- **ASP.NET Core**: Web API framework for RESTful endpoints and streaming responses
- **Entity Framework Core**: ORM for database operations through the shared Ollama_DB_layer
- **SQL Server**: Primary relational database for data persistence
- **Redis**: Distributed caching for performance optimization using Upstash
- **Ocelot**: API Gateway implementation for request routing
- **OllamaSharp**: Client library for interacting with Ollama AI models
- **JWT Authentication**: Token-based authentication implemented across all services
- **FluentValidation**: Request validation framework for input validation
- **Swagger/OpenAPI**: API documentation and interactive testing

## Integration Points
- **Ollama API**: Integration with the Ollama inference engine for AI model operations
- **Redis Cache**: Upstash-hosted Redis for distributed caching
- **SQL Server**: Hosted database for persistent storage (db19911.public.databaseasp.net)
- **Frontend Application**: Web UI consuming the microservices APIs via Gateway

## Constraints
- Must maintain backward compatibility with existing routes and APIs
- Configuration changes should be tracked and reversible
- Must support both development and production environments
- Redis caching must gracefully degrade when unavailable
- Authorization must be enforced consistently across all services
- Must handle both streaming and non-streaming response patterns

## Risks
[To be defined with project owner]