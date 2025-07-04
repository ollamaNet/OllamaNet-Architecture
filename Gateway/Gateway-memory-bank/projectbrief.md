# Gateway Project Brief

## Overview
The Gateway project is an API gateway service using Ocelot to route requests to various microservices in the OllamaNet Components ecosystem. It provides a unified entry point for client applications to access different services.

## Key Requirements

### Core Functionality
- Route client requests to appropriate backend microservices
- Handle authentication and authorization with role-based access control
- Support configuration management for routes and services
- Provide monitoring and logging capabilities
- Forward user claims to downstream services

### Modular Configuration
- Split monolithic configuration into service-specific files
- Implement variable-based configuration for service URLs
- Support dynamic configuration reloading
- Provide a management interface for configuration

### Microservices
The gateway needs to route requests to the following services:
- Authentication Service (Auth)
- Administration Service (Admin)
- Exploration Service (Explore)
- Conversation Service (Conversation)

### Technical Requirements
- Use Ocelot for API Gateway implementation
- Implement JWT-based authentication with role-based authorization
- Support environment-specific configurations
- Provide resilience and fallback mechanisms
- Implement claims forwarding to downstream services

## Success Criteria
- All service routes are properly configured and accessible
- Configuration is modular and maintainable
- Changes to configuration can be made without service restart
- Service URLs can be modified in a single location
- Configuration changes are properly validated
- Role-based authorization correctly restricts access to protected endpoints
- User claims are properly forwarded to downstream services

## Constraints
- Must maintain backward compatibility with existing routes
- Configuration changes should be tracked and reversible
- Must support both development and production environments