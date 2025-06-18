# System Patterns: Spicy Avocado

## System Architecture

Spicy Avocado follows a notebook-first architecture pattern for exposing Ollama models from cloud code editors to the internet:

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Ollama     │────▶│  Local      │────▶│  ngrok      │────▶│  Public     │
│  Model      │     │  API Server │     │  Tunnel     │     │  Internet   │
│             │     │             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

## Key Technical Decisions

1. **Jupyter Notebook Implementation**: The project is implemented as a Jupyter notebook, allowing for interactive execution, documentation, and experimentation. This makes it easier to understand the flow and debug issues.

2. **Subprocess Management**: The project uses Python's subprocess module to manage external processes (Ollama and ngrok) rather than shell scripts. This provides better control and error handling.

3. **Environment Variable Configuration**: Critical configuration is managed through environment variables, following best practices for service configuration.

4. **Polling for Service Availability**: The system uses polling with retries to verify that services are running correctly before proceeding.

5. **Separate Process Logging**: Output from long-running processes is redirected to log files for debugging and monitoring.

## Design Patterns in Use

1. **Service Orchestration Pattern**: The notebook orchestrates multiple services (Ollama and ngrok) to work together.

2. **Retry Pattern**: When fetching the ngrok URL, the code implements retries to handle potential timing issues.

3. **Environment-Based Configuration**: Configuration values are stored in environment variables, allowing for easy modification without code changes.

4. **Process Isolation**: Each service runs in its own subprocess, ensuring clean separation and independent lifecycle management.

## Component Relationships

### Ollama Component
- **Responsibilities**: 
  - Running the LLM model
  - Serving the API on a local port
  - Processing inference requests

### ngrok Component
- **Responsibilities**:
  - Creating a secure tunnel to the internet
  - Providing a public HTTPS URL
  - Forwarding requests to the local Ollama API
  - Handling TLS termination

### Orchestration Component (Jupyter Notebook)
- **Responsibilities**:
  - Installing dependencies
  - Configuring services
  - Starting services in the correct order
  - Verifying service health
  - Retrieving and displaying the public URL

## Architecture Overview
1. Ollama Server
   - Runs locally
   - Serves AI models
   - Exposed via ngrok

2. RabbitMQ Service Discovery
   - Publishes API URL
   - Uses topic exchange
   - Implements durable messaging

## Design Patterns
1. Service Discovery Pattern
   - Publisher: Ollama server
   - Exchange: service-discovery
   - Routing: inference.url.changed
   - Message Format: JSON with URL and metadata

2. Error Handling Pattern
   - Try-catch blocks
   - Connection cleanup in finally
   - Detailed error logging
   - Non-blocking error handling

3. Connection Management Pattern
   - Single connection per publish
   - Proper resource cleanup
   - Connection state verification

## Component Relationships
1. Ollama → ngrok → RabbitMQ
   - Ollama provides API
   - ngrok exposes API
   - RabbitMQ distributes URL

2. Message Flow
   ```
   Ollama Server → ngrok URL → RabbitMQ Exchange → Consumers
   ```

## Future Microservice Architecture

The next evolution of Spicy Avocado introduces a dedicated inference microservice (`avocado-infer`) implemented with **FastAPI**. This service will decouple runtime inference from the notebook, enabling scalable, container-friendly deployments.

### Proposed Layers (Clean Architecture)
1. **API Layer** – FastAPI routers & request/response models
2. **Service Layer** – Orchestrates inference, caching, and validation
3. **Domain Layer** – Core business logic & abstractions (model selector, prompt builder)
4. **Infrastructure Layer** – Ollama client wrappers, RabbitMQ publisher, persistence adapters

### Component Relationships (Updated)
```
User ▶ FastAPI ▶ Ollama ▶ ngrok ▶ RabbitMQ ▶ Consumers
```
- FastAPI exposes REST endpoints mirroring Ollama API endpoints and adds metadata routes.
- RabbitMQ remains the service-discovery backbone, now publishing both notebook and microservice URLs.
- The notebook becomes an orchestration/administration tool rather than the primary API surface.

### Design Patterns Added
1. **Clean Architecture** – Clear separation of concerns to ease testing and extensibility.
2. **Adapter Pattern** – Infrastructure adapters isolate external dependencies (Ollama, RabbitMQ).
3. **Dependency Injection** – Services are injected to promote testability.

## Technical Decisions
1. RabbitMQ Configuration
   - Topic exchange for flexible routing
   - Durable exchange for persistence
   - JSON message format for compatibility

2. Error Handling Strategy
   - Print errors but continue
   - Clean up resources
   - Log detailed information

3. Connection Management
   - Create connection per publish
   - Close connection after use
   - Verify connection state