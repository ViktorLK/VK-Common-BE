---
description: Perform a fast, grep-based architecture checklist audit on a specified BuildingBlock module. Output is inline (no file saved).
---

# Workflow: Fast Architecture Audit (Checklist Mode)

A lightweight, high-speed audit that uses **only `list_dir` and `grep_search`** to evaluate a BuildingBlock module against the VK.Blocks architectural rules. No file contents are read. Output is returned directly in the conversation.

**Reference modules (= "correct" baseline)**: `Authentication`, `Authorization`

## Step 1: Identify the Target & Load Context

- Determine the **absolute path** of the BuildingBlock module to audit from the user's input.
- If not provided, ask: `"Which module would you like me to fast-audit?"`
- **Mandatory (PS.04)**: Call `vk_get_module_context(path)` to load localized prompts and rules.
- Extract the `moduleName` from the path (e.g., `"Blob"`, `"ExceptionHandling"`).

## Step 2: Structural Checks (list_dir only)

Run `list_dir` on the module root and key subdirectories. Check the following:

| ID | Rule | Tier | Check | Pass Condition |
|:---|:-----|:-----|:------|:---------------|
| S-01 | BB.01 | 🟡 | `DependencyInjection/` exists | Directory present |
| S-02 | BB.01 | 🟡 | `DependencyInjection/Internal/` exists | Directory present |
| S-03 | BB.04 | 🟡 | `Diagnostics/` exists | Directory present |
| S-04 | BB.04 | 🟡 | `Diagnostics/Internal/` exists | Directory present |
| S-05 | BB.01 | 🔴 | Marker file exists at module root | `VK{Module}Block.cs` or similar `*Block.cs` at root level |
| S-06 | BB.01 | 🟡 | Options NOT scattered across multiple folders | Options files should be co-located (not split between `Options/`, `DependencyInjection/`, `Features/` simultaneously) |

## Step 3: Marker Checks (grep_search on *Block.cs)

| ID | Rule | Tier | Check | grep Query | Pass Condition |
|:---|:-----|:-----|:------|:-----------|:---------------|
| M-01 | BB.02 | 🔴 | Uses `[VKBlockMarker]` attribute (preferred) | `[VKBlockMarker` in `*Block.cs` | Attribute found |
| M-02 | BB.02 | 🟡 | Legacy `IVKBlockMarker` manual implementation | `: IVKBlockMarker` in `*Block.cs` | If found → **Warn** (should migrate to attribute) |
| M-03 | BB.02 | 🔴 | `sealed partial class` declaration | `sealed partial class` in `*Block.cs` | Both `sealed` and `partial` present |
| M-04 | BB.02 | 🟡 | Dependencies declared (non-Core modules only) | `Dependencies` in `*Block.cs` | Found (skip for Core) |

## Step 4: DI Registration Checks (grep_search in DependencyInjection/)

| ID | Rule | Tier | Check | grep Query | Pass Condition |
|:---|:-----|:-----|:------|:-----------|:---------------|
| D-01 | BB.03 | 🔴 | Idempotency check exists | `IsVKBlockRegistered` | Found |
| D-02 | BB.03 | 🔴 | Self-marker registration exists | `AddVKBlockMarker` | Found |
| D-03 | AP.04 | 🟡 | Options uses standard helper | `AddVKBlockOptions` | Found |
| D-04 | AP.02 | 🔴 | Uses `TryAdd` pattern | `TryAdd` | Found |
| D-05 | AP.02 | 🔴 | No direct `Add` registration | `services.Add(Singleton\|Scoped\|Transient)` (regex) | **NOT found** = Pass |
| D-06 | BB.03 | 🟡 | Wrapper → Internal delegation exists | `BlockRegistration.Register` in Extensions file | Found |

## Step 5: Options Checks (grep_search)

| ID | Rule | Tier | Check | grep Query | Pass Condition |
|:---|:-----|:-----|:------|:-----------|:---------------|
| O-01 | BB.05 | 🟡 | Options is `sealed record` | `sealed record.*IVKBlockOptions` | Found |
| O-02 | BB.05 | 🟡 | Options uses `VK` prefix in type name | `VK.*Options.*IVKBlockOptions` | Found |
| O-03 | AP.04 | 🟡 | `SectionName` is defined | `SectionName` in Options files | Found |
| O-04 | BB.05 | 🔴 | NOT `sealed class` (legacy) | `sealed class.*IVKBlockOptions` | **NOT found** = Pass |

## Step 6: Implementation Pattern Checks (grep_search, module-wide)

