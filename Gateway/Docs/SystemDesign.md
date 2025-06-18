# Gateway System Design Plan

## 1. Architecture Overview

The Gateway service acts as the central entry point for all client requests to the OllamaNet Components ecosystem. It implements the API Gateway pattern using Ocelot to route requests to various microservices while providing cross-cutting concerns like authentication, authorization, and request/response transformation.

```
Gateway/
│
├── Configurations/                # Service-specific route configurations
│   ├── Admin.json                 # Admin service routes
│   ├── Auth.json                  # Auth service routes
│   ├── Conversation.json          # Conversation service routes
│   ├── Explore.json               # Explore service routes
│   ├── Global.json                # Global gateway settings
│   ├── RoleAuthorization.json     # Role-based access control mappings
│   └── ServiceUrls.json           # Centralized service URL definitions
│
├── Middlewares/                   # Custom middleware components
│   ├── ClaimsToHeaderMiddleware.cs # Forwards user claims to downstream services
│   └── RoleAuthorizationMiddleware.cs # Enforces role-based access control
│
├── Models/                        # Data models
│   └── AuthorizationModels.cs     # Authorization-related models
│
├── Services/                      # Service implementations
│   └── ConfigurationLoader/       # Configuration management
│       ├── ConfigurationChangeMonitor.cs # Monitors configuration file changes
│       ├── ConfigurationLoader.cs # Loads and processes configuration files
│       └── CorsSettings.cs        # CORS configuration model
│
├── Diagrams/                      # Architecture and flow diagrams
│   ├── ContextDiagram/            # System context diagrams
│   └── SequenceDiagram/           # Request flow sequence diagrams
│
├── Docs/                          # Documentation
│   └── SystemDesign.md            # This document
│
├── Gateway-memory-bank/           # Project context and documentation
│
├── Program.cs                     # Application entry point
├── ServiceExtenstions.cs          # Service registration extensions
├── combined_ocelot_config.json    # Generated combined configuration
└── Gateway.csproj                 # Project file
```

---

## 2. Component Responsibilities

### Configurations

- **Service-Specific JSON Files**: Each microservice has its own configuration file (Admin.json, Auth.json, etc.) containing route definitions.
- **ServiceUrls.json**: Centralized definition of service URLs, used for variable substitution.
- **Global.json**: Global gateway settings like base URL.
- **RoleAuthorization.json**: Maps API paths to required roles for authorization.

### Middlewares

- **ClaimsToHeaderMiddleware**: Extracts claims from authenticated users and forwards them as HTTP headers to downstream services.
- **RoleAuthorizationMiddleware**: Enforces role-based access control based on the mappings in RoleAuthorization.json.

### Services

- **ConfigurationLoader**: 
  - Loads and combines configuration files
  - Performs variable substitution
  - Generates the combined Ocelot configuration
- **ConfigurationChangeMonitor**: 
  - Monitors configuration files for changes
  - Triggers configuration reload when changes are detected

### Program.cs

- Configures the ASP.NET Core application
- Sets up authentication, authorization, and CORS
- Registers the Ocelot middleware and custom middlewares
- Initializes the configuration monitoring system

---

## 3. Key Features

### Modular Configuration

The Gateway uses a modular configuration approach where:
- Each service has its own configuration file
- Service URLs are defined in a central location
- Variables are used to reference service URLs
- Configuration files are combined at runtime

### Variable Substitution

The configuration system supports variable substitution using the `${variable}` syntax:
- Service URLs are defined in ServiceUrls.json
- Routes reference these variables (e.g., `${Services.Auth.Host}`)
- Variables are replaced at runtime

### Dynamic Configuration Reloading

The Gateway monitors configuration files for changes:
- FileSystemWatcher detects file changes
- ConfigurationChangeMonitor triggers configuration reload
- Changes are applied without requiring service restart (with some limitations)

### Authentication and Authorization

The Gateway handles authentication and authorization:
- JWT Bearer token authentication
- Role-based authorization using RoleAuthorizationMiddleware
- Claims forwarding to downstream services using ClaimsToHeaderMiddleware

### CORS Configuration

The Gateway provides flexible CORS configuration:
- Configurable allowed origins, methods, and headers
- Support for credentials
- Environment-specific settings

---

## 4. Request Flow

1. Client sends request to Gateway
2. CORS middleware processes the request
3. Authentication middleware validates JWT token (if present)
4. Authorization middleware checks role-based access
5. ClaimsToHeaderMiddleware extracts claims and adds headers
6. Ocelot middleware routes the request to the appropriate service
7. Downstream service processes the request
8. Response is returned to the client

