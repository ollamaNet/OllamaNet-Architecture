# State Machine Diagram Checklist for ConversationService

## Key State Machines to Document ✅

- [ ] **Conversation States**
  - Conversation lifecycle states (new, active, archived, deleted)
  - Conversation ownership and sharing states

- [ ] **Document Processing States**
  - Document upload → processing → indexed → available
  - Error states and recovery paths

- [ ] **Chat Session States**
  - Session initialization, active, streaming, completed
  - Error handling states

- [ ] **RAG Processing States**
  - Document indexing states
  - Query processing states

- [ ] **InferenceEngine Configuration States**
  - Configuration initialization
  - URL update states
  - Connection states

- [ ] **RabbitMQ Consumer States**
  - Consumer lifecycle states
  - Message processing states
  - Connection states

## Required Files to Review ✅

### Conversation States
- [ ] `Services/Conversation/ConversationService.cs` - Conversation state management
- [ ] `Controllers/ConversationController.cs` - State transition API endpoints

### Document Processing States
- [ ] `Services/Document/Implementation/DocumentManagementService.cs` - Document lifecycle
- [ ] `Services/Document/Implementation/DocumentProcessingService.cs` - Processing states
- [ ] `Services/Document/DTOs/Responses/AttachmentResponse.cs` - State representations
- [ ] `Services/Document/DTOs/Responses/ProcessingResponse.cs` - Processing state info

### Chat Session States
- [ ] `Services/Chat/ChatService.cs` - Chat session management
- [ ] `Services/Chat/ChatHistoryManager.cs` - History state management
- [ ] `Controllers/ChatController.cs` - Chat API endpoints

### RAG Processing States
- [ ] `Services/Rag/Implementation/RagIndexingService.cs` - Indexing state management
- [ ] `Services/Rag/Implementation/RagRetrievalService.cs` - Retrieval state management

### InferenceEngine Configuration States
- [ ] `Infrastructure/Configuration/InferenceEngineConfiguration.cs` - Configuration state management
- [ ] `Infrastructure/Integration/InferenceEngineConnector.cs` - Connector state transitions

### RabbitMQ Consumer States
- [ ] `Infrastructure/Messaging/Consumers/InferenceUrlConsumer.cs` - Consumer state management
- [ ] `Infrastructure/Messaging/Resilience/RabbitMQResiliencePolicies.cs` - Connection state management

## State Definitions to Identify ✅

### Conversation States
- [ ] **State Properties**
  - Creation state
  - Active/Inactive state
  - Archived state
  - Deleted state (soft/hard)
  - Folder assignment state

- [ ] **State Transitions**
  - Create → Active
  - Active → Archived
  - Active/Archived → Deleted
  - Folder transitions

### Document States
- [ ] **State Properties**
  - Upload state
  - Processing state
  - Indexed state
  - Available state
  - Error state

- [ ] **State Transitions**
  - Uploaded → Processing
  - Processing → Indexed
  - Processing → Error
  - Error → Retry/Failed

### Chat Session States
- [ ] **State Properties**
  - Initializing state
  - Active state
  - Streaming state
  - Completed state
  - Error state

- [ ] **State Transitions**
  - Initialize → Active
  - Active → Streaming/Non-streaming
  - Streaming → Completed
  - Any → Error
  - Error → Recovery/Failed

### RAG States
- [ ] **State Properties**
  - Indexing states
  - Retrieval states
  - Query processing states
  - Context integration states

- [ ] **State Transitions**
  - Document → Chunked → Embedded → Indexed
  - Query → Embedded → Retrieved → Integrated

### InferenceEngine Configuration States
- [ ] **State Properties**
  - Uninitialized state
  - Initialized state
  - Updating state
  - Failed state
  - Redis connection state

- [ ] **State Transitions**
  - Uninitialized → Initialized (from default or Redis)
  - Initialized → Updating (on message received)
  - Updating → Initialized (on successful update)
  - Any → Failed (on error)
  - Failed → Recovery attempts

