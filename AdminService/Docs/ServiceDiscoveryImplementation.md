# Service Discovery Implementation - AdminService

## Overview

This document provides details about the service discovery implementation in AdminService. The implementation enables dynamic updating of the Inference Engine URL using RabbitMQ messaging and Redis caching, allowing for improved resilience and flexibility in the system architecture.

## Architecture

The service discovery architecture follows a message-based approach:

```
┌─────────────┐      ┌──────────┐      ┌───────────────┐
│ Discovery   │──┬──>│ RabbitMQ │─────>│ AdminService  │
│ Service     │  │   └──────────┘      │ Consumer      │
└─────────────┘  │                     └───────┬───────┘
                 │                             │
                 │                             V
┌─────────────┐  │                     ┌───────────────┐
│ Other       │  │                     │ Redis Cache   │
│ Services    │  │                     └───────┬───────┘
└─────────────┘  │                             │
                 │                             V
┌─────────────┐  │                     ┌───────────────┐
│ Inference   │──┘                     │ Inference     │
│ Engine      │                        │ Engine        │
│ Registry    │                        │ Connector     │
└─────────────┘                        └───────────────┘
```

## Key Components

### 1. Infrastructure

#### Caching
- **ICacheManager**: Interface for caching operations
- **RedisCacheService**: Redis-based implementation of ICacheManager
- **CacheKeys**: Centralized cache key management

#### Configuration
- **IInferenceEngineConfiguration**: Interface for managing inference engine configuration
- **InferenceEngineConfiguration**: Implementation that manages the base URL using cache and configuration
- **InferenceEngineOptions**: Options class for inference engine configuration

#### Validation
- **IUrlValidator**: Interface for URL validation
- **UrlValidator**: Implementation of URL validation logic

#### Messaging
- **InferenceUrlUpdateMessage**: Message schema for URL update messages
- **RabbitMQResiliencePolicies**: Resilience policies for RabbitMQ operations
- **InferenceUrlConsumer**: Background service that consumes URL update messages
- **RabbitMQOptions**: Options class for RabbitMQ configuration

### 2. Connectors

#### Interfaces
- **IInferenceEngineConnector**: New interface for connecting to inference engines
- **IOllamaConnector**: Legacy interface for connecting to Ollama (kept for backward compatibility)

#### Implementation
- **InferenceEngineConnector**: Implements both interfaces for a smooth transition

### 3. Service Integration

- **InferenceOperationsService**: Updated to use IInferenceEngineConnector
- **HealthController**: New controller for monitoring system health

## Flow of Operation

1. When the application starts, it initializes:
   - Redis cache connection
   - RabbitMQ consumer for URL updates
   - InferenceEngineConnector with the configured/cached URL

2. When a URL update message is received:
   - RabbitMQ consumer receives the message
   - URL is validated
   - Valid URL is stored in Redis cache
   - InferenceEngineConfiguration is updated

3. When services need to communicate with the inference engine:
   - InferenceEngineConnector retrieves the current URL from configuration
   - If the URL has changed, a new OllamaApiClient is created
   - API calls are made using the current client

## Resilience Features

1. **Circuit Breakers**: Prevent cascading failures when RabbitMQ is unreachable
2. **Retry Policies**: Handle transient failures in RabbitMQ operations
3. **Fallback Mechanisms**: Default to configuration if cache is unavailable
4. **Health Monitoring**: Endpoints for checking connection status

## Migration Path

The implementation follows a gradual transition approach:

1. Both IOllamaConnector and IInferenceEngineConnector are supported
2. InferenceEngineConnector implements both interfaces
3. Services can be gradually migrated to use the new connector
4. Once all services are migrated, the legacy connector can be removed

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-connection-string"
  },
  "InferenceEngine": {
    "BaseUrl": "http://localhost:11434"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "service-discovery",
    "InferenceUrlQueue": "inference-url-updates",
    "InferenceUrlRoutingKey": "inference.url.changed"
  }
}
```

## Health Monitoring

Health endpoints are available to monitor the system:

- `GET /api/admin/health/rabbitmq`: Check RabbitMQ connection health
- `GET /api/admin/health/inference-engine`: Check inference engine URL and connection status 