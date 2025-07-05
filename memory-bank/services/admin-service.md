# AdminService API Reference

Base URL: `/api/Admin`

All endpoints require `Admin` role unless otherwise specified.  Many write operations return a `ModelOperationResult` / `TagOperationResult` wrapper containing `success`, `message`, and entity identifiers.

---
## AI Model Operations
| Method | Path | Body | Description |
|--------|------|------|-------------|
| GET | `/AIModelOperations/{modelId}` | — | Retrieve model by ID. |
| POST | `/AIModelOperations` | `CreateModelRequest` | Add new model (installs & indexes). |
| PUT | `/AIModelOperations` | `UpdateModelRequest` | Update name, description, etc. |
| DELETE | `/AIModelOperations/{modelId}` | — | Hard delete model. |
| DELETE | `/AIModelOperations/{modelId}/softdelete` | — | Soft delete (hide) model. |
| POST | `/AIModelOperations/tags/add` | `ModelTagOperationRequest` | Attach tags to model. |
| POST | `/AIModelOperations/tags/remove` | `ModelTagOperationRequest` | Detach tags from model. |

`CreateModelRequest` sample:
```json
{
  "name": "Llama 3 8B",
  "sourceUrl": "https://huggingface.co/...", 
  "tags": ["open-source", "v3"]
}
```

Progress of long installations is streamed via **Inference Operations** below.

---
## Inference Operations
| Method | Path | Body | Description |
|--------|------|------|-------------|
| GET | `/InferenceOperations/installed` | — | List installed models and sizes. |
| POST | `/InferenceOperations/install` | `InstallModelRequest` | Begin background install; SSE progress endpoint stream. |
| GET | `/InferenceOperations/progress/{jobId}` | — | Poll install progress (alt to SSE). |
| POST | `/InferenceOperations/remove` | `RemoveModelRequest` | Uninstall model. |

---
## Tag Operations
| Method | Path | Body | Description |
|--------|------|------|-------------|
| GET | `/TagOperations/{id}` | — | Get tag. |
| POST | `/TagOperations` | `CreateTagRequest` | Create tag. |
| PUT | `/TagOperations` | `UpdateTagRequest` | Rename / recolor tag. |
| DELETE | `/TagOperations/{id}` | — | Delete tag. |

---
## User Operations
| Method | Path | Body | Description |
|--------|------|------|-------------|
| GET | `/UserOperations` | — | Paginated list of users. |
| GET | `/UserOperations/{id}` | — | User detail. |
| GET | `/UserOperations/search` | `searchTerm` | Search by email/username. |
| PATCH | `/UserOperations/{id}/role` | `"Admin"` | Change role. |
| PATCH | `/UserOperations/{id}/status` | `true/false` | Activate / deactivate. |
| DELETE | `/UserOperations/{id}` | — | Hard delete. |
| PATCH | `/UserOperations/{id}/soft-delete` | — | Soft delete. |
| POST | `/UserOperations/{id}/lock` | `lockoutMinutes` | Temporarily lock account. |
| POST | `/UserOperations/{id}/unlock` | — | Unlock user. |
| GET | `/UserOperations/roles` | — | Available roles. |
| POST | `/UserOperations/roles` | `CreateRoleRequest` | Create new role. |
| DELETE | `/UserOperations/roles/{id}` | — | Delete role. |

---
## Health Endpoint
`GET /Health` – Returns 200/OK if service & dependencies are healthy.

---
### Example – Add Tags to Model
```bash
curl -X POST https://api.ollamanet.dev/api/Admin/AIModelOperations/tags/add \
  -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" \
  -d '{ "modelId": "uuid", "tags": ["fast", "gpu"] }'
```

Expected response:
```json
{
  "success": true,
  "modelId": "uuid",
  "message": "Tags added"
}
```

---
*Last updated: {{date}}*