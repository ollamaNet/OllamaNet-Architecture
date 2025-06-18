# Ollama DB Layer Project Rules

## Project Patterns

### Repository Pattern Implementation

- Each entity has its own repository interface that extends `IRepository<T>`
- Repository implementations follow a consistent pattern with standard CRUD operations
- Repositories filter out soft-deleted entities in GetAllAsync methods
- Repositories use async/await pattern for all database operations

### Entity Naming Conventions

- Entity classes are singular (e.g., `AIModel`, not `AIModels`)
- Primary keys are typically string IDs, often using GUIDs
- Navigation properties use virtual keyword for lazy loading
- Soft delete is implemented via `IsDeleted` boolean property

### Unit of Work Usage

- All database operations should go through repositories
- Repositories should not call SaveChangesAsync directly
- UnitOfWork.SaveChangesAsync() should be called to commit changes
- Multiple repository operations should be wrapped in a single UnitOfWork transaction

### Database Migrations

- Migrations follow a naming convention of timestamp_description
- Migration files should be kept in source control
- Both the Designer.cs and .cs files should be maintained

## Critical Implementation Paths

### Data Access Flow

1. Controller/Service -> UnitOfWork -> Repository -> DbContext -> Database
2. Changes are only persisted when UnitOfWork.SaveChangesAsync() is called

### User Authentication

1. Extended ASP.NET Identity with custom ApplicationUser properties
2. RefreshToken handling for maintaining user sessions
3. Soft delete for users rather than hard delete

### Entity Relationships

1. One-to-many: User -> AIModels, User -> Conversations
2. Many-to-many: AIModels <-> Tags (via ModelTag junction)
3. One-to-many: Conversation -> ConversationPromptResponses

## Known Challenges

1. Complex queries may require optimization as data volume grows
2. Maintaining consistency with soft delete across related entities
3. Managing database migrations in production environments
4. Balancing between eager loading and lazy loading for related entities

## Project Evolution

1. Started with basic entity definitions and repository pattern
2. Added ASP.NET Identity integration for user management
3. Implemented Unit of Work pattern for transaction management
4. Added support for soft delete across entities
5. Enhanced with refresh token support for authentication
6. Added attachment support for file handling
7. Implemented IsActive flag for user status tracking

## Tool Usage Patterns

1. Entity Framework Core for ORM functionality
2. EF Core migrations for database schema management
3. Dependency injection for repository and UnitOfWork registration
4. ASP.NET Core for web API hosting