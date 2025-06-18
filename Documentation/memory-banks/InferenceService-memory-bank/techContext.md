# Technical Context: Spicy Avocado

## Technologies Used

### Core Technologies

1. **Python**
   - Primary programming language
   - Used for orchestration and process management
   - Version: 3.x

2. **Jupyter Notebook / Cloud Code Editors**
   - Primary runtime environment (Google Colab, Kaggle, Datadog, etc.)
   - Used for code organization, interactive execution, and documentation
   - Designed for ephemeral, restricted environments with limited networking
   - Allows step-by-step execution and verification

3. **Ollama**
   - Local LLM serving framework
   - Provides API for model inference
   - Supports various open-source models
   - Current model: llama3.2:1b

4. **ngrok**
   - Secure tunneling service
   - Creates public endpoints for local services
   - Handles TLS/SSL termination
   - Provides stable public URLs

5. **RabbitMQ**
   - Message broker for service discovery
   - Host: toucan.lmq.cloudamqp.com
   - Port: 5672
   - Virtual Host: ftyqicrl
   - Exchange: service-discovery
   - Routing Key: inference.url.changed

### Supporting Libraries

1. **subprocess**
   - Python standard library module
   - Used for managing external processes

2. **requests**
   - HTTP library for Python
   - Used to verify service status and retrieve ngrok URL

3. **time**
   - Python standard library module
   - Used for implementing delays and retries

4. **os**
   - Python standard library module
   - Used for environment variable management and system commands

5. **pika**
   - RabbitMQ client for Python
   - Used for service discovery and communication

6. **json**
   - Python standard library module
   - Used for message serialization

7. **datetime**
   - Python standard library module
   - Used for timestamp generation

## Development Setup

### Prerequisites

- Python 3.x installed
- Jupyter Notebook environment
- Internet connection for downloading Ollama models
- ngrok account for authentication token

### Environment Configuration

- **OLLAMA_HOST**: Set to "0.0.0.0:11434" to allow external connections
- **OLLAMA_ORIGIN**: Set to "*" to allow cross-origin requests
- **NGROK_AUTH_TOKEN**: Required for ngrok authentication

### Environment

- **Google Colab**
- **GPU: T4**
- **Python 3.12.1**

### Dependencies

- **fastapi==0.110.0**
- **uvicorn==0.29.0**  
- **pika==1.3.2**
- **requests**
- **ngrok**
- **ollama**

## Technical Constraints

1. **System Requirements**
   - Sufficient RAM and disk space for LLM models
   - CPU/GPU requirements depend on the specific Ollama model

2. **Network Constraints**
   - Port 11434 must be available for Ollama
   - Port 4040 used by ngrok for API access
   - Outbound internet access required for ngrok tunneling

3. **Security Considerations**
   - ngrok token should be kept secure
   - No authentication on the Ollama API by default
   - Public URL allows anyone to access the API

4. **Operational Constraints**
   - Free ngrok accounts have limitations on connections and bandwidth
   - Tunnel connection may need to be reestablished periodically
   - Public URL changes each time ngrok restarts

5. **RabbitMQ**
   - Host: toucan.lmq.cloudamqp.com
   - Port: 5672
   - Virtual Host: ftyqicrl
   - Exchange: service-discovery
   - Routing Key: inference.url.changed

6. **Message Format**
   ```json
   {
     "NewUrl": "https://your-ngrok-url.ngrok-free.app",
     "Timestamp": "2024-03-12T12:34:56Z",
     "ServiceId": "inference-engine",
     "Version": "1.0"
   }
   ```

7. **Performance Considerations**
   - RabbitMQ
     - Single connection per publish
     - Durable exchange
     - Topic-based routing
   - Resource Management
     - Connection cleanup
     - Error handling
     - Resource verification

## Dependencies

### External Dependencies

- **Ollama**: Downloaded and installed during setup
- **ngrok**: Downloaded and installed during setup

### Version Requirements

- Latest versions of Ollama and ngrok are recommended
- Python 3.6+ for compatibility with all libraries