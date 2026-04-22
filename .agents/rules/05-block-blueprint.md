# VK.Blocks: Building Block Blueprint (Refined)

### Rule 16 — Standard Folder Structure

Every new BuildingBlock MUST follow this vertical slice directory layout:
- `Abstractions/`: Internal interfaces and core logic abstractions.
- `Common/`: Shared utilities or internal constants.
- `Contracts/`: **Public** marker types (`XxxBlock`). MUST NOT contain logic.
- `DependencyInjection/`: 
    - `VK{ModuleName}BlockExtensions.cs`: **Public entry point** (Wrapper).
    - `VK{ModuleName}Options.cs`: **Public configuration** (Sealed Record).
    - `Internal/`:
        - `{ModuleName}BlockRegistration.cs`: **Principal registration logic**.
        - `{ModuleName}BlockBuilder.cs`: Custom builder implementation.
        - `{ModuleName}OptionsValidator.cs`: Options validation logic.
- `Diagnostics/`: 
    - `DiagnosticsConstants.cs`: Semantic tokens.
    - `Internal/`: `[LoggerMessage]` and `[VKBlockDiagnostics]` classes.
- `Features/`: Vertical slices (e.g., `ApiKeys/`, `Jwt/`).

### Rule 17 — The Marker Pattern (IVKBlockMarker)

Each module MUST have a sealed partial class implementing `IVKBlockMarker` in `Contracts/`:
- **Namespace**: `VK.Blocks.{ModuleName}.Contracts`.
- **Identifier**: Match the module name (e.g., "Authentication").
- **Activity/Meter**: Use `VKBlocksConstants.VKBlocksPrefix + Identifier`.

### Rule 18 — Idempotent DI Registration (Wrapper vs Core)

#### 18.1 Public Wrapper
The public extension class MUST delegate to the internal registration class.
```csharp
namespace VK.Blocks.{ModuleName};

public static class VK{ModuleName}BlockExtensions 
{
    public static IVK{ModuleName}Builder Add{ModuleName}Block(this IServiceCollection services, IConfiguration configuration)
        => {ModuleName}BlockRegistration.Register(services, configuration);
}
```

#### 18.2 Internal Core (Registration Sequence)
The `Register` method in `Internal/{ModuleName}BlockRegistration.cs` MUST follow this exact order:
1.  **Check-Self**: `if (services.IsVKBlockRegistered<{ModuleName}Block>()) return builder;`
2.  **Check-Prerequisite**: `services.EnsureVKCoreBlockRegistered<{ModuleName}Block>();`
3.  **Options Registration**: `var options = services.AddVKBlockOptions<VK{ModuleName}Options>(configuration);`
4.  **Mark-Self**: `services.AddVKBlockMarker<{ModuleName}Block>();` (MUST be called BEFORE early exit).
5.  **Options Validation**: `services.TryAddEnumerableSingleton<IValidateOptions<VK{ModuleName}Options>, {ModuleName}OptionsValidator>();`
6.  **Diagnostics/Static Metadata**: Register ActivitySource/Meter/SecurityMetadata.
7.  **Feature Toggle**: `if (!options.Enabled) return builder;`
8.  **Core Services**: Register the actual feature logic.

### Rule 19 — Diagnostics Blueprint

- **Metadata**: Register an `ISecurityMetadataProvider` (if applicable) to provide version info to diagnostics.
- **Diagnostics Class**: Annotated with `[VKBlockDiagnostics(typeof({ModuleName}Block))]`.
- **Constants**: Defined in `DiagnosticsConstants.cs`.

### Rule 20 — Options Architecture

- **Immutability**: MUST be a `sealed record` with `init` properties.
- **Naming**: MUST use `VK` prefix (e.g., `VKXxxOptions`).
- **SectionName**: Use `VKBlocksConstants.VKBlocksConfigPrefix + "{ModuleName}"`.
- **Validation**: MUST have a corresponding `IValidateOptions` implementation.
