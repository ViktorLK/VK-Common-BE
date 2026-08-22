---
layer: 3
id: validation-manifest
scope: building-blocks/validation
requires: CS.02, CS.04, AP.04, BB.07
---

# VK.Blocks Validation Manifest (Layer 3)

Defines the framework-independent rule evaluation contract for all input/business validation, feeding results exclusively into ExceptionHandling's error model.

## Architectural Boundaries

### 1. Pipeline-Only Enforcement

- Validation MUST execute as a MediatR pipeline behavior preceding the business Handler. Ad-hoc validation calls inlined inside Handlers, Controllers, or Application services are PROHIBITED.

### 2. Single Error Contract

- Validation failures MUST be aggregated into `VKErrorDetail[]` and handed off to the EH module's response pipeline. Validation MUST NOT define its own error/response shape.

### 3. Fail-Aggregate, Not Fail-Fast

- All applicable rules for a given input MUST be evaluated and their failures collected in a single pass. Short-circuiting is permitted only within a declared dependency chain (e.g. skip format-dependent rules after a format failure), never across independent rules.

### 4. Async-Native Rule Execution

- The rule engine MUST treat synchronous and asynchronous rules (e.g. uniqueness checks against a store) as first-class, uniformly awaitable within the same pipeline. Sync-over-async or blocking waits are PROHIBITED.

### 5. Object Graph & Path Fidelity

- Nested objects and collections MUST be validated recursively, with failures reporting the full field path (e.g. `Items[2].Price`). Flattened or path-less error reporting is PROHIBITED.

### 6. Contextual Rule Access

- Rules MAY depend on injected read-only context (current user, `TenantId`, read-only query access) via DI. Rules MUST NOT perform write operations or side effects.

### 7. Rule Compilation & Caching

- Rule trees MUST be compiled/cached at startup or first use — no per-request reflection-based rule resolution on the hot path.

### 8. Scenario-Scoped Rule Sets

- The same model MAY apply different rule subsets per scenario (Create vs Update) via explicit grouping, without requiring duplicate DTOs per scenario.

### 9. Localization at the Boundary

- Error messages MUST be resolved via culture/TenantId-aware localization at the point of failure reporting, not hardcoded into rule definitions.

### 10. Test-Friendly Rule Isolation

- Individual rules and validators MUST be unit-testable in isolation, without requiring the full MediatR pipeline or a live DI container.

### 11. Schema Exportability

- Rule metadata MUST be structured such that it can be exported (e.g. JSON Schema) for client-side/front-end reuse, without requiring a parallel rule definition.
