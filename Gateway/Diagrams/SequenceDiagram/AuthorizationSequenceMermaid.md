# Gateway Authentication and Authorization Flow (Mermaid)

This document contains the Mermaid version of the Gateway Authentication and Authorization flow sequence diagram. Mermaid provides a simpler visualization that can be rendered directly in markdown files.

## Authentication Flow (Login/Registration)

```mermaid
sequenceDiagram
    actor User as End User
    participant Gateway as Gateway API
    participant AuthService as Authentication Service
    
    User->>Gateway: POST /auth/login or /auth/register
    Gateway->>AuthService: Forward request
    AuthService->>AuthService: Validate credentials
    
    alt Successful Authentication
        AuthService-->>Gateway: Return JWT token
        Gateway-->>User: Return JWT token
    else Authentication Failed
        AuthService-->>Gateway: Return error
        Gateway-->>User: Return error
    end
```

## Authenticated Request Flow (Happy Path)

```mermaid
sequenceDiagram
    actor User as End User
    participant Gateway as Gateway API
    participant JWTAuth as JWT Authentication
    participant RoleAuth as Role Authorization Middleware
    participant ClaimsForward as Claims Forwarding Middleware
    participant TargetService as Target Service
    
    User->>Gateway: Request with JWT token in Authorization header
    Gateway->>JWTAuth: Validate JWT token
    
    alt Valid Token
        JWTAuth-->>Gateway: Token validated, claims extracted
        Gateway->>RoleAuth: Check authorization
        
        Note over RoleAuth: Load endpoint role requirements from RoleAuthorization.json
        RoleAuth->>RoleAuth: Compare user roles with required roles
        
        alt User has required role
            RoleAuth-->>Gateway: Authorization successful
            Gateway->>ClaimsForward: Forward request with user claims
            
            Note over ClaimsForward: Extract claims from context (UserId, Email, Roles)
            ClaimsForward->>ClaimsForward: Add claims as headers
            ClaimsForward-->>Gateway: Request with added headers
            
            Gateway->>TargetService: Forward request to appropriate service
            TargetService->>TargetService: Process request
            TargetService-->>Gateway: Return response
            Gateway-->>User: Return response
        else User lacks required role
            RoleAuth-->>Gateway: Return 403 Forbidden
            Gateway-->>User: 403 Forbidden
        end
    else Invalid Token
        JWTAuth-->>Gateway: Return 401 Unauthorized
        Gateway-->>User: 401 Unauthorized
    else Token Expired
        JWTAuth-->>Gateway: Return 401 Unauthorized with expired message
        Gateway-->>User: 401 Unauthorized (Token expired)
    end
```

## Service Unavailability Scenario

```mermaid
sequenceDiagram
    actor User as End User
    participant Gateway as Gateway API
    participant JWTAuth as JWT Authentication
    participant RoleAuth as Role Authorization Middleware
    participant ClaimsForward as Claims Forwarding Middleware
    participant TargetService as Target Service
    
    User->>Gateway: Request with valid JWT token
    Gateway->>JWTAuth: Validate JWT token
    JWTAuth-->>Gateway: Token validated
    
    Gateway->>RoleAuth: Check authorization
    RoleAuth-->>Gateway: Authorization successful
    
    Gateway->>ClaimsForward: Forward request with user claims
    ClaimsForward-->>Gateway: Request with added headers
    
    Gateway->>TargetService: Forward request
    Note over TargetService: Service is unavailable
    TargetService-->>Gateway: 503 Service Unavailable
    Gateway-->>User: 503 Service Unavailable
```

## Service Error Response Scenario

```mermaid
sequenceDiagram
    actor User as End User
    participant Gateway as Gateway API
    participant JWTAuth as JWT Authentication
    participant RoleAuth as Role Authorization Middleware
    participant ClaimsForward as Claims Forwarding Middleware
    participant TargetService as Target Service
    
    User->>Gateway: Request with valid JWT token
    Gateway->>JWTAuth: Validate JWT token
    JWTAuth-->>Gateway: Token validated
    
    Gateway->>RoleAuth: Check authorization
    RoleAuth-->>Gateway: Authorization successful
    
    Gateway->>ClaimsForward: Forward request with user claims
    ClaimsForward-->>Gateway: Request with added headers
    
    Gateway->>TargetService: Forward request
    TargetService->>TargetService: Process request
    TargetService-->>Gateway: 400/500 Error Response
    Gateway-->>User: Forward error response
```

## Notes

- The Mermaid diagrams provide a simplified view of the sequence flow.
- For more detailed information, refer to the PlantUML diagram and documentation.
- These diagrams can be rendered directly in GitHub and other markdown viewers that support Mermaid.