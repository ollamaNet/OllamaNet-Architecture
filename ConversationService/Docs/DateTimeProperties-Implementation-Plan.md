# DateTime Properties Implementation Plan - ConversationService

This document details the concrete steps to implement the DateTime property handling plan in the ConversationService codebase. For each property, we specify:
- Files/classes to be edited
- Exact changes to be made
- Effect on other code (DTOs, mappers, tests, etc.)
- Dependencies or follow-up actions

---

## 1. CreatedAt

### Affected Areas
- **Controllers**: Remove any assignment to `CreatedAt`.
- **Services**: Ensure `CreatedAt` is set using `DateTime.UtcNow` when creating new entities.
- **DTOs/Mappers**: Pass through or map `CreatedAt` as needed for client exposure.

### Files/Classes to Edit
| File/Class                                      | Change                                                                 | Effect on Other Code                |
|-------------------------------------------------|------------------------------------------------------------------------|-------------------------------------|
| Controllers/ChatController.cs                   | Remove `request.CreatedAt = DateTime.UtcNow;` in all endpoints        | Service must set `CreatedAt`        |
| ChatService/ChatService.cs                      | Set `CreatedAt = DateTime.UtcNow` when creating prompt/response       | Ensures all new records are correct |
| ChatService/Mappers/HistoryMapper.cs            | Ensure mapping uses service-set `CreatedAt`                           | Consistent timestamps in responses  |
| ConversationService/ConversationService.cs      | Ensure `CreatedAt` is set in all create methods                       | No controller-side DateTime logic   |
| NoteService/NoteService.cs                      | Ensure `CreatedAt` is set in add methods                              |                                     |
| FolderService/FolderService.cs                  | Ensure `CreatedAt` is set in create methods                           |                                     |

---

## 2. ModifiedAt

### Affected Areas
- **Services**: Set `ModifiedAt` to `DateTime.UtcNow` on every update.
- **DTOs/Mappers**: Map as needed if exposed to client.

### Files/Classes to Edit
| File/Class                                      | Change                                                                 | Effect on Other Code                |
|-------------------------------------------------|------------------------------------------------------------------------|-------------------------------------|
| FolderService/FolderService.cs                  | Set `ModifiedAt = DateTime.UtcNow` on update                          | Accurate update tracking            |
| Any other service with updatable entities       | Audit for `ModifiedAt` and apply same logic                            |                                     |

---

## 3. EndedAt

### Affected Areas
- **Services**: Set `EndedAt` to `DateTime.UtcNow` when a conversation is ended/closed.
- **DTOs/Mappers**: Map as needed if exposed to client.

### Files/Classes to Edit
| File/Class                                      | Change                                                                 | Effect on Other Code                |
|-------------------------------------------------|------------------------------------------------------------------------|-------------------------------------|
| ConversationService/ConversationService.cs      | Set `EndedAt = DateTime.UtcNow` when conversation is ended             | Enables lifecycle tracking          |

---

## 4. DTO Exposure
- Review all DTOs to ensure only relevant DateTime properties are exposed to the client.
- Remove any DateTime properties from DTOs that are not needed by the client.

---

## 5. Dependencies & Follow-up
- **Tests**: Update or add tests to verify DateTime is set in the service layer.
- **Documentation**: Update DateTimeProperties.md if new properties are added or logic changes.
- **Validation**: If any DateTime is ever client-supplied, add validation in the controller and document the reason.

---

_This plan ensures all DateTime handling is consistent, secure, and maintainable across ConversationService._
