# Gateway Context Diagram (Mermaid Version)

This document provides a simplified Context Diagram for the Gateway project using Mermaid syntax, which can be rendered directly in Markdown files on platforms like GitHub.

## Diagram

```mermaid
graph TB
    %% External Actors
    EndUser[End User] :::actor
    Admin[Administrator] :::actor
    DevOps[DevOps Engineer] :::actor
    
    %% Gateway System
    subgraph Gateway[Gateway System]
        OcelotGateway[API Gateway\n(Ocelot)] :::component
        JWTAuth[JWT Authentication] :::component
        RoleAuth[Role Authorization\nMiddleware] :::component
        ClaimsForward[Claims Forwarding\nMiddleware] :::component
        ConfigLoader[Configuration\nLoader] :::component
        ConfigMonitor[Configuration\nChange Monitor] :::component
        
        OcelotGateway --> JWTAuth
        OcelotGateway --> RoleAuth
        OcelotGateway --> ClaimsForward
        OcelotGateway --> ConfigLoader
        ConfigMonitor --> ConfigLoader
    end
    
    %% External Systems/Microservices
    AuthService[Authentication\nService] :::cloud
    AdminService[Administration\nService] :::cloud
    ExploreService[Exploration\nService] :::cloud
    ConversationService[Conversation\nService] :::cloud
    
    %% Configuration Files
    subgraph ConfigFiles[Configuration Files]
        ServiceUrls[Service URLs] :::database
        RoleConfig[Role Authorization] :::database
        ServiceRoutes[Service Routes] :::database
    end
    
    %% Connections between actors and Gateway
    EndUser --> Gateway
    Admin --> Gateway
    DevOps --> Gateway
    DevOps --> ConfigFiles
    
    %% Connections between Gateway and external systems
    Gateway --> AuthService
    Gateway --> AdminService
    Gateway --> ExploreService
    Gateway --> ConversationService
    
    %% Configuration connections
    ConfigFiles --> Gateway
    ConfigMonitor --> ConfigFiles
    
    %% Styles
    classDef actor fill:#E2F0D9,stroke:#82B366,color:#333
    classDef component fill:#DAE8FC,stroke:#6C8EBF,color:#333
    classDef cloud fill:#F8CECC,stroke:#B85450,color:#333
    classDef database fill:#FFF2CC,stroke:#D6B656,color:#333
```

## Key Elements

### External Actors
- **End User**: Accesses services through the Gateway
- **Administrator**: Manages the system, users, and models
- **DevOps Engineer**: Monitors and configures the Gateway

### Gateway System Components
- **API Gateway (Ocelot)**: Core routing engine
- **JWT Authentication**: Handles user authentication
- **Role Authorization Middleware**: Enforces role-based access control
- **Claims Forwarding Middleware**: Forwards user claims to downstream services
- **Configuration Loader**: Loads and combines configuration files
- **Configuration Change Monitor**: Monitors for configuration changes

### External Systems/Microservices
- **Authentication Service**: Handles user authentication and authorization
- **Administration Service**: Manages system administration functions
- **Exploration Service**: Provides model exploration capabilities
- **Conversation Service**: Handles conversation interactions

### Configuration Files
- **Service URLs**: Defines service endpoints
- **Role Authorization**: Maps endpoints to required roles
- **Service Routes**: Defines routing configuration for each service

## Key Interactions

1. **User Authentication Flow**:
   - End User sends authentication request to Gateway
   - Gateway routes to Authentication Service
   - Authentication Service validates credentials and returns JWT
   - Gateway returns JWT to End User

2. **Service Request Flow**:
   - End User sends request with JWT to Gateway
   - Gateway validates JWT and checks role authorization
   - Gateway forwards user claims to appropriate service
   - Service processes request and returns response
   - Gateway returns response to End User

3. **Configuration Update Flow**:
   - DevOps Engineer updates configuration files
   - Configuration Change Monitor detects changes
   - Configuration Loader reloads configuration
   - Gateway applies new configuration