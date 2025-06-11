# Architecture Diagram Generation Guide

## Overview
This guide documents the process and best practices for generating architecture diagrams for microservices, based on the experience of creating diagrams for the ConversationService. It includes lessons learned and common pitfalls to avoid.

## Step-by-Step Process

### 1. Initial Analysis
- **DO**: Start by analyzing the actual codebase
- **DO**: Review controllers, services, and interfaces
- **DON'T**: Make assumptions about the architecture without checking the code
- **DON'T**: Skip the code review phase

### 2. Diagram Types and Order
Create diagrams in this order:

1. **Context Diagram (L0)**
   - Shows the system's external interactions
   - Focus on external systems and their relationships
   - Keep it high-level and simple
   - Include only major external dependencies

2. **Container Diagram (L1)**
   - Shows the internal components
   - Include all major services and their relationships
   - Show both internal and external communication
   - Add notes for important responsibilities

3. **Component Architecture**
   - Detail the internal structure
   - Show all interfaces and implementations
   - Include validation and middleware components
   - Show proper dependency relationships

4. **Class Diagram**
   - Based on actual DTOs and interfaces
   - Include all relevant properties
   - Show proper inheritance and relationships
   - Keep it focused on the core classes

5. **Sequence Diagrams**
   - Show actual request flows
   - Include validation and error handling
   - Show caching and persistence steps
   - Include all relevant components

6. **Data Flow Diagrams**
   - Document processing pipeline flows
   - Show RAG query processing
   - Illustrate conversation history management
   - Include data transformation steps
   - Show storage and retrieval patterns

7. **State Machine Diagrams**
   - Document conversation lifecycle states
   - Show document processing transitions
   - Illustrate user session management
   - Detail RAG processing states
   - Include error and recovery states

8. **Infrastructure Diagrams**
   - Detail caching architecture
   - Show storage system layout
   - Illustrate vector database integration
   - Document service mesh configuration
   - Include monitoring infrastructure

9. **Integration Diagrams**
   - Show external API connections
   - Detail inter-service communication
   - Document event flows
   - Include message queue patterns
   - Show error handling mechanisms

### 3. Common Pitfalls to Avoid

#### Component Architecture
- **Mistake**: Missing external services
- **Solution**: Always check appsettings.json and Program.cs for external dependencies
- **Mistake**: Incorrect service relationships
- **Solution**: Review dependency injection setup in ServiceExtensions.cs

#### Class Diagrams
- **Mistake**: Incomplete DTO properties
- **Solution**: Always check the actual DTO files
- **Mistake**: Missing interfaces
- **Solution**: Review service interfaces before creating the diagram

#### Sequence Diagrams
- **Mistake**: Missing validation steps
- **Solution**: Review controller validation logic
- **Mistake**: Incorrect flow order
- **Solution**: Follow the actual code execution path

#### Data Flow Diagrams
- **Mistake**: Missing transformation steps
- **Solution**: Review processing pipelines thoroughly
- **Mistake**: Incomplete storage patterns
- **Solution**: Document all data persistence points

#### State Machine Diagrams
- **Mistake**: Missing error states
- **Solution**: Include all possible state transitions
- **Mistake**: Incomplete recovery flows
- **Solution**: Document error handling paths

### 4. Best Practices

#### Code Review
1. Start with Program.cs for service registration
2. Review ServiceExtensions.cs for dependencies
3. Check controllers for actual endpoints
4. Review DTOs for accurate properties
5. Check interfaces for correct methods

#### Diagram Creation
1. Use consistent naming conventions
2. Add descriptive notes
3. Show proper relationships
4. Include error handling paths
5. Document assumptions

#### Validation
1. Cross-reference with actual code
2. Check for missing components
3. Verify relationships
4. Ensure proper flow
5. Validate against implementation

### 5. Tools and Setup

#### Required Tools
1. PlantUML
   - Download from https://plantuml.com/download
   - Place JAR in consistent location
   - Update batch file path

2. Batch File Setup
   - Create compile_diagrams.bat
   - Set proper PlantUML path
   - Create output directory
   - Include error handling

#### Cursor Rules Setup
1. Use the provided `.cursorrules` file in the project root
   - This guides Cursor AI to generate accurate diagrams
   - Ensures consistency across team members
   
2. Global Template
   - We've included a `global_cursorrules_template.md` for use with other projects
   - Copy the template, customize for your project, and save as `.cursorrules`
   - This ensures diagram quality across all projects

#### Directory Structure
```
diagrams/
├── compiled/                     # Generated PNG files
├── templates/                    # PlantUML templates
├── *.puml                        # PlantUML source files
├── generate_diagrams.bat         # Generation script
├── README.md                     # Usage instructions
└── global_cursorrules_template.md # Template for other projects
```

### 6. Maintenance

#### Regular Updates
- Update diagrams when code changes
- Review diagrams during code reviews
- Keep diagrams in sync with documentation
- Version control diagram changes

#### Documentation
- Keep diagrams in memory-bank
- Update systemPatterns.md
- Document changes in progress.md
- Maintain consistency across documentation

## Example Process for New Service

1. **Initial Setup**
   ```bash
   mkdir diagrams
   mkdir diagrams/compiled
   mkdir diagrams/templates
   ```

2. **Setup Automation**
   - Copy `generate_diagrams.bat` to the new project
   - Copy `global_cursorrules_template.md`, customize, and save as `.cursorrules`
   - Run the generation script to create template files

3. **Create Diagrams**
   - Start with context diagram
   - Add container diagram
   - Create component architecture
   - Add class diagram
   - Create sequence diagrams
   - Add data flow diagrams
   - Create state machine diagrams
   - Add infrastructure diagrams
   - Create integration diagrams

4. **Compile Diagrams**
   ```bash
   cd diagrams
   generate_diagrams.bat
   ```

5. **Review and Update**
   - Check generated PNGs
   - Update as needed
   - Recompile
   - Document changes

## Using Cursor AI for Diagram Generation

To use Cursor AI with the provided rules:

1. Ensure `.cursorrules` is in your project root
2. Simply ask Cursor to generate a diagram:
   - "Create a context diagram for this project"
   - "Generate a component architecture diagram"
   - "Create a sequence diagram for the login process"

3. Cursor will:
   - Analyze your codebase first
   - Generate accurate PlantUML syntax
   - Follow the best practices in the rules

This approach ensures diagrams are based on actual code, not assumptions.

## Conclusion
Creating accurate architecture diagrams requires:
- Thorough code analysis
- Consistent review process
- Regular updates
- Proper documentation
- Attention to detail

Following this process will help create accurate, maintainable diagrams that truly reflect the system architecture. 