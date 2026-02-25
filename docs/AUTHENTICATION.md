# 🔐 Authentication & Authorization Guide

> Complete reference for authentication, authorization, and security in the Shop Microservices platform.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Obtaining a Token](#2-obtaining-a-token)
3. [Using the Token](#3-using-the-token)
4. [JWT Claims](#4-jwt-claims)
5. [User Context in Microservices](#5-user-context-in-microservices)
6. [Roles & Permissions](#6-roles--permissions)
7. [Security Best Practices](#7-security-best-practices)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. Overview

### How Authentication Works

The Shop Microservices platform uses **JWT (JSON Web Token) Bearer Authentication** with a centralized gateway pattern. Authentication is handled **exclusively** by the API Gateway — downstream microservices never validate JWTs directly. Instead, they receive pre-extracted user information via trusted HTTP headers.

This design provides:

- **Single point of authentication** — only the Gateway needs JWT configuration
- **Simplified microservices** — no token validation logic in each service
- **Consistent security** — all requests pass through the same auth pipeline
- **Performance** — tokens are validated once, not per microservice

### Authentication Flow

```
┌────────┐       ┌──────────────┐       ┌──────────────────┐       ┌──────────────┐
│ Client │       │  API Gateway │       │  JwtClaims       │       │ Microservice │
│        │       │  (Port 5000) │       │  Middleware       │       │ (Downstream) │
└───┬────┘       └──────┬───────┘       └────────┬─────────┘       └──────┬───────┘
    │                   │                        │                        │
    │  1. POST /api/auth/login                   │                        │
    │  { email, password }                       │                        │
    │──────────────────>│                        │                        │
    │                   │                        │                        │
    │  2. Validate credentials                   │                        │
    │     Generate JWT token                     │                        │
    │                   │                        │                        │
    │  3. { token, expiresAt, tokenType }        │                        │
    │<──────────────────│                        │                        │
    │                   │                        │                        │
    │  4. GET /api/catalog/products              │                        │
    │  Authorization: Bearer <token>             │                        │
    │──────────────────>│                        │                        │
    │                   │                        │                        │
    │                   │  5. Validate JWT        │                        │
    │                   │     (signature, expiry, │                        │
    │                   │      issuer, audience)  │                        │
    │                   │                        │                        │
    │                   │  6. Extract claims      │                        │
    │                   │─────────────────────── >│                        │
    │                   │                        │                        │
    │                   │                        │  7. Forward request     │
    │                   │                        │     + inject headers:   │
    │                   │                        │     X-User-Id           │
    │                   │                        │     X-User-Email        │
    │                   │                        │     X-User-Roles        │
    │                   │                        │───────────────────────> │
    │                   │                        │                        │
    │                   │                        │                        │  8. Read headers
    │                   │                        │                        │     via IUserContext
    │                   │                        │                        │
    │                   │                        │  9. Response            │
    │                   │                        │<─────────────────────── │
    │  10. Response     │                        │                        │
    │<──────────────────│                        │                        │
    │                   │                        │                        │
```

### Key Components

| Component | Location | Responsibility |
|---|---|---|
| `AuthController` | `ApiGateway/Controllers/` | Login endpoint, user info |
| `AuthService` | `Shared/Auth/Services/` | Credential validation (BCrypt) |
| `JwtTokenGenerator` | `Shared/Auth/Services/` | JWT creation (HMAC-SHA256) |
| `JwtClaimsMiddleware` | `ApiGateway/Middleware/` | Extract claims → inject headers |
| `HttpUserContext` | `Shared/UserContext/` | Read user headers in microservices |
| `IUserContext` | `Shared/UserContext/` | Interface for accessing user info |

---

## 2. Obtaining a Token

### Endpoint

```
POST /api/auth/login
```

### Request

```json
{
  "email": "user@example.com",
  "password": "Test@123"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `email` | `string` | ✅ | User's registered email address |
| `password` | `string` | ✅ | User's plain-text password |

### Response — Success (`200 OK`)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyLTAwMSIsImVtYWlsIjoidXNlckBleGFtcGxlLmNvbSIsImp0aSI6ImE3YjYzZjRhLTgwY2EtNDkzOC1hYjdkLWU4NzUzZDE4ZjBhNCIsImlhdCI6MTcwOTE1MDQwMCwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTcwOTE1NDAwMCwiaXNzIjoiU2hvcE1pY3Jvc2VydmljZXMiLCJhdWQiOiJTaG9wTWljcm9zZXJ2aWNlc0NsaWVudHMifQ.SIGNATURE",
  "expiresAt": "2026-02-24T00:32:00Z",
  "tokenType": "Bearer"
}
```

| Field | Type | Description |
|---|---|---|
| `token` | `string` | The signed JWT token string |
| `expiresAt` | `datetime` | Token expiration timestamp (UTC) |
| `tokenType` | `string` | Always `"Bearer"` |

### Response — Failure (`401 Unauthorized`)

```json
{
  "message": "Invalid email or password."
}
```

### Test Users

The platform includes in-memory mock users for development and testing:

| Role | Email | Password | User ID |
|---|---|---|---|
| **Regular User** | `user@example.com` | `Test@123` | `user-001` |
| **Administrator** | `admin@example.com` | `Admin@123` | `admin-001` |

> ⚠️ **Note:** These are demo-only users stored in memory with BCrypt-hashed passwords. In production, integrate a proper identity provider (e.g., database, Identity Server, or external OAuth2/OIDC).

---

## 3. Using the Token

Include the token in every authenticated request using the `Authorization` header:

```
Authorization: Bearer <token>
```

### Example with `curl`

```bash
# Step 1: Login and capture the token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Test@123"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

# Step 2: Use the token in authenticated requests
curl -X GET http://localhost:5000/api/catalog/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"

# Step 3: Create an order (requires authentication)
curl -X POST http://localhost:5000/api/ordering/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {"productId": 1, "productName": "Smartphone", "price": 999.99, "quantity": 1}
    ],
    "shippingAddress": {
      "street": "123 Main St",
      "city": "TechCity",
      "state": "TS",
      "zipCode": "12345",
      "country": "USA"
    }
  }'
```

### Example with Postman

1. **Get the Token:**
   - Create a `POST` request to `http://localhost:5000/api/auth/login`
   - Set **Body** → **raw** → **JSON**:
     ```json
     {
       "email": "user@example.com",
       "password": "Test@123"
     }
     ```
   - Send the request and copy the `token` value from the response

2. **Use the Token:**
   - Create a new request (e.g., `GET http://localhost:5000/api/catalog/products`)
   - Go to the **Authorization** tab
   - Set **Type** to `Bearer Token`
   - Paste the token into the **Token** field
   - Send the request

3. **Automate with Postman Variables (Optional):**
   - In the login request's **Tests** tab, add:
     ```javascript
     var jsonData = pm.response.json();
     pm.environment.set("auth_token", jsonData.token);
     ```
   - In subsequent requests, use `{{auth_token}}` as the Bearer Token value

### Example with JavaScript (`fetch`)

```javascript
// Login
async function login(email, password) {
  const response = await fetch('http://localhost:5000/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  if (!response.ok) {
    throw new Error('Login failed');
  }

  const data = await response.json();
  return data.token;
}

// Authenticated request
async function getProducts(token) {
  const response = await fetch('http://localhost:5000/api/catalog/products', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  return response.json();
}

// Usage
(async () => {
  const token = await login('user@example.com', 'Test@123');
  const products = await getProducts(token);
  console.log(products);
})();
```

### Example with C# (`HttpClient`)

```csharp
using var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Login
var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
{
    email = "user@example.com",
    password = "Test@123"
});

var tokenData = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

// Authenticated request
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", tokenData.Token);

var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/catalog/products");
```

---

## 4. JWT Claims

### Token Structure

A JWT consists of three Base64URL-encoded parts separated by dots:

```
Header.Payload.Signature
```

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9    ← Header
.
eyJzdWIiOiJ1c2VyLTAwMSIsImVtYWlsIjoi... ← Payload (Claims)
.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c ← Signature
```

### Header

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

| Field | Value | Description |
|---|---|---|
| `alg` | `HS256` | HMAC-SHA256 signing algorithm |
| `typ` | `JWT` | Token type |

### Payload (Claims)

```json
{
  "sub": "user-001",
  "email": "user@example.com",
  "jti": "a7b63f4a-80ca-4938-ab7d-e8753d18f0a4",
  "iat": 1709150400,
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "User",
  "exp": 1709154000,
  "iss": "ShopMicroservices",
  "aud": "ShopMicroservicesClients"
}
```

| Claim | Key | Type | Description |
|---|---|---|---|
| **Subject** | `sub` | `string` | User's unique identifier (e.g., `user-001`) |
| **Email** | `email` | `string` | User's email address |
| **JWT ID** | `jti` | `string` | Unique identifier for this specific token (UUID) |
| **Issued At** | `iat` | `integer` | Unix timestamp when the token was created |
| **Role** | `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` | `string` | User's role(s). Repeated for multiple roles |
| **Expiration** | `exp` | `integer` | Unix timestamp when the token expires |
| **Issuer** | `iss` | `string` | Token issuer: `ShopMicroservices` |
| **Audience** | `aud` | `string` | Intended audience: `ShopMicroservicesClients` |

> **Note on Role Claims:** ASP.NET Core uses the full URI `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` as the claim type for roles. When a user has multiple roles (e.g., Admin has both `User` and `Admin`), the claim appears multiple times in the JWT payload.

### How to Decode a Token

1. **[jwt.io](https://jwt.io)** — Paste the token in the debugger to inspect claims
2. **[jwt.ms](https://jwt.ms)** — Microsoft's JWT decoder
3. **Command Line:**
   ```bash
   # Decode the payload (middle part)
   echo "PAYLOAD_PART" | base64 -d 2>/dev/null | python3 -m json.tool
   ```
4. **C# Code:**
   ```csharp
   var handler = new JwtSecurityTokenHandler();
   var jsonToken = handler.ReadToken(tokenString) as JwtSecurityToken;
   Console.WriteLine($"User ID: {jsonToken?.Subject}");
   Console.WriteLine($"Expires: {jsonToken?.ValidTo}");
   ```

---

## 5. User Context in Microservices

### Architecture

Downstream microservices (Catalog, Basket, Ordering) **do not** validate JWT tokens themselves. Instead, the API Gateway validates the token and injects user information as HTTP headers before forwarding the request via Ocelot.

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Gateway                              │
│                                                                 │
│  JWT Validation  ──>  JwtClaimsMiddleware  ──>  Ocelot Proxy   │
│                           │                                     │
│                    Extracts claims and                           │
│                    sets headers:                                 │
│                      X-User-Id                                  │
│                      X-User-Email                               │
│                      X-User-Roles                               │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                    (internal network)
                              │
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
   ┌─────────────┐   ┌──────────────┐   ┌──────────────┐
   │ Catalog API  │   │  Basket API  │   │ Ordering API │
   │             │   │              │   │              │
   │ IUserContext │   │ IUserContext  │   │ IUserContext  │
   │ reads the   │   │ reads the    │   │ reads the    │
   │ X-User-*   │   │ X-User-*    │   │ X-User-*    │
   │ headers     │   │ headers      │   │ headers      │
   └─────────────┘   └──────────────┘   └──────────────┘
```

### Headers Injected by the Gateway

The `JwtClaimsMiddleware` extracts claims from the validated JWT and injects the following headers:

| Header | Source Claim | Example Value | Description |
|---|---|---|---|
| `X-User-Id` | `sub` / `ClaimTypes.NameIdentifier` | `user-001` | User's unique identifier |
| `X-User-Email` | `email` / `ClaimTypes.Email` | `user@example.com` | User's email address |
| `X-User-Roles` | `ClaimTypes.Role` | `User,Admin` | Comma-separated list of roles |

### The `IUserContext` Interface

Microservices access user information through the `IUserContext` interface, which is registered via dependency injection:

```csharp
public interface IUserContext
{
    /// <summary>User's unique identifier (from X-User-Id header).</summary>
    string? UserId { get; }

    /// <summary>User's email address (from X-User-Email header).</summary>
    string? Email { get; }

    /// <summary>User's roles (from X-User-Roles header).</summary>
    List<string> Roles { get; }

    /// <summary>True if UserId is present and non-empty.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Checks if the user has a specific role (case-insensitive).</summary>
    bool IsInRole(string role);
}
```

### Usage Example in a Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUserContext _userContext;

    public OrdersController(IUserContext userContext)
    {
        _userContext = userContext;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        // Check authentication
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized();
        }

        // Access user information
        var userId = _userContext.UserId;   // "user-001"
        var email  = _userContext.Email;    // "user@example.com"

        // Check admin role
        if (_userContext.IsInRole("Admin"))
        {
            // Admin-specific logic...
        }

        // Create order with user context
        var order = await _orderService.CreateOrderAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
}
```

### Registering `IUserContext` in a Microservice

Each microservice registers the user context in its service configuration:

```csharp
using UserContext.Extensions;

// In ServiceExtensions.cs or Program.cs:
services.AddUserContext();
```

This registers:
- `IHttpContextAccessor` — for accessing the current HTTP context
- `HttpUserContext` as `IUserContext` (scoped) — reads `X-User-*` headers per request

---

## 6. Roles & Permissions

### Available Roles

| Role | Description |
|---|---|
| `User` | Standard user. Can browse products, manage their own basket, and create orders |
| `Admin` | Administrator. Has all `User` permissions plus access to admin-only operations |

### Permission Matrix

| Operation | Endpoint | Anonymous | User | Admin |
|---|---|---|---|---|
| **Login** | `POST /api/auth/login` | ✅ | ✅ | ✅ |
| **Get Current User** | `GET /api/auth/me` | ❌ 401 | ✅ | ✅ |
| **Admin Endpoint** | `GET /api/auth/admin-only` | ❌ 401 | ❌ 403 | ✅ |
| | | | | |
| **List Categories** | `GET /api/catalog/categories` | ✅ | ✅ | ✅ |
| **Get Category** | `GET /api/catalog/categories/{id}` | ✅ | ✅ | ✅ |
| **Create Category** | `POST /api/catalog/categories` | ❌ 401 | ✅ | ✅ |
| **Update Category** | `PUT /api/catalog/categories/{id}` | ❌ 401 | ✅ | ✅ |
| **Delete Category** | `DELETE /api/catalog/categories/{id}` | ❌ 401 | ❌ 403 | ✅ |
| | | | | |
| **List Products** | `GET /api/catalog/products` | ✅ | ✅ | ✅ |
| **Get Product** | `GET /api/catalog/products/{id}` | ✅ | ✅ | ✅ |
| **Create Product** | `POST /api/catalog/products` | ❌ 401 | ✅ | ✅ |
| **Update Product** | `PUT /api/catalog/products/{id}` | ❌ 401 | ✅ | ✅ |
| **Delete Product** | `DELETE /api/catalog/products/{id}` | ❌ 401 | ❌ 403 | ✅ |
| | | | | |
| **Get Basket** | `GET /api/basket` | ❌ 401 | ✅ | ✅ |
| **Add to Basket** | `POST /api/basket/items` | ❌ 401 | ✅ | ✅ |
| **Update Basket Item** | `PUT /api/basket/items/{productId}` | ❌ 401 | ✅ | ✅ |
| **Remove from Basket** | `DELETE /api/basket/items/{productId}` | ❌ 401 | ✅ | ✅ |
| **Clear Basket** | `DELETE /api/basket` | ❌ 401 | ✅ | ✅ |
| | | | | |
| **Create Order** | `POST /api/ordering/orders` | ❌ 401 | ✅ | ✅ |
| **Get My Orders** | `GET /api/ordering/orders/my` | ❌ 401 | ✅ | ✅ |
| **Get Order by ID** | `GET /api/ordering/orders/{id}` | ✅ | ✅ | ✅ |
| **Get All Orders** | `GET /api/ordering/orders/all` | ❌ 401 | ❌ 403 | ✅ |
| **Update Order Status** | `PUT /api/ordering/orders/{id}/status` | ✅ | ✅ | ✅ |
| **Cancel Order** | `DELETE /api/ordering/orders/{id}` | ✅ | ✅ | ✅ |

### How Role Checks Work

**At the API Gateway** (for Gateway-local controllers like `AuthController`):

```csharp
// Uses ASP.NET Core's built-in [Authorize] attribute
[Authorize(Roles = "Admin")]
public IActionResult GetAdminData() { ... }
```

**At downstream microservices** (using `IUserContext`):

```csharp
// Manual check — returns 403 Forbidden
if (!_userContext.IsInRole("Admin"))
{
    return StatusCode(StatusCodes.Status403Forbidden);
}
```

> **Why `StatusCode(403)` instead of `Forbid()`?** Downstream microservices do not have a registered authentication scheme. Using `Forbid()` requires an authentication handler to generate the challenge response. Since authentication is handled by the Gateway, we return `403` directly.

---

## 7. Security Best Practices

### Current Implementation

| Practice | Status | Details |
|---|---|---|
| **JWT Signing** | ✅ Implemented | HMAC-SHA256 symmetric signing |
| **Password Hashing** | ✅ Implemented | BCrypt with automatic salt generation |
| **Token Expiration** | ✅ Implemented | 60 minutes (configurable via `Jwt:ExpirationMinutes`) |
| **Zero Clock Skew** | ✅ Implemented | `ClockSkew = TimeSpan.Zero` for strict expiration |
| **Issuer Validation** | ✅ Implemented | Validates `iss` claim matches `ShopMicroservices` |
| **Audience Validation** | ✅ Implemented | Validates `aud` claim matches `ShopMicroservicesClients` |
| **Rate Limiting** | ✅ Implemented | Ocelot rate limiting (100 requests/minute on Catalog) |
| **Minimal Secret Key** | ✅ Enforced | 256-bit minimum (32 characters) at startup |
| **BCrypt Cost Factor** | ✅ Default | BCrypt default work factor (10 rounds) |

### Production Recommendations

#### 1. Always Use HTTPS

```yaml
# docker-compose.override.yml (production)
api-gateway:
  environment:
    - ASPNETCORE_URLS=https://+:443;http://+:80
    - ASPNETCORE_Kestrel__Certificates__Default__Path=/certs/cert.pfx
    - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
```

#### 2. Use a Strong Secret Key

```bash
# Generate a cryptographically secure 64-character key
openssl rand -base64 48

# Set it in your .env file (NEVER commit this!)
JWT_SECRET_KEY=your-generated-key-here
```

> ⚠️ **Critical:** The default key in `.env.example` is for development only. Always generate a unique key for production environments. Minimum 32 characters (256 bits).

#### 3. Configure Token Expiration

```yaml
# Shorter lifetime = more secure (recommended: 15-30 min for production)
api-gateway:
  environment:
    - Jwt__ExpirationMinutes=15
```

#### 4. Refresh Tokens

🚧 **Not yet implemented** — Planned for a future release.

Current workaround: Re-authenticate when the token expires.

Planned implementation:
- Refresh token with longer expiration stored server-side
- `POST /api/auth/refresh` endpoint
- Token rotation on each refresh

#### 5. Secure Environment Variables

```bash
# NEVER commit secrets to version control
echo ".env" >> .gitignore

# Use secrets management in production:
# - Azure Key Vault
# - AWS Secrets Manager
# - Docker Secrets
# - HashiCorp Vault
```

#### 6. CORS Configuration

Currently configured as `AllowAll` for development. In production:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourapp.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

## 8. Troubleshooting

### `401 Unauthorized`

**Meaning:** The request lacks valid authentication credentials.

**Common Causes:**

| Cause | Solution |
|---|---|
| Missing `Authorization` header | Add `Authorization: Bearer <token>` to the request |
| Token has expired | Obtain a new token via `POST /api/auth/login` |
| Invalid or malformed token | Verify the token at [jwt.io](https://jwt.io) |
| Typo in `Bearer` prefix | Ensure the header is exactly `Authorization: Bearer <token>` (with a space) |
| Wrong credentials at login | Double-check email and password |

**Debugging steps:**

```bash
# 1. Check if you have a token
echo $TOKEN

# 2. Verify the token is valid (decode the payload)
echo $TOKEN | cut -d'.' -f2 | base64 -d 2>/dev/null

# 3. Check the expiration claim (exp) against current Unix time
date +%s

# 4. Get a fresh token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Test@123"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
```

### `403 Forbidden`

**Meaning:** The user is authenticated but lacks the required role/permission.

**Common Causes:**

| Cause | Solution |
|---|---|
| Regular user accessing admin endpoint | Login as an admin user (`admin@example.com`) |
| Missing role claim in token | Verify the user's roles in the `AuthService` |
| Role name mismatch (case-sensitive in JWT) | Roles are checked case-insensitively by `IUserContext` |

**Debugging steps:**

```bash
# 1. Check your current user's roles
curl -s http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer $TOKEN" | python3 -m json.tool

# 2. Login as admin for admin-only endpoints
ADMIN_TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin@123"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

# 3. Retry with admin token
curl -s http://localhost:5000/api/ordering/orders/all \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

### Token Expired — How to Renew

```bash
# Simply re-authenticate to get a new token (valid for 60 minutes)
curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Test@123"}'
```

### Gateway Returns `401` But Microservice Should Allow Anonymous

If an Ocelot route has `AuthenticationOptions` configured, all requests to that route require a valid JWT. To allow anonymous access to specific downstream endpoints, either:

1. Remove `AuthenticationOptions` from the Ocelot route (allows all requests through)
2. Create a separate Ocelot route without authentication for the public endpoints

### Microservice Returns `401` Unexpectedly

If `IUserContext.IsAuthenticated` returns `false`:

1. **Verify the Gateway is injecting headers** — check the `X-User-Id` header in the downstream request
2. **Check Ocelot route configuration** — ensure `AuthenticationOptions` is set so the Gateway validates the token
3. **Check middleware order** in the Gateway's `Program.cs`:

```csharp
// Correct order:
app.UseAuthentication();    // Validates the JWT
app.UseAuthorization();     // Evaluates [Authorize] attributes
app.UseJwtClaimsMiddleware(); // Injects X-User-* headers
await app.UseOcelot();      // Forwards to downstream services
```

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────────┐
│                  AUTHENTICATION CHEAT SHEET                 │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  LOGIN:                                                     │
│  POST http://localhost:5000/api/auth/login                  │
│  Body: {"email":"user@example.com","password":"Test@123"}   │
│                                                             │
│  USE TOKEN:                                                 │
│  Header: Authorization: Bearer <token>                      │
│                                                             │
│  TEST USERS:                                                │
│  User:  user@example.com  / Test@123   (roles: User)        │
│  Admin: admin@example.com / Admin@123  (roles: User, Admin) │
│                                                             │
│  CHECK YOURSELF:                                            │
│  GET http://localhost:5000/api/auth/me                      │
│                                                             │
│  ERRORS:                                                    │
│  401 = Missing/invalid/expired token                        │
│  403 = Valid token but insufficient role                    │
│                                                             │
│  TOKEN LIFETIME: 60 minutes                                 │
│  ALGORITHM: HMAC-SHA256                                     │
│  ISSUER: ShopMicroservices                                  │
│  AUDIENCE: ShopMicroservicesClients                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```
