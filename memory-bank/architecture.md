"Creating architecture.md file" 

# OllamaNet Architecture

This document provides a comprehensive view of the OllamaNet platform architecture, including system context, components, and service relationships.

## System Context Diagram

The following diagram shows the OllamaNet platform in context with its external systems and users:

```plantuml
@startuml OllamaNet_Context
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

title "OllamaNet - System Context Diagram"

Person(end_user, "End User", "User of the OllamaNet platform")
Person(administrator, "Administrator", "System administrator")
System_Ext(frontend_app, "Web Frontend", "Client application that provides user interface")
System_Ext(ollama_api, "Ollama API", "External LLM inference engine")
System_Ext(vector_db, "Pinecone Vector DB", "Stores and retrieves vector embeddings for RAG")

System(ollamanet_platform, "OllamaNet Platform", "Microservices platform for Ollama integration")

Rel(end_user, frontend_app, "Uses", "HTTPS")
Rel(administrator, frontend_app, "Manages via", "HTTPS")
Rel(frontend_app, ollamanet_platform, "Sends requests to", "HTTPS/REST")
Rel(ollamanet_platform, ollama_api, "Sends prompts, receives completions", "HTTP/REST")
Rel_Left(ollamanet_platform, vector_db, "Stores and retrieves embeddings", "HTTP/REST")

note right of ollamanet_platform
  Provides:
  - Conversation capabilities
  - User authentication
  - Admin controls
  - Model discovery
  - Document processing
end note

@enduml
```

## Container Diagram

The following diagram shows the internal containers (services) that make up the OllamaNet platform:

```plantuml
@startuml OllamaNet_Container
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title "OllamaNet - Container Diagram"

Person(end_user, "End User", "User of the OllamaNet platform")
Person(administrator, "Administrator", "System administrator")
System_Ext(frontend_app, "Web Frontend", "Client application")
System_Ext(ollama_api, "Ollama API", "External LLM inference engine")
System_Ext(vector_db, "Pinecone", "Vector database")

System_Boundary(ollamanet_platform, "OllamaNet Platform") {
    Container(gateway, "Gateway Service", ".NET 9, Ocelot", "Routes requests to appropriate microservices")
    Container(auth_service, "AuthService", ".NET 9", "Handles user authentication and authorization")
    Container(conversation_service, "ConversationService", ".NET 9", "Manages conversations and chat")
    Container(admin_service, "AdminService", ".NET 9", "Provides administrative capabilities")
    Container(explore_service, "ExploreService", ".NET 9", "Enables model discovery and browsing")
    ContainerDb(sql_db, "SQL Database", "SQL Server", "Stores application data")
    ContainerDb(redis_cache, "Redis Cache", "Upstash Redis", "Distributed caching")
}

Rel(end_user, frontend_app, "Uses", "HTTPS")
Rel(administrator, frontend_app, "Manages via", "HTTPS")
Rel(frontend_app, gateway, "Sends requests to", "HTTPS/REST")

Rel(gateway, auth_service, "Routes auth requests to", "HTTP/REST")
Rel(gateway, conversation_service, "Routes conversation requests to", "HTTP/REST")
Rel(gateway, admin_service, "Routes admin requests to", "HTTP/REST")
Rel(gateway, explore_service, "Routes explore requests to", "HTTP/REST")

Rel_Down(auth_service, sql_db, "Reads/writes user data", "Entity Framework")
Rel_Down(auth_service, redis_cache, "Caches tokens", "StackExchange.Redis")
Rel_Down(conversation_service, sql_db, "Reads/writes conversation data", "Entity Framework")
Rel_Down(conversation_service, redis_cache, "Caches conversation history", "StackExchange.Redis")
Rel_Down(admin_service, sql_db, "Reads/writes admin data", "Entity Framework")
Rel_Down(admin_service, redis_cache, "Caches admin data", "StackExchange.Redis")
Rel_Down(explore_service, sql_db, "Reads model data", "Entity Framework")
Rel_Down(explore_service, redis_cache, "Caches model metadata", "StackExchange.Redis")

Rel(conversation_service, ollama_api, "Sends prompts, receives completions", "HTTP/REST")
Rel(admin_service, ollama_api, "Manages models", "HTTP/REST")
Rel(conversation_service, vector_db, "Stores/retrieves embeddings", "HTTP/REST")

@enduml
```

## Service Interaction Diagram

