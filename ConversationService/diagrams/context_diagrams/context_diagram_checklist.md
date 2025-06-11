# Context Diagram Checklist for ConversationService

## Required Files to Review ✅

- [x] `appsettings.json` - Contains configuration for external services
- [x] `Program.cs` - Startup configuration 
- [x] `ServiceExtensions.cs` - Service registrations and dependencies
- [x] `Infrastructure/Integration/IOllamaConnector.cs` - LLM integration interface
- [x] `Infrastructure/Integration/OllamaConnector.cs` - LLM integration implementation
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
  
- [x] AI/LLM Service
  - Generic Inference Engine (accessed through OllamaConnector)
  - Not dependent on specific framework (can use Ollama, vLLM, llama.cpp)
  
- [x] Vector Database
  - Pinecone (current implementation, can be replaced)
  
- [x] Storage Systems
  - Document Storage (Currently local file system, planned migration to cloud blob storage)
  
- [x] Client Applications
  - Web Frontend (communicates via REST API endpoints)

## Communication Patterns to Document ✅

- [x] Synchronous API Calls
  - LLM inference requests
  - Pinecone vector database queries
  - REST API endpoints for client applications
  
- [x] Asynchronous Communication
  - Document processing pipelines
  - Streaming responses for chat interactions
  
- [x] Data Storage Access
  - SQL Server database access
  - Document file storage
  - Redis cache access
  
- [x] Security Boundaries
  - Authentication handled by API Gateway
  - Role-based authorization (Admin, User)

## Clarifying Questions and Answers ❓

1. **Authentication Flow**
   - ✅ Authentication is handled by the API Gateway before requests reach the ConversationService
   - ✅ All requests that reach the service are pre-authenticated

2. **Frontend Integration**
   - ✅ Frontend communicates with the ConversationService through REST API endpoints
   - ✅ API controllers found in the Controllers directory handle client requests

3. **External AI Services**
   - ✅ The OllamaConnector is a client class that interfaces with various LLM providers/inference engines
   - ✅ Service is not dependent on Ollama specifically, can use other frameworks like vLLM or llama.cpp

4. **Document Storage**
   - ✅ Currently using local file system storage
   - ✅ Future plans to migrate to cloud blob storage

5. **Vector Database**
   - ✅ Pinecone is the current vector database implementation
   - ✅ Architecture allows for easy replacement with other vector databases

6. **Monitoring and Logging**
   - ✅ No external logging service currently used
   - ✅ Internal logging used throughout the service (as seen in ChatService.cs)

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

3. **Data Flows**:
   - Pre-authenticated requests from API Gateway
   - Conversation data flows
   - Document upload/retrieval flows
   - LLM query/response flows
   - Vector embedding flows
   - Cache read/write flows

## Boundaries to Include 🔲

1. **System Boundary**
   - ConversationService and its internal components

2. **Security Boundary**
   - API Gateway handling authentication
   - Authorization boundaries within the service

3. **User Interaction Boundary**
   - Client application interactions via REST APIs

4. **External Service Boundaries**
   - LLM inference engine
   - Storage services
   - Database services

## Additional Context Notes 📝

- The service uses RAG (Retrieval-Augmented Generation) for enhanced responses
- Document processing is handled internally with various format processors
- Authentication is handled by the API Gateway, while authorization is handled within the service
- Redis caching is used for performance optimization
- Pinecone vector database is used for semantic search
- The service is designed to be flexible with LLM providers and storage solutions 