---
layer: 3
id: auth-manifest
scope: building-blocks/authentication
# [Specialized Rules for Auth] Security and Identity Provider Resiliency
requires: OR.02, OR.03, DL.01
---

# 🔐 VK.Blocks Authentication Manifest (Layer 3)

The Authentication module handles sensitive identity data and external Identity Providers (IDPs).

## Authentication Specific Extensions (L3)
1. **Security & PII (OR.02)**: Specifically, mask all identity tokens and PII in standard telemetry; ensure `TenantId` is present in all identity contexts.
2. **IDP Resiliency (OR.03)**: External OIDC/OAuth provider calls MUST be wrapped in a circuit-breaker to prevent startup-blocking during IDP outages.
3. **Identity Testing (DL.01)**: Mandatory failure scenario testing for claim-mismatches and expired-token boundaries.
