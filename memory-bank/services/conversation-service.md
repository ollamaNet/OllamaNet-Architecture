# ConversationService API Reference

Base URL (through Gateway): `/api`

All endpoints require a valid **JWT** in the `Authorization: Bearer` header unless stated otherwise.  Streaming endpoints additionally support **Server-Sent Events (SSE)**.

---
## High-Level Modules
1. **Chat** – Real-time interaction with LLM.
2. **Conversation** – CRUD conversations & messages.
3. **Folder** – Logical grouping of conversations.
4. **Document / Note / Feedback** – Rich content attached to conversations (future work).

---
## Chat Endpoints
| Method | Path | Body | Stream? | Description |
|--------|------|------|---------|-------------|
| POST | `/chats` | `PromptRequest` | ❌ | Send prompt, receive full response JSON. |
| POST | `/chats/stream` | `PromptRequest` | ✅ SSE | Incremental token stream. |

`PromptRequest` example:
```json
{
  "model": "llama3:8b",
  "message": "Explain Clean Architecture in 3 sentences"
}
```

### Streaming Usage (fetch)
```js
const resp = await fetch("/api/chats/stream", {
  method: "POST",
  headers: { "Authorization": `Bearer ${token}`, "Content-Type": "application/json" },
  body: JSON.stringify(prompt)
});
const reader = resp.body.getReader();
while(true){
  const { done, value } = await reader.read();
  if(done) break;
  console.log(new TextDecoder().decode(value));
}
```

---
## Conversation Endpoints
| Method | Path | Description |
|--------|------|-------------|
| POST | `/conversations` | Create new conversation (returns ID). |
| GET | `/conversations` | List conversations (pagination: `page`, `pageSize`). |
| GET | `/conversations/search?term=` | Search user conversations. |
| GET | `/conversations/{id}` | Conversation meta-data & latest message. |
| GET | `/conversations/{id}/messages` | All messages in conversation. |
| PUT | `/conversations/{id}` | Update title or meta-data. |
| PUT | `/conversations/{id}/move` | Move conversation to folder. |
| DELETE | `/conversations/{id}` | Hard delete conversation. |

### Example – List Conversations
```bash
curl -H "Authorization: Bearer $TOKEN" \
     "https://api.ollamanet.dev/api/conversations?page=1&pageSize=15"
```
Response (200):
```json
{
  "items": [ { "conversationId": "uuid", "title": "My chat" } ],
  "totalCount": 2,
  "page": 1,
  "pageSize": 15
}
```

---
## Folder Endpoints
| Method | Path | Description |
|--------|------|-------------|
| POST | `/folders` | Create folder. |
| GET | `/folders` | List folders. |
| DELETE | `/folders/{id}` | Delete folder (moves conversations to root). |

---
## Document & Note Endpoints (beta)
The service exposes preliminary endpoints for uploading documents and adding notes which will be finalised in v1.0.

---
*Last updated: {{date}}*