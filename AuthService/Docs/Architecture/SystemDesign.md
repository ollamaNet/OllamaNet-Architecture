# AuthService System Design

## 1. Folder Structure (Current)

```
AuthService/
│
├── Controllers/                # API endpoints (AuthController.cs)
├── DTOs/                      # Data Transfer Objects for requests/responses
│   ├── AuthModel.cs
│   ├── ChangePasswordModel.cs
│   ├── ForgotPasswordRequestModel.cs
│   ├── ForgotPasswordResponseModel.cs
│   ├── RegisterModel.cs
│   ├── ResetPasswordModel.cs
│   ├── RoleModel.cs
│   ├── TokenRequestModel.cs
│   ├── UpdateProfileModel.cs
│   └── logout.cs
├── Helpers/                   # Utility classes
│   ├── JWT.cs                 # JWT configuration model
│   └── JWTManager.cs          # JWT token generation and validation
├── Infrastructure/            # Cross-cutting concerns
│   ├── DataSeeding/           # Seeding logic
│   │   ├── Interfaces/        # Seeding interfaces
│   │   ├── Options/           # Seeding configuration
│   │   └── Services/          # Seeding implementations
│   └── EmailService/          # Email sending infrastructure
│       ├── Implementation/    # Email service implementation
│       ├── Interfaces/        # Email service interfaces
│       └── Models/            # Email configuration and models
├── AuthService/               # Core business logic
│   ├── AuthService.cs         # Implementation of authentication operations
│   └── IAuthService.cs        # Interface defining authentication operations
├── Docs/                      # Documentation and system design
│   ├── Architecture/          # Architecture documentation
│   └── Features/              # Feature documentation
├── diagrams/                  # Architecture, class, and flow diagrams
├── AuthService-memory-bank/   # Project context and documentation
├── Properties/                # Project properties
├── Program.cs                 # Application entry point and configuration
├── ServiceExtensions.cs       # Service registration extensions
└── AuthService.csproj         # Project file
```

---

## 2. Folder & File Responsibilities