This diagram shows how the services interact during a typical user conversation flow:

```plantuml
@startuml OllamaNet_Service_Interaction
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Dynamic.puml

LAYOUT_WITH_LEGEND()

title "OllamaNet - Service Interaction (Conversation Flow)"

Actor(end_user, "End User")
Container(frontend, "Web Frontend", "React SPA")
Container(gateway, "API Gateway", "Ocelot")
Container(auth_service, "Auth Service", ".NET 9")
Container(conversation_service, "Conversation Service", ".NET 9")
Container(explore_service, "Explore Service", ".NET 9")
ContainerDb(redis_cache, "Redis Cache", "Upstash")
ContainerDb(sql_db, "SQL Database", "SQL Server")
System_Ext(ollama_api, "Ollama API", "LLM Inference")

' Authentication flow
Rel(end_user, frontend, "1. Log in with credentials")
Rel(frontend, gateway, "2. POST /auth/login")
Rel(gateway, auth_service, "3. Forward auth request")
Rel(auth_service, sql_db, "4. Validate credentials")
Rel(auth_service, gateway, "5. Return JWT token")
Rel(gateway, frontend, "6. Return token to client")

' Model discovery flow
Rel(frontend, gateway, "7. GET /explore/models")
Rel(gateway, explore_service, "8. Forward model request")
Rel(explore_service, redis_cache, "9. Check cache for models")
Rel(explore_service, sql_db, "10. Query models (if not in cache)")
Rel(explore_service, gateway, "11. Return model data")
Rel(gateway, frontend, "12. Display model options")

' Chat flow
Rel(end_user, frontend, "13. Send message")
Rel(frontend, gateway, "14. POST /chats/stream")
Rel(gateway, conversation_service, "15. Forward message")
Rel(conversation_service, redis_cache, "16. Get conversation history")
Rel(conversation_service, ollama_api, "17. Send prompt to LLM")
Rel(ollama_api, conversation_service, "18. Stream response chunks")
Rel(conversation_service, gateway, "19. Stream response (SSE)")
Rel(gateway, frontend, "20. Display streaming response")
Rel(conversation_service, sql_db, "21. Save message history (async)")

@enduml
```

## Data Flow Diagram

This diagram illustrates the key data flows within the OllamaNet platform:

```plantuml
@startuml OllamaNet_DataFlow
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Dynamic.puml

LAYOUT_WITH_LEGEND()

title "OllamaNet - Data Flow Diagram"

Person(end_user, "End User")
System_Ext(frontend, "Frontend Application")
Container(gateway, "Gateway Service")
Container(auth, "AuthService")
Container(conversation, "ConversationService")
Container(admin, "AdminService")
Container(explore, "ExploreService")
ContainerDb(sql_db, "SQL Database")
ContainerDb(redis, "Redis Cache")
System_Ext(ollama_api, "Ollama API")
System_Ext(vector_db, "Pinecone Vector DB")

' Authentication flow
Rel(end_user, frontend, "1. Provides credentials")
Rel(frontend, gateway, "2. Authentication request")
Rel(gateway, auth, "3. Routes auth request")
Rel(auth, sql_db, "4. Validates user")
Rel(auth, gateway, "5. Returns JWT token")
Rel(gateway, frontend, "6. Returns token to frontend")

' Conversation flow
Rel(end_user, frontend, "7. Sends chat message")
Rel(frontend, gateway, "8. Message with JWT")
Rel(gateway, conversation, "9. Routes message")
Rel(conversation, redis, "10. Checks message cache")
Rel(conversation, vector_db, "11. Retrieves relevant context")
Rel(conversation, ollama_api, "12. Sends prompt to LLM")
Rel(ollama_api, conversation, "13. Returns completion")
Rel(conversation, sql_db, "14. Stores message history")
Rel(conversation, gateway, "15. Streams response")
Rel(gateway, frontend, "16. Streams to user")

@enduml
```

## Deployment Architecture

This diagram illustrates how the OllamaNet services are deployed:

