---
description: Read the CodeNormalization.md prompt and apply VK.Blocks coding standards to a target directory or file.
---

## Goal

Apply the strict coding guidelines defined in `prompts/CodeReview/CodeNormalization.md` to a user-specified target (directory or file) and modify the files directly.

## Steps

1. **Identify the Target**:
    - Determine the **absolute path of the directory or file** to be normalized based on user input.

2. **Mechanical Cleanup & Audit (Stage 1)**:
    - Run standard `dotnet format` to handle unambiguous namespaces, usings, and spacing.
    - NEW: Run `dotnet format style --verify-no-changes --severity info` to generate a quality report.
    - // turbo
    ```powershell
    $targetPath = "<absolute_path_to_target>"
    $projects = Get-ChildItem -Path $targetPath -Filter *.csproj -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }
    if ($projects.Count -eq 0) { $projects = Get-ChildItem -Path (Split-Path $targetPath) -Filter *.csproj }

    foreach ($project in $projects) {
        Write-Host "--- Stage 1: Auto-Fix ---" -ForegroundColor Cyan
        dotnet format $project.FullName style
        dotnet format $project.FullName whitespace
        
        Write-Host "--- Stage 2: Quality Audit (Info Severity) ---" -ForegroundColor Yellow
        # Capture the output for the agent to review
        dotnet format $project.FullName style --verify-no-changes --severity info
    }
    ```

3. **Load AI Context**:
    - Read `prompts/CodeReview/CodeNormalization.md` to understand semantic normalization rules.
    - Use the **Audit Output** from Stage 1.5 as input for Stage 2.

4. **Apply Semantic Normalization (Stage 2 - AI)**:
    - For each file, apply the **Documentation** and **Modern C# Advice** rules from the prompt.
    - **Member Ordering**: Ensure logical ordering (Fields -> Props -> Ctor -> Methods).
    - **XML Docs**: Add missing `/// <summary>` and `<inheritdoc />`.
    - **Region Prohibition**: **STRICTLY PROHIBITED**. Remove any existing `#region` tags.

5. **Save and Report**:
    - Overwrite files with modified code.
    - **Normalization Report**: Create `normalization-report.md` (Artifact) aggregating all `// TODO`, `// FIX`, `// PERF`, and `// SUGGEST` comments.

6. **Verify Build**:
    - Make sure to run `dotnet build` against the modified project or solution to guarantee that your non-invasive formatting / comments did not accidentally break compilation.
