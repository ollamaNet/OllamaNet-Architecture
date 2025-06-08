# ConversationService System Design Plan

## 1. Folder Structure (Current, Post-Migration)

```
ConversationService/
│
├── Controllers/                # API endpoints (Chat, Conversation, Note, Folder, Feedback)
│   └── Validators/             # FluentValidation classes for requests
├── Services/                   # All domain services grouped here
│   ├── Chat/                   # Chat domain logic
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── ChatService.cs, IChatService.cs, HistoryManager.cs
│   ├── Conversation/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── ConversationService.cs, IConversationService.cs
│   ├── Note/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── NoteService.cs, INoteService.cs
│   ├── Folder/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── FolderService.cs, IFolderService.cs
│   ├── Feedback/
│   │   ├── DTOs/
│   │   ├── Mappers/
│   │   └── FeedbackService.cs, IFeedbackService.cs
│   └── Shared/                 # (Optional) Shared logic, interfaces, or base classes
├── Infrastructure/             # Cross-cutting concerns (see below)
│   ├── Caching/                # Redis, MemoryCache, etc. (CacheManager, RedisCacheService, settings)
│   ├── Logging/                # Logging abstractions/services (future)
│   ├── Email/                  # Email sending infrastructure (future)
│   └── Integration/            # External connectors (OllamaConnector, IOllamaConnector)
├── diagrams/                   # Architecture, class, and flow diagrams
├── Docs/                       # Documentation and system design
├── ConversationService-memory-bank/ # Project context and documentation
├── Properties/
├── obj/
├── bin/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── ServiceExtensions.cs
└── ConversationService.csproj
```

---

## 2. Folder & File Responsibilities

- **Controllers/**: Only handle HTTP, validation, and delegate to services.
  - **Validators/**: All FluentValidation classes for request validation.
- **Services/**: All business logic, grouped by domain.
  - **Chat/**, **Conversation/**, **Note/**, **Folder/**, **Feedback/**: Each encapsulates business logic for a domain.
    - **DTOs/**: Data Transfer Objects for each feature.
    - **Mappers/**: Feature-specific mappers (all mappers are now feature-specific and reside in their respective domain folders).
  - **Shared/**: (Optional) Shared logic, interfaces, or base classes used by multiple domains.
- **Infrastructure/**: Cross-cutting concerns.
  - **Caching/**: `CacheManager.cs`, `RedisCacheService.cs`, `RedisCacheSettings.cs`, `CacheKeys.cs`, cache exceptions.
  - **Logging/**: (future) Logging abstractions, adapters, or providers.
  - **Email/**: (future) SMTP, SendGrid, or notification services.
  - **Integration/**: `OllamaConnector.cs`, `IOllamaConnector.cs`, any other external system connectors.
- **diagrams/**: PlantUML or other diagrams for architecture, flows, and classes.
- **Docs/**: All documentation, including this system design plan.
- **ConversationService-memory-bank/**: Project context, memory, and documentation.

---

## 3. Best Practices

- **Naming**: Use clear, descriptive, and consistent names for all files and folders.
- **DTOs**: Always grouped by feature for clarity and maintainability.
- **Services**: All business logic, one service per domain, grouped under Services/.
- **Dependency Injection**: All services, helpers, and infrastructure registered via DI.
- **Single Responsibility Principle**: Each class/folder has one clear purpose.
- **Options Pattern**: For all configuration (e.g., caching, integration).
- **Documentation**: Keep Docs/ and memory-bank/ up to date with every major change.
- **Reserve Folders**: For future cross-cutting concerns (Caching, Logging, Email, etc.), even if not yet implemented.

---

## 4. Migration Note
- The migration to the new modular structure is complete. All legacy folders (Cache, Connectors, ChatService, ConversationService, NoteService, FolderService, FeedbackService, Mappers, Helpers) have been removed. All files are now in their correct locations, and namespaces are consistent.
- All mappers are now feature-specific and reside in their respective domain folders.
- This structure is current as of Phase 9 (Cleanup).

---

## 5. Legacy Code/Deprecation
- If any files, classes, or features are identified as legacy or planned for deprecation, mark them clearly and avoid refactoring them unless necessary.
- If not, assume all code is current and should be organized for future maintainability.

---

## 6. Audit: Current State Snapshot (Post-Migration)

**Domain Folders and Files:**
- Services/Chat/: ChatService.cs, IChatService.cs, HistoryManager.cs, DTOs/, Mappers/
- Services/Conversation/: ConversationService.cs, IConversationService.cs, DTOs/, Mappers/
- Services/Note/: NoteService.cs, INoteService.cs, DTOs/, Mappers/
- Services/Folder/: FolderService.cs, IFolderService.cs, DTOs/, Mappers/
- Services/Feedback/: FeedbackService.cs, IFeedbackService.cs, DTOs/, Mappers/

**Shared, Infrastructure, and Helper Files:**
- Infrastructure/Caching/: CacheManager.cs, RedisCacheService.cs, RedisCacheSettings.cs, CacheKeys.cs, Exceptions/
- Infrastructure/Integration/: OllamaConnector.cs, IOllamaConnector.cs
- Infrastructure/Logging/: (placeholder for future use)
- Infrastructure/Email/: (placeholder for future use)
- ServiceExtensions.cs, Program.cs, RedisCache_Implementation_Guide.md, .cursorrules

**Controllers and Validators:**
- Controllers/: ChatController.cs, ConversationController.cs, NoteController.cs, FolderController.cs, FeedbackController.cs, Validators/

**Legacy/Deprecated Files:**
- No files explicitly marked as deprecated or legacy in the current structure.

---

**This snapshot provides the current state after migration to the new structure.**

**This plan ensures ConversationService remains simple, modular, and ready for future growth, following best practices for modern .NET microservices.** 