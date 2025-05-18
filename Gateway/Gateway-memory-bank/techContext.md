# Gateway Technical Context

## Technology Stack
- **Framework**: ASP.NET Core 7+
- **API Gateway**: Ocelot
- **Authentication**: JWT Bearer Tokens
- **Configuration**: JSON-based configuration
- **Hosting**: Azure Web App

## Dependencies
- **Ocelot**: Main library for API Gateway functionality
- **Newtonsoft.Json**: Used for JSON processing and variable replacement
- **Microsoft.AspNetCore.Authentication.JwtBearer**: For JWT authentication
- **System.IO.FileSystem.Watcher**: For configuration file monitoring

## Development Tools
- Visual Studio 2022 or later
- .NET Core SDK 7.0+
- Postman or similar for API testing

## Configuration System
The Gateway uses a custom configuration system with the following components:
- **Separate JSON Files**: Configuration split by service
- **Variable Substitution**: Centralized service URLs
- **ConfigurationLoader**: Custom loader to combine and process configuration files
- **FileSystemWatcher**: Monitors for configuration changes

## Authentication Flow
1. Clients authenticate against the Auth service
2. JWT tokens are included in subsequent requests
3. Gateway validates tokens using JWT Bearer middleware
4. User claims are forwarded to downstream services via headers

## Deployment
The Gateway is deployed as an Azure Web App with the following considerations:
- Multiple environments (Development, Production)
- Proper handling of configuration files
- CI/CD pipeline integration

## Technical Constraints
- Configuration files must be accessible at runtime
- File watching requires proper permissions
- Variable replacement must occur before Ocelot processes the configuration
- Configuration changes require special handling for hot reload

## Third-Party Integrations
- **Ollama API**: For model inference (via Conversation service)
- **Azure Storage**: For storing configuration backups (future feature)
- **Monitoring Solutions**: For tracking gateway performance (future feature) 