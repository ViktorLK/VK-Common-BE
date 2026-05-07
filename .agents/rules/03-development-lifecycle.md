---
trigger: model_decision
---

# VK.Blocks: Development Lifecycle (DL)

### DL.01 — Testing

- Unit Tests: MOCK all dependencies via interfaces. NO real DB / Cache / external services.
- Integration Tests: USE Testcontainers for DB and infrastructure dependencies.
- ALL public Application Layer handlers MUST have unit tests covering:
    - ✅ Happy path
    - ✅ Not found / empty result
    - ✅ Permission / tenant isolation failure
    - ✅ Infrastructure failure mapped to Result.Failure
- **Naming**: Test class: `{TargetClass}Tests.cs`. Test method: `{Method}_{Scenario}_{ExpectedResult}` (e.g. `Handle_WhenUserNotFound_ReturnsNotFoundError`).
- **Project**: Test project naming: `{ProjectName}.UnitTests` or `{ProjectName}.IntegrationTests`.

### DL.02 — Code Generation

- NEVER generate partial or placeholder code (e.g. `// TODO`, `// implement here`).
- ALL generated code MUST be immediately compilable.
- NEVER omit using statements or namespace declarations.
- If a complete implementation requires additional context, ASK before generating.

### DL.03 — Architecture Decision Trigger

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

### DL.04 — Backlog Synchronization Trigger

**Trigger Conditions** — Proactively prompt when ANY of the following occurs:

- A `README.md` is created or updated with a "Future Roadmap", "Roadmap", or "今後の展望" section.
- A `// TODO`, `// FIXME`, or `// DEBT` comment is introduced into the source code.
- A technical improvement is identified but deferred (e.g., "non-urgent refactoring").
- A technical debt item is discussed during the conversation.

**Required Action**

Interrupt the current flow and ask:

> "⚠️ A potential backlog item (Technical Debt/Roadmap) has been detected.
> Should I generate an atomic task in `docs/05-Backlogs/` using `VKAddBacklogItem`?"

If confirmed → collect title, description, and module from context and execute the tool.

