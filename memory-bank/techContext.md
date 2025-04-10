# Technical Context

## Technology Stack

### Core Technologies
- **.NET Core** - Primary development framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM and database access
- **SQL Server** - Primary database
- **Redis** - Distributed caching
- **RabbitMQ** - Message queue for asynchronous communication

### Infrastructure
- **Docker** - Containerization
- **Service Registry** - Service discovery and load balancing
- **Config Server** - Centralized configuration management
- **Monitoring Tools** - System health and performance monitoring

### Authentication & Authorization
- **JWT** - Token-based authentication
- **Identity Framework** - User management

### API Documentation
- **Swagger/OpenAPI** - API documentation and testing
- **Postman** - API testing and documentation

## Development Setup

### Local Development
1. **Prerequisites**
   - .NET Core SDK
   - Docker Desktop
   - SQL Server
   - Redis
   - RabbitMQ

2. **Development Tools**
   - Visual Studio/VS Code
   - Git
   - Postman
   - Docker

### Environment Configuration
- Development
- Staging
- Production

## Technical Constraints

### Performance Requirements
- Response time < 500ms for synchronous operations
- Asynchronous operations with progress tracking
- Scalable to handle multiple concurrent users

### Security Requirements
- HTTPS everywhere
- JWT-based authentication
- Role-based authorization
- Data encryption at rest
- Secure communication between services

### Scalability Requirements
- Horizontal scaling of services
- Load balancing
- Caching strategy
- Database sharding where needed

## Dependencies

### External Services
- LLM Inference Service
- External APIs (as needed)

### Internal Services
- Authentication Service
- Chat Services
- Explore Service
- Admin Services
- API Gateway

### Database Dependencies
- **Shared Database**
  - Single SQL Server instance
  - Entity Framework Core for data access
  - Shared data access layer
  - Unified schema management
  - Centralized data consistency

## Development Guidelines

### Code Organization
- Clean Architecture principles
- Domain-Driven Design
- SOLID principles
- Microservices best practices

### Testing Strategy
- Unit testing
- Integration testing
- End-to-end testing
- Performance testing

### Deployment Strategy
- CI/CD pipeline
- Blue-Green deployment
- Canary releases
- Feature flags 