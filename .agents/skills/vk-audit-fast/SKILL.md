---
name: vk-audit-fast
description: Perform a fast, deterministic-first architecture health check (compiler hard walls, manifest lint, and latest BB.01-BB.07 structure) on a BuildingBlock module. Output is inline.
---

# Workflow: Fast Architecture Audit (Deterministic-First)

A high-speed module health check that delegates all compiler-enforceable rules to `dotnet build` and `vk_be_manifest_lint`, focusing AI checks exclusively on structural and visibility contracts that Roslyn cannot verify. Output is inline (no file saved).

**Reference modules**: `Authentication`, `Authorization`, `AI`

## Step 1: Bootstrap Context & Differential Rule Sync (`PS.04`)

1. Determine the **absolute path** of the BuildingBlock module (if missing, ask `"Which module would you like me to fast-audit?"`).
2. Call `vk_be_get_module_context(path)` to load `/src/.prompts/pitfalls.md`, `industrial-dna.md`, and the module's `.prompts/module-manifest.md` / `pitfalls.md`.
3. Top-up only missing L3 rules via `vk_be_get_architectural_rule` from:
   - `BB.01, BB.02, BB.03, BB.05, BB.06, BB.07, AP.02, AP.03`

## Step 2: Deterministic Gate (Compiler & Manifest)

Run deterministic tools first — **do NOT replicate compiler checks with regex grep**:

1. **Compiler Hard Walls**:
   ```powershell
   dotnet build <ModuleCsprojPath> --no-restore --no-dependencies -v minimal
   ```
   Record all Roslyn diagnostics (`VK0001`–`VK0006`, `VK1001`–`VK2013`, `VKCORE001`, `CA1848`, `CA1852`, `CA2007`, `IDE0130`).
2. **Manifest & Dependency Lint** (if `.prompts/module-manifest.md` exists):
   - Call `vk_be_manifest_lint(modules: "{ModuleName}")` and record all `MAN-*` errors/warnings.

## Step 3: Non-Compiler Structural Checks (`S-01`–`S-05`)

Inspect directory layout (`list_dir`) and targeted files (`*Block.cs`, `*Options.cs`, `Common/DependencyInjection/`) for checks the compiler cannot enforce:

| ID | Rule | Tier | What to Verify |
| :--- | :--- | :---: | :--- |
| `S-01` | `BB.01` | 🟡 | Root uses `{FeatureName}/` vertical slices (no `Features/` wrapper), unified `Common/` (`Common/DependencyInjection/`, `Common/Diagnostics/`), root `VK{Module}Block.cs`, and hand-written `partial` `VK{Module}Options.cs` (if present). |
| `S-02` | `BB.02` | 🔴 | `VK{Module}Block.cs` is `public sealed partial class` with `[VKBlockMarker]` (no manual `IVKBlockMarker`), declaring `Dependencies` for non-Core blocks. |
| `S-03` | `BB.03` / `AP.02` | 🔴 | Uses Source-Generated DI (`[VKBlockMarker]` / `[VKFeature]`) or follows `IsVKBlockRegistered` → `Options` (`Func<T, T>`) → `AddVKBlockMarker` (before `Enabled` toggle) → `TryAdd` services. |
| `S-04` | `BB.05` / `BB.07` | 🟡 | Options types are `sealed record` with `init` properties, implement `IVKBlockOptions` / `IVKToggleableBlockOptions`, and reside in dedicated single-class files (no nesting). |
| `S-05` | `AP.03` | 🟡 | Level-1 Public types (`Common/Models/`, `Common/Protocols/`) carry `VK` / `IVK` prefix in flat `VK.Blocks.{Module}` namespace; `Internal/` types are `internal` without `VK` prefix. |

## Step 4: Output Inline Report

Start with the Handshake:
`Active: [L0-L3:{Module}] | Context: {Path} | Sync: [{RuleID},...]`

```markdown
# ⚡ Fast Audit: {ModuleName}
**Date**: YYYY-MM-DD | **Structural Score**: X/5 | **Build Warnings**: N | **Manifest Lint**: PASS / FAIL / N/A

## 🧱 Compiler Hard Walls (`dotnet build`)
- Summary of `VK*` / `CA*` / `IDE0130` warnings (0 = Pass)

## 📜 Manifest Lint (`vk_be_manifest_lint`)
- Summary of `MAN-*` findings (or `N/A — no module-manifest.md`)

## 🏗️ Structural Checks (`S-01`–`S-05`)
- ✅/⚠️/❌ S-01 ~ S-05 with exact file evidence
```

End with:
`Audit: ✅ (verified: build + fast-audit)` or `Audit: 🚩 [RuleID] {Rationale}` (any 🔴 Type A failure requires immediate correction or PBI tracking).
