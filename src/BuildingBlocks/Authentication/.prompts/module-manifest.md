---
layer: 3
id: auth-manifest
scope: building-blocks/authentication
requires: CS.02, CS.06, OR.02, OR.03, AP.04
---

# VK.Blocks Authentication Manifest (Layer 3)

Handles "who you are" — identity verification, token lifecycle, and IDP integration. Strictly excludes "what you can do" (Authorization's exclusive domain).

## Architectural Boundaries

### 1. Security & PII

- All identity tokens and PII (email, phone, device fingerprint) MUST be masked in telemetry/logs. `TenantId` MUST be present in all identity contexts, and cross-tenant token usage MUST be rejected at validation time.

### 2. IDP Resiliency

- External OIDC/OAuth provider calls (discovery endpoint, JWKS fetch, token introspection) MUST be wrapped in a circuit-breaker to prevent startup-blocking or request-blocking during IDP outages.

### 3. Single Validation Pipeline

- Token validation (signature, expiry, audience/issuer, clock skew) MUST flow through exactly one internal pipeline. Ad-hoc JWT parsing/validation scattered across modules is PROHIBITED.

### 4. JWKS Caching & Rotation

- Public key sets MUST be cached with background auto-refresh. Key rotation MUST be handled without blocking in-flight validation or requiring restart.

### 5. Claims Transformation Boundary

- Raw IDP claims MUST be transformed into `VKClaimTypes` through a single, declarative mapping stage. Downstream modules MUST consume only the transformed claim set — never raw IDP payloads.

### 6. Statelessness by Default

- Token-based stateless authentication is the default. Session/Cookie-based state (when required for Web scenarios) MUST be an explicit, opt-in extension — not the baseline contract.

### 7. Strict Authorization Decoupling

- Authentication MUST NOT contain permission, role, or policy evaluation logic of any kind. Any such logic belongs exclusively to the Authorization module. Violating this boundary requires a formal ADR.

### 8. Identity Testing

- Mandatory failure scenario testing for claim-mismatches, expired-token boundaries, signature tampering, and cross-tenant token rejection.

### 9. Brute-Force & Abuse Protection

- Login attempts, token refresh, and MFA verification MUST be subject to rate limiting at the pipeline level, not left to individual endpoint implementations.

### 10. Test-Friendly IDP Abstraction

- IDP integration MUST be mockable via a provider abstraction, enabling integration tests to run without a real external IDP dependency.
