# Entity Relationships: Ollama DB Layer

## Database Schema Overview

The Ollama DB Layer implements a relational database schema using Entity Framework Core. The schema is designed to support AI model management, conversation tracking, user management, and related functionality.

## Core Entities

### ApplicationUser

**Extends**: IdentityUser

**Properties**:
- Id (string, inherited from IdentityUser)
- UserName (string, inherited from IdentityUser)
- Email (string, inherited from IdentityUser)
- PasswordHash (string, inherited from IdentityUser)
- FirstName (string)
- LastName (string)
- IsDeleted (bool)
- TokenVersion (int)
- IsActive (bool)

**Relationships**:
- One-to-Many with AIModel (User owns multiple AI models)
- One-to-Many with Conversation (User has multiple conversations)
- One-to-Many with RefreshToken (User has multiple refresh tokens)

### AIModel

**Properties**:
- Id (string, primary key)
- Name (string)
- Description (string)
- Version (string)
- Size (long)
- Digest (string)
- Format (string)
- ParameterSize (long)
- QuantizationLevel (string)
- ImageUrl (string)
- IsDeleted (bool)
- UserId (string, foreign key)

**Relationships**:
- Many-to-One with ApplicationUser (Model belongs to a user)
- Many-to-Many with Tag (Models can have multiple tags)

### Conversation

**Properties**:
- Id (string, primary key)
- Title (string)
- TokensUsed (int)
- CreatedAt (DateTime)
- EndedAt (DateTime?)
- IsDeleted (bool)
- IsArchived (bool)
- Status (string)
- SystemMessage (string)
- UserId (string, foreign key)

**Relationships**:
- Many-to-One with ApplicationUser (Conversation belongs to a user)
- One-to-Many with ConversationPromptResponse (Conversation has multiple prompt-response pairs)
- One-to-Many with Attachment (Conversation has multiple attachments)

### ConversationPromptResponse

**Properties**:
- Id (string, primary key)
- PromptId (string, foreign key)
- AIResponseId (string, foreign key)
- ConversationId (string, foreign key)
- CreatedAt (DateTime)
- IsDeleted (bool)

**Relationships**:
- Many-to-One with Conversation (Prompt-response belongs to a conversation)
- One-to-One with Prompt (Prompt-response has one prompt)
- One-to-One with AIResponse (Prompt-response has one AI response)

### Prompt

**Properties**:
- Id (string, primary key)
- Content (string)
- TokensUsed (int)
- CreatedAt (DateTime)
- IsDeleted (bool)

**Relationships**:
- One-to-One with ConversationPromptResponse (Prompt is part of a prompt-response pair)

### AIResponse

**Properties**:
- Id (string, primary key)
- Content (string)
- TokensUsed (int)
- CreatedAt (DateTime)
- IsDeleted (bool)

**Relationships**:
- One-to-One with ConversationPromptResponse (AI response is part of a prompt-response pair)
- One-to-Many with Feedback (AI response can have multiple feedback items)

### Tag

**Properties**:
- Id (string, primary key)
- Name (string)
- Description (string)
- IsDeleted (bool)

**Relationships**:
- Many-to-Many with AIModel (Tags can be applied to multiple models)

### ModelTag

**Junction Entity**

**Properties**:
- ModelId (string, foreign key, part of composite primary key)
- TagId (string, foreign key, part of composite primary key)

**Relationships**:
- Many-to-One with AIModel
- Many-to-One with Tag

### RefreshToken

**Properties**:
- Id (string, primary key)
- Token (string)
- Expires (DateTime)
- Created (DateTime)
- CreatedByIp (string)
- Revoked (DateTime?)
- RevokedByIp (string)
- ReplacedByToken (string)
- ReasonRevoked (string)
- UserId (string, foreign key)

**Relationships**:
- Many-to-One with ApplicationUser (Token belongs to a user)

### Attachment

**Properties**:
- Id (string, primary key)
- FileName (string)
- FilePath (string)
- FileType (string)
- FileSize (long)
- UploadedAt (DateTime)
- IsDeleted (bool)
- ConversationId (string, foreign key)

**Relationships**:
- Many-to-One with Conversation (Attachment belongs to a conversation)

### Feedback

**Properties**:
- Id (string, primary key)
- Rating (int)
- Comment (string)
- CreatedAt (DateTime)
- IsDeleted (bool)
- AIResponseId (string, foreign key)

**Relationships**:
- Many-to-One with AIResponse (Feedback is for an AI response)

### SystemMessage

**Properties**:
- Id (string, primary key)
- Content (string)
- Name (string)
- Description (string)
- IsDeleted (bool)

## Relationship Diagrams

### User-Centric View

```
ApplicationUser
    |
    |-- AIModel (1:N)
    |
    |-- Conversation (1:N)
    |      |
    |      |-- ConversationPromptResponse (1:N)
    |      |      |
    |      |      |-- Prompt (1:1)
    |      |      |
    |      |      |-- AIResponse (1:1)
    |      |             |
    |      |             |-- Feedback (1:N)
    |      |
    |      |-- Attachment (1:N)
    |
    |-- RefreshToken (1:N)
```

### Model-Centric View

```
AIModel
    |
    |-- ApplicationUser (N:1)
    |
    |-- Tag (N:M via ModelTag)
```

### Conversation Flow

```
Conversation
    |
    |-- ConversationPromptResponse
           |
           |-- Prompt
           |
           |-- AIResponse
                  |
                  |-- Feedback
```

## Database Constraints

1. **Foreign Key Constraints**: Enforced between related entities
2. **Soft Delete**: Most entities implement IsDeleted flag for logical deletion
3. **Required Fields**: Critical fields are marked as required
4. **Timestamps**: Creation and modification timestamps for auditing

## Query Patterns

1. **User Data**: Filtered by UserId for multi-tenant isolation
2. **Soft Delete**: Queries filter out IsDeleted=true by default
3. **Eager Loading**: Related entities are often loaded together for efficiency
4. **Pagination**: Large collections support pagination

The entity relationships in the Ollama DB Layer provide a flexible and extensible foundation for the application's data needs, with proper separation of concerns and clear ownership boundaries.