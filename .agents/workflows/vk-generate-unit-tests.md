---
description: Generate high-quality unit tests for a target class or handler using the UnitTest.md prompt.
---

# Workflow: Generate Unit Tests (Industrial Testing)

## Goal

Generate production-ready unit tests for a user-specified C# class, handler, or service, following the guidelines in `prompts/CodeReview/UnitTest.md` and the `always_on` architectural rules (especially DL.01).

## Steps

1. **Identify Target & Load Context**:
    - Determine the **absolute path of the C# file or class** to generate tests for.
    - If the target is unclear, ask the user: `"Which class or file would you like me to generate unit tests for?"`
    - **Mandatory (PS.04)**: Call `vk_get_module_context(path)`.
    - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.

2. **Load Rules**:
    - Read the `prompts/CodeReview/UnitTest.md` file to understand the testing guidelines (xUnit, Moq, FluentAssertions, AAA pattern, naming conventions).

3. **Analyze the Target Code**:
    - Read the target file and identify all public methods, their dependencies (constructor-injected interfaces), and edge cases.
    - Determine which interfaces need to be mocked.

4. **Generate Tests (DL.01) 🔴**:
    - For each public method, generate tests covering the following scenarios as required by **DL.01**:
        - ✅ **Happy Path**: Core success scenario.
        - ✅ **Not Found / Empty Result**: Cases where the operation returns no data.
        - ✅ **Permission / Tenant Isolation Failure**: Unauthorized access or tenant mismatch.
        - ✅ **Infrastructure Failure → Result.Failure**: Simulated exceptions from dependencies mapped to `Result.Failure`.
    - Additionally, cover boundary and edge cases as defined in `UnitTest.md` (null, empty collections, special characters, etc.).
    - Use `[Theory]` with `[InlineData]` when the same logic has multiple input combinations.
    - Method naming (DL.01): `{MethodName}_{Condition}_{ExpectedResult}`.
    - **Async Hygiene (CS.03)**: **PROHIBITED** to use `.ConfigureAwait(false)` in test code.
    - **Determinism (CS.06)**: Mock/Inject `TimeProvider` or `IVKGuidGenerator`.
    - Use `// Arrange`, `// Act`, `// Assert` comments to clearly separate sections.

5. **Save and Report (Audit by Exception)**:
    - Determine the appropriate test project path (mirror the source structure under `tests/`).
    - Save the generated test file.
    - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.
    - Audit: `Audit: ✅ All testing DNA constraints satisfied.`
    - Report: ✅ Test file saved to `[path]` with `[N]` test methods covering `[M]` scenarios.
    - Verify with `dotnet build`.
    - // turbo
    ```powershell
    dotnet build tests/
    ```
