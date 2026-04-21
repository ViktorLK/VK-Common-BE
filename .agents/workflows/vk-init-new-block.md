# Workflow: VK.Blocks Library Initializer (Bootstrap)

Use this workflow to "Bootstrap" a new building block following the **Final Refined Architecture**.

## Step 1: Physical Layout
Initialize the base directory `src/BuildingBlocks/{ModuleName}/`:
- [ ] Create `Abstractions/`, `Contracts/`, `Diagnostics/`, `Features/`.
- [ ] Create `DependencyInjection/Internal/`.
- [ ] Create `Diagnostics/Internal/`.

## Step 2: The Marker (Contract)
- [ ] File: `Contracts/{ModuleName}Block.cs`.
- [ ] Pattern: `sealed partial class {ModuleName}Block : IVKBlockMarker`.
- [ ] Rule: No `VK` prefix for the block marker.

## Step 3: Public Entry Point (Wrapper)
- [ ] File: `DependencyInjection/VK{ModuleName}BlockExtensions.cs`.
- [ ] Namespace: `VK.Blocks.{ModuleName}` (Root).
- [ ] Logic: Delegates immediately to `{ModuleName}BlockRegistration.Register`.

## Step 4: Configuration (Options)
- [ ] File: `DependencyInjection/VK{ModuleName}Options.cs`.
- [ ] Type: `sealed record` with `init` properties.
- [ ] Implementation: `IVKBlockOptions`.
- [ ] Rule: MUST use `VK` prefix.
- [ ] Section: `VKBlocksConstants.VKBlocksConfigPrefix + "{ModuleName}"`.

## Step 5: Master Registration (Internal Core)
- [ ] File: `DependencyInjection/Internal/{ModuleName}BlockRegistration.cs`.
- [ ] Namespace: `VK.Blocks.{ModuleName}.DependencyInjection.Internal`.
- [ ] Pattern: Follow **Refined Rule 18 execution order**:
    1.  Check-Self.
    2.  Check-Prerequisite (`EnsureVKCoreBlockRegistered`).
    3.  Add Options.
    4.  Add Marker (Mark-Self).
    5.  Add Validator (`TryAddEnumerableSingleton<IValidateOptions<...>>`).
    6.  Diagnostics/Diagnostics Metadata.
    7.  Toggle Check (`if (!options.Enabled) return builder;`).
    8.  Register Core Services.

## Step 6: Custom Builder & Validator
- [ ] File: `DependencyInjection/Internal/{ModuleName}BlockBuilder.cs`.
- [ ] File: `DependencyInjection/Internal/{ModuleName}OptionsValidator.cs`.

## Step 7: Diagnostics Integration
- [ ] File: `Diagnostics/DiagnosticsConstants.cs`.
- [ ] File: `Diagnostics/Internal/{ModuleName}Diagnostics.cs` (with `[VKBlockDiagnostics]`).

## Step 8: Documentation & Verification
- [ ] Run `/vk-generate-readme`.
- [ ] Run `dotnet build`.
- [ ] Run `vk-audit-architecture` to confirm Rule 16-20 compliance.