### RabbitMQ Consumer States
- [ ] **State Properties**
  - Starting state
  - Connected state
  - Consuming state
  - Disconnected state
  - Reconnecting state
  - Failed state

- [ ] **State Transitions**
  - Starting → Connected
  - Connected → Consuming
  - Consuming → Processing message
  - Any → Disconnected (on connection loss)
  - Disconnected → Reconnecting
  - Reconnecting → Connected/Failed
  - Failed → Recovery attempts

## Transition Triggers to Document ✅

- [ ] **User Actions**
  - API calls that trigger state changes
  - User-initiated transitions

- [ ] **System Events**
  - Automated state transitions
  - Timed transitions
  - Background processes
  - Message arrivals

- [ ] **Error Conditions**
  - Error triggers and transitions
  - Recovery paths
  - Connection failures

- [ ] **Completion Events**
  - Successful operation completions
  - Transition to final states
  - Message processing completion

- [ ] **External Events**
  - RabbitMQ messages
  - Configuration updates
  - Service discovery events

## State Guards and Conditions ✅

- [ ] **Preconditions**
  - Required conditions for state transitions
  - Validation rules
  - Connection availability

- [ ] **State Validation**
  - Rules for valid state transitions
  - Invalid state handling
  - Message validation

- [ ] **Authorization Guards**
  - Permission requirements for transitions
  - Role-based state access
  - URL validation

- [ ] **Business Rules**
  - Domain-specific rules affecting states
  - Constraint enforcement
  - Circuit breaker conditions

## Error States and Recovery ✅

- [ ] **Error State Types**
  - Transient errors
  - Permanent errors
  - Validation errors
  - Connection errors
  - Configuration errors

- [ ] **Recovery Paths**
  - Retry mechanisms
  - Fallback strategies
  - Manual intervention points
  - Circuit breaker patterns
  - Default configuration values

- [ ] **Error Notifications**
  - User notifications
  - System alerts
  - Logging requirements
  - Health monitoring

## Clarifying Questions ❓

1. **Conversation Lifecycle**
   - What are all possible states for a conversation?
   - What triggers state transitions?
   - Are there any automated state changes?

2. **Document Processing**
   - What is the complete lifecycle of a document?
   - How are processing errors handled?
   - Are there any document expiration or archival states?

3. **Chat Session Management**
   - How are chat sessions initialized and terminated?
   - What states exist during streaming vs. non-streaming?
   - How are interrupted sessions handled?

4. **RAG Processing**
   - What states exist during document indexing?
   - How is query processing state managed?
   - Are there any background processing states?

5. **Error Recovery**
   - What recovery mechanisms exist for each error state?
   - Are there automatic retries for any processes?
   - When is manual intervention required?

6. **InferenceEngine Configuration**
   - How is the configuration initialized and updated?
   - What happens when Redis is unavailable?
   - How are configuration changes propagated to dependents?

7. **RabbitMQ Consumer**
   - What is the lifecycle of a message consumer?
   - How are connection failures handled?
   - What retry policies are implemented?
   - How are circuit breaker patterns applied?

## State Machine Diagram Elements ✏️

1. **States**
   - Clear state nodes with descriptive names
   - Initial and final states
   - Error states
   - Connection states

2. **Transitions**
   - Labeled arrows showing triggers
   - Guard conditions where applicable
   - Actions performed during transitions
   - Message-triggered transitions

3. **Sub-states**
   - Hierarchical states where appropriate
   - Composite states for complex processes
   - Connection state hierarchies

4. **Notes and Annotations**
   - Important business rules
   - Implementation details
   - Error handling approaches
   - Resilience patterns

## Additional Notes 📝

- Use hierarchical state machines for complex processes
- Document both normal and exceptional flows
- Include timeouts and automatic transitions
- Note any persistent state storage mechanisms
- Include any state history requirements
- Document state initialization conditions
- Note any state synchronization requirements
- Include concurrent state handling if applicable 
- Document message consumer lifecycle states
- Include RabbitMQ connection state transitions
- Document Redis persistence for configuration states
- Include circuit breaker state transitions
- Note the relationship between configuration states and connector behavior 