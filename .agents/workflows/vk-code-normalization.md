---
description: Read the CodeNormalization.md prompt and apply VK.Blocks coding standards to a target directory or file.
---

## Goal

Apply the strict coding guidelines defined in `prompts/CodeReview/CodeNormalization.md` to a user-specified target (directory or file) and modify the files directly.

## Steps

1. **Identify the Target**:
    - Determine the **absolute path of the directory or file** to be normalized based on user input.
    - If the target is unclear, ask the user: `"Which directory (or file) would you like me to normalize?"`

2. **Load Rules**:
    - Read the `prompts/CodeReview/CodeNormalization.md` file to fully understand the code normalization rules (Namespace, Usings, Regions, Documentation, etc.).

3. **Enumerate and Read Files**:
    - If the target is a directory, recursively enumerate all `.cs` files within it and read their contents.
    - If the target is a single file, read its content.

4. **Apply Code Modifications (Direct AI Editing)**:
    - For each file read, modify the code strictly according to the **rules in `CodeNormalization.md`** AND the **`always_on` architectural rules**.
    - Specific expected modifications include (but are not limited to):
        - Converting `namespace` declarations to **File-scoped namespaces**.
        - Sorting `using` directives (System -> Microsoft -> Third-party -> Current Project) and removing unused ones.
        - Structuring class members with proper `#region` tags (`Fields`, `Constructors`, `Properties`, `Public Methods`, `Private Methods`) in the exact specified order.
        - Adding missing XML documentation (`/// <summary>`) for all public/internal members.

5. **Save and Report**:
    - **Overwrite** the original files with the modified code.
    - Report the results to the user using the following format:
    - Report the results to the user using the following format:
        - ✅ [Filename]: Brief summary of applied changes (e.g., "Added regions, sorted usings, converted namespace").
        - If a file required no changes, report it as `Skipped`.
    - **Actionable Summary**: Create a Markdown file named `normalization-report.md` (save it using your Artifact tool) that aggregates a clear list of all `// TODO`, `// FIX`, `// PERF`, and `// SUGGEST` comments that were added or discovered during the run. Group them by filename. This creates a persistent action plan for the user rather than cluttering the chat.

6. **Verify Build**:
    - Make sure to run `dotnet build` against the modified project or solution to guarantee that your non-invasive formatting / comments did not accidentally break compilation.
