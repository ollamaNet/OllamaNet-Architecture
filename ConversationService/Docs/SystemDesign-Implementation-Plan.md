# ConversationService System Design Implementation Plan

This plan details the step-by-step migration to the new folder structure and best practices as described in SystemDesign.md. Each phase lists the exact actions to be taken for each file and folder.

---

## Phase 1: Prepare and Audit
- **Review the current folder and file structure.**
  - Open the ConversationService directory and list all files and folders.
- **Identify all files belonging to each domain (Chat, Conversation, Note, Folder, Feedback).**
  - Make a list of all files in ChatService, ConversationService, NoteService, FolderService, FeedbackService.
- **Identify shared, infrastructure, and helper files.**
  - List files in Cache, Connectors, Mappers, and any utility/helper files.
- **Mark any legacy or deprecated files for exclusion from refactor.**
  - Review each file for comments or documentation indicating deprecation. Mark these for later removal or archiving.

---

## Phase 2: Create New Structure
- **Create the following top-level folders if they do not exist:**
  - Create `Services/` at the root.
    - Inside `Services/`, create `Chat/`, `Conversation/`, `Note/`, `Folder/`, `Feedback/`, and `Shared/`.
  - Create `Infrastructure/` at the root.
    - Inside `Infrastructure/`, create `Caching/`, `Logging/`, `Email/`, and `Integration/`.
  - Create `Helpers/` at the root.
- **Create `DTOs/` and `Mappers/` subfolders within each domain folder as needed.**
  - For each domain in `Services/`, create `DTOs/` and `Mappers/` if not present.
- **Create placeholder `README.md` files in new folders if desired for documentation.**
  - Add a short description in each new folder's `README.md`.

---

## Phase 3: Move and Refactor Domain Services
- **Move all service files to their respective domain subfolders under `Services/`:**
  - Move `ChatService/ChatService.cs`, `ChatService/IChatService.cs`, `ChatService/HistoryManager.cs` to `Services/Chat/`.
  - Move `ChatService/DTOs/` to `Services/Chat/DTOs/`.
  - Move `ChatService/Mappers/` to `Services/Chat/Mappers/`.
  - Move `ConversationService/ConversationService.cs`, `ConversationService/IConversationService.cs` to `Services/Conversation/`.
  - Move `ConversationService/DTOs/` to `Services/Conversation/DTOs/`.
  - Move `NoteService/NoteService.cs`, `NoteService/INoteService.cs` to `Services/Note/`.
  - Move `NoteService/DTOs/` to `Services/Note/DTOs/`.
  - Move `FolderService/FolderService.cs`, `FolderService/IFolderService.cs` to `Services/Folder/`.
  - Move `FolderService/DTOs/` to `Services/Folder/DTOs/`.
  - Move `FeedbackService/FeedbackService.cs`, `FeedbackService/IFeedbackService.cs` to `Services/Feedback/`.
  - Move `FeedbackService/DTOs/` to `Services/Feedback/DTOs/`.
- **Move any shared interfaces, base classes, or logic to `Services/Shared/` if used by multiple domains.**
  - Identify and move shared code, updating references as needed.
- **Update namespaces in all moved files to reflect their new locations.**
  - For each moved file, change the namespace to `ConversationService.Services.<Domain>`.
- **Update all references/imports in the codebase to use the new namespaces.**
  - Search and replace old namespaces in all files, including controllers, DI registration, and tests.

---

## Phase 4: Move and Refactor Infrastructure & Helpers
- **Move all caching-related files to `Infrastructure/Caching/`:**
  - Move `Cache/CacheManager.cs`, `Cache/RedisCacheService.cs`, `Cache/RedisCacheSettings.cs`, `Cache/CacheKeys.cs`, and all files in `Cache/Exceptions/` to `Infrastructure/Caching/`.
- **Move all external connectors to `Infrastructure/Integration/`.**
  - Move `Connectors/OllamaConnector.cs`, `Connectors/IOllamaConnector.cs` to `Infrastructure/Integration/`.
- **Move any stateless utility classes to `Helpers/`.**
  - Move utility/helper files (e.g., string/date helpers, custom validation) to `Helpers/`.
- **Move or create placeholder folders for `Infrastructure/Logging/` and `Infrastructure/Email/` for future use.**
  - Create empty folders if not present.
- **Update namespaces and references for all moved files.**
  - Change namespaces to `ConversationService.Infrastructure.<Area>` or `ConversationService.Helpers` as appropriate.
  - Update all references in the codebase.

---

## Phase 5: Controllers & Validators
- **Ensure all controllers remain in `Controllers/`.**
  - Move any misplaced controllers to `Controllers/`.
- **Ensure all validators remain in `Controllers/Validators/`.**
  - Move any misplaced validators to `Controllers/Validators/`.
- **Update any references to DTOs, services, or helpers to use the new namespaces.**
  - Update using statements in all controllers and validators.

---

## Phase 6: Mappers
- **Move any shared mappers to `Mappers/`.**
  - Identify mappers used by multiple domains and move them to `Mappers/`.
- **Keep feature-specific mappers in their respective domain folders.**
  - Ensure mappers only used by one domain are in `Services/<Domain>/Mappers/`.
- **Update namespaces and references as needed.**
  - Update using statements in all files that use mappers.

---

## Phase 7: Update Dependency Injection & Startup
- **Update `ServiceExtensions.cs` and `Program.cs` to register services, helpers, and infrastructure from their new locations.**
  - Change DI registration to use new namespaces and folder structure.
- **Ensure all DI registrations use the new namespaces.**
  - Update all `AddScoped`, `AddSingleton`, etc., calls.
- **Test that the application starts and all services are resolved correctly.**
  - Run the application and fix any DI errors.

---

## Phase 8: Documentation & Diagrams
- **Update any diagrams in `diagrams/` to reflect the new structure.**
  - Edit PlantUML or other diagram files to match the new folder and class structure.
- **Update `Docs/` and `ConversationService-memory-bank/` with the new structure and any lessons learned.**
  - Add notes on the migration and new structure.
- **Add or update `README.md` files in new folders for clarity.**
  - Write a short description for each major folder.

---

## Phase 9: Final Audit & Cleanup
- **Remove any empty or obsolete folders left after the migration.**
  - Delete old service, cache, connector, or helper folders if empty.
- **Remove or archive any legacy/deprecated files if marked earlier.**
  - Move deprecated files to an archive folder or delete as appropriate.
- **Run a full build and test suite to ensure nothing is broken.**
  - Fix any errors or warnings.
- **Commit all changes with clear commit messages for each phase.**
  - Use descriptive commit messages (e.g., 'Phase 3: Move and refactor domain services').

---

**This phased plan ensures a smooth, traceable migration to the new structure, with minimal disruption and maximum maintainability.** 