# Standard 05: Development Lifecycle

## 1. Testing Protocol (DL.01)
- **Unit Tests**: Inherit `VKUnitTestBase`. Mock dependencies via `GetMock<T>()`. Use `VKTestDataBuilder<T>` for domain models/aggregates and `VKFakeGuidGenerator` for deterministic GUIDs. Use `VKResultAssertionExtensions` (`.Should().BeSuccess()`, `.Should().BeFailure(errorCode)`).
- **Coverage Paths**: ALL public handlers/stages MUST cover: Happy Path, Not Found / Empty, Permission / Tenant Isolation Failure, and Infrastructure Failure -> Result.Failure.
- **Integration Tests**: Use Testcontainers for DB and external infrastructure dependencies.
- **Naming**: `Method_Scenario_ExpectedResult`.
- **Async (CS.03)**: Prohibit `.ConfigureAwait(false)` in test methods.

## 2. ADR (Architecture Decision Record)
Significant architectural shifts MUST be documented in `docs/02-ArchitectureDecisionRecords/`.
- **Trigger**: Contract change, new pattern adoption, or breaking change to Public API.
- **Closed Loop**: Every Implementation Plan must audit if an ADR is required (PS.01).

## 3. RFC (Request for Comments)
Experimental or complex features MUST start as an RFC in `docs/06-RFCs/`.
- **Structure**: Metaphor ➡️ Mapping ➡️ Code Blueprint.
- **Approval**: Must be approved by the lead architect/user before decomposition into atomic tasks.

## 4. Backlog Management
Use the `VKAddBacklogItem` tool to create atomic, folder-based tasks in `docs/05-Backlogs/`.
- **ID Format**: `{MODULE}-{INDEX}` (e.g., `CORE-001`).
- **Sprint Sync**: Tasks are automatically synchronized to `Active_Sprint.md`.

## 5. Audit Checklist
Before closing any PR or turn, the master checklist (`vk-blocks-checklist.md`) must be verified against the implementation.

