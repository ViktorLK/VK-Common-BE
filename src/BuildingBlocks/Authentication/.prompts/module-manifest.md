---
layer: 3
id: auth-manifest
scope: building-blocks/authentication
# [Specialized Rules for Auth] Security and Identity Provider Resiliency
requires: OR.02, OR.03
requires: DL.01
---

# 🔐 VK.Blocks Authentication Manifest (Layer 3)

The Authentication module handles sensitive identity data and external Identity Providers (IDPs).

## Authentication Mandates
1. **Security & PII (OR.02)**: Ensure all tokens, secrets, and user PII are masked in logs and handled with zero-leakage protocols.
2. **IDP Resiliency (OR.03)**: External IDP calls (OIDC, OAuth) MUST handle transient network failures via standard retry policies.
3. **Identity Testing (DL.01)**: Comprehensive testing for claim mapping, token validation, and failure scenarios is mandatory.
