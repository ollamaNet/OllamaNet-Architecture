# Technical Context for AuthService

## Core Technologies
- **.NET 9.0**: Latest .NET platform for building the API
- **ASP.NET Core**: Web API framework and middleware
- **ASP.NET Identity**: User and role management framework
- **Entity Framework Core**: ORM for database operations
- **SQL Server**: Primary database for user and token storage
- **JWT**: Authentication and authorization mechanism
- **Redis**: Configured for potential caching (minimal current usage)
- **Swagger/OpenAPI**: API documentation and testing

## Key Dependencies
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication middleware
- **Microsoft.AspNetCore.Identity**: User and role management
- **FluentValidation**: Input validation
- **StackExchange.Redis**: Redis client (configured but minimal usage)
- **Swashbuckle.AspNetCore**: Swagger integration
- **Scalar.AspNetCore**: API schema generation
- **Ollama_DB_layer**: Database access layer with repositories and entities

## JWT Implementation
- **Token Structure**: Standard JWT with claims for identity, roles, and user ID
- **Signing**: HMAC SHA-256
- **Expiration**: Configurable duration (default 30 days)
- **Validation Parameters**:
  - Issuer validation
  - Audience validation
  - Lifetime validation
  - Signing key validation
  - ClockSkew set to zero for precise expiration
- **Claims**: Username, email, user ID, roles, jti (unique token ID)

## Refresh Token Implementation
- **Storage**: In database linked to user via ApplicationUser.RefreshTokens collection
- **Generation**: Random number via RNGCryptoServiceProvider (32 bytes converted to Base64)
- **Expiration**: 10 days from generation
- **Rotation**: 
  - New token issued on login if no active tokens exist
  - Old token revoked on refresh (RevokedOn property set)
  - One-time use implementation for security
- **Validation**: IsActive property based on expiration date and revocation status
- **Transport**: HTTP-only, secure cookies with SameSite=None
- **Cookie Properties**:
  - HttpOnly: True (not accessible via JavaScript)
  - Secure: True (HTTPS only)
  - Expires: Same as token expiration
  - IsEssential: True
  - SameSite: None (allows cross-site requests needed for microservices)

## ASP.NET Identity Configuration
- **User Store**: EntityFrameworkCore
- **Password Reset**: Default token provider
- **Default Roles**: User, Admin (seeded on application startup)
- **User Creation**: With automatic "User" role assignment
- **Password Validation**: Default ASP.NET Identity rules

## API Endpoint Security
- **Protected Endpoints**: Marked with [Authorize] attribute
- **Role-Specific Endpoints**: Marked with [Authorize(Roles = "Admin")]
- **Public Endpoints**:
  - Registration
  - Login
  - Forgot/reset password
  - Refresh token
- **Token Transport**:
  - Access Token: Bearer header
  - Refresh Token: HTTP-only cookie

## Development Environment
- **IDE**: Visual Studio or VS Code
- **Runtime**: .NET 9.0 SDK
- **Database**: SQL Server (configurable connection string)
- **Cache**: Redis instance (configurable)
- **API Testing**: Swagger UI or AuthService.http

## External Services
- **Database ASP.NET**: Hosted SQL Server instance (db19911.public.databaseasp.net)
- **Upstash**: Redis cloud service (content-ghoul-42217.upstash.io)

## Configuration
- **Connection Strings**: Database and Redis connections in appsettings.json
- **JWT Settings**:
  - Key: Base64-encoded secret key
  - Issuer: "SecureApi"
  - Audience: "SecureApiUser"
  - DurationInDays: 30

## API Endpoints
- **POST /api/auth/register**: User registration with automatic role assignment
- **POST /api/auth/login**: Authentication with token issuance
- **POST /api/auth/updateprofile**: Profile update (protected)
- **POST /api/auth/changepassword**: Password change (protected)
- **POST /api/auth/forgotpassword**: Password recovery with token generation
- **POST /api/auth/resetpassword**: Password reset with token validation
- **POST /api/auth/assignrole**: Role assignment (Admin only)
- **POST /api/auth/deassignrole**: Role removal (Admin only)
- **GET /api/auth/refreshtoken**: Token refresh using HTTP cookie
- **POST /api/auth/logout**: Logout with token revocation
- **GET /api/auth/getroles/{userId}**: Role retrieval
- **GET /api/auth**: Secure endpoint test (Admin only)

## User Creation Flow
1. Registration endpoint creates user with Identity
2. User is assigned the "User" role automatically
3. Root folder is created for the user in the database
4. JWT token and refresh token are generated and returned
5. Refresh token is stored in the database and sent as HTTP-only cookie

## Password Management
- **Change Password**: Requires old password verification
- **Forgot Password**: Generates a token via Identity's token provider
- **Reset Password**: Uses the token to validate the reset request
- **Password Rules**: Enforced by ASP.NET Identity (complexity, uniqueness)

## Security Considerations
- JWT authentication for API security
- Refresh tokens stored securely in database and transmitted via HTTP-only cookies
- Refresh token revocation on logout and refresh
- Input validation with ModelState and explicit checks
- HTTPS required in production
- Proper error handling to avoid leaking sensitive information
- Password policy enforcement via ASP.NET Identity
- Role-based authorization policies
- Clear separation between authentication and business logic 