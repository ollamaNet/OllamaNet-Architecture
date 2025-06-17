# Controllers

This directory contains all API controllers for the AdminService, organized by domain.

## Structure

- **User/** - Controllers for user management operations
- **AIModel/** - Controllers for AI model management operations
- **Tag/** - Controllers for tag management operations
- **Inference/** - Controllers for inference engine operations
- **Validators/** - FluentValidation validators for request models, organized by domain

## Responsibilities

Controllers in this directory are responsible for:
- Handling HTTP requests and responses
- Validating incoming requests using FluentValidation
- Delegating business logic to appropriate services
- Returning appropriate HTTP status codes and responses
- Maintaining API contracts for backward compatibility 