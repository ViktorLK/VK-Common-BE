---
name: vk-review-diff
description: Run a context-isolated, read-only subagent to grade a git diff against VK.Blocks semantic rules (PASS/FAIL with file:line evidence), replacing subjective self-grading.
---

# Skill: Context-Isolated Diff Review (`vk-review-diff`)

## Goal

Eliminate author self-grading bias (Rule 4: *"The one who did the work never grades it"*) by having a context-isolated, read-only subagent evaluate a `git diff` against a narrow semantic checklist and report `PASS` / `FAIL` / `N/A` with `file:line` evidence.

## When to Use / Skip

- **Use**: Before reporting completion or closing a PBI (`vk-sprint-task-done`) on any non-trivial code change (>15 changed lines, new types/contracts, DI changes, or behavioral changes).
- **Skip**: Pure documentation edits, typo fixes, or trivial mechanical formatting where `dotnet build -warnaserror` already covers the change.

## Steps

### 1. Prepare Scope (No Author Rationale)
- Collect only the target scope: `git diff` (or the exact list of changed file paths and line ranges).
- **Isolation Rule (Separate Desks)**: NEVER pass your own reasoning, justifications, chat history, or explanation of why the code is "fine" to the reviewer subagent. Give it only the changed files / diff and the checklist below.

### 2. Invoke Isolated Read-Only Subagent
- Call `invoke_subagent` with:
  - `TypeName`: `"research"`
  - `Role`: `"Isolated Diff Checker"`
  - `Prompt`: Include the changed file paths (and/or `git diff` command to inspect), the exact **Narrow Semantic Checklist** below, and the **Judge-Only Output Contract**.

### 3. Narrow Semantic Checklist (Compiler Gaps Only)

> Do **NOT** re-check rules already enforced by `VK.Tools.Analyzers` and `.editorconfig` (`VK0001`–`VK0006`, `CA1848`, `CA1852`, `CA2007`, `IDE0130`). Focus exclusively on semantic boundaries:

| ID | Rule | Check |
| :--- | :--- | :--- |
| `C-01` | `CS.01` | Application/Domain logic returns `Result<T>`/`VKResult<T>` with centralized error constants; no bare `null`, swallowed exceptions, or exception-driven business flow. |
| `C-02` | `AP.01` | Constructors and public/boundary methods validate preconditions with `VKGuard` (no manual `if (x == null) throw`). |
| `C-03` | `AP.06` / `AP.07` | No silent `NoOp`/`Null` fallback masking missing required config; Core blocks cause no implicit domain/persistence side effects. |
| `C-04` | `ARCH-01/02` / `CS.02` | Dependencies flow strictly downward; infrastructure types (EF Core, Redis, SDK types) do not leak into Domain, Core, or Level-1 Public APIs. |
| `C-05` | `AP.03` / `BB.01` | Level-1 Public types use `VK` prefix and flat module root namespace; `Internal/` types are `internal` without `VK` prefix; one primary type per file. |
| `C-06` | `CS.03` / `CS.07` | Async I/O methods accept and propagate `CancellationToken`; required dependencies use `GetRequiredService` (not `GetService` without documented fallback). |
| `C-07` | `DL.02/03` / `BASE-10` | No placeholder stubs or bare `// TODO`; diff is minimal with no unrelated refactoring; public contract/DI order changes note ADR requirement. |
| `C-08` | `Safety` | No forbidden files modified (e.g., `*Tests.cs`, `*.test.ts`, unrelated staged items), no hardcoded secrets or plaintext PII in logs. |

### 3b. Architecture Escalation (Auto-Trigger for Structural / Refactoring Changes)

If the diff touches **any** of the following paths/patterns, automatically enable **Architecture Mode**:
- `*.csproj`, `Directory.Build.props`, `Directory.Packages.props`
- `.prompts/module-manifest.md`, module root `*Block.cs`, `*Options.cs`
- `Common/Protocols/`, `Common/Models/`, `DependencyInjection/`, or new/split vertical slices or BuildingBlocks

**Mandatory Deterministic Pre-Checks (Parent Agent before invoking subagent)**:
1. Run `dotnet build -warnaserror`.
2. If `.csproj` or `module-manifest.md` changed → run MCP `vk_be_manifest_lint`.
3. If EF Core migrations changed → run MCP `vk_be_audit_migrations`.
4. If `.agents/**`, `.prompts/pitfalls.md`, or `McpTools*.cs` changed → run `vk_be_agents_lint` (or CLI `--agents-lint`).
5. Pass affected `module-manifest.md` and changed `.csproj` files into the isolated subagent alongside the diff (still NO author rationale).

**Additional Architecture Checklist (`A-01`–`A-05`)**:

| ID | Rule | Check |
| :--- | :--- | :--- |
| `A-01` | `ARCH-01` / `Manifest` | Actual `.csproj` references and `using`s strictly respect `module-manifest.md` (`allowed`/`forbidden`), with zero undeclared lateral or reverse coupling. |
| `A-02` | `BASE-06` / `ARCH-04` | New abstractions, extension points, or packages have a confirmed consumer; package splits correspond to distinct third-party dependencies or deployment boundaries. |
| `A-03` | `ARCH-03` / `ARCH-05` | Core stays free of I/O and external SDKs; `Fallback` defaults co-locate with contracts and overrides are explicit and registration-order independent (`TryAdd` + Builder). |
| `A-04` | `DL.03` / `BASE-12` / `FS-09` | Level-1 Public API, error code, config key, or wire contract changes classify Additive vs Breaking and sync Manifest/docs and ADR. |
| `A-05` | `BB.02` / `BB.03` / `BB.06` | DI registration sequence, `AddVKBlockMarker` before `Enabled` toggle, and Source Generator hooks remain intact. |

*(For whole-module deep structural review after major refactoring, follow up with `/vk-audit-architecture` via an isolated read-only subagent.)*

### 4. Judge-Only Output Contract (Paste into Subagent Prompt)

```text
You did not write this code. You are an independent read-only judge.
Do not rewrite code, do not suggest speculative refactors, and do not nitpick outside the checklist.
Read the changed files / git diff directly and grade ONLY C-01 through C-08 (plus A-01 through A-05 if Architecture Mode is active).

For each item output one line:
- C-0X [PASS | FAIL | N/A]: <file:line evidence or concise reason>
(and - A-0X [PASS | FAIL | N/A]: ... when Architecture Mode is active)

End with exactly one verdict line:
Verdict: PASS
or
Verdict: FAIL (<N> items)
```

### 5. Act on Verdict (Fix & Escalation)
- **`Verdict: PASS`**: Proceed and mark the final response with `Audit: ✅ (verified: build + isolated-review)`.
- **`Verdict: FAIL`**: Fix the cited items surgically, rebuild, and re-run `vk-review-diff` with a fresh isolated subagent.
- **Stop Rule**: If the same checklist item fails twice in a row, STOP and report to the user using `EXPECTED / SAW / ALREADY TRIED`.
