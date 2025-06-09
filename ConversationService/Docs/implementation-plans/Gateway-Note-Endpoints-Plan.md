# Gateway Configuration Plan - Note Endpoints

## Overview

This document outlines the plan for adding Note endpoints from `NoteController.cs` to the gateway configuration in `Conversation.json`. The configuration will follow the established patterns in the existing gateway routes.

## Current Gateway Configuration Structure

Each route in `Conversation.json` follows this pattern:
```json
{
  "DownstreamPathTemplate": "/api/{controller}/{action}",  // Backend endpoint
  "DownstreamScheme": "${Services.Conversation.Scheme}",   // Always same
  "DownstreamHostAndPorts": [                             // Always same structure
    {
      "Host": "${Services.Conversation.Host}",
      "Port": "${Services.Conversation.Port}"
    }
  ],
  "UpstreamPathTemplate": "/{controller}/{action}",       // Client-facing endpoint
  "UpstreamHttpMethod": [ "VERB" ],                      // HTTP method(s)
  "AuthenticationOptions": {                              // Optional auth config
    "AuthenticationProviderKey": "Bearer",
    "AllowedScopes": []
  }
}
```

## Note Endpoints Analysis

### 1. AddNote
- **Method**: POST
- **Route**: `/api/Note`
- **Parameters**: Body (AddNoteRequest)
- **Auth Required**: Yes

### 2. DeleteNote
- **Method**: DELETE
- **Route**: `/api/Note/{responseId}/{noteId}`
- **Parameters**: Path (responseId, noteId)
- **Auth Required**: Yes

### 3. SoftDeleteNote
- **Method**: DELETE
- **Route**: `/api/Note/soft-delete/{responseId}/{noteId}`
- **Parameters**: Path (responseId, noteId)
- **Auth Required**: Yes

### 4. UpdateNote
- **Method**: PUT
- **Route**: `/api/Note/{responseId}/{noteId}`
- **Parameters**: Path (responseId, noteId), Body (UpdateNoteRequest)
- **Auth Required**: Yes

### 5. GetNotesByResponseId
- **Method**: GET
- **Route**: `/api/Note/response/{responseId}`
- **Parameters**: Path (responseId)
- **Auth Required**: Yes

### 6. GetNote
- **Method**: GET
- **Route**: `/api/Note/{responseId}/{noteId}`
- **Parameters**: Path (responseId, noteId)
- **Auth Required**: Yes

### 7. GetNotesInConversation
- **Method**: GET
- **Route**: `/api/Note/conversation/{conversationId}`
- **Parameters**: Path (conversationId)
- **Auth Required**: Yes

## Implementation Plan

1. **Group Similar Routes**
   - Group by resource type (notes)
   - Keep CRUD operations together
   - Keep special operations (like soft-delete) separate

2. **Route Naming Convention**
   - Use lowercase for all routes
   - Use hyphenation for compound words
   - Keep consistent with existing routes

3. **Authentication**
   - Add Bearer authentication to all routes
   - Follow existing authentication pattern

4. **Order of Implementation**
   1. Basic CRUD operations first
   2. Special operations second
   3. Query operations last

## Gateway Configuration Template

```json
{
  "Routes": [
    // Create Note
    {
      "DownstreamPathTemplate": "/api/Note",
      "DownstreamScheme": "${Services.Conversation.Scheme}",
      "DownstreamHostAndPorts": [
        {
          "Host": "${Services.Conversation.Host}",
          "Port": "${Services.Conversation.Port}"
        }
      ],
      "UpstreamPathTemplate": "/notes",
      "UpstreamHttpMethod": [ "POST" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    },
    // Other routes will follow the same pattern...
  ]
}
```

## Next Steps

1. Implement routes in order of priority:
   - Basic CRUD operations
   - Special operations (soft-delete)
   - Query operations

2. Add authentication to all routes

3. Document any special considerations or patterns discovered during implementation

## Notes

- All routes will use the Conversation service scheme and host/port
- All routes require authentication
- Follow existing patterns in `Conversation.json`
- Keep route grouping logical and maintainable 