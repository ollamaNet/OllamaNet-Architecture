# AIModelOperationsService Implementation Plan

## Overview
This document outlines the implementation details for each method in the `AIModelOperationsService` class, which handles operations related to AI models in the Admin Service.

## Methods

### 1. GetModelByIdAsync(string modelId)

**Purpose:**  
Retrieve a specific AI model by its ID (name).

**Implementation Steps:**
1. Fetch the model by ID from the repository.
2. If not found, log a warning and throw `ModelNotFoundException`.
3. Map the entity to `AIModelResponse` using `AIModelMapper.ToResponseDto`.
4. Return the mapped response.

**Error Handling:**
- If the model is not found, throw `ModelNotFoundException` with appropriate GUID.
- The controller/caller should handle this exception to return a proper HTTP response.

---

### 2. CreateModelAsync(CreateModelRequest request, string userId)

**Purpose:**  
Create a new AI model, either from Ollama API data or from direct request data.

**Implementation Steps:**
1. Check for duplicate model name; throw `ModelAlreadyExistsException` if found.
2. If `FromOllama` is true, fetch from Ollama API and map with `AIModelMapper.FromOllamaToAIModel`.
   - If Ollama API fails, throw `ExternalModelFetchException`.
3. If `FromOllama` is false, map directly with `AIModelMapper.FromRequestToAIModel`.
4. Assign tags if present in the request.
5. Save the model to the database.
6. Return a success result with appropriate message.

**Error Handling:**
- Duplicate model name: `ModelAlreadyExistsException`
- Ollama API failure: `ExternalModelFetchException`
- Other errors are propagated for global handling.

---

### 3. UpdateModelAsync(UpdateModelRequest request)

**Purpose:**  
Update an existing model's properties with new values from the request.

**Implementation Steps:**
1. Fetch the model by name (ID); throw `ModelNotFoundException` if not found.
2. If name is being changed, check for duplicates; throw `ModelAlreadyExistsException` if found.
3. Use the mapper to update only non-null properties.
4. Save changes to the database.
5. Return a success result.

**Error Handling:**
- Model not found: `ModelNotFoundException`
- Duplicate model name: `ModelAlreadyExistsException`

---

### 4. AddTagsToModelAsync(ModelTagOperationRequest request)

**Purpose:**  
Add one or more tags to a model.

**Implementation Steps:**
1. Fetch the model by ID; throw `ModelNotFoundException` if not found.
2. For each tag ID:
   - If the tag does not exist, add to list of failed tags.
   - If the tag is already assigned, add to list of skipped tags.
   - Otherwise, add the tag to the model.
3. Save changes if any tags were added.
4. Return a result message indicating which tags were added, not found, or skipped.

**Error Handling:**
- Model not found: `ModelNotFoundException`
- Invalid tags: Tracked in result message, not treated as an error.

---

### 5. RemoveTagsFromModelAsync(ModelTagOperationRequest request)

**Purpose:**  
Remove one or more tags from a model.

**Implementation Steps:**
1. Fetch the model by ID; throw `ModelNotFoundException` if not found.
2. For each tag ID:
   - If the tag is not assigned, add to list of skipped tags.
   - Otherwise, remove the tag from the model.
3. Save changes if any tags were removed.
4. Return a result message indicating which tags were removed and which were skipped.

**Error Handling:**
- Model not found: `ModelNotFoundException`
- Unassigned tags: Tracked in result message, not treated as an error.

---

### 6. DeleteModelAsync(string modelId)

**Purpose:**  
Permanently delete a model.

**Implementation Steps:**
1. Fetch the model by ID; throw `ModelNotFoundException` if not found.
2. Remove the model from the database (hard delete).
3. Save changes.
4. Return a success result.

**Error Handling:**
- Model not found: `ModelNotFoundException`

---

### 7. SoftDeleteModelAsync(string modelId)

**Purpose:**  
Mark a model as deleted without removing it from the database.

**Implementation Steps:**
1. Fetch the model by ID; throw `ModelNotFoundException` if not found.
2. Set `IsDeleted = true`.
3. Save changes.
4. Log the soft delete operation.
5. Return a success result.

**Error Handling:**
- Model not found: `ModelNotFoundException`

---

## Common Patterns

1. **Repository Access:**
   - All methods use the unit of work pattern for data access.
   - Each operation follows a consistent sequence: get, modify, save.

2. **Exception Handling:**
   - Domain-specific exceptions for common error cases.
   - Logging for all errors and significant operations.

3. **Mapping:**
   - Use of `AIModelMapper` for entity-to-DTO conversions.
   - Consistent handling of related entities (tags).

4. **Result Return:**
   - Use of `ModelOperationResult` for consistent result reporting.
   - Detailed messages that include operation outcomes. 