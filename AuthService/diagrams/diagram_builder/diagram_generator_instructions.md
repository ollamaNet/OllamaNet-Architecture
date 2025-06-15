# Architecture Diagram Generation Guide for AuthService

## Overview
This guide documents the process and best practices for generating architecture diagrams for the AuthService, based on the experience of creating diagrams for microservices. It includes lessons learned and common pitfalls to avoid.

## Step-by-Step Process

### 1. Initial Analysis
- **DO**: Start by analyzing the actual codebase
- **DO**: Review Program.cs, ServiceExtensions.cs, controllers, and services
- **DON'T**: Make assumptions about the architecture without checking the code
- **DON'T**: Skip the code review phase

### 2. Diagram Types and Order
Create diagrams in this order:

1. **Context Diagram (L0)**
   - Show AuthService in relation to external systems
   - Include frontend, database, and other services
   - Show authentication boundaries
   - Indicate data flow directions

2. **Container Diagram (L1)**
   - Include all major AuthService containers
   - Show user storage, token management, and identity systems
   - Document caching mechanisms if used
   - Show external dependencies
   - Indicate primary data flows

3. **Component Architecture**
   - Show AuthController, IAuthService, and JWTManager components
   - Include ASP.NET Identity integration
   - Document refresh token mechanisms
   - Show validation components
   - Indicate service and helper relationships

4. **Class Diagram**
   - Based on actual DTOs, entities, and interfaces
   - Show ApplicationUser and RefreshToken classes
   - Include JWT configuration objects
   - Document service interfaces and implementations
   - Show relationships between entities

5. **Sequence Diagrams**
   - Show user registration flow
   - Document login and token issuance
   - Include token refresh process
   - Show password management flows
   - Document role management processes
   - Include logout and token invalidation

6. **Data Flow Diagrams**
   - Document user data flow during registration
   - Show authentication data processing
   - Illustrate token generation and validation flows
   - Include password reset data flow
   - Show role management data paths

7. **State Machine Diagrams**
   - Document user account states
   - Show token lifecycle states
   - Illustrate password reset token states
   - Include error and recovery states
   - Show session state transitions

8. **Infrastructure Diagrams**
   - Detail database architecture for users/tokens
   - Show Redis integration (if used)
   - Document ASP.NET Identity infrastructure
   - Show service registration patterns
   - Include security infrastructure

9. **Integration Diagrams**
   - Show authentication flows with other services
   - Detail frontend integration points
   - Document database connections
   - Include root folder creation process
   - Show error handling for integrations

### 3. Common Pitfalls to Avoid

#### Context Diagrams
- **Mistake**: Missing authentication consumers
- **Solution**: Check Program.cs and CORS settings for frontend and service endpoints
- **Mistake**: Incorrect data flow directions
- **Solution**: Carefully trace authentication requests and responses

#### Container Diagrams
- **Mistake**: Missing token storage components
- **Solution**: Look for RefreshToken entities and storage mechanisms
- **Mistake**: Overlooking ASP.NET Identity components
- **Solution**: Review Identity configuration in ServiceExtensions.cs

#### Component Architecture
- **Mistake**: Missing JWT validation components
- **Solution**: Check JWTManager and validation parameters
- **Mistake**: Incomplete refresh token handling
- **Solution**: Review token rotation and one-time use patterns

#### Class Diagrams
- **Mistake**: Incomplete user/token properties
- **Solution**: Review ApplicationUser and RefreshToken classes thoroughly
- **Mistake**: Missing service interfaces
- **Solution**: Check IAuthService and its methods

#### Sequence Diagrams
- **Mistake**: Oversimplifying authentication flow
- **Solution**: Include token generation, cookie setting, and role assignment
- **Mistake**: Missing refresh token rotation
- **Solution**: Document old token revocation and new token issuance

#### Data Flow Diagrams
- **Mistake**: Incomplete token flow
- **Solution**: Follow token from generation through validation and revocation
- **Mistake**: Missing password reset flow steps
- **Solution**: Document token generation, validation, and password update

#### State Machine Diagrams
- **Mistake**: Missing token states
- **Solution**: Include active, expired, and revoked states
- **Mistake**: Incomplete password reset states
- **Solution**: Show token generated, validated, and consumed states

### 4. Best Practices

