# Project Brief: ConversationService

## Overview

The ConversationService is a critical microservice within the OllamaNet platform, responsible for managing AI-powered conversations. It provides a comprehensive set of APIs for conversation management, real-time chat, message history, folder organization, note management, feedback collection, and document processing with RAG capabilities. The service is designed to be scalable, performant, and secure, with a focus on providing a seamless user experience.

## Key Features

1. **Conversation Management**
   - Create, retrieve, update, and delete conversations
   - Search conversations by content or metadata
   - Move conversations between folders

2. **Real-time Chat**
   - Send messages to AI models
   - Receive streaming responses
   - Maintain conversation context

3. **Message History**
   - Store and retrieve conversation history
   - Paginate through message history
   - Cache frequently accessed messages

4. **Folder Organization**
   - Create, retrieve, update, and delete folders
   - Organize conversations in folders
   - Navigate folder hierarchies

5. **Note Management**
   - Add notes to conversations
   - Update and delete notes
   - Retrieve notes by conversation

6. **Feedback Collection**
   - Submit feedback on AI responses
   - Retrieve feedback for analysis
   - Track feedback metrics

7. **Document Processing with RAG**
   - Upload documents to conversations
   - Process and extract text from documents
   - Generate embeddings and index in vector database
   - Retrieve relevant context for AI responses

8. **Caching**
   - Cache conversation data
   - Cache chat history
   - Cache frequently used model information
   - Implement cache invalidation strategies

## Technology Stack

- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for data access
- **SQL Server**: Relational database
- **Redis (Upstash)**: Distributed caching
- **InferenceEngine Client**: AI model integration
- **FluentValidation**: Request validation
- **Semantic Kernel**: AI orchestration
- **Pinecone**: Vector database for RAG
- **JWT Authentication**: Security

## Integration Points

1. **InferenceEngine Service**
   - Send chat requests
   - Receive model responses
   - Generate text embeddings

2. **Identity Service**
   - Validate user authentication
   - Authorize user actions

3. **Frontend Applications**
   - Web interface
   - Mobile applications

4. **Analytics Service**
   - Send usage metrics
   - Track performance data

5. **Admin Service**
   - Receive configuration updates
   - Report service health

## Key Requirements

1. **Performance**
   - Response time < 100ms for cached data
   - Response time < 1s for new requests
   - Support for 10,000+ concurrent users

2. **Scalability**
   - Horizontal scaling capability
   - Efficient resource utilization
   - Stateless design where possible

3. **Reliability**
   - 99.9% uptime
   - Graceful degradation
   - Comprehensive error handling

4. **Security**
   - JWT authentication
   - Role-based authorization
   - Data encryption
   - Input validation

5. **Real-time Interaction**
   - Streaming responses
   - Low latency
   - Connection resilience

## Success Criteria

1. **Functional**
   - All API endpoints implemented and tested
   - Integration with InferenceEngine successful
   - Document processing pipeline operational
   - Caching strategy implemented

2. **Performance**
   - Message delivery under 100ms
   - Document processing under 30s
   - Cache hit ratio > 80%

3. **Scalability**
   - Support for 10,000+ concurrent users
   - Linear scaling with added resources

4. **Reliability**
   - 99.9% uptime
   - Successful recovery from failures

5. **User Experience**
   - Seamless conversation flow
   - Intuitive organization
   - Fast response times

## Current Status

The ConversationService is fully implemented with all core features operational. The service uses a modular, best-practices folder structure with all components properly organized. Recent work has focused on implementing the RAG document processing system, refining the caching strategy, and establishing a robust service discovery mechanism using RabbitMQ.

Current efforts are directed toward performance optimization, enhancing the RAG capabilities, and preparing for advanced search features. The service is deployed and operational, serving user requests reliably with excellent performance metrics.