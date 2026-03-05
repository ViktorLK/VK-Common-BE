---
description: Generate production-ready sample code for a BuildingBlock module using the SampleGeneration.md prompt.
---

## Goal

Generate a production-ready sample project demonstrating how to use a specified BuildingBlock module, following the guidelines in `prompts/CodeReview/SampleGeneration.md` and the `always_on` architectural rules.

## Steps

1. **Identify the Target Module**:
    - Determine which BuildingBlock module to create a sample for.
    - If the target is unclear, ask the user: `"Which BuildingBlock module would you like me to generate a sample for?"`

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
    - Follow modern C# conventions: file-scoped namespaces, primary constructors, async/await.

5. **Save and Report**:
    - Save all generated files to the output directory.
    - Report: ✅ Sample project created at `[path]` with `[N]` files.
