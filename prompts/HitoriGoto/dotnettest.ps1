param (
    [string]$Project = ".",
    [string]$Configuration = "Release",
    [switch]$Clean = $false,
    [string]$ReportTypes = "Html;TextSummary",
    [switch]$NoBrowser = $false
)

$ErrorActionPreference = "Stop"

# 1. Resolve Paths
$targetPath = (Resolve-Path $Project).Path
if (-not (Test-Path $targetPath -PathType Container)) {
    $targetDir = Split-Path -Parent $targetPath
} else {
    $targetDir = $targetPath
}

Write-Host "Target Directory: $targetDir" -ForegroundColor Cyan

# 2. Cleanup if requested
if ($Clean) {
    Write-Host "Cleaning up bin, obj and TestResults..." -ForegroundColor Yellow
    Get-ChildItem -Path $targetDir -Include bin,obj -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    if (Test-Path "$targetDir/TestResults") {
        Remove-Item -Path "$targetDir/TestResults" -Recurse -Force
    }
}

# 3. Build & Test
Write-Host "Building and Testing..." -ForegroundColor Cyan
dotnet build $targetPath --configuration $Configuration
dotnet test $targetPath --configuration $Configuration --collect:"XPlat Code Coverage" --results-directory "$targetDir/TestResults" --no-build

# 4. Find Coverage File
$coverageFile = Get-ChildItem -Path "$targetDir/TestResults" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1
if (-not $coverageFile) {
    Write-Error "Coverage file (coverage.cobertura.xml) not found in $targetDir/TestResults"
}
Write-Host "Found coverage file: $($coverageFile.FullName)" -ForegroundColor Green

# 5. Generate Report
Write-Host "Generating Reports ($ReportTypes)..." -ForegroundColor Cyan
$reportDir = "$targetDir/TestResults/Reports"
if (-not (Test-Path $reportDir)) { New-Item -ItemType Directory -Path $reportDir -Force }

reportgenerator `
    -reports:"$($coverageFile.FullName)" `
    -targetdir:"$reportDir" `
    -reporttypes:"$ReportTypes"

# 6. Output Summary to Console (AI Friendly)
$summaryPath = "$reportDir/Summary.txt"
if (Test-Path $summaryPath) {
    Write-Host "`n--- Coverage Summary ---" -ForegroundColor Cyan
    Get-Content $summaryPath
    Write-Host "-------------------------`n" -ForegroundColor Cyan
}

# 7. Open Browser if requested
if (-not $NoBrowser -and $ReportTypes -like "*Html*") {
    $indexPath = "$reportDir/index.html"
    if (Test-Path $indexPath) {
        Write-Host "Opening report: $indexPath" -ForegroundColor Green
        Start-Process $indexPath
    }
}

Write-Host "Operation completed successfully." -ForegroundColor Green
