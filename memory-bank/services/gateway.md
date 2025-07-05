# Gateway Reference

The Gateway is built with **Ocelot** and serves as the single ingress point for all client traffic.  It applies cross-cutting concerns – authentication, rate limiting, CORS, and error translation – before proxying to downstream micro-services.

## Configuration
Ocelot routes are split by downstream service for maintainability:

```
Gateway/
  ocelot.auth.json
  ocelot.conversation.json
  ocelot.explore.json
  ocelot.admin.json
```
At runtime the files are merged into a single configuration.

### Example Route (snip)
```json
{
  "RouteKeys": ["conversation"],
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{"Host": "conversation-service", "Port": 80}],
  "UpstreamPathTemplate": "/api/{everything}",
  "UpstreamHttpMethod": ["GET","POST","PUT","DELETE"],
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer",
    "AllowedScopes": []
  }
}
```

## Middleware Pipeline
1. **JWT Handler** – Validates token & adds claims.
2. **Cache** (optional) – Uses Redis for GET requests.
3. **Rate Limiter** – Token bucket per IP & user.
4. **Load Balancer** – Round-robin across replica pods.
5. **Downstream Proxy** – Forwards to service.

## Health
`GET /gateway/health` – Synthetic check verifying downstream services are reachable.

## Running Locally
```bash
cd Gateway
export ASPNETCORE_ENVIRONMENT=Development
export Jwt__Secret="super-secret"
dotnet run
```
Gateway listens on `http://localhost:5000` by default.

---
*Last updated: {{date}}*