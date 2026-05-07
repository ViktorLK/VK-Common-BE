---
description: Generate production-ready sample code for a BuildingBlock module using the SampleGeneration.md prompt.
---

# Workflow: Generate Sample (Industrial Demo)

## Goal

Generate a production-ready sample project demonstrating how to use a specified BuildingBlock module, following the guidelines in `prompts/CodeReview/SampleGeneration.md` and the `always_on` architectural rules.

## Steps

1. **Identify Target & Load Context**:
    - Determine which BuildingBlock module to create a sample for.
    - If the target is unclear, ask the user: `"Which BuildingBlock module would you like me to generate a sample for?"`
    - **Mandatory (PS.04)**: Call `vk_get_module_context(path)`.
    - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.

2. **Load Rules**:
    - Read the `prompts/CodeReview/SampleGeneration.md` file to understand the sample code requirements (Result Pattern, DI, IOptions, logging, design pattern annotations).

3. **Analyze the Module**:
    - Read all source files and public APIs in the target module.
    - Identify the key interfaces, extension methods, and configuration options that consumers would use.

4. **Generate the Sample Project**:
    - Create a self-contained sample project under `samples/VK.Blocks.<ModuleName>.Sample/`.
    - Include:
        - A `.csproj` file referencing the target BuildingBlock.
        - A `Program.cs` or `Startup.cs` demonstrating DI registration via `Add[FeatureName]`.
        - One or more usage examples showing the core functionality.
        - `// [PATTERN]` annotations on key components (Strategy, Factory, Observer, etc.).
        - `// [RuleID]` tags for compliance points (e.g., `// [CS.01]`, `// [AP.01]`).
    - **Industrial DNA**:
        - Use `sealed record` for options/data. // [AP.01, BB.05]
        - Handle `Result<T>` patterns properly. // [CS.01]
        - Inject `TimeProvider` for time-sensitive logic. // [CS.06]
    - Follow modern C# conventions: file-scoped namespaces, primary constructors, async/await.

5. **Save and Report (Audit by Exception)**:
    - Save all generated files to the output directory.
    - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.
    - Audit: `Audit: ✅ Sample project created with Industrial DNA.`
    - Report: ✅ Sample project created at `[path]` with `[N]` files.
    - Verify with `dotnet build`.
    - // turbo
    ```powershell
    dotnet build samples/VK.Blocks.<ModuleName>.Sample/
    ```
