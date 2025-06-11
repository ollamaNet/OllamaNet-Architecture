# ConversationService Container Diagram

## System Overview

The ConversationService is designed using a clean architecture approach with clear separation between layers. The container diagram visualizes the high-level components of the system and their interactions.

## Layer Structure

### 1. API Layer (Blue Components)
The API layer serves as the entry point for client applications and exposes RESTful endpoints:

- **Chat Controller**: Handles chat-related requests including streaming and non-streaming messages
- **Conversation Controller**: Manages conversation lifecycle operations
- **Document Controller**: Handles document upload and processing requests
- **Feedback Controller**: Manages feedback on AI responses
- **Folder Controller**: Handles folder organization operations
- **Note Controller**: Manages note-related operations
- **Request Validators**: Validates all incoming requests using FluentValidation

### 2. Service Layer (Green Components)
The service layer contains the core business logic of the application:

- **Chat Service**: Processes chat requests, interacts with LLMs, and manages streaming
- **Conversation Service**: Handles conversation CRUD operations and history
- **Document Management Service**: Manages document storage and organization
- **Document Processing Service**: Extracts text, processes, and indexes document content
- **Feedback Service**: Handles feedback operations for AI responses
- **Folder Service**: Manages folder hierarchy and organization
- **Note Service**: Handles notes attached to conversations
- **Chat History Manager**: Specialized service for conversation history
- **RAG Services**: 
  - **RAG Indexing Service**: Indexes document chunks in the vector database
  - **RAG Retrieval Service**: Retrieves relevant context for chat queries

### 3. Infrastructure Layer (Gray Components) 
The infrastructure layer provides technical capabilities to the services:

- **Cache Manager**: Manages Redis caching with fallback strategies
- **Document Storage**: Handles secure file storage operations
- **Ollama Connector**: Provides an interface to the Ollama LLM API
- **Text Embedding Generation**: Creates vector embeddings for text
- **Pinecone Service**: Interfaces with the vector database
- **Document Processors**: Format-specific processors for different document types
  - PDF Processor
  - Text Processor
  - Word Processor

### 4. Data Access Layer (Gray Components)
The data access layer manages persistence:

- **Unit of Work**: Coordinates repository operations
- **Repositories**: Data access components for each entity type
  - Conversation Repository
  - Response Repository
  - Attachment Repository
  - Folder Repository
  - Note Repository
  - Feedback Repository

## External Systems

The ConversationService interacts with several external systems:

- **SQL Database**: Persistent storage for application data
- **Redis Cache**: In-memory cache for performance optimization
- **Ollama API**: LLM provider for text generation and embeddings
- **Pinecone Vector DB**: Vector database for semantic search
- **File System**: Storage for document files

## Key Relationships

### Cross-Layer Dependencies
- API controllers depend on service layer components
- Service layer components depend on infrastructure components
- Infrastructure and services depend on data access layer

### Cross-Service Dependencies
- Chat Service uses RAG Retrieval Service to enhance responses
- Document Processing Service uses RAG Indexing Service for document integration
- Multiple services use Cache Manager for performance optimization

### External Dependencies
- Document Storage integrates with File System
- Cache Manager integrates with Redis Cache
- Ollama Connector and Text Embedding Generation connect to Ollama API
- Pinecone Service connects to Pinecone Vector DB
- Repositories connect to SQL Database

## Data Flows

### Chat Flow
1. Client → Chat Controller → Chat Service → Ollama Connector → Ollama API
2. With RAG: Chat Service → RAG Retrieval Service → Pinecone Service → Pinecone DB

### Document Processing Flow
1. Client → Document Controller → Document Management Service → Document Storage → File System
2. Document Controller → Document Processing Service → Document Processors → RAG Indexing Service → Pinecone Service → Pinecone DB

### Conversation Management Flow
Client → Conversation Controller → Conversation Service → Unit of Work → Repositories → SQL Database

## Architecture Patterns

- **Clean Architecture**: Clear separation of concerns between API, service, infrastructure, and data layers
- **Repository Pattern**: Abstraction of data persistence concerns
- **Strategy Pattern**: Document processors implement a common interface for different formats
- **Decorator Pattern**: Cache manager enhances service operations with caching capability
- **Dependency Injection**: Extensive use throughout the application
- **Unit of Work**: Coordination of database operations 