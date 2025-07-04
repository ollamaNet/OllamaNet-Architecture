# Active Context for ConversationService

## Current Focus
- RAG (Retrieval-Augmented Generation) Document Processing feature implemented with full support for document uploads, processing, and retrieval
- Feature enhancements: advanced search, caching optimization, conversation archiving, rate limiting, error handling, and integration testing
- Ongoing optimization of Redis caching for conversation history and streaming responses
- Planning and implementing conversation archiving strategy for data management
- Monitoring and improving background processing for post-streaming operations
- Implementing a service discovery mechanism using RabbitMQ to dynamically update the InferenceEngine URL

## Recent Changes
- Completed RAG Document Processing feature implementation:
  - Added document upload, storage, and processing capabilities
  - Implemented processors for multiple document formats (PDF, Text, Word, Markdown)
  - Enhanced RagIndexingService to handle document chunks
  - Added document metadata support for improved context retrieval
  - Implemented comprehensive security and monitoring
- Completed RAG system refactoring:
  - Split into Infrastructure and Service layers
  - Moved embedding operations to Infrastructure/Rag/Embedding
  - Moved vector database operations to Infrastructure/Rag/VectorDb
  - Reorganized RAG services in Services/Rag directory
  - Updated service registrations and dependencies
- Migration to modular, best-practices folder and namespace structure completed (Phases 1-9)
- All legacy folders removed; all files are in correct locations with consistent namespaces
- Documentation and diagrams updated to reflect the new structure
- Memory bank documentation reviewed and aligned with current architecture
- Redis caching and streaming response implementation reviewed and optimized
- Completed Service Discovery Implementation:
  - Created a RabbitMQ-based service discovery mechanism for dynamically updating the InferenceEngine URL
  - Implemented InferenceEngineConfiguration for centralized URL management
  - Added RabbitMQ message consumer to listen for URL updates
  - Integrated Redis for persistent URL storage
  - Renamed OllamaConnector to InferenceEngineConnector for better abstraction
  - Added resilience patterns with Polly for RabbitMQ connections
- Configuration Updates:
  - Updated appsettings.json with new RabbitMQ connection details:
    - Host: toucan.lmq.cloudamqp.com
    - Username: ftyqicrl
    - VirtualHost: ftyqicrl
    - Custom exchange and queue for service discovery messages
- Technical Issues Resolved:
  - Fixed CircuitState ambiguity issues with different Polly versions
  - Made Redis cache access fault-tolerant
  - Ensured proper virtual host configuration for RabbitMQ

## Current Status
- ConversationService is fully implemented and organized according to modern .NET microservice best practices:
  - Modular folder structure with domain-specific services, DTOs, and mappers
  - Comprehensive API for conversation, chat, folder, note, and feedback management
  - Document upload and processing for RAG-enhanced conversations
  - Real-time chat with streaming and non-streaming endpoints
  - Redis-based distributed caching with Upstash
  - Server-Sent Events for streaming AI responses
  - JWT authentication and role-based authorization
  - FluentValidation for request validation
  - OllamaConnector integration for AI model inference
  - Error handling and logging implemented
  - RAG system with Pinecone vector database integration
- Documentation and diagrams are up to date and reflect the current architecture

## Active Decisions
- Continue with feature enhancements: advanced search, caching optimization, archiving, rate limiting, and integration testing
- Maintain modular structure and best practices for all new features
- Monitor and optimize performance and error handling
- Temporarily disabled query cleaning functionality in RAG system for evaluation
- Document processing metrics monitoring for performance optimization
- Chose RabbitMQ over alternatives for service discovery due to its reliability and existing infrastructure
- Implemented both Redis persistence and RabbitMQ messaging for complete solution
- Renamed components from Ollama-specific to more generic InferenceEngine for flexibility
- Made Redis cache access optional to improve system resilience

## Implementation Insights
- RAG Document Processing feature enables document-enhanced conversations
- RAG system follows clean architecture principles with clear separation of concerns
- Document processors support multiple file formats with extensible design
- ChatHistoryManager integrates caching and database for efficient history retrieval
- Modular structure enables clear separation of concerns and maintainability
- RedisCacheSettings and CacheManager provide domain-specific caching strategies
- Streaming implementation leverages IAsyncEnumerable and Response.BodyWriter
- Error handling and logging are centralized and consistent

## Next Steps
- Monitor and optimize RAG Document Processing performance
- Re-evaluate and potentially re-enable query cleaning functionality in RAG system
- Implement advanced search functionality with database-level search
- Optimize caching for large conversation histories
- Develop and implement conversation archiving mechanism
- Add rate limiting to chat endpoints
- Enhance error handling for Redis and external service failures
- Expand integration testing for critical paths
- Monitor and improve background processing for streaming operations
- Testing and Verification:
  - Verify dynamic URL updates through RabbitMQ
  - Test system resilience when RabbitMQ or Redis are unavailable
  - Ensure proper fallback to configuration values
- Documentation:
  - Update sequence diagrams to include the new service discovery flow
  - Document the RabbitMQ message format and routing strategy
- Future Enhancements:
  - Add monitoring and metrics for service discovery health
  - Implement additional validation for URL updates
  - Consider extending the system to other dynamic configuration parameters

## Open Questions
- How effective is the document chunking strategy for different document types?
- What additional document formats should be supported in the future?
- Should query cleaning functionality be re-enabled in the RAG system?
- What is the optimal approach for implementing full-text search for conversations?
- How should conversation history be pruned or archived for long-running conversations?
- What metrics should be collected for monitoring service performance?
- What rate limiting strategy would be most effective for chat endpoints?
- How can background processing tasks be monitored and managed effectively?

## Current Context
The ConversationService is now a fully modular, best-practices .NET microservice providing conversation management, real-time chat, content organization, and feedback collection for the OllamaNet platform. The recent implementation of the RAG Document Processing feature has enhanced the service with document upload, processing, and retrieval capabilities for improved context in AI conversations. The service is clean, maintainable, and ready for future enhancements, with all documentation and diagrams up to date. The current focus is on feature enhancements and performance optimization.

## Important Files
- `Infrastructure/Configuration/InferenceEngineConfiguration.cs`: Manages InferenceEngine URL
- `Infrastructure/Messaging/Consumers/InferenceUrlConsumer.cs`: RabbitMQ message consumer
- `Infrastructure/Messaging/Extensions/MessagingExtensions.cs`: Service registration
- `Infrastructure/Integration/InferenceEngineConnector.cs`: Updated connector (formerly OllamaConnector)
- `appsettings.json`: Updated with RabbitMQ configuration