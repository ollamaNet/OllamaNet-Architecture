# OllamaNet Platform – Project Brief

## Vision
OllamaNet is a modular, cloud-native AI platform that enables authenticated users and administrators to converse with, manage, and explore Large Language Models (LLMs) through a set of independently deployable micro-services.  The platform aims to provide a production-ready backbone for building rich generative-AI applications while preserving clear separation of concerns, strong security, and easy extensibility.

## Core Objectives
1. **Conversational AI Experience**  – Deliver real-time, streaming chat interactions powered by Ollama-hosted LLMs.
2. **Model Lifecycle Management**  – Allow administrators to install, update, tag, and remove AI models through secure administrative APIs.
3. **User & Role Management**      – Provide robust authentication (JWT) and role-based authorization for end-users (**User**) and operators (**Admin**).
4. **Content Exploration**         – Expose fast, paginated discovery endpoints for browsing available models, tags, and meta-data.
5. **Scalable, Resilient Core**    – Employ proven architectural & resilience patterns (Clean Architecture, CQRS-style services, Redis cache, RabbitMQ retries) so services can scale independently.
6. **First-Class Documentation**    – Ship with a living *Memory Bank* that captures all knowledge required to understand, extend, and operate the system after any reset.

## Out-of-Scope (v1)
• Real-time WebSocket gateway  • Multi-tenancy  • Billing & usage quotas  • Fine-tuning pipeline

## Success Criteria
* 95% automated test coverage on core business logic
* P95 chat latency < 800 ms for a 50-token prompt (baseline model)
* Zero P1 defects in production for 30 days
* < 15 minutes to on-board a new engineer using Memory Bank alone

## Stakeholders
| Role           | Interest                                  |
|----------------|-------------------------------------------|
| Product Owner  | Feature delivery, UX quality              |
| ML Engineer    | Fast model iteration & deployment pipeline|
| DevOps         | Observability, reliability, cost          |
| End-User       | Low-latency, secure chat experience       |
| Admin          | Complete control over users & models      |

## High-Level Requirements
1. Public REST APIs documented via OpenAPI/Swagger for **Gateway** & each micro-service.
2. Streaming responses for chat endpoints using Server-Sent Events (SSE).
3. Centralised identity provider with refresh-token flow (30-day lifetime).
4. Fault-tolerant cache layer backed by **Redis** with per-domain TTLs.
5. Shared SQL Server schema accessed through **Ollama_DB_layer** repositories.
6. Infrastructure as code (IaC) scripts for cloud provisioning (future work).

---
*Last updated: {{date}}*