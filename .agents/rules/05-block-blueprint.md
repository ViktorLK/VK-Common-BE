---
trigger: always_on
---

# VK.Blocks: Building Block Blueprint (Refined)

> **Note**: This file provides the concrete blueprint and implementation templates for the architectural principles defined in `04-architecture-patterns.md` (specifically Rules 13-15).

### Rule 16 — Standard Folder Structure

Every new BuildingBlock MUST follow this vertical slice directory layout:
- `VK{ModuleName}Block.cs`: **Public** marker type placed directly in the module's root directory.
- `Abstractions/`: Shared interfaces and core logic abstractions.
    - `Internal/`: Encapsulated implementations for shared abstractions (if any).
- `Common/`: Shared utilities, constants, or cross-cutting models.
    - `Internal/`: Encapsulated implementations for shared utilities (if any).
- `Contracts/`: Cross-boundary public contracts (e.g., Integration Events, external DTOs).
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

- `{FeatureName}/`: First-level domain folders for vertical slices (e.g., `ApiKeys/`, `Jwt/`). **MUST NOT** be wrapped in a `Features/` folder.
    - `Internal/`: Encapsulated implementations (Handlers, Validators, feature-specific `[LoggerMessage]` classes, etc).

### Rule 17 — The Marker Pattern (IVKBlockMarker)

Each module MUST have a sealed partial class implementing `IVKBlockMarker` placed in the module's root directory:
- **Namespace**: `VK.Blocks.{ModuleName}` (library root namespace, per Rule 14 — public API surface).
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
1.  **Check-Self & Prerequisite**: `if (services.IsVKBlockRegistered<{ModuleName}Block>()) return builder;` (This smart check automatically validates dependencies).
2.  **Options Registration**: `var options = services.AddVKBlockOptions<VK{ModuleName}Options>(configuration);`
3.  **Mark-Self**: `services.AddVKBlockMarker<{ModuleName}Block>();` (MUST be called BEFORE early exit).
4.  **Options Validation**: `services.TryAddEnumerableSingleton<IValidateOptions<VK{ModuleName}Options>, {ModuleName}OptionsValidator>();`
5.  **Diagnostics/Static Metadata**: Register ActivitySource/Meter/SecurityMetadata.
6.  **Feature Toggle**: `if (!options.Enabled) return builder;`
7.  **Core Services**: Register the actual feature logic.

### Rule 19 — Diagnostics Blueprint

- **Metadata**: Register an `ISecurityMetadataProvider` (if applicable) to provide version info to diagnostics.
- **Diagnostics Class**: Annotated with `[VKBlockDiagnostics(typeof({ModuleName}Block))]`.
- **Constants**: Defined in `DiagnosticsConstants.cs`.

### Rule 20 — Options Architecture

- **Immutability**: MUST be a `sealed record` with `init` properties.
- **Naming**: MUST use `VK` prefix (e.g., `VKXxxOptions`).
- **Interface**: MUST implement `IVKBlockOptions` (as required by Rule 15).
- **SectionName**: Formatted according to Rule 15.
- **Validation**: MUST have a corresponding `IValidateOptions` implementation.
