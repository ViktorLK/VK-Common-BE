---
layer: 3
id: observability-manifest
scope: building-blocks/observability
requires: CS.02, CS.06, AP.04, BB.07, OR.03
---

# VK.Blocks Observability Manifest (Layer 3)

Defines the framework-independent contract for the three observability pillars — structured logging, distributed tracing, and metrics — plus health checks. This package MUST contain zero references to any concrete telemetry backend (Application Insights, Prometheus, Jaeger); concrete exporters are exclusively downstream packages (e.g. `Observability.ApplicationInsights`).

## Architectural Boundaries

### 1. Zero Backend Dependency

- This package MUST NOT reference any concrete APM/exporter SDK. All contracts MUST be resolvable purely against BCL + `System.Diagnostics` (`ActivitySource`, `Meter`) + Core.

### 2. Telemetry Failure Is Never Business Failure

- All telemetry emission (log write, span export, metric record) MUST be non-blocking relative to the business operation and MUST degrade to silent drop on failure. A telemetry backend outage MUST NOT throw, retry-block, or otherwise impact request latency/success.

### 3. Structured Logging Only

- All logging MUST go through `[LoggerMessage]` Source-Generated methods (R6). String-interpolated or concatenated log messages, and direct `ILogger.LogXxx(string)` calls, are PROHIBITED.

### 4. Centralized Diagnostics Namespace Registry

- Every module's `ActivitySource`/`Meter` name MUST be registered in a single central registry to prevent collisions. Modules MUST NOT declare diagnostics namespaces independently outside this registry (see `Persistence.Diagnostics`/`Storage.Diagnostics` precedent — this package is their common root).

### 5. OpenTelemetry Semantic Convention Compliance

- Span/Activity attribute keys MUST follow OTel Semantic Conventions where an applicable convention exists (e.g. `db.system`, `http.method`). Inventing custom keys for concepts already covered by the convention is PROHIBITED.

### 6. Automatic Cross-Signal Correlation

- TenantId, CorrelationId, and (masked) UserId MUST be automatically attached to every Log entry, Span, and Metric emission where available in context — correlation MUST NOT depend on each call site manually re-supplying these values.

### 7. PII Redaction Pipeline Reuse

- Sensitive-field masking MUST flow through a single shared redaction pipeline, reused by Logging, Tracing, and Metrics alike. Authentication/Authorization/Storage modules' own PII-masking requirements MUST delegate to this same pipeline rather than reimplementing redaction logic.

### 8. High-Cardinality Dimension Prohibition

- Unbounded-cardinality values (UserId, email, request ID, free-text) MUST NOT be used as Metric tag/dimension values. Such values MAY appear as Log/Trace attributes only, never as Metric dimensions.

### 9. Exception-to-Trace Binding

- Unhandled exceptions MUST be recorded as an Exception Event on the active Span and correlated with the TraceId surfaced by ExceptionHandling's response. This binding MUST occur automatically at the point of capture, not require manual instrumentation per catch block.

### 10. W3C Trace Context Propagation

- Distributed trace context MUST propagate via W3C Trace Context headers across HTTP and message-queue boundaries by default. Custom propagation formats are permitted only as an additive, explicitly configured extension.

### 11. Configurable Sampling

- Trace sampling strategy (always-on, probabilistic, tail-based) MUST be externally configurable per environment. Hardcoded always-on sampling in production configuration is PROHIBITED beyond low-traffic/dev scenarios.

### 12. Modular DI Registration

- `VKObservabilityBlock` MUST follow the standard 8-step DI registration order (R13). `IVKObservabilityBuilder` is the sole extension point for concrete exporters — exporters MUST NOT bypass it via direct `IServiceCollection` manipulation.

### 13. Multi-Exporter Composability

- The pipeline MUST support composing multiple exporters (e.g. cloud APM + local file) without one exporter's failure blocking another's delivery, and without requiring the core contract to know exporter count/type in advance.

### 14. Test-Friendly In-Memory Exporter

- An in-memory exporter/collector MUST be available so unit tests can assert "was this operation traced/measured/logged" without requiring a real telemetry backend.
