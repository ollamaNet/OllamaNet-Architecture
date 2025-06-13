# Sequence Diagram Checklist for ConversationService

## Key Flows to Document ✅

- [ ] **Chat Message Flow**
  - User sends a chat message, receives a response (streaming and non-streaming)

- [ ] **Conversation Management Flow**
  - Create, update, delete, search conversations

- [ ] **Document Processing Flow**
  - Upload, process, and retrieve document content

- [ ] **RAG Integration Flow**
  - Document indexing and context retrieval for enhanced responses

- [ ] **Folder Management Flow**
  - Create, update, delete folders and move conversations

- [ ] **Feedback Management Flow**
  - Add, update, delete feedback on responses

- [ ] **Note Management Flow**
  - Add, update, delete notes on responses

- [ ] **Service Discovery Flow**
  - RabbitMQ message reception and configuration update
  - Dynamic InferenceEngine URL update process

- [ ] **Configuration Management Flow**
  - Configuration initialization from Redis or defaults
  - Configuration update and notification to dependents

## Required Files to Review ✅

### Chat Flow
- [ ] `Controllers/ChatController.cs` - Request handling
- [ ] `Services/Chat/ChatService.cs` - Business logic
- [ ] `Services/Chat/ChatHistoryManager.cs` - History management
- [ ] `Infrastructure/Integration/InferenceEngineConnector.cs` - LLM integration
- [ ] `Infrastructure/Configuration/InferenceEngineConfiguration.cs` - Dynamic configuration
- [ ] `Services/Rag/Implementation/RagRetrievalService.cs` - Context retrieval
- [ ] `Infrastructure/Caching/CacheManager.cs` - Cache operations

### Conversation Flow
- [ ] `Controllers/ConversationController.cs` - Request handling
- [ ] `Services/Conversation/ConversationService.cs` - Business logic
- [ ] `Controllers/Validators/ConversationValidators.cs` - Validation

### Document Processing Flow
- [ ] `Controllers/DocumentController.cs` - Request handling
- [ ] `Services/Document/Implementation/DocumentManagementService.cs` - Management
- [ ] `Services/Document/Implementation/DocumentProcessingService.cs` - Processing
- [ ] `Services/Document/Processors/` - Format-specific processors
- [ ] `Infrastructure/Document/Storage/FileSystemDocumentStorage.cs` - Storage
- [ ] `Services/Rag/Implementation/RagIndexingService.cs` - Document indexing

### Folder Management Flow
- [ ] `Controllers/FolderController.cs` - Request handling
- [ ] `Services/Folder/FolderService.cs` - Business logic
- [ ] `Controllers/Validators/CreateFolderRequestValidator.cs` - Validation
- [ ] `Controllers/Validators/UpdateFolderRequestValidator.cs` - Validation

### Feedback Flow
- [ ] `Controllers/FeedbackController.cs` - Request handling
- [ ] `Services/Feedback/FeedbackService.cs` - Business logic
- [ ] `Controllers/Validators/AddFeedbackRequestValidator.cs` - Validation
- [ ] `Controllers/Validators/UpdateFeedbackRequestValidator.cs` - Validation

### Note Flow
- [ ] `Controllers/NoteController.cs` - Request handling
- [ ] `Services/Note/NoteService.cs` - Business logic

### Service Discovery Flow
- [ ] `Infrastructure/Messaging/Consumers/InferenceUrlConsumer.cs` - Message consumer
- [ ] `Infrastructure/Messaging/Models/InferenceUrlUpdateMessage.cs` - Message model
- [ ] `Infrastructure/Messaging/Validators/UrlValidator.cs` - URL validation
- [ ] `Infrastructure/Messaging/Resilience/RabbitMQResiliencePolicies.cs` - Resilience patterns
- [ ] `Infrastructure/Configuration/InferenceEngineConfiguration.cs` - Configuration management
- [ ] `Infrastructure/Integration/InferenceEngineConnector.cs` - Configuration subscriber

## Sequence Steps to Include ✅

### For Each Flow
- [ ] **Request Receipt**
  - API controller receives request
  - Request validation
  - Authentication check

- [ ] **Service Processing**
  - Business logic handling
  - Service interactions
  - External dependencies

- [ ] **Data Operations**
  - Database interactions
  - Cache operations
  - Storage operations

- [ ] **Response Generation**
  - Response creation
  - Error handling
  - Response mapping

- [ ] **Asynchronous Operations**
  - Background processes
  - Event handling
  - Delayed operations
  - Message consumption

### For Service Discovery Flow
- [ ] **Message Reception**
  - RabbitMQ message arrival
  - Message deserialization
  - Message validation

- [ ] **Configuration Update**
  - URL validation
  - Configuration service update
  - Redis persistence

- [ ] **Notification**
  - Event emission
  - Subscriber notification
  - Connector reconfiguration

- [ ] **Error Handling**
  - Connection failures
  - Validation failures
  - Redis unavailability

## Error Paths to Include ✅

