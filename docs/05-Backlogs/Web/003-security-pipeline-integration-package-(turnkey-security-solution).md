# Task: Security Pipeline Integration Package (Turnkey Security Solution)
**ID**: WEB-003
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Web/Security/`
**Ref**: docs/01-Architecture/SecurityPipeline.md

## 📝 Description
Create a turnkey security pipeline integration package (e.g. VK.Blocks.Web.Security or VK.Blocks.Security.Pipeline) providing:
1. Ambient Identity Bridge Middleware that activates IVKIdentityContextAccessor.BeginScope(tenantId, userId) from authenticated ClaimsPrincipal.
2. Centralized IVKSecurityAuditHook for auditing authentication and authorization decisions.
3. Turnkey Fluent Configuration builder (AddVKWebSecurityPipeline with WithJwtAuthentication, WithApiKeyAuthentication, WithTenantAuthorization).
4. Endpoint security metadata helpers (.RequireTenantMatch, .RequireRank).

## ✅ DoD (Definition of Done)
- [ ] Security Pipeline Integration Package (Turnkey Security Solution)
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests