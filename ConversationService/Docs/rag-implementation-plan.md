# RAG Implementation Plan - Migration Focus

This plan focuses on migrating the existing RAG implementation to match our system design patterns, without adding new functionality.

## Current Implementation Analysis

### Existing Components
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

## Migration Plan

### Phase 1: Infrastructure Setup

#### Create Directory Structure
```
ConversationService/
├── Infrastructure/
│   └── Rag/
│       ├── Embedding/
│       ├── VectorDb/
│       └── Options/
```

#### Move Files
1. **Embedding Infrastructure**
   - Move `ITextEmbeddingGeneration.cs` → `Infrastructure/Rag/Embedding/ITextEmbeddingGeneration.cs`
   - Move `OllamaTextEmbeddingGeneration.cs` → `Infrastructure/Rag/Embedding/OllamaTextEmbeddingGeneration.cs`

2. **Vector Database**
   - Create `Infrastructure/Rag/VectorDb/PineconeService.cs` (extract from RagIndexingService & RagRetrievalService)

3. **Configuration**
   - Move `PineconeOptions.cs` → `Infrastructure/Rag/Options/PineconeOptions.cs`
   - Move `RagOptions.cs` → `Infrastructure/Rag/Options/RagOptions.cs`

### Phase 2: Service Layer Setup

#### Create Directory Structure
```
ConversationService/
├── Services/
│   └── Rag/
│       ├── DTOs/
│       ├── Interfaces/
│       ├── Helpers/
│       └── Implementation/
```

#### Move and Refactor Files
1. **Interfaces**
   - Move `IRagIndexingService.cs` → `Services/Rag/Interfaces/IRagIndexingService.cs`
   - Move `IRagRetrievalService.cs` → `Services/Rag/Interfaces/IRagRetrievalService.cs`

2. **Implementation**
   - Move `RagIndexingService.cs` → `Services/Rag/Implementation/RagIndexingService.cs`
   - Move `RagRetrievalService.cs` → `Services/Rag/Implementation/RagRetrievalService.cs`

3. **DTOs**
   - Extract `Chunk` class from RagIndexingService → `Services/Rag/DTOs/DocumentChunk.cs`

4. **Helpers**
   - Move `QueryCleaner.cs` → `Services/Rag/Helpers/QueryCleaner.cs`

### Phase 3: Update Dependencies

1. **Update Namespaces**
   ```csharp
   // Infrastructure
   namespace ConversationService.Infrastructure.Rag.Embedding
   namespace ConversationService.Infrastructure.Rag.VectorDb
   namespace ConversationService.Infrastructure.Rag.Options

   // Services
   namespace ConversationService.Services.Rag.Interfaces
   namespace ConversationService.Services.Rag.Implementation
   namespace ConversationService.Services.Rag.DTOs
   namespace ConversationService.Services.Rag.Helpers
   ```

2. **Update Service Registration**
   ```csharp
   // Program.cs
   services.Configure<PineconeOptions>(Configuration.GetSection("Pinecone"));
   services.Configure<RagOptions>(Configuration.GetSection("Rag"));
   
   // Infrastructure
   services.AddSingleton<ITextEmbeddingGeneration, OllamaTextEmbeddingGeneration>();
   services.AddSingleton<IPineconeService, PineconeService>();
   
   // Services
   services.AddScoped<IRagIndexingService, RagIndexingService>();
   services.AddScoped<IRagRetrievalService, RagRetrievalService>();
   ```

### Phase 4: Clean Up

1. Remove old files from `ChatService/RagService/`
2. Update any references in other services
3. Update imports in all affected files

## Migration Strategy

1. Create new folders
2. Move files one at a time
3. Update namespaces
4. Update dependencies
5. Test each component
6. Remove old files

## Next Steps

1. Review and approve migration plan
2. Begin Phase 1 implementation
3. Schedule code review points 