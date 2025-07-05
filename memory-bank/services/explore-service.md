# ExploreService API Reference

Base URL: `/api/v1/explore`

Public endpoints – no authentication required.

---
## Endpoints
| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/models` | `page`, `pageSize` | Paginated catalogue of available AI models. |
| GET | `/models/{id}` | — | Detailed info for a specific model. |
| GET | `/tags` | — | List all model tags. |
| GET | `/tags/{tagId}/models` | — | Models associated with tag. |

### Example – Browse Models
```bash
curl "https://api.ollamanet.dev/api/v1/explore/models?page=1&pageSize=10"
```
Response (200):
```json
{
  "items": [
    { "id": "llama3:8b", "name": "Llama 3 8B", "description": "…" }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 42
}
```

---
Caching
* Results cached in Redis for 30 min (`models`) and 6 h (`tags`).

---
*Last updated: {{date}}*