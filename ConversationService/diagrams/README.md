# Architecture Diagram System

This directory contains a comprehensive architecture diagram system with tools, templates, and diagrams for the ConversationService.

## Directory Structure

```
diagrams/
├── sequence_diagrams/            # Sequence diagrams (.puml) and checklist
├── state_machine_diagrams/       # State machine diagrams (.puml) and checklist
├── component_diagrams/           # Component diagrams (.puml) and checklist
├── context_diagrams/             # Context diagrams (.puml) and checklist
├── container_diagrams/           # Container diagrams (.puml) and checklist
├── infrastructure_diagrams/      # Infrastructure diagrams (.puml) and checklist
├── integration_diagrams/         # Integration diagrams (.puml) and checklist
├── data_flow_diagrams/           # Data flow diagrams (.puml) and checklist
├── class_diagrams/               # Class diagrams (.puml) and checklist
├── diagram_builder/              # Tools for diagram generation
│   ├── compile_diagrams.bat      # Batch script for diagram generation
│   ├── diagram_generator_instructions.md # Detailed guide
│   ├── diagram_implementation_plan.md # Implementation steps
│   └── other documentation files
├── README.md                     # This file
└── .cursorrules                  # Rules for Cursor AI
```

## Getting Started

### Viewing Diagrams

The `.puml` files in each directory are the source files for the architecture diagrams. To view them:

1. Use the tools in the `diagram_builder` directory to compile them to PNG format
2. The compiled PNG files will be placed in the respective directories

### Generating New Diagrams

1. Navigate to the appropriate diagram type directory
2. Create or modify `.puml` files
3. Run the compile script in the `diagram_builder` directory
4. Check the resulting PNG files

## Setting Up in Other Projects

To use this diagram generation system in another project:

1. Copy the diagram generation tools to your project
2. Install the appropriate `.cursorrules` file
3. Configure project-specific settings as needed

## Using Cursor AI

This project includes a `.cursorrules` file that guides Cursor AI to produce accurate architecture diagrams:

1. When using Cursor, ask it to "Create a [diagram type] for this project"
2. Cursor will follow the guidelines in the `.cursorrules` file to:
   - Analyze the codebase first
   - Create an accurate diagram based on actual code
   - Use proper PlantUML syntax

## Diagram Types

The system supports the following diagram types, each in its own directory:

1. **Sequence Diagrams** - Illustrates process flows and interactions
   - Chat message flow
   - Document processing flow
   - RAG integration flow
   - Conversation management flow

2. **State Machine Diagrams** - Shows state transitions and behaviors
   - Chat session states
   - Document processing states
   - RAG processing states
   - Conversation lifecycle states

3. **Component Diagrams** - Details internal structure and dependencies

4. **Context Diagrams (L0)** - Shows the system's external interactions

5. **Container Diagrams (L1)** - Shows internal high-level components

6. **Infrastructure Diagrams** - Shows the technical infrastructure

7. **Integration Diagrams** - Shows external API connections

8. **Data Flow Diagrams** - Shows data movement and transformations

9. **Class Diagrams** - Shows key classes, interfaces, and their relationships

## Best Practices

1. Always base diagrams on the actual code, not assumptions
2. Start with higher-level diagrams (Context, Container) before detailed ones
3. Ensure consistent naming across all diagrams
4. Update diagrams when code structure changes
5. Include diagrams in code reviews
6. Use descriptive notes to clarify complex parts

## Troubleshooting

Common issues:

1. **PlantUML not found**: Update the path in the batch file or install PlantUML
2. **Diagram not compiling**: Check PlantUML syntax and ensure no special characters are used
3. **Inaccurate diagrams**: Review the codebase again and update the diagram based on actual code

## Extending the System

To enhance this diagram generation system:

1. Add more specialized templates
2. Extend the `.cursorrules` file with more project-specific guidelines
3. Add automation to analyze the codebase and generate diagrams automatically 