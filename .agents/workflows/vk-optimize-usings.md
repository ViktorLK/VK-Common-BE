---
description: Automatically remove unused using directives and normalize whitespace across the solution using 'dotnet format'.
---

# VK.Blocks: Using Directive Optimization (Automated)

This workflow automates the "Remove Unused Usings" and "Normalize Whitespace" routines across the workspace using the standard .NET CLI tooling. It is faster and more reliable than manual editing for large-scale cleanup.

## Goal

Ensure all C# files in the solution follow a clean, standardized structure by removing unnecessary imports and fixing whitespace issues without manual intervention.

## Prerequisites

- .NET SDK (which includes `dotnet format`)
- A valid `.editorconfig` file in the root.

## Steps

### 1. Configure Rule Severity
Ensure `.editorconfig` has the `IDE0005` rule enabled so that `dotnet format` recognizes unused usings as actionable items.

// turbo
```powershell
# Check if IDE0005 is enabled, add it if missing
$ecPath = ".editorconfig"
if (!(Select-String -Path $ecPath -Pattern "dotnet_diagnostic.IDE0005.severity")) {
    Add-Content $ecPath "`n[*.{cs,vb}]`ndotnet_diagnostic.IDE0005.severity = warning"
}
```

### 2. Execute Automated Formatting
Run the format command on the entire workspace. This will iteratively process all project files.

// turbo
```powershell
# Get all project files to ensure coverage even without a primary .sln
$projects = Get-ChildItem -Path . -Filter *.csproj -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }

foreach ($project in $projects) {
    Write-Host "Processing: $($project.FullName)" -ForegroundColor Cyan
    # Remove unused usings (IDE0005)
    dotnet format $project style --diagnostics IDE0005
    # Normalize whitespace
    dotnet format $project whitespace
}
```

### 3. Verify Build Integrity
Ensure that the automated cleanup did not accidentally break any dependencies or generated code.

// turbo
```powershell
# Run build on core projects or the full solution if available
dotnet build src/BuildingBlocks/Core/VK.Blocks.Core.csproj
```

## Tips

- **Diagnostic Mode**: If formatting fails on a specific project, run `dotnet format <project> --verbosity diagnostic` to see detailed MSBuild errors.
- **Shared Standards**: Prefer committing the `.editorconfig` changes so all team members benefit from the same automated cleanup rules.
