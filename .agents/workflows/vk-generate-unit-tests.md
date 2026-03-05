---
description: Generate high-quality unit tests for a target class or handler using the UnitTest.md prompt.
---

## Goal

Generate production-ready unit tests for a user-specified C# class, handler, or service, following the guidelines in `prompts/CodeReview/UnitTest.md` and the `always_on` architectural rules (especially Rule 9).

## Steps

1. **Identify the Target**:
    - Determine the **absolute path of the C# file or class** to generate tests for.
    - If the target is unclear, ask the user: `"Which class or file would you like me to generate unit tests for?"`

2. **Load Rules**:
    - Read the `prompts/CodeReview/UnitTest.md` file to understand the testing guidelines (xUnit, Moq, FluentAssertions, AAA pattern, naming conventions).

3. **Analyze the Target Code**:
    - Read the target file and identify all public methods, their dependencies (constructor-injected interfaces), and edge cases.
    - Determine which interfaces need to be mocked.

4. **Generate Tests**:
    - For each public method, generate tests covering the following scenarios as required by `always_on` Rule 9:
        - ✅ **Happy Path**: Core success scenario.
        - ✅ **Not Found / Empty Result**: Cases where the operation returns no data.
        - ✅ **Permission / Tenant Isolation Failure**: Unauthorized access or tenant mismatch.
        - ✅ **Infrastructure Failure → Result.Failure**: Simulated exceptions from dependencies mapped to `Result.Failure`.
    - Additionally, cover boundary and edge cases as defined in `UnitTest.md` (null, empty collections, special characters, etc.).
    - Use `[Theory]` with `[InlineData]` when the same logic has multiple input combinations.
    - Method naming: `MethodName_Condition_ExpectedResult`.
    - Use `// Arrange`, `// Act`, `// Assert` comments to clearly separate sections.

5. **Save and Report**:
    - Determine the appropriate test project path (mirror the source structure under `tests/`).
    - Save the generated test file.
    - Report to the user: ✅ Test file saved to `[path]` with `[N]` test methods covering `[M]` scenarios.
