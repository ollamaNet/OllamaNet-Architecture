# Implementation Plan: GetModelByIdAsync

## Purpose
Replace the placeholder logic in `GetModelByIdAsync` with a real implementation that fetches a model by its GUID from the repository, includes tags, and handles not-found cases robustly.

## Steps
1. **Dependency Injection**
   - Inject `IUnitOfWork` (which provides `IAIModelRepository`) into the service constructor.

2. **Input Validation**
   - Parse the `modelId` string to a `Guid`. If invalid, log a warning and throw `ModelNotFoundException` with `Guid.Empty`.

3. **Repository Fetch**
   - Use `_unitOfWork.AIModelRepo.GetByIdAsync(modelGuid)` to fetch the model, including its tags (as per repo implementation).

4. **Not Found Handling**
   - If the model is not found, log a warning and throw `ModelNotFoundException` with the requested GUID.

5. **Mapping**
   - Use `AIModelMapper.ToResponseDto(model)` to map the entity to `AIModelResponse`.
   - Map tags (`ModelTags`) to a list of `ModelTagResponse` and assign to the response.

6. **Return**
   - Return the fully mapped `AIModelResponse`.

## Error Handling
- All not-found cases throw a `ModelNotFoundException`, which can be caught by the controller or middleware to return a 404 or custom error response.
- Invalid GUIDs are treated as not found for security and consistency.

## Logging
- Logs are written for both invalid GUIDs and not-found models for traceability.

## Template for Other Methods
- Use repository methods for data access.
- Use custom exceptions for error signaling.
- Use mappers for DTO conversion.
- Log all error and warning cases.
- Return DTOs with all required related data (e.g., tags). 