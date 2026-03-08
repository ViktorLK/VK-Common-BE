---
description: Run dotnet test with code coverage and generate HTML report using a PowerShell script.
---

1. **Determine Test Target**:
    - If a test project (e.g., `.csproj` under `tests/` directory) is currently selected or specified, use its directory.
    - If not, ask the user: `"Which test project or directory should I generate the report for?"`

2. **Verify the script exists**:
    - Path: `prompts/HitoriGoto/dotnettest.ps1`

3. **Run the script**:
    - Execute the PowerShell script in the target test project directory to perform clean, build, test, and report generation.
    - Note: This script will open the HTML report in the default browser.

// turbo 4. **Execute**:

```powershell
powershell.exe -ExecutionPolicy Bypass -File prompts/HitoriGoto/dotnettest.ps1
```

5. **Completion**:
    - Report the result to the user, confirming the script has been executed and the report should be opening.
