<#
.SYNOPSIS
    Removes redundant duplicate 'using' statements from C# files.
.PARAMETER TargetDir
    The root directory to scan for .cs files (Default: current directory).
#>
param(
    [Parameter(Mandatory=$false)]
    [string]$TargetDir = "."
)

# Standardized cleanup function for removing duplicate using directives
function Remove-Duplicate-Usings {
    param($path, $filter)
    
    Write-Host "Cleaning duplicate usings in: $(Get-Item $path).FullName"
    
    # Recursively find .cs files, excluding build artifacts
    Get-ChildItem -Path $path -Filter $filter -Recurse | Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $name = $_.FullName
        
        # Regex to match 'using Some.Namespace;' statements line by line
        $matches = [regex]::Matches($content, "(?m)^using\s+(.*?);")
        if ($matches.Count -gt 0) {
            $distinctUsings = @{}
            $toRemove = @()
            $newContent = $content
            
            foreach ($match in $matches) {
                # Extract the namespace and check for prior occurrences
                $ns = $match.Groups[1].Value.Trim()
                if ($distinctUsings.ContainsKey($ns)) {
                    # Mark for removal if it's already been encountered
                    $toRemove += $match.Value
                } else {
                    $distinctUsings[$ns] = $true
                }
            }
            
            if ($toRemove.Count -gt 0) {
                Write-Host "  - Found overlaps in: $(Split-Path $name -Leaf)"
                foreach ($dup in $toRemove) {
                    # Remove the duplicate line precisely
                    $newContent = $newContent -replace "(?m)^\Q$dup\E\r?\n", ""
                }
                
                # Save only if content changed
                if ($newContent -ne $content) {
                    $newContent | Set-Content $name -NoNewline
                }
            }
        }
    }
}

Remove-Duplicate-Usings $TargetDir "*.cs"
