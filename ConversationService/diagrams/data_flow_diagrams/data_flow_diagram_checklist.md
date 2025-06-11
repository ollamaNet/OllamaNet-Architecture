# Data Flow Diagram Checklist for ConversationService

## Key Data Flows to Document ✅

- [ ] **Document Processing Pipeline**
  - Upload → Storage → Processing → Indexing → Retrieval

- [ ] **RAG Query Flow**
  - User Query → Query Processing → Vector Search → Context Retrieval → Enhanced Prompt → LLM → Response

- [ ] **Conversation History Flow**
  - Conversation Creation → Message Storage → History Retrieval → Caching → Presentation

- [ ] **Chat Response Flow**
  - User Message → Prompt Construction → RAG Integration → LLM Query → Response Generation → Storage

## Required Files to Review ✅

### Document Processing Pipeline
- [ ] `Controllers/DocumentController.cs` - Upload handling
- [ ] `Services/Document/Implementation/DocumentManagementService.cs` - Storage coordination
- [ ] `Services/Document/Implementation/DocumentProcessingService.cs` - Processing orchestration
- [ ] `Services/Document/Processors/` - Format-specific processors
- [ ] `Infrastructure/Document/Storage/FileSystemDocumentStorage.cs` - Storage implementation
- [ ] `Services/Rag/Implementation/RagIndexingService.cs` - Document indexing
- [ ] `Infrastructure/Rag/Embedding/OllamaTextEmbeddingGeneration.cs` - Text embedding
- [ ] `Infrastructure/Rag/VectorDb/PineconeService.cs` - Vector storage

### RAG Query Flow
- [ ] `Services/Chat/ChatService.cs` - RAG integration point
- [ ] `Services/Rag/Implementation/RagRetrievalService.cs` - Context retrieval
- [ ] `Services/Rag/Helpers/QueryCleaner.cs` - Query preprocessing
- [ ] `Infrastructure/Rag/Embedding/OllamaTextEmbeddingGeneration.cs` - Query embedding
- [ ] `Infrastructure/Rag/VectorDb/PineconeService.cs` - Vector search
- [ ] `Infrastructure/Integration/OllamaConnector.cs` - LLM integration

### Conversation History Flow
- [ ] `Services/Conversation/ConversationService.cs` - Conversation management
- [ ] `Services/Chat/ChatHistoryManager.cs` - History management
- [ ] `Infrastructure/Caching/CacheManager.cs` - Cache operations

### Chat Response Flow
- [ ] `Controllers/ChatController.cs` - Request handling
- [ ] `Services/Chat/ChatService.cs` - Chat processing
- [ ] `Services/Chat/ChatHistoryManager.cs` - History updates
- [ ] `Infrastructure/Integration/OllamaConnector.cs` - LLM integration
- [ ] `Services/Rag/Implementation/RagRetrievalService.cs` - Context retrieval

## Data Stores to Identify ✅

- [ ] **SQL Database**
  - Conversation data
  - User data
  - Message data
  - Feedback data
  - Note data
  - Folder data
  - Attachment metadata

- [ ] **Redis Cache**
  - Conversation history cache
  - Chat response cache
  - Frequently accessed data

- [ ] **File System**
  - Document storage
  - Temporary processing files

- [ ] **Vector Database**
  - Document embeddings
  - Chunk metadata
  - Vector indices

## Data Transformations to Document ✅

- [ ] **Document Processing**
  - Raw file → Text extraction
  - Text → Chunks
  - Chunks → Embeddings
  - Embeddings → Vector DB storage

- [ ] **RAG Query Processing**
  - User query → Cleaned query
  - Query → Query embedding
  - Embedding → Retrieved context
  - Context → Enhanced prompt

- [ ] **Conversation Management**
  - User messages → Conversation history
  - Conversation data → API responses
  - Responses → Stored history

## Data Flow Boundaries ✅

- [ ] **External Boundaries**
  - Client → API Gateway → Service
  - Service → External Services

