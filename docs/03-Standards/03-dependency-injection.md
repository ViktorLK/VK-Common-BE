# Standard 03: Dependency Injection & Configuration

## 1. Idempotent Registration Sequence
Every building block must follow the exact synchronous registration order in its `Internal/{ModuleName}BlockRegistration.cs`:

1.  **IsVKBlockRegistered**: Early exit if already registered.
2.  **AddVKBlockOptions**: Register and bind options from `IConfiguration`.
3.  **AddVKBlockMarker**: Mark the module as registered.
4.  **TryAddEnumerable (Validator)**: Register options validators.
5.  **Diagnostics**: Initialize ActivitySource/Meter.
6.  **Feature Toggle**: Exit if `options.Enabled == false`.
7.  **TryAddCoreServices**: Register internal logic using `TryAdd`.

## 2. Options Architecture
- **Immutable Records**: Options must be `sealed record` with `init` properties.
- **Transformation**: Use `Func<TOptions, TOptions> configure` pattern (ADR-016).
- **Validation**: All options must implement `IVKBlockOptions` and have a corresponding `IValidateOptions`.

## 3. Marker Pattern
- **[VKBlockMarker]**: Apply to a `sealed partial class` in the root of the module.
- **Dependency Validation**: Prerequisite modules (like `Core`) are automatically validated via the marker.

## 4. Implementation Example
```csharp
public static IServiceCollection AddAuthBlock(this IServiceCollection services, IConfiguration config)
{
    if (services.IsVKBlockRegistered<VKAuthBlock>()) return services;
    
    var options = services.AddVKBlockOptions<VKAuthOptions>(config);
    services.AddVKBlockMarker<VKAuthBlock>();
    
    // ... rest of the sequence
    return services;
}
```
