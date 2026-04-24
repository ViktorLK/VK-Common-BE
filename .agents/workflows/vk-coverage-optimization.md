---
description: Iterative process for optimizing code coverage by adding tests or justifying exclusions.
---

# VK.Blocks Coverage Optimization Loop

Use this workflow when the code coverage of a BuildingBlock is below target (typically 80-90%) or when requested by the user.

## 1. Execute Analysis
Run the automated coverage analysis to identify gaps.

// turbo 
```powershell
# Use the MCP tool if available, or run the script manually
powershell.exe -ExecutionPolicy Bypass -File prompts/HitoriGoto/dotnettest.ps1 -Project [TestProject] -Clean -NoBrowser
```

## 2. Identify Gaps
Read the `TestResults/Reports/Summary.txt` (or MCP output) and list all classes/methods with coverage below the target threshold.

## 3. Decision Matrix (Exclude vs. Test)
For each gap, evaluate based on the following criteria:

### CASE A: Exclude from Coverage
**Conditions**:
- Pure constant definitions or static error catalogs.
- Metadata providers/models with no branching logic.
- Infrastructure attributes used only for Source Generation.
- DTOs/Records with no custom logic, populated by external sources.

**Action**:
1. Add `[ExcludeFromCodeCoverage(Justification = "...")]` to the type or member.
2. The `Justification` MUST be in professional English.

### CASE B: Add Unit Tests
**Conditions**:
- Business logic, conditional branching (`if`, `switch`).
- Result pattern flow (`Result.Success`, `Result.Failure`).
- Async operations, I/O handling, or error mapping.
- Extension methods with non-trivial transformations.

**Action**:
1. Invoke `/vk-generate-unit-tests` for the target class.
2. Focus on the specific uncovered lines/branches identified in the report.

## 4. Verification
Rerun the analysis to confirm the metrics have improved. Repeat until the target is reached.

## 5. Reporting
Provide a final summary to the user:
- Final Line Coverage %.
- List of new tests added.
- List of components excluded and why.
