# DateTime Properties Documentation - ConversationService

This document lists all properties in the ConversationService scope that are intended to carry DateTime data (e.g., CreatedAt, ModifiedAt, EndedAt, etc.).

| Property                | Location (File/Class)                                 | Purpose/Usage                        | Set In         | Exposed to Client | Notes |
|-------------------------|------------------------------------------------------|--------------------------------------|----------------|-------------------|-------|
| CreatedAt               | ChatService/DTOs/PromptRequest.cs                    | When prompt was created              | Service        | Yes               |       |
| CreatedAt               | ChatService/DTOs/ChatResponse.cs                     | When chat response was created       | Service        | Yes               |       |
| CreatedAt               | ChatService/DTOs/OllamaModelResponse.cs              | When model response was created      | Service        | Yes               |       |
| CreatedAt               | NoteService/DTOs/NoteResponse.cs                     | When note was created                | Service        | Yes               |       |
| CreatedAt               | ConversationService/DTOs/GetConversationInfoResponse.cs | When conversation was created    | Service        | Yes               |       |
| CreatedAt               | ConversationService/ConversationService.cs (entity)  | When conversation was created        | Service        | Yes               |       |
| EndedAt                 | ConversationService/ConversationService.cs (entity)  | When conversation ended              | Service        | Maybe             |       |
| ModifiedAt              | FolderService/FolderService.cs (entity)              | When folder was last modified        | Service        | Maybe             |       |

## Details
- **Set In**: Always set in the service layer unless the property is meant to be client-supplied (rare, and should be documented per endpoint).
- **Time Format**: Always UTC (`DateTime.UtcNow`).
- **Exposed to Client**: Yes, unless the property is for internal use only ("Maybe" means it depends on the DTOs returned by the endpoint).

## Audit Notes
- All DateTime properties should be set in the service layer for consistency and security.
- If a property is not exposed to the client, ensure it is not included in DTOs returned by controllers.
- If you add new DateTime properties, update this document and ensure they follow the same conventions.

---

_Last updated: [auto-generated]_

---

## Implementation Plan for DateTime Properties

This section outlines the concrete steps to be taken for each DateTime property in the ConversationService codebase. **No changes will be made to the repository or database layers.**

| Property    | Action in Code (Service Layer)                                                                 | Effect on Requests/Other Code                                                                                 |
|-------------|-----------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------|
| CreatedAt   | Set to `DateTime.UtcNow` when creating new entities (prompts, responses, notes, conversations, folders). | Ensures all new records have a consistent, server-generated creation timestamp. No client-supplied values.   |
| ModifiedAt  | Set to `DateTime.UtcNow` whenever an entity (e.g., folder) is updated.                        | Guarantees updates are tracked. Update endpoints will always refresh this value.                             |
| EndedAt     | Set to `DateTime.UtcNow` when a conversation is ended/closed in the service.                  | Allows tracking of conversation lifecycle. Only set when a conversation is logically ended.                  |

### Additional Notes
- **Controllers**: Remove any logic that sets DateTime properties; this must be handled in the service layer.
- **DTOs**: Only expose DateTime properties to the client if they are relevant for display, sorting, or auditing.
- **Validation**: If a DateTime property is ever client-supplied (rare), validate it in the controller and document the reason.
- **Testing**: For now, always use `DateTime.UtcNow`. If testability is needed, consider injecting a time provider in the future.

### No Changes to Repository/DB Layer
- All DateTime logic remains in the service/application layer. The repository and database layers will simply persist whatever values are set by the service.

---

_This plan ensures consistency, security, and maintainability for all DateTime handling in ConversationService._ 