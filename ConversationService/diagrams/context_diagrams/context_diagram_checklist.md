# Context Diagram Checklist for ConversationService

## Required Files to Review ✅

- [x] `appsettings.json` - Contains configuration for external services
- [x] `Program.cs` - Startup configuration 
- [x] `ServiceExtensions.cs` - Service registrations and dependencies
- [x] `Infrastructure/Integration/IInferenceEngineConnector.cs` - LLM integration interface
- [x] `Infrastructure/Integration/InferenceEngineConnector.cs` - LLM integration implementation
- [x] `Infrastructure/Configuration/InferenceEngineConfiguration.cs` - Dynamic configuration management
- [x] `Infrastructure/Messaging/Consumers/InferenceUrlConsumer.cs` - RabbitMQ message consumer
- [x] `Infrastructure/Messaging/Models/InferenceUrlUpdateMessage.cs` - Message model
- [x] `Infrastructure/Messaging/Options/RabbitMQOptions.cs` - RabbitMQ configuration
- [x] `Infrastructure/Rag/VectorDb/` - Vector database connections
- [x] `Infrastructure/Document/Storage/` - Document storage implementations
- [x] `Infrastructure/Caching/` - Redis caching implementation
- [x] `Controllers/` - API endpoints for client applications

## External Systems to Identify ✅

- [x] Database System
  - SQL Server (DefaultConnection in appsettings.json)
  
- [x] Authentication/Identity Provider
  - Authentication handled by API Gateway (before requests reach ConversationService)
  
- [x] Caching System
  - Redis (Upstash Redis configured in appsettings.json)
  - Used for both data caching and configuration storage
  
- [x] AI/LLM Service
  - Generic Inference Engine (accessed through InferenceEngineConnector)
  - Not dependent on specific framework (can use Ollama, vLLM, llama.cpp)
  - Dynamically configured through RabbitMQ service discovery
  
- [x] Vector Database
  - Pinecone (current implementation, can be replaced)
  
- [x] Storage Systems
  - Document Storage (Currently local file system, planned migration to cloud blob storage)
  
- [x] Client Applications
  - Web Frontend (communicates via REST API endpoints)

- [x] Message Broker
  - RabbitMQ (CloudAMQP service)
  - Used for service discovery and dynamic configuration

- [x] Admin Service
  - Sends configuration updates through RabbitMQ
  - Manages service discovery

## Communication Patterns to Document ✅

- [x] Synchronous API Calls
  - LLM inference requests
  - Pinecone vector database queries
  - REST API endpoints for client applications
  
- [x] Asynchronous Communication
  - Document processing pipelines
  - Streaming responses for chat interactions
  - RabbitMQ message consumption
  - Configuration updates
  
- [x] Data Storage Access
  - SQL Server database access
  - Document file storage
  - Redis cache access
  - Redis configuration storage
  
- [x] Security Boundaries
  - Authentication handled by API Gateway
  - Role-based authorization (Admin, User)
  - URL validation for configuration updates

- [x] Message-Based Communication
  - RabbitMQ for service discovery
  - Message consumers as background services
  - Event-based configuration updates

## Clarifying Questions and Answers ❓

1. **Authentication Flow**
   - ✅ Authentication is handled by the API Gateway before requests reach the ConversationService
   - ✅ All requests that reach the service are pre-authenticated

2. **Frontend Integration**
   - ✅ Frontend communicates with the ConversationService through REST API endpoints
   - ✅ API controllers found in the Controllers directory handle client requests

3. **External AI Services**
   - ✅ The InferenceEngineConnector is a client class that interfaces with various LLM providers/inference engines
   - ✅ Service is not dependent on specific LLM implementation, can use frameworks like Ollama, vLLM, or llama.cpp
   - ✅ Connection URL is dynamically configured through RabbitMQ service discovery

4. **Document Storage**
   - ✅ Currently using local file system storage
   - ✅ Future plans to migrate to cloud blob storage

5. **Vector Database**
   - ✅ Pinecone is the current vector database implementation
   - ✅ Architecture allows for easy replacement with other vector databases

6. **Monitoring and Logging**
   - ✅ No external logging service currently used
   - ✅ Internal logging used throughout the service (as seen in ChatService.cs)

7. **Service Discovery**
   - ✅ RabbitMQ used for service discovery and dynamic configuration
   - ✅ Admin Service sends configuration updates through RabbitMQ
   - ✅ Redis used for persistent storage of configuration
   - ✅ InferenceEngine URL can be updated at runtime without service restart

## Context Diagram Elements ✏️

1. **ConversationService** (Central System)

2. **External Systems**:
   - API Gateway (Authentication)
   - SQL Database
   - Redis Cache
   - LLM Inference Engine
   - Pinecone Vector DB
   - Client Applications
   - Document Storage (File System → Cloud Storage)
   - RabbitMQ Message Broker
   - Admin Service (Configuration Management)

3. **Data Flows**:
   - Pre-authenticated requests from API Gateway
   - Conversation data flows
   - Document upload/retrieval flows
   - LLM query/response flows
   - Vector embedding flows
   - Cache read/write flows
   - Configuration message flows
   - Service discovery updates

## Boundaries to Include 🔲

1. **System Boundary**
   - ConversationService and its internal components

2. **Security Boundary**
   - API Gateway handling authentication
   - Authorization boundaries within the service
   - URL validation for configuration updates

3. **User Interaction Boundary**
   - Client application interactions via REST APIs

4. **External Service Boundaries**
   - LLM inference engine
   - Storage services
   - Database services
   - Message broker services

5. **Configuration Boundary**
   - Admin Service → RabbitMQ → ConversationService flow
   - Dynamic configuration updates

## Additional Context Notes 📝

- The service uses RAG (Retrieval-Augmented Generation) for enhanced responses
- Document processing is handled internally with various format processors
- Authentication is handled by the API Gateway, while authorization is handled within the service
- Redis caching is used for performance optimization and configuration storage
- Pinecone vector database is used for semantic search
- The service is designed to be flexible with LLM providers and storage solutions 
- RabbitMQ is used for service discovery and dynamic configuration
- InferenceEngine URL can be updated at runtime without service restart
- Redis provides persistence for configuration across service restarts
- Resilience patterns are implemented for RabbitMQ and Redis connections 