<#
.SYNOPSIS
    Automatically injects standard .NET namespaces into C# files based on code patterns.
.PARAMETER TargetDir
    The root directory to scan for .cs files (Default: current directory).
#>
param(
    [Parameter(Mandatory=$false)]
    [string]$TargetDir = "."
)

# Mapping of 'using' statements to the types/patterns that trigger them
$mapping = @{
    "using System;" = @("\bGuid\b", "\bException\b", "\bAttribute\b", "\bFunc\b", "\bFunc<", "\bAction\b", "\bAction<", "\bDateTime\b", "\bDateTimeOffset\b", "\bTimeSpan\b", "\bIComparable\b", "\bIConvertible\b", "\bIEquatable\b", "\bIFormattable\b", "\bArgumentException\b", "\bArgumentNullException\b", "\bAttributeTargets\b", "\bAttributeUsage\b", "\bFlags\b", "\bType\b", "\bIServiceProvider\b", "\bInvalidOperationException\b", "\bTimeProvider\b", "\bObjectDisposedException\b", "\bUri\b", "\bIServiceScope\b", "\bNotImplementedException\b", "\bRandom\b", "\bArray\b", "\bIDisposable\b", "\bIAsyncDisposable\b", "\bStringComparison\b", "\bStringSplitOptions\b", "\bMath\b", "\bEnvironment\b", "\bNullable\b", "\bAppDomain\b", "\bPredicate\b")
    "using System.Collections.Generic;" = @("\bIEnumerable<", "\bList<", "\bDictionary<", "\bICollection<", "\bIList<", "\bIDictionary<", "\bHashSet<", "\bIReadOnlyList<", "\bIReadOnlyDictionary<", "\bIEqualityComparer<", "\bKeyValuePair<", "\bIAsyncEnumerable<", "\bConcurrentDictionary<", "\bStack<", "\bQueue<", "\bIDictionary\b", "\bTryAdd\b", "\bTryGetValue\b")
    "using System.Linq;" = @("\bEnumerable\.", "\bIQueryable\b", "\bIQueryable<", "\.Select\(", "\.Where\(", "\.Any\(", "\.ToList\(", "\.ToArray\(", "\.First\(", "\.Single\(", "\.FirstOrDefault\(", "\.SingleOrDefault\(", "\.OrderBy\(", "\.GroupBy\(", "\.SequenceEqual\(", "\.Aggregate\(", "\.Count\(", "\.All\(", "\.Contains\(", "\.Min\(", "\.Max\(", "\.Sum\(", "\.Average\(")
    "using System.Threading;" = @("\bCancellationToken\b", "\bSemaphoreSlim\b", "\bInterlocked\b", "\bMonitor\b", "\bThreadPool\b")
    "using System.Threading.Tasks;" = @("\bTask\b", "\bValueTask\b", "\bTask<", "\bValueTask<")
    "using System.IO;" = @("\bStream\b", "\bFile\b", "\bDirectory\b", "\bPath\b", "\bMemoryStream\b", "\bBinaryReader\b", "\bBinaryWriter\b")
    "using System.Net;" = @("\bHttpStatusCode\b", "\bNetworkCredential\b")
    "using System.Net.Http;" = @("\bIHttpClientFactory\b", "\bHttpClient\b", "\bHttpRequestMessage\b", "\bHttpResponseMessage\b", "\bHttpContent\b")
    "using System.Text;" = @("\bStringBuilder\b", "\bEncoding\b")
    "using System.Text.Json;" = @("\bJsonSerializer\b", "\bJsonElement\b", "\bJsonDocument\b")
    "using System.Text.Json.Serialization;" = @("\bJsonPropertyName\b", "\bJsonConverter\b", "\bJsonIgnore\b")
    "using Microsoft.Extensions.DependencyInjection;" = @("\bIServiceCollection\b", "\bTryAdd\b", "\bAddScoped\b", "\bAddSingleton\b", "\bAddTransient\b", "\bAddOptions\b", "\bSubstitute\b")
    "using Microsoft.Extensions.Configuration;" = @("\bIConfiguration\b", "\bIConfigurationSection\b")
    "using Microsoft.Extensions.Logging;" = @("\bILogger\b", "\bILoggerFactory\b", "\bLoggerMessage\b")
    "using Microsoft.AspNetCore.Mvc;" = @("\[ApiController\]", "\bControllerBase\b", "\bIActionResult\b", "\bActionResult\b", "\[HttpGet\b", "\[HttpPost\b", "\[HttpPut\b", "\[HttpDelete\b", "\[Route\b")
    "using Microsoft.AspNetCore.Http;" = @("\bHttpContext\b", "\bIHeaderDictionary\b", "\bRequestDelegate\b", "\bHttpRequest\b", "\bHttpResponse\b")
    "using Microsoft.EntityFrameworkCore;" = @("\bIDbContext\b", "\bDbSet<", "\bDbContext\b", "\bEntityState\b", "\.AsNoTracking\(", "\.Include\(", "\.ExecuteUpdateAsync\(", "\.ExecuteDeleteAsync\(")
}

function Process-File {
    param($filePath)
    
    # Read file content and identify the current namespace to avoid self-referencing usings
    $content = Get-Content $filePath -Raw
    $originalContent = $content
    $added = @()

    $namespace = ""
    if ($content -match "namespace\s+(.*?);") {
        $namespace = $matches[1].Trim()
    }

    foreach ($using in $mapping.Keys) {
        $nsToImport = $using -replace "using (.*?);", '$1'
        
        # Rule of thumb: Persistence.Abstractions should not depend on EFCore directly
        if ($filePath -match "Persistence.Abstractions" -and $nsToImport -eq "Microsoft.EntityFrameworkCore") { continue }
        
        # Skip if already present or if it's the current namespace
        if ($content -match [regex]::Escape($using)) { continue }
        if ($namespace -eq $nsToImport) { continue }

        $patterns = $mapping[$using]
        foreach ($pattern in $patterns) {
            # Match word boundaries or generic angle brackets
            if ($content -match $pattern) {
                $added += $using
                break
            }
        }
    }

    if ($added.Count -gt 0) {
        # Sort and inject new usings at the top of the file
        $newUsings = ($added | Sort-Object) -join "`r`n"
        $content = $newUsings + "`r`n" + $content
        
        # Standardize line breaks and remove any duplicate usings introduced by the injection
        $content = $content -replace "(?m)^using (.*?);using (.*?);", "using `$1;`r`nusing `$2;"
        $lines = $content -split "`r?`n"
        $distinctLines = @()
        $foundUsings = @{}
        foreach ($line in $lines) {
            if ($line -match "^using (.*?);$") {
                $ns = $matches[1].Trim()
                if ($foundUsings.ContainsKey($ns) -or $ns -eq $namespace) { continue }
                $foundUsings[$ns] = $true
            }
            $distinctLines += $line
        }
        $content = $distinctLines -join "`r`n"
    }

    # Save only if modified
    if ($content -ne $originalContent) {
        $content | Set-Content $filePath -NoNewline
        Write-Host "Updated: $filePath"
    }
}

# Recursively process all .cs files excluding build artifacts
Write-Host "Injecting Standard Usings in: $(Get-Item $TargetDir).FullName"
Get-ChildItem -Path $TargetDir -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } | ForEach-Object {
    Process-File $_.FullName
}
