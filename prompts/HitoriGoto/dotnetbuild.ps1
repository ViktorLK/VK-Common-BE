$projects = Get-ChildItem -Path "src\BuildingBlocks" -Recurse -Filter "*.csproj"

foreach ($project in $projects) {
    Write-Host "Building $($project.Name)..." -ForegroundColor Cyan
    dotnet build $project.FullName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed for $($project.Name)"
        exit 1
    }
}

Write-Host "All $($projects.Count) projects built successfully." -ForegroundColor Green
