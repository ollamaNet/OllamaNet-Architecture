# AuthService Architecture Diagrams

This directory contains the architecture diagrams for the AuthService microservice, documenting its structure, flows, and interactions.

## Directory Structure

```
diagrams/
├── class_diagrams/           # Class structure and relationships
├── component_diagrams/       # Component architecture
├── container_diagrams/       # Container-level architecture
├── context_diagrams/         # System context diagrams
├── data_flow_diagrams/       # Data flow through the system
├── infrastructure_diagrams/  # Infrastructure setup
├── integration_diagrams/     # Integration with other services
├── sequence_diagrams/        # Request/process sequences
├── state_machine_diagrams/   # State transitions
├── compiled/                 # Generated PNG diagrams
└── diagram_builder/          # Scripts and tools for diagram generation
```

## Diagram Types

### Context Diagram (L0)
High-level overview showing AuthService in relation to external systems and users.

### Container Diagram (L1)
Shows the major containers (applications, data stores, microservices) that make up AuthService.

### Component Diagrams
Decomposition of containers into components, showing internal architecture.

### Class Diagrams
Key classes, interfaces, and their relationships within AuthService.

### Sequence Diagrams
Important process flows such as:
- User registration
- Authentication (login)
- Token refresh
- Password management
- Role assignment

### Data Flow Diagrams
How data moves through the system, focusing on:
- User registration process
- Authentication process
- Token management
- Role management

### State Machine Diagrams
State transitions for:
- User accounts
- Authentication tokens
- Password reset process

### Infrastructure Diagrams
Infrastructure components that support AuthService:
- Database
- Caching layer
- Identity services

### Integration Diagrams
How AuthService integrates with other services in the OllamaNet platform.

## How to Generate Diagrams

1. Create or edit PlantUML (.puml) files in the appropriate directories
2. Run the compilation script from the diagram_builder directory:
   ```
   cd diagram_builder
   compile_diagrams.bat
   ```
3. View the compiled diagrams in the 'compiled' directory

## Maintenance

Diagrams should be updated whenever:
- New components are added
- Existing components are modified
- Flows change
- Integration points change
- Architecture evolves

## Best Practices

1. Follow the naming conventions for diagram files
2. Keep diagrams focused and specific to their purpose
3. Use consistent styling across all diagrams
4. Include notes explaining complex parts
5. Document any assumptions made 