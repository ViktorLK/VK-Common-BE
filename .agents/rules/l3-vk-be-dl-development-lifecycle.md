---
trigger: manual
---

<ruleset category="DL" name="Development Lifecycle">

<rule id="DL.01" type="A" name="Testing">
- Unit Tests: MOCK all dependencies via interfaces. NO real DB / Cache / external services.
- Integration Tests: USE Testcontainers for DB and infrastructure dependencies.
- ALL public Application Layer handlers MUST have unit tests covering:
    - ✅ Happy path
    - ✅ Not found / empty result
    - ✅ Permission / tenant isolation failure
    - ✅ Infrastructure failure mapped to Result.Failure
- **Naming**: Test class: `{TargetClass}Tests.cs`. Test method: `{Method}_{Scenario}_{ExpectedResult}` (e.g. `Handle_WhenUserNotFound_ReturnsNotFoundError`).
- **Project**: Test project naming: `{ProjectName}.UnitTests` or `{ProjectName}.IntegrationTests`.
</rule>

<rule id="DL.02" type="B" name="Code Generation">
- NEVER generate partial or placeholder code (e.g. `// TODO`, `// implement here`).
- ALL generated code MUST be immediately compilable.
- NEVER omit using statements or namespace declarations.
- If a complete implementation requires additional context, ASK before generating.
</rule>

<rule id="DL.03" type="B" name="Architecture Decision Trigger">
**Trigger Conditions** — Proactively prompt when ANY of the following occurs:
- An interface contract is introduced or modified
- A design pattern is adopted or replaced (e.g. Result<T>, CQRS, Soft Delete)
- Cross-cutting concerns are refactored (e.g. multi-tenant filtering, audit logging, exception handling)
- A technical trade-off is explicitly resolved
- An existing approach is intentionally abandoned in favor of another

**Required Action**: Interrupt the current flow and prompt to generate an ADR before continuing.
</rule>

<rule id="DL.04" type="B" name="Backlog Synchronization Trigger">
**Trigger Conditions** — Proactively prompt when ANY of the following occurs:
- A `README.md` is created or updated with a "Future Roadmap", "Roadmap", or "今後の展望" section.
- A `// TODO`, `// FIXME`, or `// DEBT` comment is **discovered in existing code** during review or audit.
- A technical improvement is identified but deferred.
- A technical debt item is discussed during the conversation.

**Required Action**: Interrupt the current flow and prompt to generate an atomic task in `docs/07-Backlogs/` using `vk_be_add_backlog_item`.
</rule>

<rule id="DL.05" type="B" name="Source Generator Documentation">
**Mandatory Tagging** — ALL code elements that interact with or are driven by Source Generators MUST be documented with specific tags:
- **`[SG Hook]`**: Used for `partial` methods implemented manually to extend generated logic.
- **`[SG Marker]`**: Used for classes decorated with architectural markers (e.g., `[VKBlockMarker]`).
- **`[SG Diagnostics]`**: Used for classes decorated with diagnostics markers (e.g., `[VKBlockDiagnostics]`).
- **`[SG Logger]`**: Used for `partial` classes containing `[LoggerMessage]` definitions.
</rule>

</ruleset>