| ID | Rule | Tier | Check | grep Query | Pass Condition |
|:---|:-----|:-----|:------|:-----------|:---------------|
| I-01 | AP.01 | 🔴 | Sealed usage | Count `public sealed` vs `public class ` (no sealed) | Ratio reported |
| I-02 | AP.01 | 🔴 | VKGuard usage | `VKGuard.` | Found (if module has constructors/boundaries) |
| I-03 | CS.03 | 🔴 | ConfigureAwait compliance | Count `ConfigureAwait(false)` vs `await ` | Ratio reported |
| I-04 | OR.01 | 🟡 | LoggerMessage source gen | `[LoggerMessage` | Found (if module has logging) |
| I-05 | OR.01 | 🔴 | No direct logger calls | `.Log(Information\|Warning\|Error\|Debug\|Critical)\(` (regex) | **NOT found** = Pass |
| I-06 | BB.04 | 🟡 | Diagnostics attribute | `[VKBlockDiagnostics` | Found |
| I-07 | CS.01 | 🔴 | Result pattern usage | `Result<` or `Result.Failure` | Found (if applicable) |
| I-08 | CS.06 | 🔴 | Core: TimeProvider usage | `DateTime\.(UtcNow\|Now)` | **NOT found** = Pass |
| I-09 | CS.06 | 🔴 | Core: Guid Generator usage | `Guid\.NewGuid\(\)` | **NOT found** = Pass |
| I-10 | CS.06 | 🔴 | Core: JSON Serializer usage | `JsonSerializer\.(Serialize\|Deserialize)` | **NOT found** = Pass |
| I-11 | CS.02 | 🟡 | Dependency Pollution Check | `using Microsoft\.EntityFrameworkCore;` or `StackExchange\.Redis` | **NOT found** in Application logic (e.g. `Features/`, `Internal/` outside of Persistence/Infrastructure boundaries) |

## Step 7: Naming & Visibility Checks (AP.03)

| ID | Rule | Tier | Check | grep Query (regex) | Pass Condition |
|:---|:-----|:-----|:------|:-----------|:---------------|
| N-01 | AP.03 | 🟡 | Level 1 public types use `VK` prefix | `public (class\|interface\|record\|struct) (?!VK\|IVK)` | **NOT found** in root/1st-level dirs |
| N-02 | AP.03 | 🟡 | Level 2+ types are `internal` | `public (class\|interface\|record\|struct)` | **NOT found** in `Internal/` or deep dirs |
| N-03 | AP.03 | 🟡 | Deep namespaces use matching path | `namespace (?!.*\.Internal)` | **NOT found** in `*/Internal/*` |

## Step 8: Output the Report

Every response MUST start with the **Handshake**:
`Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`

Then, output the results **directly in the conversation** using this format:

```markdown
# ⚡ Fast Audit: {ModuleName}
**Date**: YYYY-MM-DD | **Score**: XX/YY (ZZ%)

## 📁 Structure (BB.01)
- ✅/❌ S-01 ~ S-06

## 🏷️ Marker (BB.02)
- ✅/⚠️/❌ M-01 ~ M-04

## 🔌 DI Registration (BB.03, AP.02/04)
- ✅/❌ D-01 ~ D-06

## ⚙️ Options (BB.05, AP.04)
- ✅/⚠️/❌ O-01 ~ O-04

## 🔍 Implementation Patterns (CS.01/03/06, OR.01, AP.01, BB.04)
- ✅/⚠️/❌ I-01 ~ I-11

## 📛 Naming & Visibility (AP.03)
- ✅/⚠️/❌ N-01 ~ N-03

## 📊 Summary Table
| Category | Tier | ✅ | ❌ | ⚠️ |
| :--- | :--- | :--- | :--- | :--- |
| Structure | 🟡 | | | |
| Marker | 🔴 | | | |
| DI Registration | 🔴 | | | |
| Options | 🟡 | | | |
| Implementation | 🔴 | | | |
| Naming | 🟡 | | | |

## 🚩 Audit Exceptions
Audit: {✅ All constraints satisfied. | 🚩 [RuleID] {Rationale}}
```

### Scoring Rules
- ✅ = Pass (1 point)
- ⚠️ = Warning (0.5 points) — functional but not aligned with latest standard
- ❌ = Fail (0 points)
- **N/A** items (e.g., Result pattern in a pure-abstraction module) are excluded from scoring
- Score = (earned points / applicable items) × 100%

### Tier Classification
- **🔴 Type A (Zero Tolerance)**: Any ❌ in a 🔴 Tier item requires immediate stopping and correction.
- **🟡 Type B (Industrial Habit)**: ❌ or ⚠️ are allowed in `src/Labs` but must be flagged.
