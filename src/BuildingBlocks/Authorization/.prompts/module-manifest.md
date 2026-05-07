---
layer: 3
id: authz-manifest
scope: building-blocks/authorization
# [Specialized Rules for AuthZ] Multi-tenancy and Policy Integrity
requires: OR.02
requires: CS.02
requires: DL.01
---

# 🛡️ VK.Blocks Authorization Manifest (Layer 3)

The Authorization module enforces access control and multi-tenant isolation.

## Authorization Mandates
1. **Tenant Isolation (OR.02)**: Verify that all authorization checks correctly propagate and validate `TenantId` context.
2. **Policy Purity (CS.02)**: Authorization handlers and requirements MUST NOT depend on database implementation details; use domain-level policy abstractions.
3. **Failure Scenarios (DL.01)**: Testing must explicitly cover "Forbidden" (403) scenarios and role/permission mismatches.
