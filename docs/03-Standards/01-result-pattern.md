# Standard 01: Result Pattern & Error Handling

## 1. Core Philosophy
All operations in the Application Layer MUST return a `Result<T>` (or `Result` / `Result<Unit>` for void operations). Exceptions are for exceptional infrastructure failures ONLY and must be caught at the boundary.

## 2. Error Code Hierarchy
Error codes must follow the structured format: `{ModuleName}.{Category}.{Reason}`.

| Level | Component | Example |
| :--- | :--- | :--- |
| 1 | Module | `Auth`, `Core`, `AI` |
| 2 | Category | `ApiKey`, `Permission`, `Cache` |
| 3 | Reason | `Invalid`, `Expired`, `NotFound` |

### Global Errors
Cross-cutting errors belong to `VKCoreErrors` (e.g., `Core.Validation.Failed`).

## 3. Implementation Rules
- **No Nulls**: Never return null. If data is missing, return `Result.Failure(Error.NotFound)`.
- **Predefined Errors**: Use `static readonly Error` constants. Avoid raw string failures.
- **Fluent Validation**: Use `VKGuard` for boundary checks before entering logic.
- **RFC 7807**: HTTP responses must map Result failures to Problem Details.

## 4. Usage Example
```csharp
public async Task<Result<Unit>> Handle(UpdateKeyCommand request, CancellationToken ct)
{
    var key = await _store.GetAsync(request.Id, ct);
    if (key is null) return Result.Failure(AuthErrors.ApiKey.NotFound);

    key.Update(request.Value);
    await _store.SaveAsync(key, ct);
    
    return Result.Success();
}
```
