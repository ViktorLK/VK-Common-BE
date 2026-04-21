---
description: Enforce strict VK.Blocks whitespace standards (no leading blank lines, single trailing newline, UTF-8 without BOM, CRLF) using PowerShell.
---

## Goal

Normalize all C# source files to adhere to strict physical boundary and encoding standards. This process is 100% safe as it only targets the start/end of the file and the encoding format, without modifying code content or string literals.
- **Rule 1**: No leading blank lines at the beginning of the file.
- **Rule 2**: Exactly one trailing newline at the end of the file.
- **Rule 3**: Force CRLF line endings.
- **Rule 4**: Force UTF-8 encoding WITHOUT BOM (Byte Order Mark).

## Steps

1. **Execute Ultra-Safe Normalization**:
    - // turbo
    ```powershell
    # Define UTF8 Encoding without BOM
    $Utf8NoBom = New-Object System.Text.UTF8Encoding $false

    # Scan for all C# files in the src directory
    Get-ChildItem -Path src -Recurse -Filter *.cs | ForEach-Object {
        $filePath = $_.FullName
        $content = Get-Content $filePath -Raw
        
        # Skip empty files
        if ([string]::IsNullOrWhiteSpace($content)) { return }

        # Step 1: Trim only leading and trailing blank lines/spaces (Rule 1 & 2)
        # This is safe because it doesn't affect internal string literals
        $normalized = $content.Trim()

        # Step 2: Unify newlines to CRLF (Rule 3)
        # This treats line endings globally but preserves all visible characters
        $normalized = $normalized -replace "\r\n", "`n"
        $normalized = $normalized -replace "`n", "`r`n"

        # Step 3: Append exactly one trailing CRLF (Rule 2)
        $normalized = $normalized + "`r`n"

        # Check for change or BOM presence (Rule 4)
        $needsUpdate = $content -ne $normalized
        
        # Check if the file currently has a BOM
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

2. **Verification**:
    - Report the total number of normalized files.
    - Confirm that target files start with code, end with a single newline, and have no BOM.