- [ ] **Validation Errors**
  - Invalid request handling
  - Validation response
  - URL validation failures

- [ ] **Authentication Errors**
  - Unauthorized access handling

- [ ] **Business Logic Errors**
  - Domain-specific error handling
  - Not found conditions

- [ ] **External Service Errors**
  - LLM service failures
  - Database failures
  - Cache failures
  - RabbitMQ connection failures
  - Redis connection failures

- [ ] **Retry Mechanisms**
  - Retry patterns for transient failures
  - Circuit breaker patterns
  - Fallback mechanisms

## Clarifying Questions ❓

1. **Flow Boundaries**
   - Where does each flow start and end? - each request start from the Controller endpoint and end with its response track in the each controller one by one to get better context finish each one before starting to the next you can find the controllers in the `ConversationService\Controllers`
   - Are there any cross-flow dependencies? - Document processing flow connects to RAG indexing flow; Service discovery flow affects Chat flow through InferenceEngineConnector

2. **Error Handling**
   - What are the key error scenarios for each flow? - returning exception classes and try-catch statements
   - How are errors propagated back to the client? - using exception classes and messages.
   - How are RabbitMQ and Redis connection failures handled? - using resilience policies and fallback mechanisms

3. **Caching Strategy**
   - When and how is caching used in each flow? for the chat we retrieve the chat history.
   - What is the cache invalidation strategy? - invalidate on update/delete operations
   - How is configuration data cached and retrieved? - stored in Redis with fallback to default values

4. **Asynchronous Processing**
   - Which operations are handled asynchronously? - document processing and indexing, message consumption
   - How are long-running operations managed? - background tasks, hosted services
   - How are message consumers registered and started? - as hosted services in the dependency injection container

5. **Performance Considerations**
   - How are streaming responses handled? - server sent events
   - How do configuration updates affect ongoing operations? - updates are applied to new requests without interrupting current operations

6. **Service Discovery**
   - How does the service discover and update InferenceEngine URLs? - through RabbitMQ messages from Admin Service
   - What happens when a new URL is received? - validated, stored in Redis, and notified to dependents
   - How are URL validation failures handled? - invalid URLs are logged and rejected

## Sequence Diagram Elements ✏️

1. **Actors**
   - Client/User
   - API Gateway
   - Admin Service

2. **System Components**
   - Controllers
   - Validators
   - Services
   - Infrastructure components
   - External systems
   - Message consumers
   - Configuration services

3. **Message Flows**
   - Synchronous calls
   - Asynchronous calls
   - Return values
   - Exceptions
   - RabbitMQ messages
   - Configuration updates

4. **Conditional Logic**
   - Decision points
   - Alternative paths
   - Error conditions
   - Fallback mechanisms

5. **Parallel Operations**
   - Concurrent processing
   - Asynchronous operations
   - Background services

## Special Focus Areas ✅

- [ ] **Streaming Chat Flow**
  - How are streaming responses managed?
  - What happens after streaming completes?

- [ ] **RAG Integration**
  - How and when is RAG context retrieved?
  - How is it integrated with the chat prompt?

- [ ] **Document Processing Pipeline**
  - How do different document processors work?
  - How is content extracted and processed?

- [ ] **Caching Strategy**
  - What is cached and when?
  - How is cache invalidation handled?

- [ ] **Service Discovery**
  - How are RabbitMQ messages consumed?
  - How are configuration updates propagated?
  - How are connection failures handled?

- [ ] **Dynamic Configuration**
  - How is configuration initialized?
  - How are updates processed and stored?
  - How are dependents notified of changes?

## Consistent Styling for Sequence Diagrams 🎨

Use the following PlantUML styling for all sequence diagrams:

```plantuml
!define ACCENT_COLOR #4285F4
!define SECONDARY_COLOR #34A853
!define WARNING_COLOR #FBBC05
!define ERROR_COLOR #EA4335

!define SUCCESS #34A853
!define WARNING #FBBC05
!define FAILURE #EA4335

skinparam ParticipantPadding 20
skinparam BoxPadding 10
skinparam SequenceArrowThickness 1
skinparam SequenceGroupHeaderFontStyle bold

skinparam sequence {
    ArrowColor #5C5C5C
    LifeLineBorderColor #CCCCCC
    LifeLineBackgroundColor #EEEEEE
    
    ParticipantBorderColor #CCCCCC
    ParticipantBackgroundColor #FFFFFF
    ParticipantFontColor #000000
    
    ActorBorderColor #CCCCCC
    ActorBackgroundColor #FFFFFF
    ActorFontColor #000000
}
```

## Additional Notes 📝

- Focus on one complete flow per diagram
- Include timing information where relevant
- Document both happy path and error paths
- Show caching operations explicitly
- Include validation steps and error handling
- Note any asynchronous operations
- Highlight important conditional logic
- Include database interactions
- Show external service calls
- Use consistent color scheme across all diagrams 
- Document message flow from Admin Service through RabbitMQ to configuration components
- Show how configuration updates affect dependent components
- Include resilience patterns for external service connections 