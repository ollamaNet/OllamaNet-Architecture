# Product Context: Spicy Avocado

## Why This Project Exists

Spicy Avocado exists to solve the challenge of making LLM models run from cloud code editors (such as Google Colab, Kaggle, and Datadog) accessible over the internet. With the growing popularity of running LLMs in notebook environments, there is a need for a simple, portable way to expose these models to external applications or users—despite the limitations of ephemeral, restricted cloud environments.

## Problems It Solves

1. **Local-Only Limitation**: By default, Ollama models run locally and are only accessible from the same machine. Spicy Avocado overcomes this limitation by creating a secure tunnel to the internet.

2. **Deployment Complexity**: Setting up a proper server environment with public IP addressing and security can be complex. This project simplifies the process using ngrok for tunneling.

3. **Configuration Management**: The project handles the necessary environment variables and configuration settings for both Ollama and ngrok.

4. **Accessibility**: Enables sharing access to powerful LLM capabilities with others without requiring them to install and configure Ollama themselves.

## How It Should Work

1. **Setup Phase**:
   - Install Ollama and ngrok
   - Configure authentication tokens and environment variables

2. **Deployment Phase**:
   - Pull the specified Ollama model
   - Start the Ollama server with proper host configuration
   - Create a ngrok tunnel to the Ollama API port

3. **Operation Phase**:
   - Provide a public URL that forwards requests to the local Ollama API
   - Allow standard Ollama API calls through this public endpoint
   - Maintain the tunnel for as long as the service is needed

## User Experience Goals

1. **Simplicity**: Users should be able to deploy and share their Ollama models with minimal configuration.

2. **Reliability**: The connection should be stable and the service should handle reconnection if issues occur.

3. **Transparency**: Clear feedback about the status of the service, including successful startup and the public URL.

4. **Flexibility**: Support for different Ollama models by changing a simple configuration variable.

5. **Portability**: The solution should work across different environments where Ollama and ngrok can be installed.