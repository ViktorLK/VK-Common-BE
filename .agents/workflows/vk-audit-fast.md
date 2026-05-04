---
description: Perform a fast, grep-based architecture checklist audit on a specified BuildingBlock module. Output is inline (no file saved).
---

# Workflow: Fast Architecture Audit (Checklist Mode)

A lightweight, high-speed audit that uses **only `list_dir` and `grep_search`** to evaluate a BuildingBlock module against the VK.Blocks architectural rules. No file contents are read. Output is returned directly in the conversation.

**Reference modules (= "correct" baseline)**: `Authentication`, `Authorization`

## Step 1: Identify the Target

- Determine the **absolute path** of the BuildingBlock module to audit from the user's input.
- If not provided, ask: `"Which module would you like me to fast-audit?"`
- Extract the `moduleName` from the path (e.g., `"Blob"`, `"ExceptionHandling"`).

## Step 2: Structural Checks (list_dir only)

Run `list_dir` on the module root and key subdirectories. Check the following:

| ID | Rule | Check | Pass Condition |
|:---|:-----|:------|:---------------|
| S-01 | 16 | `DependencyInjection/` exists | Directory present |
| S-02 | 16 | `DependencyInjection/Internal/` exists | Directory present |
| S-03 | 16 | `Diagnostics/` exists | Directory present |
| S-04 | 16 | `Diagnostics/Internal/` exists | Directory present |
| S-05 | 17 | Marker file exists at module root | `VK{Module}Block.cs` or similar `*Block.cs` at root level |
| S-06 | 14 | Options NOT scattered across multiple folders | Options files should be co-located (not split between `Options/`, `DependencyInjection/`, `Features/` simultaneously) |

## Step 3: Marker Checks (grep_search on *Block.cs)

| ID | Rule | Check | grep Query | Pass Condition |
|:---|:-----|:------|:-----------|:---------------|
| M-01 | 17 | Uses `[VKBlockMarker]` attribute (preferred) | `[VKBlockMarker` in `*Block.cs` | Attribute found |
| M-02 | 17 | Legacy `IVKBlockMarker` manual implementation | `: IVKBlockMarker` in `*Block.cs` | If found → **Warn** (should migrate to attribute) |
| M-03 | 17 | `sealed partial class` declaration | `sealed partial class` in `*Block.cs` | Both `sealed` and `partial` present |
| M-04 | 17 | Dependencies declared (non-Core modules only) | `Dependencies` in `*Block.cs` | Found (skip for Core) |

## Step 4: DI Registration Checks (grep_search in DependencyInjection/)

| ID | Rule | Check | grep Query | Pass Condition |
|:---|:-----|:------|:-----------|:---------------|
| D-01 | 18 | Idempotency check exists | `IsVKBlockRegistered` | Found |
| D-02 | 18 | Self-marker registration exists | `AddVKBlockMarker` | Found |
| D-03 | 15 | Options uses standard helper | `AddVKBlockOptions` | Found |
| D-04 | 13 | Uses `TryAdd` pattern | `TryAdd` | Found |
| D-05 | 13 | No direct `Add` registration | `services.Add(Singleton\|Scoped\|Transient)` (regex) | **NOT found** = Pass |
| D-06 | 18 | Wrapper → Internal delegation exists | `BlockRegistration.Register` in Extensions file | Found |

## Step 5: Options Checks (grep_search)

| ID | Rule | Check | grep Query | Pass Condition |
|:---|:-----|:------|:-----------|:---------------|
| O-01 | 20 | Options is `sealed record` | `sealed record.*IVKBlockOptions` | Found |
| O-02 | 20 | Options uses `VK` prefix in type name | `VK.*Options.*IVKBlockOptions` | Found |
| O-03 | 15 | `SectionName` is defined | `SectionName` in Options files | Found |
| O-04 | 20 | NOT `sealed class` (legacy) | `sealed class.*IVKBlockOptions` | **NOT found** = Pass |

## Step 6: Implementation Pattern Checks (grep_search, module-wide)

| ID | Rule | Check | grep Query | Pass Condition |
|:---|:-----|:------|:-----------|:---------------|
| I-01 | 12 | Sealed usage | Count `public sealed` vs `public class ` (no sealed) | Ratio reported |
| I-02 | 12 | VKGuard usage | `VKGuard.` | Found (if module has constructors/boundaries) |
| I-03 | 3 | ConfigureAwait compliance | Count `ConfigureAwait(false)` vs `await ` | Ratio reported |
| I-04 | 6 | LoggerMessage source gen | `[LoggerMessage` | Found (if module has logging) |
| I-05 | 6 | No direct logger calls | `.Log(Information\|Warning\|Error\|Debug\|Critical)\(` (regex) | **NOT found** = Pass |
| I-06 | 19 | Diagnostics attribute | `[VKBlockDiagnostics` | Found |
| I-07 | 1 | Result pattern usage | `Result<` or `Result.Failure` | Found (if applicable) |
| I-08 | 5.1 | Core: TimeProvider usage | `DateTime\.(UtcNow\|Now)` | **NOT found** = Pass |
| I-09 | 5.1 | Core: Guid Generator usage | `Guid\.NewGuid\(\)` | **NOT found** = Pass |
| I-10 | 5.1 | Core: JSON Serializer usage | `JsonSerializer\.(Serialize\|Deserialize)` | **NOT found** = Pass |

## Step 7: Naming & Visibility Checks (Rule 14)

| ID | Rule | Check | grep Query (regex) | Pass Condition |
|:---|:-----|:------|:-----------|:---------------|
| N-01 | 14 | Level 1 public types use `VK` prefix | `public (class\|interface\|record\|struct) (?!VK\|IVK)` | **NOT found** in root/1st-level dirs |
| N-02 | 14 | Level 2+ types are `internal` | `public (class\|interface\|record\|struct)` | **NOT found** in `Internal/` or deep dirs |
| N-03 | 14 | Deep namespaces use matching path | `namespace (?!.*\.Internal)` | **NOT found** in `*/Internal/*` |

## Step 8: Output the Report

Output the results **directly in the conversation** using this format:

```
# ⚡ Fast Audit: {ModuleName}
**Date**: YYYY-MM-DD | **Score**: XX/YY (ZZ%)

## 📁 Structure (Rule 16)
- ✅/❌ S-01 ~ S-06

## 🏷️ Marker (Rule 17)
- ✅/⚠️/❌ M-01 ~ M-04

## 🔌 DI Registration (Rules 13, 15, 18)
- ✅/❌ D-01 ~ D-06

## ⚙️ Options (Rules 15, 20)
- ✅/⚠️/❌ O-01 ~ O-04

## 🔍 Implementation Patterns (Rules 1, 3, 5.1, 6, 12, 19)
- ✅/⚠️/❌ I-01 ~ I-10

## 📛 Naming & Visibility (Rule 14)
- ✅/⚠️/❌ N-01 ~ N-03

## 📊 Summary Table
| Category | ✅ | ❌ | ⚠️ |
|...       |...|...|...|
```

### Scoring Rules
- ✅ = Pass (1 point)
- ⚠️ = Warning (0.5 points) — functional but not aligned with latest standard
- ❌ = Fail (0 points)
- **N/A** items (e.g., Result pattern in a pure-abstraction module) are excluded from scoring
- Score = (earned points / applicable items) × 100%

### Severity Classification
- **❌ Critical** (S-05, M-01, D-01): Missing marker or idempotency = broken registration
- **❌ Major** (D-05, O-04): Direct Add / legacy class = maintenance risk
- **⚠️ Minor** (M-02, I-03 ratio < 100%): Functional but should be improved