#### Code Review
1. Start with Program.cs for service registration patterns
2. Review ServiceExtensions.cs for dependencies and configuration
3. Check AuthController for API endpoints and flows
4. Review IAuthService and implementation for business logic
5. Examine JWTManager for token handling

#### Diagram Creation
1. Use consistent naming conventions across all diagrams
2. Add notes explaining JWT and refresh token mechanisms
3. Show proper security boundaries
4. Include error paths for authentication failures
5. Document cookie handling for refresh tokens

#### Validation
1. Cross-reference with actual code
2. Verify token handling matches implementation
3. Ensure all authentication flows are documented
4. Check that role management is correctly shown
5. Verify integration points with other services

### 5. Tools and Setup

#### Required Tools
1. PlantUML
   - Download from https://plantuml.com/download
   - Place JAR in consistent location
   - Update batch file path

2. Batch File Setup
   - Use compile_diagrams.bat
   - Set proper PlantUML path
   - Ensure all diagram directories exist
   - Include error handling

#### Directory Structure
```
diagrams/
├── compiled/                     # Generated PNG files
├── context_diagrams/             # System context diagrams
├── container_diagrams/           # Container diagrams
├── component_diagrams/           # Component architecture
├── class_diagrams/               # Class structure
├── sequence_diagrams/            # Authentication flows
├── data_flow_diagrams/           # Data flows
├── state_machine_diagrams/       # State transitions
├── infrastructure_diagrams/      # Infrastructure setup
├── integration_diagrams/         # Service integrations
├── diagram_builder/              # Generation tools
│   ├── compile_diagrams.bat      # Compilation script
│   └── tools/                    # PlantUML JAR
└── README.md                     # Usage instructions
```

### 6. Maintenance

#### Regular Updates
- Update diagrams when authentication flows change
- Review diagrams when token handling is modified
- Keep diagrams in sync with user management changes
- Version control diagram changes
- Update when integration points change

#### Documentation
- Keep diagrams in AuthService/diagrams
- Update activeContext.md when diagrams change
- Document changes in progress.md
- Maintain consistency between code and diagrams

## Specific AuthService Diagrams

### 1. Authentication Flow Sequence
```
User -> AuthController: Login Request
AuthController -> IAuthService: AuthenticateAsync
IAuthService -> UserManager: CheckPasswordAsync
UserManager -> IAuthService: Result
IAuthService -> JWTManager: GenerateJWT
JWTManager -> IAuthService: JWT Token
IAuthService -> JWTManager: GenerateRefreshToken
JWTManager -> IAuthService: Refresh Token
IAuthService -> Repository: StoreRefreshToken
IAuthService -> AuthController: AuthResponse
AuthController -> User: JWT + Refresh Token (Cookie)
```

### 2. Token Refresh Sequence
```
User -> AuthController: RefreshToken Request
AuthController -> IAuthService: RefreshTokenAsync
IAuthService -> Repository: Get RefreshToken
IAuthService -> JWTManager: ValidateRefreshToken
JWTManager -> IAuthService: Validation Result
IAuthService -> Repository: Revoke Old Token
IAuthService -> JWTManager: Generate New Tokens
JWTManager -> IAuthService: New Tokens
IAuthService -> Repository: Store New Refresh Token
IAuthService -> AuthController: New Tokens
AuthController -> User: New JWT + Refresh Token (Cookie)
```

### 3. User Registration Sequence
```
User -> AuthController: Register Request
AuthController -> IAuthService: RegisterAsync
IAuthService -> UserManager: CreateAsync(user)
UserManager -> IAuthService: Creation Result
IAuthService -> UserManager: AddToRoleAsync("User")
IAuthService -> Repository: CreateUserRootFolder
IAuthService -> JWTManager: Generate Tokens
JWTManager -> IAuthService: Tokens
IAuthService -> Repository: Store RefreshToken
IAuthService -> AuthController: Registration Result
AuthController -> User: JWT + Refresh Token (Cookie)
```

## Conclusion
Creating accurate AuthService diagrams requires:
- Understanding of JWT authentication and refresh token flows
- Knowledge of ASP.NET Identity integration
- Comprehension of role-based authorization
- Attention to security boundaries and token handling
- Clear documentation of all authentication workflows

Following this process will help create accurate, maintainable diagrams that truly reflect the AuthService architecture. 