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

### 7. Route Convention Enforcement

- Routes MUST follow the standard template (`api/v{version}/{module}/{resource}`). Ad-hoc `[Route]` attributes deviating from this convention are PROHIBITED without an ADR.

### 8. Rate Limiting Response Consistency

- 429 responses MUST be shaped through the same `VKProblemDetails`/EH error contract as other failures — not a bare framework default response.

### 9. Security Headers by Default

- Standard security headers (`X-Content-Type-Options`, `Strict-Transport-Security`, `Content-Security-Policy`) MUST be injected globally via middleware. Per-controller opt-in is PROHIBITED — omission must be an explicit, documented exception, not the default.

### 10. Streaming File Transfer

- File upload/download endpoints MUST use streaming request/response bodies, consuming Storage module's chunked-upload contract. Buffering entire file bodies into memory within a controller action is PROHIBITED.

### 11. Idempotency for Unsafe Retries

- State-mutating endpoints exposed to client-side retry (e.g. payment, order creation) MUST support an `Idempotency-Key` header mechanism. Silent duplicate-write risk on retry is not acceptable for endpoints explicitly marked idempotent-required.

### 12. Health Endpoint Aggregation Only

- `/health`, `/health/ready`, `/health/live` MUST aggregate results from Observability's `IHealthCheck` registry. Web MUST NOT define its own ad-hoc health probes — probe implementations belong to each module's own package (e.g. `Persistence.EFCore`, `Storage.Azure`).

### 13. Graceful Shutdown

- The pipeline MUST integrate `IHostApplicationLifetime` to drain in-flight requests before process termination. Abrupt connection termination on deployment/restart is PROHIBITED.

### 14. Environment-Differentiated CORS

- CORS policy MUST be explicitly configured per environment. A permissive/wildcard policy MUST NOT be reachable in a production configuration path, even as a fallback default.

### 15. Test-Friendly Pipeline

- A `WebApplicationFactory`-based test harness MUST be provided, supporting authenticated-user simulation and tenant-context injection without standing up a real IDP or database.