```plantuml
@startuml OllamaNet_Deployment
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Deployment.puml

LAYOUT_WITH_LEGEND()

title "OllamaNet - Deployment Diagram"

Deployment_Node(developer_machine, "Developer Machine", "Windows/Linux") {
    Container(frontend_dev, "Frontend Application", "React, npm")
}

Deployment_Node(azure_cloud, "Azure Cloud", "Production/Staging") {
    Deployment_Node(web_app, "Azure Web App Service", "Windows") {
        Deployment_Node(gateway_node, "Gateway Service", ".NET 9") {
            Container(gateway_container, "Gateway Service", ".NET, Ocelot")
        }
        Deployment_Node(auth_node, "Auth Service", ".NET 9") {
            Container(auth_container, "Auth Service", ".NET")
        }
        Deployment_Node(admin_node, "Admin Service", ".NET 9") {
            Container(admin_container, "Admin Service", ".NET")
        }
        Deployment_Node(explore_node, "Explore Service", ".NET 9") {
            Container(explore_container, "Explore Service", ".NET")
        }
        Deployment_Node(conversation_node, "Conversation Service", ".NET 9") {
            Container(conversation_container, "Conversation Service", ".NET")
        }
    }
    
    Deployment_Node(azure_sql, "Azure SQL Database") {
        ContainerDb(sql_db, "OllamaNet Database", "SQL Server")
    }
}

Deployment_Node(upstash_cloud, "Upstash Cloud") {
    ContainerDb(redis_cache, "Redis Cache", "Upstash Redis")
}

Deployment_Node(pinecone_cloud, "Pinecone Cloud") {
    ContainerDb(vector_db, "Vector Database", "Pinecone")
}

Deployment_Node(ollama_server, "Ollama Server", "Cloud VM") {
    Container(ollama_service, "Ollama API", "Ollama")
    Container(ngrok, "ngrok", "Tunnel")
}

Rel(developer_machine, web_app, "Deploys to", "Continuous Deployment")
Rel(gateway_container, auth_container, "Routes requests to")
Rel(gateway_container, admin_container, "Routes requests to")
Rel(gateway_container, explore_container, "Routes requests to")
Rel(gateway_container, conversation_container, "Routes requests to")

Rel_Down(auth_container, sql_db, "Reads/writes")
Rel_Down(admin_container, sql_db, "Reads/writes")
Rel_Down(explore_container, sql_db, "Reads/writes")
Rel_Down(conversation_container, sql_db, "Reads/writes")

Rel(auth_container, redis_cache, "Caches data")
Rel(admin_container, redis_cache, "Caches data")
Rel(explore_container, redis_cache, "Caches data")
Rel(conversation_container, redis_cache, "Caches data")

Rel(conversation_container, vector_db, "Stores/retrieves embeddings")
Rel(conversation_container, ngrok, "Sends prompts to")
Rel(admin_container, ngrok, "Manages models via")
Rel(ngrok, ollama_service, "Forwards requests to")

@enduml
```

## Key Architecture Decisions

### ADR 1: Microservices Architecture

**Context**: The OllamaNet platform needs to support various functionalities including conversation management, authentication, administration, and model exploration.

**Decision**: Implement a microservices architecture with separate services for different domains.

**Rationale**:
- Enables independent deployment and scaling of services
- Supports domain-specific development and optimization
- Allows technology choices appropriate for each domain
- Improves fault isolation and resilience

### ADR 2: Shared Database

**Context**: The microservices need to share data and maintain consistency.

**Decision**: Use a shared database approach with a common data layer (Ollama_DB_layer) but separate repositories for each service.

**Rationale**:
- Simplifies data consistency challenges
- Avoids complexity of distributed transactions
- Allows efficient data access through shared schemas
- Maintains separation through repository interfaces

### ADR 3: Caching Strategy

**Context**: Services need to minimize database load and improve response times.

**Decision**: Implement a two-tier Redis caching architecture with domain-specific TTL values.

**Rationale**:
- Reduces database load for frequently accessed data
- Improves response times for common operations
- Provides resilience through fallback mechanisms
- Allows domain-specific caching policies

### ADR 4: Authentication Approach

**Context**: The platform needs secure authentication with centralized control.

**Decision**: Use JWT-based authentication with token validation at the Gateway level.

**Rationale**:
- Provides stateless authentication for microservices
- Centralizes authentication logic at the gateway
- Enables role-based authorization
- Supports claims forwarding to downstream services

### ADR 5: Streaming Response Pattern

**Context**: Chat interactions require real-time streaming of AI responses.

**Decision**: Implement Server-Sent Events (SSE) with IAsyncEnumerable for streaming responses.

**Rationale**:
- Provides real-time updates to users
- Reduces perceived latency
- More efficient than polling
- Native support in ASP.NET Core and modern browsers