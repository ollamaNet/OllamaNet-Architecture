# System Patterns for AuthService

## Architecture Overview
AuthService follows a clean, layered architecture pattern within a microservices ecosystem:

- **API Layer**: Controllers handling HTTP requests and responses (AuthController)
- **Service Layer**: Business logic encapsulation (IAuthService implementation)
- **Security Layer**: JWT token generation, validation, and refresh token handling (JWTManager)
- **Identity Layer**: User and role management (ASP.NET Identity)
- **Data Access Layer**: Repository pattern for data operations (via IUnitOfWork)

## Design Patterns
- **Repository Pattern**: Abstracts data access through repository interfaces
- **Unit of Work**: Manages transactions and repository coordination (IUnitOfWork)
- **Dependency Injection**: For loose coupling and testability
- **Factory Pattern**: JWTManager for token generation and validation
- **Options Pattern**: Strongly-typed settings with IOptions<JWT>
- **Singleton Pattern**: For token validation and generation services
- **Cookie-Based Token Storage**: HTTP-only cookies for refresh tokens

## Component Relationships
```
Controllers → IAuthService → UserManager/JWTManager → Repositories → Database
```

- Controllers depend on the IAuthService interface
- AuthService implementation orchestrates Identity operations and token handling
- JWTManager handles token creation, validation and refresh token generation
- UserManager from ASP.NET Identity handles user operations
- Repositories provide data access for user-related entities
- RefreshToken entities are linked to ApplicationUser entities

## Authentication Flow
1. **Registration**:
   - Create user → Assign "User" role → Create root folder → Generate tokens → Return tokens
2. **Login**:
   - Validate credentials → Retrieve user → Generate tokens → Return tokens
3. **Token Refresh**:
   - Extract refresh token from cookie → Validate token → Revoke old token → Generate new tokens
4. **Logout**:
   - Extract refresh token from cookie or request body → Revoke token by setting RevokedOn property

## Cookie Management
- HTTP-only cookies prevent JavaScript access
- Secure flag ensures HTTPS-only transmission
- SameSite=None enables cross-site requests (needed for microservices)
- IsEssential=True ensures cookie functionality despite cookie policies
- Expiration set to match refresh token expiration

## Configuration Management
- JWT settings organized in appsettings.json
- Identity options configured in service registration
- Service registrations organized via extension methods
- Environment-specific configurations for development and production
- Role seeding performed on application startup

## API Design
- RESTful endpoints following REST principles
- Clear endpoint naming conventions (/api/auth/*)
- Consistent request/response patterns
- Proper HTTP status codes for different scenarios
- Input validation with ModelState and explicit property checks
- Structured error responses

## Security Patterns
- JWT authentication with proper validation parameters
- Refresh token rotation with one-time use pattern
- Old token revocation on refresh for enhanced security
- Password policy enforcement through ASP.NET Identity
- Role-based authorization with [Authorize] attributes
- Token-based user identification via claims
- Secure cookie handling for refresh tokens
- Explicit input validation at controller level

## Input Validation
- ModelState validation at controller level
- Explicit null/empty checks for critical fields
- Email format validation using EmailAddressAttribute
- Password validation via ASP.NET Identity
- Structured error responses for validation failures
- Authentication failure handling with appropriate messages

## Exception Handling
- Structured error responses with appropriate HTTP status codes
- Validation errors returned with descriptive messages
- Authentication/authorization failures handled gracefully
- Return of minimal error information to avoid security leaks
- Business logic errors returned as operation-specific messages

## Cross-Cutting Concerns
- CORS configuration for frontend integration
- Swagger documentation for API discoverability
- Authorization policies for role-based access
- Input validation across all endpoints
- Secure transport of sensitive information

## Data Management
- User creation with automatic role assignment
- Root folder creation for new users
- Refresh token storage and validation
- Password reset token generation and verification
- Role assignment and removal 