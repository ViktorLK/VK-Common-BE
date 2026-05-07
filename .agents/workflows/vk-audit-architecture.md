---
description: Perform a deep, phased architecture audit on a specified BuildingBlock module and save a detailed report.
---

# Workflow: Full Architecture Audit (Phased Deep Analysis)

A comprehensive, multi-phase architecture audit that evaluates a BuildingBlock module from the outside in. All phases are always executed — findings are recorded regardless of severity.

**Reference modules (= "correct" baseline)**: `Authentication`, `Authorization`

## Phase 0: Setup & Context

1. **Identify the Target**:
    - Determine the **absolute path** of the BuildingBlock module from the user's input.
    - If not provided, ask: `"Which module would you like me to full-audit?"`
    - Extract `moduleName` from the path.

2. **Mandatory (PS.04)**: Call `vk_get_module_context(path)`.

3. **Load Rules**:
    - Read the audit blueprint from `docs/00-Blueprints/ArchitectureAudit.md`.

4. **Prepare Report Metadata**:
    - Date in `YYYYMMDD` format.
    - Output path: `docs/04-AuditReports/<ModuleName>/<ModuleName>_<Date>.md`.

5. **Handshake**:
    `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`

## Phase 1: Structural Audit

**Tool**: `list_dir` + `grep_search` only (= Fast Audit)

- Execute `/vk-audit-fast` against the target module.
- Record the Fast Audit score and all findings (Pass / Fail / Warn).
- All results are carried forward into the final report — **do NOT stop even if Critical items (🔴) fail**.

## Phase 2: Registration Audit (DI Layer Only)

**Tool**: `view_file` — read ONLY files under `DependencyInjection/` (including `Internal/`)

Checks that require reading file contents (grep cannot verify these):

| Check | Rule | Tier | What to Verify |
|:------|:-----|:-----|:---------------|
| Execution Order | BB.03 | 🔴 | The 8 steps must appear in exact sequence: Check-Self → Options → Mark-Self → Validator → Diagnostics → Toggle → Core Services |
| Func Transform | BB.03 | 🔴 | `configure` parameter must be `Func<T, T>` (not `Action<T>`) for immutable `record` options (ADR-016) |
| Enabled Policy Position | BB.03 | 🔴 | `if (!options.Enabled)` must appear AFTER `AddVKBlockMarker` |
| Builder Pattern | BB.03 | 🟡 | Builder class returns `IVKBlockBuilder<T>` and uses `TryAdd` extensions |
| OptionsValidator Quality | BB.05 | 🔴 | `IValidateOptions<T>` implementation validates all critical properties |

Record all findings. Proceed to Phase 3 regardless of results.

## Phase 3: Implementation Audit (Deep Analysis)

**Tool**: `view_file` or `export_codebase_to_markdown` (for large modules)

- For modules with **≤ 10 implementation files**: read files directly with `view_file`.
- For modules with **> 10 implementation files**: use `export_codebase_to_markdown` to create a Snapshot, then analyze.

Evaluate against ALL 7 audit dimensions defined in `ArchitectureAudit.md`:

1. **Design Principles** — SOLID / KISS / YAGNI / DRY
2. **Design Patterns** — Strategy, Factory, Observer etc.
3. **Architectural Principles** — Separation of Concerns, Encapsulation, Cohesion, Coupling
4. **Architectural Styles** — Clean Architecture, Vertical Slice alignment
5. **Architectural Patterns** — CQRS, MediatR, DDD adherence
6. **Enterprise Patterns** — Idempotency, Caching, Circuit Breaker, Observability
7. **VK.Blocks Compliance (Deep)** — Error constants (CS.01), CancellationToken propagation (CS.03), Visibility (AP.03), Core Abstractions (CS.06)

## Phase 4: Report Generation

1. **Write the Report** following the output schema in `ArchitectureAudit.md`:
    - **Language**: Business IT Japanese (ビジネスIT日本語).
    - **Links**: 必ずリポジトリルートからの相対パス（`[file.cs](/src/...)`）を使用し、エディタ上でクリック可能な形式にすること。
    - **Fast Audit Reference**: Include the Phase 1 score in the 監査サマリー section.
    - **Schema**: `ArchitectureAudit.md` の「Output Schema」で定義されている全ヘッダー（絵文字含む）と項目を完全にコピーして使用すること。
    - **Audit Header**: `Audit: {✅ | 🚩 [RuleID]}`.

2. **Save the Report**:
    - Ensure the directory exists (create if necessary).
    - Save to: `docs/04-AuditReports/<ModuleName>/<ModuleName>_<Date>.md`.

3. **Confirm Completion**:
    - Output: `✅ Full Audit complete. Saved to: [path]`
    - Include a one-line summary: `Phase 1: X/Y | Phase 2: PASS/FAIL | Phase 3 Score: ZZ/100`
    - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`
