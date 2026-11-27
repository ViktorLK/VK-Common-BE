---
name: vk-audit-architecture
description: Perform a deep, context-isolated architecture audit on a BuildingBlock module (combining deterministic checks, Manifest invariants, Semantic Industrial DNA, and state-evolution analysis) and save a report.
---

# Workflow: Deep Architecture Audit (Context-Isolated)

A comprehensive module-level audit combining deterministic pre-checks, `.prompts/module-manifest.md` invariant verification, semantic Industrial DNA checks, and deep logic/state-evolution simulation via a context-isolated read-only subagent.

**Reference modules**: `Authentication`, `Authorization`, `AI`

## Phase 0: Setup & Context (`PS.04`)

1. Determine the **absolute path** and `moduleName` of the target BuildingBlock (if missing, ask `"Which module would you like me to full-audit?"`).
2. Call `vk_be_get_module_context(path)` to load `/src/.prompts/pitfalls.md`, `industrial-dna.md`, and the module's `.prompts/module-manifest.md` / `pitfalls.md`.
3. Top-up missing L3 rules via `vk_be_get_architectural_rule`:
   - `BB.01, BB.02, BB.03, BB.04, BB.05, BB.06, BB.07, AP.02, AP.03, AP.05, CS.01, CS.02, CS.04, OR.03`
4. Read `docs/00-Blueprints/06-ArchitectureAuditBlueprint.md`.

## Phase 1: Deterministic Evidence (Parent Agent)

1. Run `/vk-audit-fast` against the target module to collect:
   - Exact Roslyn compiler warnings (`VK*`, `CA*`, `IDE0130`)
   - `vk_be_manifest_lint` findings
   - `S-01`–`S-05` structural score
2. If the module has **> 10 implementation `.cs` files**, call `vk_be_export_codebase_as_markdown` to generate an analysis snapshot.

## Phase 2: Context-Isolated Deep Audit (Read-Only Subagent)

Invoke a fresh read-only subagent (`invoke_subagent`, `TypeName: "research"`, `Role: "Isolated Architecture Auditor"`).
- **Separate Desks**: Pass ONLY the module source files (or snapshot), `.prompts/module-manifest.md`, Phase 1 objective findings, and `06-ArchitectureAuditBlueprint.md`. Do **NOT** pass author chat history or implementation justifications.
- **Strictly NO `*Tests.cs` reads or edits**.

The isolated auditor evaluates the 4 dimensions in `06-ArchitectureAuditBlueprint.md`:
1. **Module Manifest Invariants & Boundaries**: Verify every declared invariant (e.g. `AIB-01`–`AIB-08`) and `Ownership` / `Boundaries` claim against actual code with `file:line` evidence.
2. **DI & Source Generator Integration (`BB.03`, `BB.06`, `AP.06`)**: Inspect `Common/DependencyInjection/` (including `Internal/` hooks `RegisterBlockCustom` / `ValidateBlockCustom`), `AddVKBlockMarker` ordering, and zero silent `NoOp` fallbacks masking missing required config.
3. **Semantic Industrial DNA (Compiler Gaps)**:
   - `CS.01`: `Result<T>` / `VKResult<T>` flow, centralized `Errors` constants, exception-to-Result boundary logging.
   - `AP.01` / `AP.05`: `VKGuard` boundary checks and `Args` merge (`args?.Prop ?? _options.Prop`).
   - `CS.02` / `CS.04`: Zero infra leakage into Application/Domain/Public APIs; `.AsNoTracking()`, bounded pagination, `ArrayPool<T>` returned in `finally`.
   - `OR.03` / `AP.07`: Polly/SDK resilience on external I/O; no implicit Core domain/persistence side effects.
4. **Deep Logic & State Evolution Simulation**: Mental execution of Happy Path + mid-flight failures, detection of dead-end options/dropped results/swallowed errors, and destructive failure-mode analysis.

## Phase 3: Save Report & Close Pitfall Loop (`BASE-14`, `DL.04`)

1. Save the Business IT Japanese report (following the Output Schema in `06-ArchitectureAuditBlueprint.md` with repo-root-relative links `/src/...`) to:
   `docs/06-AuditReports/<ModuleName>/<ModuleName>_<YYYYMMDD>.md`
2. **Feedback Loop**:
   - If the audit uncovers a non-obvious recurring pitfall, propose appending a concise entry to `src/.prompts/pitfalls.md` or `src/BuildingBlocks/{ModuleName}/.prompts/pitfalls.md` (`BASE-14`).
   - If concrete remediation debt is identified, propose creating PBI items via `/vk-sprint-add-task` (`DL.04`).
3. Output Handshake `Active: [L0-L3:{Module}] | Context: {Path} | Sync: [{RuleID},...]`, confirmation path, summary (`Phase 1 Structural: X/5 | Build Warnings: N | Deep Score: ZZ/100`), and `Audit: ✅ (verified: build + fast-audit + isolated-review)`.
