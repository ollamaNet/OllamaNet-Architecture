# Active Context for ConversationService

## Current State

The ConversationService is fully operational with all core features implemented. The codebase has been completely refactored into a modular, best-practices folder structure with proper separation of concerns. All legacy code has been removed, and the service now follows a clean architecture pattern with distinct layers for API, Service, Infrastructure, and Data Access.

## Recent Developments

### RAG Document Processing

The RAG (Retrieval-Augmented Generation) Document Processing feature has been fully implemented and is working as expected. This feature allows users to upload documents, which are then processed, chunked, embedded, and stored in Pinecone for later retrieval during conversations. The system supports multiple document formats including PDF, Word, Text, and Markdown.

### Refactoring Efforts

- **Modular Structure**: The codebase has been completely reorganized into a clean, modular structure with proper separation of concerns.
- **Service Implementations**: All services now follow a consistent pattern with interfaces and implementations.
- **Caching Strategy**: A comprehensive caching strategy has been implemented using Redis with domain-specific TTLs.
- **API Endpoints**: All API controllers have been refactored to follow RESTful principles.
- **RAG System**: The RAG system has been modularized with clear separation between document processing, embedding generation, and retrieval.

### Service Discovery

A robust service discovery mechanism has been implemented using RabbitMQ. This allows the ConversationService to dynamically update its configuration when the InferenceEngine URL changes. The implementation includes:

- RabbitMQ message consumer
- Redis persistence for configuration values
- Resilience patterns for handling connection issues
- Fallback mechanisms for service unavailability

## Working Features

- ✅ Conversation CRUD operations
- ✅ Real-time chat with streaming responses
- ✅ Folder organization
- ✅ Note management
- ✅ Feedback collection
- ✅ Document upload and RAG integration
- ✅ Caching with Redis
- ✅ Service discovery with RabbitMQ

## In-Progress Tasks

- 🔄 Performance optimization for RAG retrieval
- 🔄 Re-evaluation of query cleaning approach
- 🔄 Enhanced document chunking strategies
- 🔄 Improved caching for frequently accessed data

## Pending Features

- ⏳ Support for additional document formats
- ⏳ Advanced semantic search across conversations
- ⏳ Rate limiting implementation
- ⏳ Comprehensive integration testing

## Known Issues

- ⚠️ Query cleaning is currently disabled due to performance concerns
- ⚠️ Search functionality is limited to basic term matching
- ⚠️ Caching strategy needs optimization for certain scenarios

## Recent Technical Decisions

- **Vector Database**: Pinecone was chosen for its performance and ease of integration.
- **Chunking Strategy**: Documents are chunked with a 1000-token size and 200-token overlap for optimal retrieval.
- **Embedding Model**: Using InferenceEngine's embedding endpoint for consistency across the platform.
- **Service Discovery**: RabbitMQ was selected for its reliability and support for pub/sub patterns.
- **Caching TTLs**: Domain-specific TTLs based on data volatility (conversations: 30 min, folders: 1 hour, etc.).

## Current Architecture

The ConversationService follows a modular, layered architecture:

### API Layer
- Controllers for each domain entity (Conversation, Chat, Folder, Note, Document, Feedback)
- DTOs for request/response objects
- Middleware for authentication, logging, and error handling

### Service Layer
- Business logic implementations
- Interface-based design for testability
- Domain-specific services for each feature area

### Infrastructure Layer
- External service integrations (InferenceEngine, Pinecone)
- Caching implementation with Redis
- Document processing pipeline
- Service discovery with RabbitMQ

### Data Access Layer
- Entity Framework Core repositories
- Domain models and configurations
- Database migrations

## Next Steps

1. **Performance Optimization**: Focus on improving RAG retrieval performance and caching strategy.
2. **Feature Expansion**: Implement advanced search capabilities and support for additional document formats.
3. **Testing**: Expand the integration testing suite to cover more scenarios.
4. **Monitoring**: Implement comprehensive monitoring and telemetry.
5. **Documentation**: Update API documentation and developer guides.