---
description: Generate CQRS boilerplate (Command/Query, Handler, Validator, Endpoint) for a specific Entity/Action.
---

# Workflow: Generate Endpoint (CQRS Slice)

## Goal

Automatically generate a fully compliant Vertical Slice / CQRS features (Command or Query, Handler, Validator, and Minimal API Endpoint) for a given entity and action, strictly following all VK.Blocks `always_on` architectural rules.

## Steps

1. **Identify Target & Load Context**:
    - Ask the user for the **Action** and **Entity** (e.g., `Create`, `User`).
    - Ask the user for the **Target Module Path** (e.g., `src/BuildingBlocks/Identity`).
    - If missing, prompt: `"What is the Action (e.g. Create, Get, Update) and Entity? And which module should I place this in?"`
    - **Mandatory (PS.04)**: Call `vk_get_module_context(path)`.
    - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.

2. **Analyze the Context**:
    - Briefly read existing folders in the target module to understand the project structure (e.g., are features grouped by folder like `Features/Users/CreateUser/`?).

3. **Generate Component 1: Command / Query**:
    - Generate `[Action][Entity]Command.cs` or `[Action][Entity]Query.cs`.
    - Implement `IRequest<Result<ResponseDto>>` (using MediatR and the VK.Blocks Result Pattern). // [CS.01]
    - Use `record` struct or `sealed record` for immutability. // [AP.01]
    - Use `required` for mandatory fields. // [AP.01]

4. **Generate Component 2: Handler**:
    - Generate `[Action][Entity]CommandHandler.cs`.
    - Implement `IRequestHandler<Command, Result<ResponseDto>>`.
    - **Enforce Rules**:
        - ✅ **CS.01**: Return `Result.Success(...)` or `Result.Failure(...)`. // [CS.01]
        - ✅ **CS.03**: Use `async/await` and pass `CancellationToken`. Use `.ConfigureAwait(false)`. // [CS.03]
        - ✅ **CS.04**: Use `.AsNoTracking()` if it's a Query. // [CS.04]
        - ✅ **OR.01**: Inject `ILogger` and use `[LoggerMessage]` source gen. // [OR.01]
        - ✅ **AP.01**: Use `sealed` and `VKGuard` in Ctor. // [AP.01]
        - ✅ **CS.06**: Use `TimeProvider` or `IVKGuidGenerator`. // [CS.06]

5. **Generate Component 3: Validator**:
    - Generate `[Action][Entity]Validator.cs`.
    - Inherit from `AbstractValidator<Command>` (FluentValidation).
    - Add standard validation rules.

6. **Generate Component 4: Minimal API Endpoint**:
    - Generate `[Entity]Endpoints.cs` (or append to existing).
    - Use `MapPost`, `MapGet`, etc.
    - Inject `ISender` (MediatR).
    - Map `Result<T>` to RFC 7807 problem details if failure (e.g., returning 400 Bad Request or 404 Not Found).

7. **Save and Report (Audit by Exception)**:
    - Save all generated files into the target module directory.
    - Ensure `file-scoped namespaces` and XML documentation are used.
    - Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.
    - Audit: `Audit: ✅ All DNA constraints satisfied.`
    - Report: ✅ Generated CQRS feature `[Action][Entity]` with 4 components at `[Path]`.
    - Include `// [RuleID]` tags in the code at decision boundaries.
