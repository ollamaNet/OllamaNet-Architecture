# Technical Context – OllamaNet

## Languages & Frameworks
| Layer             | Tech                                      | Version |
|-------------------|-------------------------------------------|---------|
| Services          | .NET 8 (ASP.NET Core Web API)             | 8.x     |
| Gateway           | Ocelot                                    | 2.x     |
| Persistence       | SQL Server 2022, Entity Framework Core    | 8.x     |
| Distributed Cache | Redis (Upstash cloud)                     | 7.x     |
| Messaging         | RabbitMQ                                  | 3.x     |
| Validation        | FluentValidation                          | 11.x    |
| Auth              | Microsoft.AspNetCore.Authentication.JwtBearer | 8.x |
| Docs              | Swagger / Swashbuckle                     | 6.x     |
| CI/CD             | GitHub Actions (planned)                  | —       |

## Development Environment
1. **Prerequisites**
   - .NET SDK 8+
   - Docker Desktop (for SQL, Redis, RabbitMQ)
   - VS Code / Rider / Visual Studio 2022
2. **Clone & Boot**
   ```bash
   git clone <repo>
   cd OllamaNet_Components
   docker compose up -d sql redis rabbitmq
   dotnet build OllamaNet_Components.sln
   dotnet run --project Gateway/Gateway.csproj
   ```
3. **Environment Variables**
   | Key | Sample | Notes |
   |-----|--------|-------|
   | JWT__Secret | `super-long-random-key` | 256-bit min |
   | ConnectionStrings__Sql | `Server=localhost,1433;Database=Ollama;User Id=sa;Password=Pass@word1` | |
   | Redis__ConnectionString | `redis://localhost:6379` | |

## Service Discovery & Routing
Requests enter through the **Gateway** which uses Ocelot JSON configuration files (one per downstream service) to map incoming paths to service URLs.  Each micro-service advertises its Swagger at `/swagger/v1/swagger.json`, letting Gateway generate an aggregated OpenAPI (future enhancement).

## Testing Strategy
* **Unit Tests** – xUnit, Moq, AutoFixture.
* **Integration Tests** – Testcontainers spins up SQL + Redis in CI.
* **Contract Tests** – Pact (planned) to validate Gateway ↔ downstream.

## Deployment
1. Build Docker images per service (`Dockerfile` root).
2. Push to container registry.
3. Deploy to K8s cluster via Helm charts (charts/ directory – TBD).

## Constraints
* Single shared SQL DB simplifies joins but can become coupling point → revisit schema separation once >10 services.
* Upstash Redis free tier limits 200 MB memory and 30 req/s → upgrade for production.

---
*Last updated: {{date}}*