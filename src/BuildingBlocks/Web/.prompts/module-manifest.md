---
layer: 3
id: web-manifest
scope: building-blocks/web
requires: CS.02, CS.06, AP.04, BB.07, BB.08, OR.02
---

# VK.Blocks Web Manifest (Layer 3)

Defines the ASP.NET Core presentation-layer infrastructure — controllers, middleware pipeline, response mapping, and API cross-cutting concerns. This is the sole consumer-facing translation layer between `VKResult<T>` (Core) and HTTP.

## Architectural Boundaries

### 1. Controller Base Uniformity

- All API controllers MUST derive from `VKApiController`. Direct use of bare `ControllerBase` for business endpoints is PROHIBITED. Response construction MUST flow through `HandleResult(Result)` — manual `IActionResult` construction bypassing this method is PROHIBITED.

### 2. Single Result-to-Response Mapping Authority

- `VKResult<T>` → HTTP status code / `VKProblemDetails` translation MUST occur through exactly one mapping pipeline. Controllers MUST NOT hand-roll status code decisions (`if (result.IsFailure) return StatusCode(...)`) outside this pipeline.

### 3. Mandatory Middleware Ordering

- The request pipeline MUST register middleware in this fixed order: Exception Handling → Tracing → Multi-Tenant Resolution → Authentication → Authorization → Rate Limiting → Business Endpoints. Reordering any of these without a formal ADR is PROHIBITED, as it silently breaks exception capture, trace parenting, or tenant-scoped authorization.

### 4. Model Binding Failure Uses EH Contract

- ASP.NET Core's default `ModelState`-based 400 response MUST be disabled/overridden. Model binding and validation failures MUST surface as `VKErrorDetail[]` through the same EH response contract as all other errors — no dual error format is permitted.

### 5. Early Multi-Tenant Resolution

- TenantId resolution (Route → Header chain, per Core's established resolver order) MUST complete before Authorization, Rate Limiting, and any tenant-scoped logging occurs. Resolving TenantId inside individual endpoint handlers is PROHIBITED.

### 6. Centralized Serialization Configuration

- All JSON serialization MUST consume Core's `IVKJsonSerializer` configuration. Per-controller or per-endpoint `JsonSerializerOptions` overrides are PROHIBITED outside a documented exception.

### 7. Security Headers by Default

- Standard security headers (`X-Content-Type-Options`, `Strict-Transport-Security`, `Content-Security-Policy`) MUST be injected globally via middleware. Per-controller opt-in is PROHIBITED — omission must be an explicit, documented exception, not the default.

### 8. Graceful Shutdown

- The pipeline MUST integrate `IHostApplicationLifetime` to drain in-flight requests before process termination. Abrupt connection termination on deployment/restart is PROHIBITED.

### 9. Environment-Differentiated CORS

- CORS policy MUST be explicitly configured per environment. A permissive/wildcard policy MUST NOT be reachable in a production configuration path, even as a fallback default.
