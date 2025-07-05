# Product Context – OllamaNet

## Why OllamaNet Exists
Generative-AI capabilities are advancing rapidly, yet integrating them into production-grade applications remains complex:

* **Fragmented tool-chain** – Developers juggle model runners, tokenizers, streaming transports, and user management.
* **Operational overhead** – Scaling LLM inference & caching demands specialised DevOps knowledge.
* **Security concerns** – Exposing AI models without proper authentication invites misuse.
* **Documentation debt** – AI prototype projects often lack the depth of docs needed for long-term maintenance.

OllamaNet solves these challenges by packaging best-practice micro-service templates, a secure gateway, and a rich Memory Bank so teams can focus on delivering value, not re-inventing plumbing.

## Target Users
1. **Application Developers** building chatbots, copilots, or knowledge-retrieval features.
2. **ML Engineers** needing safe, observable endpoints to serve new checkpoints.
3. **Administrators/Operators** who govern user access, moderate content, and control infrastructure costs.
4. **End-Users** who just want a fast, reliable chat experience.

## Key User Journeys
| Persona | Journey | Success Signal |
|---------|---------|----------------|
| End-User | Sign up -> Log in -> Ask a question -> Receive streamed answer | Latency & accuracy acceptable; feels snappy |
| Admin    | Add new model -> Tag -> Verify availability -> Delete old version | Model appears in Explore within 30s |
| Developer| `npm create myapp` -> fetch chat SDK -> Call `/api/chats/stream` | Minimal boilerplate; typed SDK |

## Experience Goals
* 🎯 **Clarity** – Every API is self-describing via Swagger + Memory Bank.
* ⚡ **Speed**   – P95 chat response < 1 s for small models.
* 🔒 **Trust**   – End-to-end TLS, JWT, and rate-limits by default.
* 🔄 **Continuity** – Seamless conversation history across devices.

## Differentiators
1. **Memory Bank** – Persisted knowledge that survives engineer turnover.
2. **Pluggable Inference Back-Ends** – Swap local Ollama runtime with remote GPU cluster without API changes.
3. **Clean Architecture** – Easy unit-testing & domain separation.
4. **Observability-First** – Structured logging & health endpoints in every service.

---
*Last updated: {{date}}*