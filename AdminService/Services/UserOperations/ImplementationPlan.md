# UserOperationsService Implementation Plan

## Overview
This document outlines the implementation details for each method in the `UserOperationsService` class, which handles operations related to users in the Admin Service.

## Methods

### 1. GetUserByIdAsync(string userId)

**Purpose:**  
Retrieve a specific user by their ID.

**Implementation Steps:**
1. Fetch the user by ID from the repository.
2. If not found, log a warning and throw `UserNotFoundException`.
3. Map the entity to `UserResponse` DTO.
4. Return the mapped response.

**Error Handling:**
- If the user is not found, throw `UserNotFoundException`.
- The controller/caller should handle this exception to return a proper HTTP response.

---

### 2. GetUserByEmailAsync(string email)

**Purpose:**  
Retrieve a specific user by their email address.

**Implementation Steps:**
1. Fetch the user by email from the repository.
2. If not found, log a warning and throw `UserNotFoundException` with a flag indicating email search.
3. Map the entity to `UserResponse` DTO.
4. Return the mapped response.

**Error Handling:**
- If the user is not found, throw `UserNotFoundException`.

---

### 3. SearchUsersAsync(string searchTerm, string? role = null)

**Purpose:**  
Search for users by a search term, with optional role filtering.

**Implementation Steps:**
1. Call the repository's search method with the search term and optional role.
2. Map the resulting collection of entities to `UserResponse` DTOs.
3. Return the collection of mapped responses.

**Error Handling:**
- Repository errors are propagated for global handling.
- Returns an empty collection if no users match the criteria.

---

### 4. GetUsersPaginatedAsync(int pageNumber, int pageSize, string? role = null)

**Purpose:**  
Get a paginated list of users with optional role filtering.

**Implementation Steps:**
1. Call the repository's pagination method with page number, page size, and optional role.
2. Map the resulting collection of entities to `UserResponse` DTOs.
3. Return a tuple with the collection of mapped responses and the total count.

**Error Handling:**
- Repository errors are propagated for global handling.
- Returns an empty collection and zero count if no users match the criteria.

---

### 5. ChangeUserRoleAsync(string userId, string newRole)

**Purpose:**  
Change a user's role.

**Implementation Steps:**
1. Validate that the new role is not null or empty.
2. Fetch the user by ID; throw `UserNotFoundException` if not found.
3. Call the repository's change role method.
4. Save changes to the database.
5. Return true on success.

**Error Handling:**
- User not found: `UserNotFoundException`
- Invalid role: `ArgumentException`

---

### 6. ToggleUserStatusAsync(string userId, bool isActive)

**Purpose:**  
Activate or deactivate a user.

**Implementation Steps:**
1. Fetch the user by ID; throw `UserNotFoundException` if not found.
2. Update the user's active status.
3. Save changes to the database.
4. Return true on success.

**Error Handling:**
- User not found: `UserNotFoundException`

---

### 7. DeleteUserAsync(string userId)

**Purpose:**  
Permanently delete a user.

**Implementation Steps:**
1. Fetch the user by ID; throw `UserNotFoundException` if not found.
2. Delete the user from the database.
3. Save changes.
4. Return true on success.

**Error Handling:**
- User not found: `UserNotFoundException`

---

### 8. SoftDeleteUserAsync(string userId)

**Purpose:**  
Mark a user as deleted without removing them from the database.

**Implementation Steps:**
1. Fetch the user by ID; throw `UserNotFoundException` if not found.
2. Set user's IsDeleted property to true.
3. Save changes.
4. Return true on success.

**Error Handling:**
- User not found: `UserNotFoundException`

---

### 9. LockUserAccountAsync(string userId, TimeSpan duration)

**Purpose:**  
Temporarily lock a user account for a specified duration.

**Implementation Steps:**
1. Fetch the user by ID; throw `UserNotFoundException` if not found.
2. Calculate the lockout end date.
3. Set the user's lockout end date.
4. Save changes.
5. Return true on success.

**Error Handling:**
- User not found: `UserNotFoundException`

---

### 10. UnlockUserAccountAsync(string userId)

**Purpose:**  
Remove a lock from a user account.

**Implementation Steps:**
1. Fetch the user by ID; throw `UserNotFoundException` if not found.
2. Clear the user's lockout end date.
3. Save changes.
4. Return true on success.

**Error Handling:**
- User not found: `UserNotFoundException`

---

## Common Patterns

1. **Repository Access:**
   - All methods use the unit of work pattern for data access.
   - Each operation follows a consistent sequence: get, modify, save.

2. **Exception Handling:**
   - Domain-specific exceptions for common error cases.
   - Logging for all errors and significant operations.

3. **Mapping:**
   - Direct mapping from entity to DTO for consistent response format.

4. **Return Values:**
   - User operations return boolean success indicators.
   - Query operations return appropriate DTOs or collections. 