# Workflow: VK.Blocks Library Initializer (Bootstrap)

Use this workflow to "Bootstrap" a new building block following the **Final Refined Architecture (Industrial DNA v2)**.

## Step 1: Physical Layout (BB.01, AP.03) 🟡
Initialize the base directory `src/BuildingBlocks/{ModuleName}/`:
- [ ] Create `Contracts/`, `Diagnostics/`.
- [ ] Create `{PrimaryFeature}/Internal/`. // Preferred over generic 'Features/'
- [ ] Create `DependencyInjection/Internal/`.
- [ ] Create `Diagnostics/Internal/`.
- [ ] Root: Create `VK{ModuleName}Block.cs`. // [BB.01/02]

## Step 2: The Marker (Contract - BB.02) 🔴
- [ ] File: `VK{ModuleName}Block.cs` (Root).
- [ ] Pattern: `public sealed partial class VK{ModuleName}Block`. // [BB.02, AP.01]
- [ ] Attribute: `[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]`. // [BB.02]

## Step 3: Public Entry Point (Wrapper - BB.03) 🔴
- [ ] File: `DependencyInjection/VK{ModuleName}BlockExtensions.cs`.
- [ ] Namespace: `VK.Blocks.{ModuleName}` (Root).
- [ ] Method: `AddVK{ModuleName}Block`. // Align with Authorization/Authentication
- [ ] Logic: Delegates immediately to `{ModuleName}BlockRegistration.Register`. // [AP.03: No VK prefix for internal]

## Step 4: Configuration (Options - BB.05, AP.04/05) 🔴
- [ ] File: `DependencyInjection/VK{ModuleName}Options.cs`.
- [ ] Type: `public sealed record VK{ModuleName}Options : IVKBlockOptions`. // [BB.05]
- [ ] Implementation: `IVKBlockOptions`.
- [ ] Rule: MUST use `VK` prefix and `init` properties.
- [ ] Section: `VKBlocksConstants.VKBlocksConfigPrefix + ":{ModuleName}"`. // Use colon separator
- [ ] Safety: Use `required` for mandatory fields. No `default!`. // [AP.01]

## Step 5: Master Registration (Internal Core - BB.03) 🔴
- [ ] File: `DependencyInjection/Internal/{ModuleName}BlockRegistration.cs`. // [AP.03: No VK prefix]
- [ ] Namespace: `VK.Blocks.{ModuleName}.DependencyInjection.Internal`.
- [ ] Pattern: Follow **Refined BB.03 execution order**:
    1.  **Check-Self**: `if (services.IsVKBlockRegistered<VK{ModuleName}Block>()) return builder;`.
    2.  **Add Options**: `services.AddVKBlockOptions<VK{ModuleName}Options>(...)`.
    3.  **Add Marker (Mark-Self)**: `services.AddVKBlockMarker<VK{ModuleName}Block>()`. **CRITICAL**: The Marker MUST be registered BEFORE the `Enabled` toggle check. Even if the module is disabled, its Marker must exist in the DI container to prevent downstream modules from crashing during dependency scanning.
    4.  **Add Validator**: `services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VK{ModuleName}Options>, {ModuleName}OptionsValidator>());`.
    5.  **Diagnostics Metadata**: (Optional) `services.AddVKBlockDiagnostics<VK{ModuleName}Block>();`.
    6.  **Toggle Check**: `if (!options.Enabled) return builder;`.
    7.  **Register Core Services**: Actual implementation services using `TryAdd`.

## Step 6: Custom Builder & Validator
- [ ] File: `DependencyInjection/Internal/{ModuleName}BlockBuilder.cs`. // [AP.03: No VK prefix]
- [ ] File: `DependencyInjection/Internal/{ModuleName}OptionsValidator.cs`. // [AP.03: No VK prefix]

## Step 7: Diagnostics Integration (BB.04) 🟡
- [ ] File: `Diagnostics/Internal/{ModuleName}Diagnostics.cs`. // [AP.03: No VK prefix]
- [ ] Attribute: `[VKBlockDiagnostics<VK{ModuleName}Block>]`.

## Step 8: Documentation & Verification
- [ ] Run `/vk-generate-readme`.
- [ ] Run `dotnet build`.
- [ ] Run `/vk-audit-fast` to confirm BB.01-05 and AP.01-05 compliance.
- [ ] Handshake: `Active: [L1+L2:{moduleName}] | Context: {path} | Sync: Ready`.
- [ ] Audit: `Audit: ✅ New block bootstrapped with Industrial DNA.`
