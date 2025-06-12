# Gateway Authentication and Authorization Flow Documentation

This document explains the sequence diagram that illustrates the authentication and authorization flow in the Gateway API. The diagram shows how user requests flow through JWT authentication, role authorization, claims forwarding, and finally to the target service.

## Participants

1. **End User**: The client that initiates requests with a JWT token.
2. **Gateway API**: The entry point for all requests in the system.
3. **JWT Authentication**: The middleware that validates JWT tokens.
4. **Role Authorization Middleware**: The middleware that checks user roles against required roles for endpoints.
5. **Claims Forwarding Middleware**: The middleware that extracts and forwards user claims as headers.
6. **Authentication Service**: The service that handles login and registration requests.
7. **Target Service**: The service that processes the authenticated and authorized request.

## Flow Scenarios

The sequence diagram illustrates four main scenarios:

### 1. Authentication Flow (Login/Registration)

This scenario shows how a user obtains a JWT token through login or registration:

1. User sends a login or registration request to the Gateway.
2. Gateway forwards the request to the Authentication Service.
3. Authentication Service validates the credentials.
4. If successful, the Authentication Service generates a JWT token and returns it to the user via the Gateway.
5. If unsuccessful, an error is returned to the user.

### 2. Authenticated Request Flow (Happy Path)

This scenario shows the normal flow for an authenticated request:

1. User sends a request with a JWT token in the Authorization header.
2. Gateway passes the token to JWT Authentication middleware for validation.
3. If the token is valid, the claims are extracted and the request proceeds to Role Authorization middleware.
4. Role Authorization middleware loads endpoint role requirements from RoleAuthorization.json.
5. Role Authorization middleware compares the user's roles with the required roles for the endpoint.
6. If the user has the required role, the request proceeds to Claims Forwarding middleware.
7. Claims Forwarding middleware extracts user claims (UserId, Email, Roles) and adds them as headers.
8. Gateway forwards the request with the added headers to the appropriate Target Service.
9. Target Service processes the request and returns a response.
10. Gateway returns the response to the user.

### 3. Service Unavailability Scenario

This scenario shows what happens when a service is unavailable:

1. User sends a request with a valid JWT token.
2. Request passes through JWT Authentication, Role Authorization, and Claims Forwarding middleware successfully.
3. Gateway attempts to forward the request to the Target Service, but the service is unavailable.
4. Gateway receives a 503 Service Unavailable response.
5. Gateway forwards the 503 response to the user.

### 4. Service Error Response Scenario

This scenario shows what happens when a service returns an error:

1. User sends a request with a valid JWT token.
2. Request passes through JWT Authentication, Role Authorization, and Claims Forwarding middleware successfully.
3. Gateway forwards the request to the Target Service.
4. Target Service processes the request but encounters an error.
5. Target Service returns a 400 or 500 error response.
6. Gateway forwards the error response to the user.

## Alternative Paths

The sequence diagram also illustrates several alternative paths:

### 1. Invalid Token

If the JWT token is invalid:

1. JWT Authentication middleware returns a 401 Unauthorized response.
2. Gateway forwards the 401 response to the user.

### 2. Token Expired

If the JWT token has expired:

1. JWT Authentication middleware returns a 401 Unauthorized response with an expired message.
2. Gateway forwards the 401 response to the user.

### 3. Insufficient Permissions

If the user lacks the required role for the endpoint:

1. Role Authorization middleware returns a 403 Forbidden response.
2. Gateway forwards the 403 response to the user.

## Implementation Details

### JWT Authentication

JWT Authentication is configured in `ServiceExtenstions.cs` using the `AddJwtAuthentication` method. The middleware validates the token's signature, expiration, issuer, and audience based on the configuration.

### Role Authorization

Role Authorization is implemented in `RoleAuthorizationMiddleware.cs`. The middleware loads endpoint role requirements from `RoleAuthorization.json` and compares the user's roles with the required roles for the endpoint.

### Claims Forwarding

Claims Forwarding is implemented in `ClaimsToHeaderMiddleware.cs`. The middleware extracts user claims from the authenticated request and adds them as headers to be forwarded to downstream services.

### Request Routing

Request routing is configured in `Program.cs` and various JSON configuration files in the `Configurations` folder. The Gateway uses these configurations to determine which service should receive each request.

## Security Considerations

1. **Token Validation**: JWT tokens are validated for signature, expiration, issuer, and audience to prevent tampering and replay attacks.
2. **Role-Based Access Control**: Endpoints are protected based on user roles to ensure that users can only access resources they are authorized for.
3. **Claims Forwarding**: User claims are forwarded to downstream services to maintain the user's identity throughout the request pipeline.

## Conclusion

The Gateway authentication and authorization flow provides a secure and flexible way to handle user authentication, role-based access control, and identity propagation in a microservices architecture. The sequence diagram illustrates the various paths a request can take through the system, including both successful and error scenarios.