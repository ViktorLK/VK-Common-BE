---
description: Perform a deep architecture audit on a specified directory using the official guidelines and save the report.
---

## MVP Workflow: Architecture Audit

This workflow performs a comprehensive architecture audit on a target directory, evaluating it against the VK.Blocks guidelines, and produces a detailed report in Japanese.

## Steps

1. **Identify the Target**:
    - Determine the **absolute path of the directory** to be audited from the user's input (e.g., `src/BuildingBlocks/Authentication`).
    - If the target is not provided, ask the user: `"Which directory would you like me to audit?"`

2. **Load Rules**:
    - Read the architectural audit guidelines from `docs/Blueprints/ArchitectureAudit.md`.

3. **Analyze the Codebase**:
    - Recursively read all C# files (`.cs`) in the target directory to fully understand the module's implementation.
    - Evaluate the code against the principles, patterns, and architectural styles defined in the audit guidelines.
    - Maintain a critical eye for architectural smells, technical debt, and adherence to the zero-tolerance `always_on` rules.

4. **Prepare the Report Metadata**:
    - Extract the `moduleName` from the target directory path (e.g., "Authentication").
    - Determine today's date in `YYYY-MM-DD` format (e.g., `2026-03-05`).
    - Construct the output path: `docs/04-AuditReports/<ModuleName>/<ModuleName>_<Date>.md`.
        - _Example: `docs/04-AuditReports/Authentication/Authentication_20260305.md`_

5. **Generate and Save the Report**:
    - Write the comprehensive Architecture Audit Report.
    - **Language Requirement**: The report MUST be written in **Business IT Japanese (ビジネスIT日本語)**.
    - **Link Generation**: When referencing source code, use **project-relative paths** (e.g., `src/BuildingBlocks/...`) instead of absolute local paths to ensure portability.
    - The report structure MUST follow the schema defined in `ArchitectureAudit.md` (Executive Summary, Scores, Findings, Roadmap, etc.).
    - Ensure the directory exists (create it if necessary).
    - Save the generated Markdown report directly to the calculated output path.

6. **Confirm Completion**:
    - Send a concise message to the user confirming the audit is complete.
    - Output the file link to the generated report: ✅ Saved to: `[Path]`
