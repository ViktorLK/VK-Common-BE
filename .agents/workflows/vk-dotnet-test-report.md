---
description: Run dotnet test with code coverage and generate HTML report using a PowerShell script.
---

1. **Determine Test Target**:
    - Identify the test project (e.g., `.csproj` under `test/` directory).
    - Determine if a deep clean is needed (e.g., if files were moved or coverage seems stale).

2. **Run the script**:
    - Execute the PowerShell script with appropriate parameters.
    - Path: `prompts/HitoriGoto/dotnettest.ps1`

// turbo 3. **Execute**:

```powershell
# Basic execution
powershell.exe -ExecutionPolicy Bypass -File prompts/HitoriGoto/dotnettest.ps1 -Project test/BuildingBlocks/Authentication/VK.Blocks.Authentication.UnitTests.csproj -NoBrowser

# Deep clean and specific reports
powershell.exe -ExecutionPolicy Bypass -File prompts/HitoriGoto/dotnettest.ps1 -Project test/BuildingBlocks/Authentication/VK.Blocks.Authentication.UnitTests.csproj -Clean -ReportTypes "Html;TextSummary" -NoBrowser
```

4. **Completion**:
    - Analyze the coverage summary displayed in the console output.
    - Report the key metrics and any areas needing improvement to the user.

