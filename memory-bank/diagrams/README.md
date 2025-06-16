# OllamaNet Architectural Diagrams

This directory contains PlantUML diagrams that visualize the OllamaNet platform architecture from different perspectives. These diagrams follow the C4 model approach, providing multiple levels of abstraction to understand the system.

## Available Diagrams

### System Overview Diagram
`system-overview.puml` - A container-level diagram showing the main components of the OllamaNet platform and their relationships. This diagram provides a high-level view of how the different services interact with each other and with external systems.

### Data Flow Diagram
`data-flow.puml` - A dynamic diagram illustrating how data flows through the system during key operations like authentication, conversation processing, model exploration, and administration. This diagram helps understand the sequence of interactions between components.

### Deployment Diagram
`deployment.puml` - A deployment diagram showing how the OllamaNet components are deployed across different infrastructure nodes. This diagram helps understand the physical or virtual infrastructure required to run the system.

## How to Use These Diagrams

### Viewing the Diagrams
These diagrams are written in PlantUML format and can be rendered using:

1. **Online PlantUML Server**: Copy the content of any .puml file and paste it into the [PlantUML Online Server](http://www.plantuml.com/plantuml/uml/)

2. **VS Code with PlantUML Extension**: Install the PlantUML extension in VS Code, then open the .puml file and use Alt+D to preview

3. **Command Line**: Use the PlantUML JAR to generate images:
   ```
   java -jar plantuml.jar system-overview.puml
   ```

### Modifying the Diagrams
These diagrams can be modified to reflect changes in the architecture:

1. Update the appropriate .puml file using any text editor
2. Render the updated diagram using one of the methods above
3. Review the changes to ensure they accurately represent the architecture

### C4 Model Diagram Types

These diagrams follow the C4 model approach, which includes:

1. **Context Diagrams**: System in context with users and external systems
2. **Container Diagrams**: Components within the system (our system-overview.puml)
3. **Component Diagrams**: Internal components of each container
4. **Code Diagrams**: Detailed code structure (classes, interfaces)

Our diagrams primarily focus on container and component levels, with deployment information added for infrastructure planning.

## Diagram Standards

All diagrams follow these standards:

1. **Consistent Naming**: Component names are consistent across all diagrams
2. **Color Coding**: Standard C4 model color scheme for different types of components
3. **Relationship Labels**: Clear descriptions of how components interact
4. **Technology Stack**: Technology information included where relevant
5. **Layout**: Organized to minimize crossing lines and maximize readability

## Integration with Documentation

These diagrams are referenced in the main architecture documentation and should be kept in sync with any architectural changes. When updating the architecture, remember to:

1. Update the relevant diagram(s)
2. Update the corresponding documentation in the memory-bank directory
3. Ensure consistency between diagrams and written documentation 