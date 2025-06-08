# RAG System Refactoring Plan

## Current Analysis

The RAG (Retrieval-Augmented Generation) system currently resides in the ChatService directory and consists of:

1. **Services**
   - `IRagIndexingService` & `RagIndexingService`
   - `IRagRetrievalService` & `RagRetrievalService`
   - `ITextEmbeddingGeneration` & `OllamaTextEmbeddingGeneration`

2. **Configuration**
   - `RagOptions`
   - `PineconeOptions`

3. **Helper**
   - `QueryCleaner`

4. **Internal Models**
   - `Chunk` (inside RagIndexingService)

## Architectural Decision

The RAG system will be split into two main components:

1. **Infrastructure Layer** (`Infrastructure/Rag/`)
   - Embedding generation (Ollama)
   - Vector database operations (Pinecone)
   - Configuration options

2. **Service Layer** (`Services/Rag/`)
   - RAG business logic
   - DTOs and interfaces
   - Helper utilities

## Directory Structure

```
ConversationService/
├── Infrastructure/
│   └── Rag/
│       ├── Embedding/
│       │   ├── ITextEmbeddingGeneration.cs
│       │   └── OllamaTextEmbeddingGeneration.cs
│       ├── VectorDb/
│       │   └── PineconeService.cs
│       └── Options/
│           ├── PineconeOptions.cs
│           └── RagOptions.cs
├── Services/
│   └── Rag/
│       ├── DTOs/
│       │   └── DocumentChunk.cs
│       ├── Interfaces/
│       │   ├── IRagIndexingService.cs
│       │   └── IRagRetrievalService.cs
│       ├── Helpers/
│       │   └── QueryCleaner.cs
│       └── Implementation/
│           ├── RagIndexingService.cs
│           └── RagRetrievalService.cs
```

## Implementation Plan

### Phase 1: Infrastructure Setup
1. Create infrastructure directory structure
2. Move embedding-related files
3. Extract Pinecone operations into dedicated service
4. Move configuration files

### Phase 2: Service Layer Setup
1. Create service directory structure
2. Move interfaces to dedicated folder
3. Move service implementations
4. Extract DTOs
5. Move helper utilities

### Phase 3: Update Dependencies
1. Update all namespaces to match new structure
2. Update service registration in Program.cs
3. Fix all references and imports

### Phase 4: Clean Up
1. Remove old files from ChatService/RagService/
2. Update any references in other services
3. Test all components

## Migration Strategy

1. Create new folders alongside existing code
2. Move files one at a time
3. Update namespaces and dependencies
4. Test each component
5. Remove old implementation

## Next Steps

1. Begin Phase 1 implementation
2. Schedule code review points
3. Plan testing strategy (to be done later)
