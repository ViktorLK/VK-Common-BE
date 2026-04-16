---
trigger: always_on
---

# VK.Blocks: Development Lifecycle

### Rule 9 — Testing

- Unit Tests: MOCK all dependencies via interfaces. NO real DB / Cache / external services.
- Integration Tests: USE Testcontainers for DB and infrastructure dependencies.
- ALL public Application Layer handlers MUST have unit tests covering:
    - ✅ Happy path
    - ✅ Not found / empty result
    - ✅ Permission / tenant isolation failure
    - ✅ Infrastructure failure mapped to Result.Failure
- **Naming**: Test class: `{TargetClass}Tests.cs`. Test method: `{Method}_{Scenario}_{ExpectedResult}` (e.g. `Handle_WhenUserNotFound_ReturnsNotFoundError`).
- **Project**: Test project naming: `{ProjectName}.Tests`.

### Rule 10 — Code Generation

- NEVER generate partial or placeholder code (e.g. `// TODO`, `// implement here`).
- ALL generated code MUST be immediately compilable.
- NEVER omit using statements or namespace declarations.
- If a complete implementation requires additional context, ASK before generating.

### Rule 11 — Architecture Decision Trigger

**Trigger Conditions** — Proactively prompt when ANY of the following occurs:

- An interface contract is introduced or modified
- A design pattern is adopted or replaced (e.g. Result<T>, CQRS, Soft Delete)
- Cross-cutting concerns are refactored (e.g. multi-tenant filtering, audit logging, exception handling)
- A technical trade-off is explicitly resolved
- An existing approach is intentionally abandoned in favor of another

**Required Action**

Interrupt the current flow and ask:

> "⚠️ An architectural decision point has been detected.
> Should I generate an ADR to record this decision before we continue?"

If confirmed → trigger `/publish-adr` using the current conversation as context.
Goal: Ensure _why this change was made_ is captured in real time, not reconstructed retroactively.
