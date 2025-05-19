# ConversationService Project Brief

## Project Overview
ConversationService is a critical microservice component of the OllamaNet platform that manages all aspects of user conversations with AI models. It provides APIs for creating, managing, and interacting with conversations, including real-time chat capabilities. The service implements both standard REST endpoints and streaming responses for AI model interactions, supporting a robust and responsive user experience for conversational AI applications.

## Core Functionality
- **Conversation Management**: Create, read, update, delete, and organize conversations
- **Real-time Chat**: Process user messages and stream AI model responses
- **Message History**: Store and retrieve conversation history for continuity
- **Folder Organization**: Support for organizing conversations in user folders
- **Note Management**: Create and manage notes associated with conversations
- **Feedback Collection**: Collect and manage user feedback on AI responses
- **Caching Strategy**: Redis-based caching for improved performance

## Technical Stack
- **ASP.NET Core Web API (.NET 9.0)**: Core framework for RESTful API development
- **Entity Framework Core**: ORM for data access through shared Ollama_DB_layer
- **SQL Server**: Primary database for persistence
- **Redis**: Distributed caching for performance optimization
- **OllamaSharp**: Client library for Ollama AI model integration
- **FluentValidation**: Request validation framework
- **Semantic Kernel**: Microsoft's framework for AI chat completion
- **JWT Authentication**: Security implementation for user authentication
- **Swagger/OpenAPI**: API documentation and testing

## Project Scope
ConversationService is a central component in the OllamaNet microservices architecture, handling all conversational interactions between users and AI models. It provides a robust API for frontend applications to manage conversations, interact with AI models, and organize content. The service implements advanced features like conversation organization, streaming responses, and sophisticated caching strategies to deliver a performant and reliable user experience.

## Integration Points
- **Ollama_DB_layer**: Shared database access layer for data persistence
- **OllamaSharp**: Integration with Ollama inference engine API
- **Authentication Service**: JWT validation and user identification
- **Redis Cache**: Distributed caching for performance optimization
- **Frontend Application**: Web UI consuming the ConversationService API
- **Other Microservices**: Admin and Explore services for model information

## Key Requirements
- **Conversation Persistence**: Reliable storage and retrieval of conversation data
- **Real-time Interaction**: Low-latency responses for chat interactions
- **Streaming Capability**: Server-sent events for real-time model responses
- **Organizational Structure**: Support for folders and conversation management
- **Performance Optimization**: Caching strategies to minimize latency
- **Scalability**: Robust design for handling increased load
- **Security**: Proper authentication and authorization checks
- **Error Handling**: Comprehensive exception management and fallback strategies

## Success Criteria
1. Real-time message delivery under 100ms
2. 99.9% uptime
3. Support for 10,000+ concurrent users
4. Message history retrieval within 200ms
5. Successful integration with all dependent services 