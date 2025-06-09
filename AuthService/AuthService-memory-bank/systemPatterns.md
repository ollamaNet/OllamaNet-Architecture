# System Patterns for AuthService

## Architecture Overview
AuthService follows a clean, layered architecture pattern within a microservices ecosystem:

- **API Layer**: Controllers handling HTTP requests and responses (AuthController)
- **Service Layer**: Business logic encapsulation (IAuthService implementation)
- **Security Layer**: JWT token generation, validation, and refresh token handling (JWTManager)
- **Identity Layer**: User and role management (ASP.NET Identity)
- **Data Access Layer**: Repository pattern for data operations (via IUnitOfWork)
- **Infrastructure Layer**: Cross-cutting concerns (DataSeeding, Caching, Logging, Email, etc.)

## Design Patterns
- **Repository Pattern**: Abstracts data access through repository interfaces
- **Unit of Work**: Manages transactions and repository coordination (IUnitOfWork)
- **Dependency Injection**: For loose coupling and testability; all services, helpers, and infrastructure components are registered and resolved via DI
- **Options Pattern**: Strongly-typed settings with IOptions<T> for all configuration (e.g., JWT, DataSeeding)
- **Factory Pattern**: JWTManager for token generation and validation
- **Options Pattern**: Strongly-typed settings with IOptions<JWT>
- **Singleton Pattern**: For token validation and generation services
- **Single Responsibility Principle**: Each class/folder has one clear purpose
- **Cookie-Based Token Storage**: HTTP-only cookies for refresh tokens

## Folder Structure & Cross-Cutting Concerns
- All cross-cutting concerns are placed under `Infrastructure/`:
  - `DataSeeding/`: Seeding logic (Interfaces, Options, Services)
  - `Caching/`: (Future) Caching strategies/services
  - `Logging/`: (Future) Logging abstractions/services
  - `Email/`: (Future) Email sending infrastructure
- This structure ensures consistency, readability, and future extensibility.

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
- Infrastructure services (e.g., DataSeeding) are injected where needed

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
- All configuration sections use the Options pattern (IOptions<T>)
- JWT, DataSeeding, and future concerns (e.g., Caching, Logging) are strongly-typed and bound from appsettings.json
- No direct use of IConfiguration for settings that can be options-bound
- Service registrations are organized via extension methods
- Environment-specific configurations for development and production

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

## Exception Handling & Input Validation
- Structured error responses with appropriate HTTP status codes
- ModelState validation at controller level
- Explicit null/empty checks for critical fields
- Email format validation using EmailAddressAttribute
- Password validation via ASP.NET Identity
- Authentication/authorization failures handled gracefully
- Minimal error information to avoid security leaks

## Cross-Cutting Concerns
- CORS configuration for frontend integration
- Swagger documentation for API discoverability
- Authorization policies for role-based access
- Input validation across all endpoints
- Secure transport of sensitive information
- Infrastructure folder reserved for future concerns (Caching, Logging, Email, etc.)

## Data Management
- User creation with automatic role assignment
- Root folder creation for new users
- Refresh token storage and validation
- Password reset token generation and verification
- Role assignment and removal 