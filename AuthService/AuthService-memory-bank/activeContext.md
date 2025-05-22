# Active Context for AuthService

## Current Focus
- Completing and enhancing the memory bank documentation for AuthService
- Refining understanding of the token handling and security patterns
- Analyzing refresh token implementation and cookie management

## Recent Changes
- Memory bank documentation enhanced with implementation details
- Documentation of token handling, cookie management, and security patterns
- Accurate documentation of refresh token flow and management

## Current Status
- AuthService is fully implemented with comprehensive authentication functionality:
  - User registration with automatic role assignment and root folder creation
  - Login with JWT token issuance and HTTP-only cookie for refresh token
  - Complete password management:
    - Change password with old password verification
    - Forgot password with token generation
    - Reset password with token validation
  - Profile management
  - Role management (assign/deassign roles) with Admin restrictions
  - Token refresh mechanism with old token revocation
  - Logout functionality with token invalidation
- Swagger documentation enabled for API testing
- Role-based authorization configured (User, Admin roles)
- Input validation implemented at controller level (both ModelState and explicit checks)

## Active Decisions
- JWT-based authentication with refresh tokens for persistent sessions
- Refresh token handling via HTTP-only, secure cookies
- Refresh token rotation with one-time use pattern
- Role-based authorization with specific admin capabilities
- User management via ASP.NET Identity
- Input validation at the controller level with explicit property checks
- Root folder creation during user registration

## Next Steps
- Implement automated cleanup of expired/revoked refresh tokens
- Add comprehensive security-focused logging
- Implement rate limiting for authentication endpoints
- Move sensitive configuration to secure storage (e.g., Azure Key Vault)
- Enhance token revocation mechanism for security breach scenarios
- Audit login failures and suspicious activity
- Consider implementing Two-Factor Authentication

## Implementation Details
- Refresh tokens are stored in the ApplicationUser.RefreshTokens collection
- Refresh tokens are managed with IsActive property and RevokedOn timestamp
- Token validation includes checks for expiration and revocation
- Cookies are configured with HttpOnly, Secure, and SameSite=None flags
- Input validation combines ModelState with explicit property checks
- Password validation leverages ASP.NET Identity's built-in rules

## Open Questions
- What is the monitoring strategy for failed authentication attempts?
- How should expired/revoked refresh tokens be cleaned up from the database?
- Is there a need for additional roles beyond User and Admin?
- Should we implement more granular permissions within roles?
- What is the strategy for handling mass token revocation in security breach scenarios?
- How should password policy be enhanced beyond ASP.NET Identity defaults?
- Should we implement API rate limiting to prevent brute force attacks?

## Current Context
The AuthService provides comprehensive authentication and authorization capabilities for the OllamaNet platform. It manages user identity, sessions, and access control through a secure JWT implementation with refresh tokens stored in HTTP-only cookies. The service follows modern security practices with token rotation, explicit validation, and role-based access control. It integrates with ASP.NET Identity for user management and supports various password management workflows. 