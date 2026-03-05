---
description: Generate a production-grade Azure DevOps YAML pipeline using the DevOpsWorkflow.md prompt.
---

## Goal

Generate a complete, production-grade Azure DevOps YAML pipeline (azure-pipelines.yml) for the project, following the guidelines in `prompts/CodeReview/DevOpsWorkflow.md`.

## Steps

1. **Identify the Scope**:
    - Determine which project(s) the pipeline should cover (e.g., entire solution or specific modules).
    - Ask the user for deployment target if not specified: `"What is the deployment target? (App Service / AKS / Docker Container)"`

2. **Load Rules**:
    - Read the `prompts/CodeReview/DevOpsWorkflow.md` file to understand the pipeline stage requirements (Build, Test & Coverage, Deploy).

3. **Analyze the Project Structure**:
    - Identify `.csproj` files, test projects, and solution structure to configure correct paths and patterns.

4. **Generate the Pipeline**:
    - Create a complete `azure-pipelines.yml` with:
        - **Build Stage**: `DotNetCoreCLI@2` for restore/build, `Cache@2` for NuGet caching.
        - **Test & Coverage Stage**: `dotnet test` with `coverlet.collector`, cobertura XML output, HTML report generation via `reportgenerator`, `PublishCodeCoverageResults@1`, `PublishTestResults@2`, and an 80% coverage threshold gate.
        - **Deploy Stage**: Environment-bound with approval support, variable groups for secrets, targeting the user's chosen platform.
    - Include advanced features: pipeline artifacts, concurrency limits, minimal-permission `GITHUB_TOKEN`.

5. **Save and Report**:
    - Save the pipeline YAML file to the project root (or user-specified path).
    - List required NuGet packages (e.g., `coverlet.collector`).
    - Provide a brief note on how to view the HTML coverage report in Azure DevOps.
    - Report: ✅ Pipeline saved to `[path]`.
