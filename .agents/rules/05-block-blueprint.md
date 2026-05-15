---
trigger: model_decision
---

# VK.Blocks: Building Block Blueprint (BB)

> **Note**: This file provides the concrete blueprint and implementation templates for the architectural principles defined in `04-architecture-patterns.md` (specifically AP.02–AP.04).

### BB.01 — Standard Folder Structure (Vertical Slice Priority)

Every BuildingBlock MUST prioritize a domain-driven vertical slice layout. Generic technical folders should be used sparingly as a last resort:

- **`{FeatureName}/` (MANDATORY)**: First-level domain folders for vertical slices (e.g., `ApiKeys/`, `Guids/`).
    - MUST contain all logic related to that domain (Interfaces, Handlers, Validators, etc.).
    - `Internal/`: Encapsulated implementations for the feature. **MUST NOT** be wrapped in a `Features/` folder.
- `VK{ModuleName}Block.cs`: **Public** marker type placed directly in the module's root directory. This SHOULD be the only `.cs` file in the root to maintain a clean entry point.
- `Abstractions/` (OPTIONAL): Use ONLY for top-level contracts shared across multiple features that cannot be assigned to a specific domain.
- `Common/` (OPTIONAL): Use ONLY for true cross-cutting utilities (e.g., `Utilities/`, `Constants/`).
- `Contracts/`: Cross-boundary public contracts (e.g., Integration Events, external DTOs).
- `DependencyInjection/`:
    - `VK{ModuleName}BlockExtensions.cs`: **Public entry point** (Wrapper).
    - `IVK{ModuleName}Builder.cs`: **Public builder interface** (Industrial DSL).
    - `VK{ModuleName}Options.cs`: **Public configuration** (Sealed Record).
    - `Internal/`:
        - `{ModuleName}BlockRegistration.cs`: **Principal registration logic**.
        - `{ModuleName}BlockBuilder.cs`: **Custom builder implementation**.
        - `{ModuleName}OptionsValidator.cs`: Options validation logic.
- `Diagnostics/`:
    - `DiagnosticsConstants.cs`: Semantic tokens.
    - `Internal/`: `[LoggerMessage]` and `[VKBlockDiagnostics]` classes.

### BB.02 — The Marker Pattern ([VKBlockMarker])

Each module MUST define a sealed partial class decorated with the `[VKBlockMarker]` attribute, placed in the module's root directory:

- **Source Generation**: DO NOT manually implement `IVKBlockMarker`. The interface and its properties (BlockName, ActivitySource, etc.) are automatically implemented via Source Generation based on the attribute metadata.
- **Partial Declaration**: The class MUST be declared as `sealed partial class`.
- **Namespace**: `VK.Blocks.{ModuleName}` (library root namespace, per AP.03 — public API surface).
- **Dependencies**: Explicitly define prerequisite blocks using the `Dependencies` property (e.g., `[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]`).
- **Activity/Meter**: The generated implementation uses `VKBlocksConstants.VKBlocksPrefix + ModuleName`.

### BB.03 — Idempotent DI Registration (Wrapper vs Core)

#### Public Wrapper

The public extension class MUST delegate to the internal registration class.

```csharp
namespace VK.Blocks.{ModuleName};

public static class VK{ModuleName}BlockExtensions
{
    public static IVK{ModuleName}Builder AddVK{ModuleName}Block(this IServiceCollection services, IConfiguration configuration)
        => {ModuleName}BlockRegistration.Register(services, configuration);
}
```

#### Internal Core (Registration Sequence)

**STRICT CONSTRAINT**: The entire registration flow MUST be synchronous. Executing I/O operations (e.g., database calls, network requests) or using blocking async calls (`Task.Wait()`, `.GetAwaiter().GetResult()`) inside DI extensions is STRICTLY PROHIBITED as it leads to startup deadlocks and thread pool starvation.

The `Register` method in `Internal/{ModuleName}BlockRegistration.cs` MUST follow this exact order:

1.  **Check-Self & Prerequisite**: `if (services.IsVKBlockRegistered<{ModuleName}Block>()) return builder;` (Includes `VKGuard` boundary checks for parameters).
2.  **Options Registration**: `var options = services.AddVKBlockOptions<VK{ModuleName}Options>(configuration);`
3.  **Mark-Self**: `services.AddVKBlockMarker<{ModuleName}Block>();` (MUST be called BEFORE early exit).
4.  **Options Validation**: `services.TryAddEnumerableSingleton<IValidateOptions<VK{ModuleName}Options>, {ModuleName}OptionsValidator>();`
5.  **Diagnostics & Metadata**: Register `IVKSecurityMetadataProvider` and `ActivitySource`/`Meter`.
6.  **Core Services**: Register actual logic using idempotent `TryAdd` patterns.
7.  **SG Integration**: `services.AddGenerated{ModuleName}Handlers();` (If using Source Generators for Handlers/Validators).
8.  **Feature Toggle**: `if (!options.Enabled) return builder;`

### BB.04 — Diagnostics Blueprint

- **Metadata**: Register an `ISecurityMetadataProvider` (if applicable) to provide version info to diagnostics.
- **Diagnostics Class**: Annotated with `[VKBlockDiagnostics(typeof({ModuleName}Block))]`.
- **Constants**: Defined in `DiagnosticsConstants.cs`.

### BB.05 — Options Architecture

- **Immutability**: MUST be a `sealed record` with `init` properties.
- **Functional Transformation (ADR-016)**: ANY code-based configuration MUST use the **`Func<T, T> configure`** pattern (instead of `Action<T>`) to support immutability via `with` expressions.
- **Naming**: MUST use `VK` prefix (e.g., `VKXxxOptions`).
- **Interface**: MUST implement `IVKBlockOptions` (as required by AP.04).
- **SectionName**: Formatted according to AP.04.
- **Validation**: MUST have a corresponding `IValidateOptions` implementation.

### BB.06 — Modular Feature Pattern

Complex blocks containing multiple independent features MUST follow this sub-registration pattern:

- **Feature Marker**: Define an internal marker class decorated with `[VKFeatureMarker("FeatureName", typeof(VKParentBlock))]`.
- **Chained Builder**: Use extension methods on `IVK{ModuleName}Builder` to add features (e.g., `builder.AddFeatureA()`).
- **Idempotent Feature Registration**: Each feature registration method MUST check its own `[VKFeatureMarker]` before proceeding.
- **Hierarchical Options**: Feature options MUST reside under the parent block's configuration section (e.g., `VK:Blocks:Parent:Feature`).
