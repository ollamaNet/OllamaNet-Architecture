# Active Context: Spicy Avocado

## Current Work Focus

The current focus of the Spicy Avocado project is enabling users to run Ollama models from cloud code editors (Google Colab, Kaggle, Datadog, etc.) and expose them as public APIs via ngrok. The implementation is optimized for notebook/cloud environments, ensuring cross-platform compatibility and ease of remote access.

## Recent Changes

- Initial implementation of the Ollama server setup
- Configuration of ngrok for public access
- Environment variable management for both services
- Implementation of error handling and status checking
- Logging setup for debugging purposes
- Added RabbitMQ integration:
  - Added pika client for RabbitMQ
  - Configured RabbitMQ connection details
  - Implemented URL publishing to service-discovery exchange
  - Added error handling and connection management
- Updated notebook structure:
  - Added new cell for RabbitMQ publishing
  - Updated summary cell with RabbitMQ status
  - Added RabbitMQ configuration section

### Additional Recent Changes (2025-06-17)
- Drafted comprehensive notebook refactor plan focusing on cell restructuring, centralized configuration, and enhanced RabbitMQ integration.
- Added architectural & project plan docs for FastAPI-based `avocado-infer` microservice.

## Next Steps

### Short-term Tasks

1. **Refine Error Handling**
   - Improve error detection and reporting
   - Add more robust retry mechanisms

2. **Enhance Logging**
   - Implement more detailed logging
   - Create a centralized log collection mechanism

3. **Configuration Management**
   - Move configuration to a separate file
   - Add support for configuration through environment variables

4. **Documentation**
   - Add more detailed usage instructions
   - Document API endpoints and examples

5. **Implement Notebook Refactor Plan**
   - Reorganize notebook per refactor document
   - Verify all cells execute successfully

### Medium-term Goals

1. **Service Reliability**
   - Implement automatic reconnection for ngrok
   - Add health checks for the Ollama service

2. **Model Management**
   - Add support for switching between different models
   - Implement model caching and versioning

3. **Containerization**
   - Create Docker configuration for easier deployment
   - Ensure container orchestration compatibility

4. **Backend Microservice (FastAPI)**
   - Begin building FastAPI-based `avocado-infer` microservice
   - Align REST endpoints with Ollama API

## Active Decisions and Considerations

### Current Decisions

1. **Jupyter Notebook Format**
   - Decision: Use Jupyter notebook for implementation
   - Rationale: Provides interactive execution and documentation
   - Status: Implemented

2. **Model Selection**
   - Decision: Use llama3.2:1b as the default model
   - Rationale: Good balance of performance and resource requirements
   - Status: Implemented

3. **Process Management**
   - Decision: Use subprocess for managing external processes
   - Rationale: Better control and error handling than shell commands
   - Status: Implemented

### Open Considerations

1. **Authentication**
   - Question: Should we implement authentication for the API?
   - Options: Basic auth, API keys, OAuth
   - Current status: No authentication implemented

2. **Persistent Storage**
   - Question: How should we handle model persistence between restarts?
   - Options: Local storage, mounted volumes, cloud storage
   - Current status: Using default Ollama storage

3. **Scaling**
   - Question: How can we scale to handle multiple models or higher load?
   - Options: Multiple instances, load balancing, larger models
   - Current status: Single instance, single model

### RabbitMQ Considerations

1. **RabbitMQ for Service Discovery**
   - Decision: Use RabbitMQ for service discovery
   - Exchange: service-discovery
   - Routing Key: inference.url.changed
   - Message format includes URL, timestamp, service ID, and version

2. **Error Handling Strategy**
   - Decision: Print errors but continue execution
   - Rationale: Allows for continued operation while handling errors
   - Status: Implemented

3. **Connection Management**
   - Decision: Proper connection cleanup in finally block
   - Rationale: Ensures resources are released even in case of errors
   - Status: Implemented

4. **Logging**
   - Decision: Detailed logging of success/failure
   - Rationale: Helps in debugging and monitoring
   - Status: Implemented

5. **RabbitMQ Connection Stability**
   - Question: How stable is the RabbitMQ connection?
   - Options: Stable, occasionally unstable, frequently unstable
   - Current status: Stable

6. **Message Delivery Reliability**
   - Question: How reliable is message delivery over RabbitMQ?
   - Options: Highly reliable, occasionally unreliable, frequently unreliable
   - Current status: Highly reliable

7. **Error Handling Effectiveness**
   - Question: How effective is the current error handling strategy?
   - Options: Very effective, somewhat effective, not effective
   - Current status: Very effective

8. **Security of Credentials**
   - Question: How secure are the RabbitMQ credentials?
   - Options: Very secure, somewhat secure, not secure
   - Current status: Very secure

## Next Steps for RabbitMQ

1. **Monitor RabbitMQ Message Delivery**
   - Task: Set up monitoring for RabbitMQ message delivery
   - Rationale: Helps in understanding the reliability of the service
   - Status: Not implemented

2. **Consider Adding Retry Logic**
   - Task: Implement retry logic for failed publishes
   - Rationale: Helps in handling transient errors
   - Status: Not implemented

3. **Consider Implementing Periodic URL Updates**
   - Task: Implement periodic URL updates
   - Rationale: Helps in keeping the API URL up-to-date
   - Status: Not implemented

4. **Consider Moving Credentials to Environment Variables**
   - Task: Move RabbitMQ credentials to environment variables
   - Rationale: Helps in securing the configuration
   - Status: Not implemented