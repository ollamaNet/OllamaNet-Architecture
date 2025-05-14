# Architecture Diagram System

This directory contains a comprehensive architecture diagram system with tools, templates, and diagrams for the ConversationService.

## Directory Structure

```
diagrams/
├── *.puml                     # PlantUML diagram source files
├── README.md                  # This file
├── output/                    # Generated diagram PNG files
└── diagram_builder/           # Tools for diagram generation
    ├── *.bat                  # Batch scripts for generation and deployment
    ├── project_cursorrules    # Project-specific Cursor rules
    ├── *.md                   # Documentation and templates
    └── templates/             # Template files (created by scripts)
```

## Getting Started

### Viewing Diagrams

The `.puml` files in this directory are the source files for the architecture diagrams. To view them:

1. Use the tools in the `diagram_builder` directory to compile them to PNG format
2. The compiled PNG files will be placed in the `output` directory

### Generating New Diagrams

1. Navigate to the `diagram_builder` directory
2. Run `generate_diagrams.bat` to create templates and compile diagrams
3. Create or modify `.puml` files in the main `diagrams` directory
4. Run `generate_diagrams.bat` again to compile to PNG

## Deploying to Other Projects

To use this diagram generation system in another project:

1. Navigate to the `diagram_builder` directory
2. Run `deploy_diagram_tools.bat`
3. Follow the prompts to install all necessary files to the target project

## Using Cursor AI

This project includes a `.cursorrules` file (copied as `project_cursorrules` in the `diagram_builder` directory) that guides Cursor AI to produce accurate architecture diagrams:

1. When using Cursor, ask it to "Create a [diagram type] for this project"
2. Cursor will follow the guidelines in the `.cursorrules` file to:
   - Analyze the codebase first
   - Create an accurate diagram based on actual code
   - Use proper PlantUML syntax

For more detailed information, see the documentation in the `diagram_builder` directory:

- `diagram_generator_instructions.md` - Detailed best practices
- `global_cursorrules_template.md` - Template for use in other projects
- `solution_summary.md` - Overview of the entire solution

## Diagram Types

The system supports the following diagram types:

1. **Context Diagram (L0)** - Shows the system's external interactions
2. **Container Diagram (L1)** - Shows internal high-level components
3. **Component Architecture** - Details internal structure and dependencies
4. **Class Diagram** - Shows key classes, interfaces, and their relationships
5. **Sequence Diagrams** - Illustrates process flows and interactions
6. **Deployment Diagram** - Shows the production deployment architecture

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
4. **Deployment script issues**: Ensure you have write permissions to the target directory

## Extending the System

To enhance this diagram generation system:

1. Add more specialized templates
2. Extend the `.cursorrules` file with more project-specific guidelines
3. Add automation to analyze the codebase and generate diagrams automatically
4. Improve the deployment script with additional features 