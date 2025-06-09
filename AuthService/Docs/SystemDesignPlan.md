# AuthService System Design Plan

## 1. Folder Structure (Recommended)

```
AuthService/
│
├── Controllers/         # API endpoints (e.g., AuthController.cs)
├── DTOs/                # Data Transfer Objects (request/response models)
├── Helpers/             # Utility classes (e.g., JWTManager, JWT)
├── Infrastructure/      # Cross-cutting concerns
│   └── DataSeeding/     # Seeding logic (Interfaces, Options, Services)
│   └── Caching/         # (Future) Caching strategies/services
│   └── Logging/         # (Future) Logging abstractions/services
│   └── Email/           # (Future) Email sending infrastructure
├── AuthService/         # Core business logic (AuthService.cs, IAuthService.cs)
├── Docs/                # Documentation and diagrams
├── AuthService-memory-bank/ # Project context and documentation
├── Properties/
├── obj/
├── bin/
├── appsettings.json
├── Program.cs
├── ServiceExtensions.cs
└── ...
```

**Note:**
- If not already, move `DataSeeding/` under `Infrastructure/` for consistency.
- Keep feature folders (User, Auth, Admin, etc.) in mind for future scaling, but not needed now.

---

## 2. Class Responsibility Map

- **Controllers/**: Only handle HTTP, validation, and delegate to services.
- **DTOs/**: Pure data, no logic.
- **Helpers/**: Stateless utility logic (e.g., JWT, password hashing).
- **Infrastructure/**:  
  - **DataSeeding/**: All seeding logic.
  - **Caching/**: Redis, MemoryCache, etc. (future).
  - **Logging/**: Custom logging providers, adapters, etc. (future).
  - **Email/**: SMTP, SendGrid, etc. (future).
- **AuthService/**:  
  - `AuthService.cs`, `IAuthService.cs`: Core business logic, orchestrate repositories, helpers, etc.

---

## 3. Patterns & Principles

- **Dependency Injection**: All services, helpers, and infrastructure.
- **Options Pattern**: For all configuration (e.g., JWT, DataSeeding, Caching).
- **Repository/Unit of Work**: For data access (already present).
- **Single Responsibility Principle**: Each class/folder has one clear purpose.
- **Consistent Naming**: Use clear, descriptive names for all classes and folders.

---

## 4. Future Cross-Cutting Concerns (Suggestions)

- **Caching**:  
  - MemoryCache for short-lived data.
  - Redis for distributed caching.
- **Logging**:  
  - Centralized logging (Serilog, NLog, or custom abstractions).
  - Log enrichment (user, request, correlation IDs).
- **Email**:  
  - Email sending infrastructure (SMTP, SendGrid, etc.).
  - Email templates and notification services.
- **Health Checks**:  
  - Infrastructure for service health and readiness checks.
- **Background Jobs**:  
  - Hangfire, Quartz.NET, or hosted services for scheduled/background tasks.

---

## 5. Documentation & Consistency

- Keep Docs/ and AuthService-memory-bank/ up to date with every major change.
- Document new infrastructure (e.g., caching, logging) as it's added.
- Use diagrams for complex flows (authentication, seeding, etc.).

---

## Implementation Phases

### Phase 1: Folder Audit & Refactor
- Move `DataSeeding/` under `Infrastructure/` if not already.
- Create placeholder folders for future cross-cutting concerns: `Caching/`, `Logging/`, `Email/`.
- Ensure all folders follow the recommended structure.

### Phase 2: Code Audit & Refactor
- Ensure all cross-cutting concerns (like seeding) are under `Infrastructure/`.
- Check that all DTOs, helpers, and business logic are in their correct folders.
- Refactor any misplaced files.

### Phase 3: Pattern Enforcement
- Ensure all new and existing services use dependency injection.
- Use the Options pattern for all configuration sections.
- Maintain single responsibility for all classes.

### Phase 4: Documentation
- Update `Docs/` and `AuthService-memory-bank/` with any new patterns, folder changes, or architectural decisions.
- Add or update diagrams for complex flows if needed.

### Phase 5: Prepare for Future Features
- Reserve folders and document patterns for Caching, Logging, Email, etc.
- Add stubs or interfaces if you want to encourage future consistency.

---

**Each phase should be implemented and reviewed one by one for best results and maintainability.**

## Next Steps

1. **Move DataSeeding under Infrastructure** (if not already).
2. **Document this system design plan** in your Docs/ or memory bank for future reference.
3. **Adopt the folder/class map and patterns** for all new features and refactors.
4. **Plan for future cross-cutting concerns** by reserving folders and using the Options pattern. 