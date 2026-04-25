---
description: Perform rapid, tool-based code normalization (Physical boundaries, Using directives, and Whitespace) without AI overhead.
---

# VK.Blocks: Mechanical Code Normalization (Lightweight)

This workflow combines strict physical file standards with automated .NET CLI formatting. It is the recommended first step for any code cleanup or before a Pull Request.

## Goal

Ensure all C# files adhere to VK.Blocks physical standards and standard .NET formatting rules:
- **Physical**: No leading blanks, single trailing newline, CRLF, UTF-8 (No BOM).
- **Logical**: Remove unused `using` directives (IDE0005).
- **Style**: Standardize indentation and spacing using `dotnet format`.

## Steps

### 1. Identify the Target
- Determine the **absolute path** of the directory or file (default: `src`).

### 2. Execute Mechanical Normalization
This script first fixes the physical file boundaries and then applies standard .NET formatting.

// turbo
```powershell
# Configuration
$targetPath = "<absolute_path_to_target>"
$Utf8NoBom = New-Object System.Text.UTF8Encoding $false

# 1. Physical Normalization
Write-Host "--- Step 1: Physical Normalization (CRLF, UTF-8, Boundaries) ---" -ForegroundColor Cyan
$items = if (Test-Path $targetPath -PathType Leaf) { 
    Get-Item $targetPath 
} else { 
    Get-ChildItem -Path $targetPath -Recurse -Filter *.cs 
}

$items | ForEach-Object {
    $filePath = $_.FullName
    if ($_.Extension -ne ".cs") { return }

    $content = [System.IO.File]::ReadAllBytes($filePath)
    $text = [System.Text.Encoding]::UTF8.GetString($content)
    
    # Trim leading/trailing blank lines
    $normalized = $text.Trim()
    # Unify to CRLF
    $normalized = $normalized -replace "\r\n", "`n"
    $normalized = $normalized -replace "`n", "`r`n"
    # Single trailing newline
    $normalized = $normalized + "`r`n"

    # Check for BOM (EF BB BF)
    $hasBom = $content.Length -ge 3 -and $content[0] -eq 0xEF -and $content[1] -eq 0xBB -and $content[2] -eq 0xBF
    
    if ($text -ne $normalized -or $hasBom) {
        [System.IO.File]::WriteAllText($filePath, $normalized, $Utf8NoBom)
        Write-Host "Normalized Physical: $filePath" -ForegroundColor Gray
    }
}

# 2. Logical & Style Normalization
Write-Host "`n--- Step 2: Logical & Style Normalization (dotnet format) ---" -ForegroundColor Cyan
$projects = Get-ChildItem -Path $targetPath -Filter *.csproj -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" }
if ($projects.Count -eq 0) { 
    $parentPath = Split-Path $targetPath
    $projects = Get-ChildItem -Path $parentPath -Filter *.csproj 
}

foreach ($project in $projects) {
    Write-Host "Formatting Project: $($project.Name)" -ForegroundColor Yellow
    # Remove unused usings
    dotnet format $project.FullName style --diagnostics IDE0005
    # General whitespace/style
    dotnet format $project.FullName whitespace
}
```

### 3. Verification
- Confirm that files are clean and the project still builds.

// turbo
```powershell
# Run a quick build to ensure no breaking changes
dotnet build <path_to_relevant_csproj_or_sln> --no-restore
```

## Why this is "Mechanical"
Unlike `vk-code-normalization`, this workflow does NOT use AI to analyze code. It relies entirely on deterministic tools, making it safe to run on large codebases in seconds.
