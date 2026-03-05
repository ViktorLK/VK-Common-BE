---
description: Scan and refactor legacy exception throwing to VK.Blocks standard Result.Failure<T> pattern.
---

## Goal

Identify legacy exception-throwing patterns (e.g., `throw new Exception`, `throw new NotFoundException`) in the Application Layer and refactor them to strictly use the `Result<T>` pattern and centralized `Error` constants, strictly adhering to VK.Blocks Rule 1.

## Steps

1. **Identify the Target**:
    - Get the absolute path of the directory or file to refactor.
    - If not provided, ask: `"Which directory or file would you like me to refactor exceptions for?"`

2. **Scan for Violations**:
    - Read the `.cs` files in the target path.
    - Search for `throw new` statements deep within the Application Layer or Domain Layer business logic.
    - _Note: Exceptions thrown at the Infrastructure boundary (e.g., uncatchable DB errors) might be okay, but application logic MUST use `Result<T>`._

3. **Refactor Errors Class**:
    - Identify or create the domain's centralized `Errors` class (e.g., `[Domain]Errors.cs`).
    - Translate the hardcoded exception messages into `static readonly Error` definitions.
        - Example: `public static readonly Error UserNotFound = Error.NotFound("User.NotFound", "The requested user does not exist.");`

4. **Refactor Methods**:
    - Change the return type of the offending methods from `Task<T>` or `T` to `Task<Result<T>>` or `Result<T>`.
    - Replace `throw new ...` with `return Result.Failure<T>([Domain]Errors.[SpecificError]);`
    - Replace successful returns like `return data;` with `return Result.Success(data);`
    - Propagate the `Result<T>` change up the call stack to the Handler or Controller/Endpoint level.

5. **Save and Report**:
    - **Overwrite** the original files with the refactored, compile-ready code.
    - Output a list of files completely refactored.
    - Briefly summarize how many exceptions were replaced gracefully.
