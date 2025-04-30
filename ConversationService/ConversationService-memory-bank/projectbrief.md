# ConversationService Project Brief

## Overview
The ConversationService is a microservice responsible for managing chat conversations and message history in the OllamaNet system. It provides real-time chat capabilities, message persistence, and conversation management features.

## Core Requirements

### Functional Requirements
1. Real-time chat functionality
2. Message history management
3. Conversation state persistence
4. User authentication and authorization
5. Caching for performance optimization

### Non-Functional Requirements
1. High availability and scalability
2. Low latency for real-time communication
3. Data consistency and reliability
4. Secure communication
5. Easy integration with other services

## Technical Stack
- .NET Core
- Redis for caching
- Entity Framework Core
- JWT Authentication
- Swagger for API documentation

## Key Components
1. ChatController - Handles real-time chat operations
2. ConversationController - Manages conversation lifecycle
3. ChatService - Core business logic implementation
4. HistoryManager - Message history management
5. CacheService - Caching implementation

## Integration Points
- Authentication Service
- User Service
- Frontend Applications
- Other microservices requiring chat functionality

## Success Criteria
1. Real-time message delivery under 100ms
2. 99.9% uptime
3. Support for 10,000+ concurrent users
4. Message history retrieval within 200ms
5. Successful integration with all dependent services 