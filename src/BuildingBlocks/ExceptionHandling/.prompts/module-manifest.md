---
layer: 3
id: exceptionhandling-manifest
scope: building-blocks/exceptionhandling
requires: CS.02, CS.06, AP.04, BB.07
---

# VK.Blocks ExceptionHandling Manifest (Layer 3)

Defines the framework-independent exception-to-response contract shared across all presentation layers (Web, gRPC, MessageBus, etc.).

## Architectural Boundaries

### 1. Framework Independence
- Core exception model MUST have zero dependency on ASP.NET Core / MVC. Presentation-specific types live in the consuming layer and MUST map FROM the core model, never the reverse.

### 2. Single Mapping Authority
- Exception → Response translation MUST flow through exactly one mapper per presentation layer. Ad-hoc `try/catch` → response construction outside this mapper is PROHIBITED.

### 3. Information Disclosure Control
- Exposure of internal details (stack trace, exception type, raw message) is gated by a single flag resolved from environment/config, decided within EH — NOT scattered across controllers or handlers.
- Response-level metadata (e.g. `Timestamp`) MUST be set at the Mapper boundary, never inside the exception itself.

### 4. Multi-Error Aggregation
- Field-level or batch validation failures MUST be aggregated into a single response. Aggregation decisions belong to the Validation stage; EH only carries the resulting structure.

### 5. PII & Multi-Tenant Safety
- Exception payloads MUST NOT leak PII or cross-tenant data. `TenantId` is carried for correlation only and MUST be masked before external exposure.

### 6. Structured, Non-Duplicated Logging
- Every unhandled exception MUST be logged exactly once via `[LoggerMessage]` SG methods, at the point of capture. Re-logging the same exception at multiple layers is PROHIBITED.

### 7. Correlation
- Every EH response MUST carry a TraceId/CorrelationId sourced from the current diagnostic context.

### 8. No Control-Flow Abuse
- Exceptions MUST NOT be used for expected business outcomes. Expected failures flow through `VKResult<T>`; EH activates only for truly unhandled/exceptional paths.

### 9. Extensibility Without Reflection
- Custom exception → status/code mappings MUST be registered declaratively at startup, not resolved via runtime reflection or type-scanning in the hot path.