---

## 5. Configuration System

### Configuration Loading Process

1. ConfigurationLoader.LoadAndCombineConfigurations is called
2. Service URLs are loaded from ServiceUrls.json
3. Global settings are loaded from Global.json
4. Service-specific route configurations are loaded
5. Variables are replaced in all route configurations
6. All routes are combined into a single configuration
7. Combined configuration is saved to combined_ocelot_config.json
8. Ocelot is initialized with the combined configuration

### Variable Replacement

The variable replacement process:
1. Regex pattern matches `${variable}` syntax
2. Variables are looked up in the ServiceUrls.json object
3. Variable references are replaced with actual values
4. Process is recursive to handle nested variables

### Configuration Change Detection

The configuration change detection process:
1. ConfigurationChangeMonitor creates a FileSystemWatcher
2. FileSystemWatcher monitors the Configurations directory
3. When a file changes, the OnChanged event is triggered
4. The configuration reload action is called
5. New configuration is loaded and applied

---

## 6. Authentication and Authorization

### JWT Authentication

The Gateway uses JWT Bearer token authentication:
- JWT tokens are validated using the configured issuer, audience, and key
- Token validation parameters are configured in ServiceExtensions.cs
- Authentication is applied to routes using AuthenticationOptions

### Role-Based Authorization

The Gateway implements role-based authorization:
- RoleAuthorization.json maps API paths to required roles
- RoleAuthorizationMiddleware checks if the user has the required role
- Unauthorized requests are rejected with 401 or 403 status codes

### Claims Forwarding

The Gateway forwards user claims to downstream services:
- ClaimsToHeaderMiddleware extracts claims from the authenticated user
- Claims are added as HTTP headers (X-User-Id, X-User-Email, X-User-Role)
- Downstream services can access these headers to identify the user

---

## 7. Current Status

### Completed Features

- ✅ Modular configuration structure
- ✅ Service-specific configuration files
- ✅ Variable substitution for service URLs
- ✅ Configuration loading and combining
- ✅ JWT authentication
- ✅ Role-based authorization
- ✅ Claims forwarding
- ✅ Configuration change monitoring
- ✅ CORS configuration

### In Progress

- 🔄 Configuration validation
- 🔄 Comprehensive testing of all routes
- 🔄 Documentation updates

### Planned Features

- ⏱️ Configuration dashboard
- ⏱️ Environment-specific configuration
- ⏱️ Configuration versioning and rollback
- ⏱️ Enhanced logging and monitoring
- ⏱️ Rate limiting and circuit breaker patterns

---

## 8. Best Practices

- **Modular Configuration**: Keep configuration files small and focused on specific services.
- **Variable Substitution**: Use variables for values that might change or are used in multiple places.
- **Separation of Concerns**: Use middleware to handle cross-cutting concerns.
- **Error Handling**: Implement proper error handling for configuration loading and request processing.
- **Monitoring**: Monitor configuration changes and log important events.
- **Security**: Implement proper authentication, authorization, and claims forwarding.
- **Testing**: Test all routes and configuration changes thoroughly.

---

## 9. Extension Points

### Custom Middleware

The Gateway supports custom middleware for request/response processing:
- Add new middleware classes in the Middlewares directory
- Register middleware in Program.cs
- Use middleware to implement cross-cutting concerns

### Configuration Processors

The configuration system can be extended with custom processors:
- Add new methods to ConfigurationLoader.cs
- Implement custom logic for processing configuration files
- Use processors to implement validation, transformation, etc.

### Dashboard Integration

The planned configuration dashboard will provide extension points for:
- Custom validation rules
- Configuration templates
- Import/export functionality
- Environment promotion workflows

---

## 10. Known Issues and Limitations

1. **Hot Reload Limitations**: Configuration changes require application restart to fully apply
2. **Error Handling**: Need more robust error handling for malformed configurations
3. **Logging**: Configuration changes should be logged more extensively
4. **Security**: The configuration files need additional security measures
5. **Role Authorization Testing**: Comprehensive testing needed for role-based access control
6. **Claims Forwarding**: Need to verify all necessary claims are forwarded correctly
7. **Configuration Validation**: No validation for configuration files before loading

---

This system design document provides a comprehensive overview of the Gateway service architecture, components, and implementation details. It serves as a guide for understanding the current state of the system and planning future enhancements.