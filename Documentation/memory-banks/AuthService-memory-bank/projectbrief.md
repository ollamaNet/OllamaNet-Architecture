# AuthService Project Brief

## Project Overview
AuthService is a microservice component of the OllamaNet system responsible for user authentication and authorization. It provides comprehensive identity management functionality including user registration, login, password management, and role-based access control.

## Core Functionality
- User registration with automatic role assignment
- Secure authentication with JWT token issuance
- Password management (change, reset, forgot password flows)
- Role-based authorization with admin capabilities
- User profile management
- Refresh token functionality for persistent sessions
- Secure logout handling

## Technical Stack
- ASP.NET Core Web API (.NET 9.0)
- ASP.NET Identity for user management
- JWT authentication with refresh tokens
- Entity Framework Core for data access
- SQL Server for persistence
- Redis for token caching (configured but usage is minimal)
- Swagger/OpenAPI for API documentation
- Service Defaults for common microservice functionality

## Project Scope
This service is part of a larger microservices architecture for the OllamaNet platform. It handles all authentication and user management concerns, allowing other services to focus on their specific business domains.

## Integration Points
- Database layer (Ollama_DB_layer) for user and token storage
- Frontend application (referenced in CORS policy)
- Other microservices that require user authentication
- Service defaults for common functionality

## Key Requirements
- Secure user authentication and authorization
- Role-based access control
- Persistent sessions with refresh tokens
- Password policy enforcement
- User profile management
- Admin capabilities for role management
- Clean, maintainable service registration 