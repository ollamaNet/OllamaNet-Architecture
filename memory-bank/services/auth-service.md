# AuthService API Reference

Base URL (through Gateway): `/api/Auth`

Authentication flows use JSON Web Tokens (JWT) stored in the `Authorization: Bearer <token>` header and an **http-only** `refreshToken` cookie for silent renewal.

---
## Endpoints

| Method | Path | Body | Auth | Description |
|--------|------|------|------|-------------|
| GET | `/` | — | `Admin` | Proof‐of‐life endpoint restricted to Admin role. |
| POST | `/register` | `RegisterModel` | Public | Register a new user and receive JWT + refresh token. |
| POST | `/login` | `TokenRequestModel` | Public | Authenticate existing user. |
| POST | `/updateprofile` | `UpdateProfileModel` | `User` | Update email / username. |
| POST | `/changepassword` | `ChangePasswordModel` | `User` | Change password when logged-in. |
| POST | `/forgotpassword` | `ForgotPasswordRequestModel` | Public | Initiate password reset email. |
| POST | `/resetpassword` | `ResetPasswordModel` | Public | Complete password reset with token. |
| POST | `/assignrole` | `RoleModel` | `Admin` | Grant a role to a user. |
| POST | `/deassignrole` | `RoleModel` | `Admin` | Remove a role from a user. |
| GET | `/RefreshToken` | — | Cookie | Get fresh access/refresh tokens using cookie. |
| POST | `/logout` | `{ "refreshToken": "…" }` | Cookie | Invalidate refresh token and clear cookie. |
| GET | `/getroles/{userId}` | — | `Admin` | List roles for specified user. |

### Data Contracts

```csharp
// RegisterModel
{
  "Email": "user@example.com",
  "Username": "user123",
  "Password": "P@ssw0rd!"
}

// TokenRequestModel
{
  "Email": "user@example.com",
  "Password": "P@ssw0rd!"
}
```

### Example – Register
```bash
curl -X POST https://api.ollamanet.dev/api/Auth/register \
     -H "Content-Type: application/json" \
     -d '{ "email": "alice@example.com", "username": "alice", "password": "S3cur3!" }'
```
Successful response (200):
```json
{
  "token": "<jwt>",
  "refreshToken": "<guid>",
  "refreshTokenExpiration": "2025-04-09T10:52:22Z",
  "email": "alice@example.com",
  "roles": ["User"]
}
```

### Refresh Token Flow
1. Client stores the `refreshToken` http-only cookie.
2. When the JWT nears expiry, call `GET /RefreshToken`; server rotates refresh token.
3. Updated JWT returned in JSON; new cookie set.

---
*Last updated: {{date}}*