---
layer: 3
id: authz-manifest
scope: building-blocks/authorization
# [Specialized Rules for AuthZ] Multi-tenancy and Policy Integrity
requires: OR.02, CS.02, DL.01
---

# 🛡️ VK.Blocks Authorization Manifest (Layer 3)

The Authorization module enforces access control and multi-tenant isolation.

## Authorization Specific Extensions (L3)
1. **Tenant Isolation (OR.02)**: Specifically, ensure `TenantId` is verified at the entry point of every evaluation pipeline.
2. **Policy Purity (CS.02)**: Authorization logic MUST use domain-level abstractions; no raw database context access within policy handlers.
3. **Failure Scenarios (DL.01)**: Mandatory negative testing for "Forbidden" (403) and role-mismatch boundaries.
