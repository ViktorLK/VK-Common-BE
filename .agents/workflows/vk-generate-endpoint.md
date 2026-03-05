---
description: Generate CQRS boilerplate (Command/Query, Handler, Validator, Endpoint) for a specific Entity/Action.
---

## Goal

Automatically generate a fully compliant Vertical Slice / CQRS features (Command or Query, Handler, Validator, and Minimal API Endpoint) for a given entity and action, strictly following all VK.Blocks `always_on` architectural rules.

## Steps

1. **Identify the Target**:
    - Ask the user for the **Action** and **Entity** (e.g., `Create`, `User`).
    - Ask the user for the **Target Module Path** (e.g., `src/BuildingBlocks/Identity`).
    - If missing, prompt: `"What is the Action (e.g. Create, Get, Update) and Entity? And which module should I place this in?"`

2. **Analyze the Context**:
    - Briefly read existing folders in the target module to understand the project structure (e.g., are features grouped by folder like `Features/Users/CreateUser/`?).

3. **Generate Component 1: Command / Query**:
    - Generate `[Action][Entity]Command.cs` or `[Action][Entity]Query.cs`.
    - Implement `IRequest<Result<ResponseDto>>` (using MediatR and the VK.Blocks Result Pattern).
    - Use `record` struct or `sealed record` for immutability.

4. **Generate Component 2: Handler**:
    - Generate `[Action][Entity]CommandHandler.cs`.
    - Implement `IRequestHandler<Command, Result<ResponseDto>>`.
    - **Enforce Rules**:
        - ✅ Rule 1: Return `Result.Success(...)` or `Result.Failure(...)`.
        - ✅ Rule 3: Use `async/await` and pass `CancellationToken`.
        - ✅ Rule 4: Use `.AsNoTracking()` if it's a Query.
        - ✅ Rule 6: Inject `ILogger` and use structured logging without string interpolation.

5. **Generate Component 3: Validator**:
    - Generate `[Action][Entity]Validator.cs`.
    - Inherit from `AbstractValidator<Command>` (FluentValidation).
    - Add standard validation rules.

6. **Generate Component 4: Minimal API Endpoint**:
    - Generate `[Entity]Endpoints.cs` (or append to existing).
    - Use `MapPost`, `MapGet`, etc.
    - Inject `ISender` (MediatR).
    - Map `Result<T>` to RFC 7807 problem details if failure (e.g., returning 400 Bad Request or 404 Not Found).

7. **Save and Report**:
    - Save all generated files into the target module directory.
    - Ensure `file-scoped namespaces` and XML documentation are used.
    - Report: ✅ Generated CQRS feature `[Action][Entity]` with 4 components at `[Path]`.