- **Controllers/**: Handle HTTP requests, validation, and delegate to services.
  - **AuthController.cs**: Provides endpoints for authentication operations (register, login, profile management, password management, role management, token refresh, logout).

- **DTOs/**: Data Transfer Objects for request/response models.
  - **AuthModel.cs**: Authentication response with tokens and user information.
  - **ChangePasswordModel.cs**: Model for password change requests.
  - **ForgotPasswordRequestModel.cs**: Model for password reset requests.
  - **ForgotPasswordResponseModel.cs**: Response for password reset requests.
  - **RegisterModel.cs**: User registration request model.
  - **ResetPasswordModel.cs**: Password reset request model.
  - **RoleModel.cs**: Role assignment/deassignment request model.
  - **TokenRequestModel.cs**: Login request model.
  - **UpdateProfileModel.cs**: Profile update request model.
  - **logout.cs**: Logout request model.

- **Helpers/**: Utility classes for common operations.
  - **JWT.cs**: JWT configuration options.
  - **JWTManager.cs**: JWT token generation, validation, and refresh token handling.

- **Infrastructure/**: Cross-cutting concerns.
  - **DataSeeding/**: Initial data seeding logic.
    - **Interfaces/**: Contracts for seeding operations.
    - **Options/**: Configuration for seeding operations.
    - **Services/**: Implementation of seeding operations.
  - **EmailService/**: Email sending infrastructure.
    - **Implementation/**: Email service implementation.
    - **Interfaces/**: Email service contracts.
    - **Models/**: Email configuration and templates.

- **AuthService/**: Core business logic.
  - **AuthService.cs**: Implementation of authentication operations.
  - **IAuthService.cs**: Interface defining authentication operations.

- **Docs/**: Documentation and system design.
  - **Architecture/**: Architecture documentation.
  - **Features/**: Feature documentation.

- **diagrams/**: Architecture, class, and flow diagrams.

- **AuthService-memory-bank/**: Project context and documentation.

- **Program.cs**: Application entry point, configuration, and service registration.

- **ServiceExtensions.cs**: Extension methods for organizing service registrations.

---

## 3. Best Practices

- **Naming**: Clear, descriptive, and consistent names for all files and folders.
- **DTOs**: Separate models for different request/response types.
- **Services**: Core business logic encapsulated in the AuthService implementation.
- **Dependency Injection**: All services, helpers, and infrastructure registered via DI.
- **Single Responsibility Principle**: Each class/folder has one clear purpose.
- **Options Pattern**: For all configuration (JWT, DataSeeding, EmailService).
- **Documentation**: Docs/ and memory-bank/ for architecture and implementation details.
- **Infrastructure Organization**: Cross-cutting concerns placed under Infrastructure/.
- **Extension Methods**: Service registrations organized using extension methods.
- **Cookie-Based Token Storage**: HTTP-only cookies for refresh tokens.
- **Token Rotation**: One-time use pattern for refresh tokens with revocation.
- **Secure Password Management**: Password validation, reset, and change workflows.

---

## 4. Current State Snapshot

**Domain Files:**
- AuthService/AuthService.cs: Core authentication service implementation.
- AuthService/IAuthService.cs: Interface defining authentication operations.

**Infrastructure Files:**
- Infrastructure/DataSeeding/: Role and user seeding implementation.
- Infrastructure/EmailService/: Email sending for registration and password reset.

**Helper Files:**
- Helpers/JWT.cs: JWT configuration options.
- Helpers/JWTManager.cs: JWT token generation and validation.

**Controllers:**
- Controllers/AuthController.cs: Authentication API endpoints.

**DTOs:**
- DTOs/: Various request and response models for authentication operations.

---

## 5. Authentication Architecture

The Authentication system follows a layered architecture with clear separation of concerns:

### API Layer (`Controllers/`)
- **AuthController**
  - Handles HTTP requests and responses
  - Performs input validation
  - Delegates to IAuthService
  - Manages refresh token cookies
  - Implements role-based authorization

### Service Layer (`AuthService/`)
- **IAuthService & AuthService**
  - Core business logic for authentication operations
  - User registration with role assignment
  - Authentication with token issuance
  - Profile and password management
  - Role management
  - Token refresh and revocation

### Security Layer (`Helpers/`)
- **JWTManager**
  - JWT token generation with claims
  - Token validation
  - Refresh token generation and validation
  - Cryptographic operations for secure tokens

### Infrastructure Layer
- **DataSeeding**
  - Role seeding
  - Admin user seeding
  - Retry policies for resilience
- **EmailService**
  - Password reset emails
  - Registration confirmation emails
  - Email templates and configuration

### Key Features
- **JWT Authentication**: Secure token-based authentication
- **Refresh Token Rotation**: One-time use pattern with revocation
- **Role-Based Authorization**: Admin and User role support
- **Password Management**: Change, forgot, and reset workflows
- **Profile Management**: Update user profile information
- **Secure Cookie Handling**: HTTP-only cookies for refresh tokens
- **Email Notifications**: Registration and password reset emails

### Authentication Flow
```
┌──────────────┐     ┌───────────┐     ┌───────────────────┐
│    Client    │────>│ Controller│────>│    AuthService    │
└──────────────┘     └───────────┘     └────────┬──────────┘
                                                │
                                                ▼
                                     ┌─────────────────────┐
                                     │     JWTManager      │
                                     └────────┬────────────┘
                                              │
                          ┌───────────────────┼───────────────────┐
                          ▼                   ▼                   ▼
                ┌─────────────────┐  ┌─────────────┐    ┌───────────────┐
                │   UserManager   │  │ RoleManager │    │   UnitOfWork  │
                └─────────────────┘  └─────────────┘    └───────────────┘
```

### Current Status
- Core authentication functionality implemented and operational
- JWT token generation and validation working
- Refresh token mechanism with one-time use pattern implemented
- Password management workflows (change, forgot, reset) functioning
- Role-based authorization implemented
- Email service for notifications implemented
- Data seeding for roles and admin user implemented

## 6. Email Service Architecture

The Email Service follows a clean architecture pattern with separation between interfaces and implementation:

### Infrastructure Layer (`Infrastructure/EmailService/`)
- **Interfaces**
  - `IEmailService`: Interface for email operations
- **Implementation**
  - `EmailService`: MailKit-based implementation
- **Models**
  - `EmailSettings`: Configuration for SMTP server

### Key Features
- **Template-Based Emails**: HTML templates for different email types
- **Configuration-Driven**: SMTP settings via options pattern
- **Multiple Email Types**: Registration success, password reset
- **HTML and Plain Text Support**: Flexible content formatting
- **Error Handling**: Exception handling for email failures

### Current Status
- Email service fully implemented and operational
- Templates for registration and password reset created
- Integration with authentication workflows complete

## 7. Data Seeding Architecture

The Data Seeding system follows a modular architecture with interfaces for extensibility:

### Infrastructure Layer (`Infrastructure/DataSeeding/`)
- **Interfaces**
  - `IDataSeeder`: Main seeding orchestration
  - `IRoleSeeder`: Role seeding operations
  - `IUserSeeder`: User seeding operations
- **Services**
  - `DataSeeder`: Coordinates seeding operations
  - `RoleSeeder`: Creates default roles
  - `UserSeeder`: Creates admin user
- **Options**
  - `DataSeedingOptions`: Configuration for seeding

### Key Features
- **Modular Design**: Separate seeders for different entity types
- **Configuration-Driven**: Seeding options via appsettings.json
- **Resilience Patterns**: Retry policies for seeding operations
- **Conditional Seeding**: ShouldSeedAsync for conditional execution
- **Logging**: Comprehensive logging of seeding operations

### Current Status
- Data seeding fully implemented and operational
- Role seeding for default roles (Admin, User) complete
- Admin user seeding with configuration complete
- Retry policies for resilience implemented

## 8. Future Cross-Cutting Concerns

### Caching Architecture (Planned)
- **MemoryCache**: For short-lived data
- **Redis**: For distributed caching
- **Cache Invalidation**: Strategies for keeping cache fresh
- **Cache Keys**: Standardized key generation

### Logging Architecture (Planned)
- **Centralized Logging**: Structured logging with context
- **Log Enrichment**: User, request, correlation IDs
- **Security Logging**: Authentication events, failures, suspicious activity
- **Performance Logging**: Timing for critical operations

### Health Checks (Planned)
- **Database Health**: Connection and query checks
- **External Dependencies**: Email service, cache availability
- **Self-Diagnostics**: Internal service health

### Background Jobs (Planned)
- **Token Cleanup**: Automated cleanup of expired tokens
- **Audit Logging**: Background processing of security events
- **Email Queue**: Asynchronous email sending

---

**This plan ensures AuthService remains secure, modular, and ready for future growth, following best practices for modern .NET microservices.**