- [ ] **Internal Boundaries**
  - API Layer → Service Layer
  - Service Layer → Infrastructure Layer
  - Infrastructure Layer → External Systems

- [ ] **Data Persistence Boundaries**
  - In-memory → Cache
  - Cache → Persistent Storage
  - Temporary → Permanent Storage

## Clarifying Questions ❓

1. **Document Processing**
   - What are the exact transformation steps for each document type?
   - How is chunking configured and implemented?
   - How are document metadata and content linked?
   
   **Answer:** Document processing follows a pipeline: upload → storage → text extraction → chunking → embedding → indexing. Different processors handle specific document types (PDF, Text, Word), each implementing the IDocumentProcessor interface with format-specific extraction logic. Chunking is likely controlled by configurable parameters (chunk size, overlap). Document metadata links to chunks through unique identifiers that preserve the relationship between original document and its processed parts.

2. **RAG Implementation**
   - How exactly is the RAG context integrated with user queries?
   - What is the ranking or filtering mechanism for retrieved chunks?
   - How are source attributions handled?
   
   **Answer:** The RAG flow processes user queries through: query cleaning → embedding creation → vector search → context retrieval → prompt enhancement → LLM query. Retrieved chunks are ranked by similarity scores from the vector search. Context is integrated into prompts with specific templates that guide the LLM to use the provided information. Source attribution is maintained through chunk metadata that preserves document origin details.

3. **Caching Strategy**
   - What specific items are cached and for how long?
   - What is the cache invalidation strategy?
   - How are cache misses handled?
   
   **Answer:** Conversation history and frequently accessed data are cached in Redis with configurable TTL. Cache invalidation likely occurs on time basis and when entities are modified (write-through invalidation). Cache misses trigger retrieval from the primary data store (SQL database) with results then stored in cache to improve subsequent access.

4. **Data Retention**
   - How long is conversation history retained?
   - Is there any data archival or cleanup process?
   - How are unused documents or embeddings managed?
   
   **Answer:** Conversation history retention periods are likely configurable based on user preferences or system settings. Background processes would handle data archival and cleanup based on usage patterns and retention policies. Unused documents and embeddings might be flagged for cleanup after a period of inactivity.

5. **Performance Optimization**
   - Are there any batching operations for data processing?
   - How are large data volumes handled?
   - Are there any throttling mechanisms?
   
   **Answer:** Document processing likely happens asynchronously to handle large documents without blocking user interactions. Large documents are managed through chunking to process manageable pieces independently. Processing status is tracked throughout the pipeline to monitor progress and handle failures. Throttling mechanisms may exist for rate limiting external API calls (like embedding generation) to stay within quotas.

## Data Flow Diagram Elements ✏️

1. **External Entities**
   - Users/Clients
   - External Systems

2. **Processes**
   - Document Processing
   - RAG Query Processing
   - Conversation Management
   - Chat Response Generation

3. **Data Stores**
   - SQL Database
   - Redis Cache
   - File System
   - Vector Database

4. **Data Flows**
   - Document Upload Flow
   - Query Processing Flow
   - Conversation History Flow
   - Response Generation Flow

## Data Attributes to Include ✅

- [ ] **Document Data**
  - File metadata
  - Content type
  - Extraction results
  - Chunk information
  - Processing status

- [ ] **Conversation Data**
  - Conversation metadata
  - Message content
  - User information
  - Timestamps
  - Related entities (folders, etc.)

- [ ] **RAG Data**
  - Query embeddings
  - Document embeddings
  - Similarity scores
  - Source information
  - Context chunks

- [ ] **Response Data**
  - Generated content
  - Completion metrics
  - RAG context used
  - Timing information

## Additional Notes 📝

- Focus on data transformations rather than control flow
- Show how data moves between components
- Highlight storage points and persistence
- Include caching operations explicitly
- Show both transient and persistent data stores
- Document data format transformations
- Note any data validation or enrichment steps
- Include error flows for data processing failures 