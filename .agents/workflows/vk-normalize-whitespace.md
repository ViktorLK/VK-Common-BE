---
description: Enforce strict VK.Blocks whitespace standards (no leading blank lines, single trailing newline, UTF-8 without BOM, CRLF) using PowerShell on a specified target path.
---

## Goal

Normalize C# source files within a specified directory or file to adhere to strict physical boundary and encoding standards.
- **Rule 1**: No leading blank lines at the beginning of the file.
- **Rule 2**: Exactly one trailing newline at the end of the file.
- **Rule 3**: Force CRLF line endings.
- **Rule 4**: Force UTF-8 encoding WITHOUT BOM (Byte Order Mark).

## Steps

1. **Identify the Target**:
    - Determine the **absolute path of the directory or file** to be normalized based on user input (default: `src`).

2. **Execute Ultra-Safe Normalization**:
    - // turbo
    ```powershell
    # Configuration
    $targetPath = "<absolute_path_to_target>"
    $Utf8NoBom = New-Object System.Text.UTF8Encoding $false

    # Resolve items (handle single file or directory)
    $items = if (Test-Path $targetPath -PathType Leaf) { 
        Get-Item $targetPath 
    } else { 
        Get-ChildItem -Path $targetPath -Recurse -Filter *.cs 
    }

    $items | ForEach-Object {
        $filePath = $_.FullName
        if ($_.Extension -ne ".cs") { return } # Safety check

        $content = Get-Content $filePath -Raw
        if ([string]::IsNullOrWhiteSpace($content)) { return }

        # Step 1: Trim only leading and trailing blank lines/spaces
        $normalized = $content.Trim()

        # Step 2: Unify newlines to CRLF
        $normalized = $normalized -replace "\r\n", "`n"
        $normalized = $normalized -replace "`n", "`r`n"

        # Step 3: Append exactly one trailing CRLF
        $normalized = $normalized + "`r`n"

        # Step 4: Check for change or BOM presence
        $needsUpdate = $content -ne $normalized
        $bytes = [System.IO.File]::ReadAllBytes($filePath)
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            $needsUpdate = $true
        }

        if ($needsUpdate) {
            [System.IO.File]::WriteAllText($filePath, $normalized, $Utf8NoBom)
            Write-Host "Normalized (Ultra-Safe): $filePath" -ForegroundColor Cyan
        }
    }
    ```

3. **Verification**:
    - Report the total number of normalized files.
    - Confirm that target files start with code, end with a single newline, and have no BOM.
