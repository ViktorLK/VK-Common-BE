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

### 1. Identify Target & Verify Clean Git State
- Determine the **absolute path** of the directory or file (default: `src`).
- **Git Clean State Pre-Check**: Ensure the working directory and staging area are completely clean before proceeding to prevent mixing formatting changes with uncommitted work.

// turbo
```powershell
# Pre-Check: Verify Git Working Tree & Staging Area are Clean
$status = git status --porcelain
if ($status) {
    Write-Error "Git working tree or index is not clean! Please commit, stage, or stash your changes before running mechanical normalization."
    git status -s
    return
}
Write-Host "✅ Git status clean: ready for mechanical normalization." -ForegroundColor Green
```

- Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.

### 2. Execute Mechanical Normalization
This script first fixes physical file boundaries (UTF-8 No-BOM, CRLF, trailing newline) and then applies automated .NET formatting (unused usings & whitespace).

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
    Get-ChildItem -Path $targetPath -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
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
    # Remove unused usings (IDE0005)
    dotnet format $project.FullName style --diagnostics IDE0005
    # General whitespace/style
    dotnet format $project.FullName whitespace
}
```

### 3. Diff Audit & Verification
- Inspect the generated diff to verify that **only mechanical changes** occurred (e.g. removed `using` lines, indentation, newlines).
- Confirm that the project builds cleanly.

// turbo
```powershell
# 1. Audit Diff: Inspect changed files
Write-Host "`n--- Step 3: Diff Audit ---" -ForegroundColor Cyan
git diff --stat

# 2. Run quick build to ensure no breaking changes
dotnet build <path_to_relevant_csproj_or_sln> --no-restore
```

- **Reporting Protocol**:
  - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.
  - Audit: `Audit: ✅ Mechanical normalization complete. Diff verified.`

## Why this is "Mechanical"
Unlike `vk-code-normalization`, this workflow does NOT use AI to analyze code. It relies entirely on deterministic tools (Git, PowerShell, and `dotnet format`), making it safe and repeatable across large codebases in seconds.
