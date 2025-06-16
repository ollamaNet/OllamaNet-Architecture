# AdminService API Contracts

This document outlines the existing API contracts for AdminService that must be maintained during the migration to the new architecture.

## User Operations

### Get Users (Paginated)
- **Endpoint**: `GET /api/Admin/UserOperations`
- **Query Parameters**:
  - `pageNumber`: Page number (default: 1)
  - `pageSize`: Number of items per page (default: 10)
  - `role`: Optional filter by role
- **Response**: 
  ```json
  {
    "users": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10
  }
  ```

### Get User by ID
- **Endpoint**: `GET /api/Admin/UserOperations/{id}`
- **Response**: User details

### Search Users
- **Endpoint**: `GET /api/Admin/UserOperations/search`
- **Query Parameters**:
  - `searchTerm`: Search term (required)
  - `role`: Optional filter by role
- **Response**: Array of matching users

### Change User Role
- **Endpoint**: `PATCH /api/Admin/UserOperations/{id}/role`
- **Body**: String (new role name)
- **Response**: Success message

### Toggle User Status
- **Endpoint**: `PATCH /api/Admin/UserOperations/{id}/status`
- **Body**: Boolean (isActive)
- **Response**: Success message

### Delete User
- **Endpoint**: `DELETE /api/Admin/UserOperations/{id}`
- **Response**: Success message

### Soft Delete User
- **Endpoint**: `PATCH /api/Admin/UserOperations/{id}/soft-delete`
- **Response**: Success message

### Lock User
- **Endpoint**: `POST /api/Admin/UserOperations/{id}/lock`
- **Body**: Integer (lockoutMinutes, default: 30)
- **Response**: Success message

### Unlock User
- **Endpoint**: `POST /api/Admin/UserOperations/{id}/unlock`
- **Response**: Success message

### Get Available Roles
- **Endpoint**: `GET /api/Admin/UserOperations/roles`
- **Response**: Array of role names

### Create Role
- **Endpoint**: `POST /api/Admin/UserOperations/roles`
- **Body**: 
  ```json
  {
    "name": "RoleName"
  }
  ```
- **Response**: Success message

### Delete Role
- **Endpoint**: `DELETE /api/Admin/UserOperations/roles/{id}`
- **Response**: Success message

## AI Model Operations

### Get Model by ID
- **Endpoint**: `GET /api/Admin/AIModelOperations/{modelId}`
- **Response**: Model details

### Create Model
- **Endpoint**: `POST /api/Admin/AIModelOperations`
- **Body**: 
  ```json
  {
    "name": "ModelName",
    "description": "Model description",
    "version": "1.0",
    "tags": ["tag1", "tag2"]
  }
  ```
- **Response**: Created model details

### Update Model
- **Endpoint**: `PUT /api/Admin/AIModelOperations`
- **Body**: 
  ```json
  {
    "id": "modelId",
    "name": "Updated name",
    "description": "Updated description",
    "version": "1.1"
  }
  ```
- **Response**: Updated model details

### Add Tags to Model
- **Endpoint**: `POST /api/Admin/AIModelOperations/tags/add`
- **Body**: 
  ```json
  {
    "modelId": "modelId",
    "tagIds": ["tag1", "tag2"]
  }
  ```
- **Response**: Success message
### Remove Tags from Model
- **Endpoint**: `POST /api/Admin/AIModelOperations/tags/remove`
- **Body**: 
  ```json
  {
    "modelId": "modelId",
    "tagIds": ["tag1", "tag2"]
  }
  ```
- **Response**: Success message

### Hard Delete Model
- **Endpoint**: `DELETE /api/Admin/AIModelOperations/{modelId}`
- **Response**: Success message

### Soft Delete Model
- **Endpoint**: `DELETE /api/Admin/AIModelOperations/{modelId}/softdelete`
- **Response**: Success message

## Tag Operations

### Get Tag by ID
- **Endpoint**: `GET /api/Admin/TagOperations/{id}`
- **Response**: Tag details

### Create Tag
- **Endpoint**: `POST /api/Admin/TagOperations`
- **Body**: 
  ```json
  {
    "name": "TagName",
    "description": "Tag description"
  }
  ```
- **Response**: Created tag details

### Update Tag
- **Endpoint**: `PUT /api/Admin/TagOperations`
- **Body**: 
  ```json
  {
    "id": "tagId",
    "name": "Updated name",
    "description": "Updated description"
  }
  ```
- **Response**: Updated tag details

### Delete Tag
- **Endpoint**: `DELETE /api/Admin/TagOperations/{id}`
- **Response**: Success message

## Inference Operations

### Get Model Information
- **Endpoint**: `GET /api/Admin/InferenceOperations/models/info`
- **Query Parameters**:
  - `name`: Model name
- **Response**: Model information

### Get Installed Models
- **Endpoint**: `GET /api/Admin/InferenceOperations/models`
- **Query Parameters**:
  - `pageNumber`: Page number (default: 1)
  - `pageSize`: Number of items per page (default: 10)
- **Response**: Array of installed models

### Pull/Install Model
- **Endpoint**: `POST /api/Admin/InferenceOperations/models/pull`
- **Body**: 
  ```json
  {
    "modelName": "ModelName"
  }
  ```
- **Response**: Server-sent events stream with installation progress

### Uninstall Model
- **Endpoint**: `DELETE /api/Admin/InferenceOperations/models`
- **Query Parameters**:
  - `modelName`: Model name to uninstall
- **Response**: Success message

## Response Formats

### Success Response
```json
{
  "message": "Success message"
}
```

### Error Response
```json
{
  "errors": [
    {
      "property": "PropertyName",
      "error": "Error message"
    }
  ]
}
```

### Validation Error Response
```json
[
  {
    "property": "PropertyName",
    "error": "Validation error message"
  }
]
```

## Status Codes

- **200 OK**: Successful operation
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid input or validation error
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error 
