---
trigger: always_on
---

# VK.Blocks: Architecture & Design Patterns

### Rule 12 — Modern C# Semantics

- **Sealed by Default**: ALL Application and Infrastructure classes (Handlers, Providers, Evaluators, Attributes) MUST be declared as `sealed class` unless polymorphism is explicitly required.
- **Immutable Data**: Use `sealed record` for all DTOs, domain settings, and authorization requirements instead of plain classes to guarantee immutability and value equality. Use `with` expressions for non-destructive mutation instead of manual copy constructors.
- **Required Properties**: Use `required` keyword for all non-nullable properties in `record` or DTO types to ensure compile-time safety. STRICTLY PROHIBIT the use of `default!` for property initialization.
- **Modern C# Idioms**: Use C# 12+ features (Collection expressions `[]`, Primary constructors) where appropriate. STRICTLY ADHERE to the project's `.editorconfig` for formatting rules (e.g., preference for explicit types over `var` for built-in types).
- **Pattern Matching**: Prefer `is` and `switch` expressions over `if`/`else` chains and type casting for concise, readable branching.
- **Null Handling**: Prefer `??` / `??=` / `?.` over explicit null checks. Use `is null` / `is not null` over `== null` to avoid operator overload side-effects and ensure pattern consistency.
- **Defensive Programming (VKGuard)**: 
    - **Mandatory Boundary Checks**: ALL method and constructor boundaries MUST use `VKGuard` to enforce preconditions. Manual `if (x == null) throw` patterns are STRICTLY PROHIBITED.
    - **Specific Guard Selection**: 
        - Use `VKGuard.NotNull(x)` for reference types.
        - Use `VKGuard.NotNullOrWhiteSpace(s)` for strings.
        - Use `VKGuard.NotEmpty(list)` for collections.
        - Use `VKGuard.NotEmptyGuid(id)` for unique identifiers.
        - Use `VKGuard.EnumDefined(e)` for enum parameters.
    - **Fluent Assignment**: Leverage the return value of `VKGuard` for single-line field initialization (e.g., `_service = VKGuard.NotNull(service);`) or expression-bodied members.
- **Collection Expressions**: Use `[]` initializer syntax (C# 12+) over `new List<T>()` or `new T[] {}` where applicable.

### Rule 13 — Service Registration Pattern

- Each BuildingBlock module MUST define a dedicated marker type (e.g. `public sealed class AuthenticationBlock;`).
- Each registration method MUST implement the **"Check-Self, Check-Prerequisite, Options/Mark-Self, Feature Toggle, Core Services"** pattern:
    1.  **Check-Self & Check-Prerequisite**: Check for self-registration and validate dependencies recursively via `IsVKBlockRegistered<OwnBlock>()`. Return early if true. This "Smart Check" ensures that both idempotency and prerequisite safety (e.g. Core block existence) are handled in a single call.
    2.  **Options Registration**: Register configuration options (`AddVKBlockOptions<T>`).
    3.  **Mark-Self**: Register the self-marker using `services.AddVKBlockMarker<OwnBlock>()` immediately after options registration, but BEFORE any early returns related to feature-toggle (`Enabled`) checks. This ensures the block is recognized for dependency resolution.
    4.  **Feature Toggle**: Return early if the feature is disabled (`!options.Enabled`).
    5.  **Core Services**: Perform actual service registration using idempotent `TryAdd` patterns.
- For the complete 8-step implementation sequence (including Options Validation and Diagnostics registration), refer to **Rule 18** in `05-block-blueprint.md`.
- ALL idempotency checks (including `TOptions` registration) MUST use the semantic `IsVKBlockRegistered<T>()` helper instead of manual `Any()` checks.
- Every individual service or provider MUST be registered using the **`TryAdd`** pattern (e.g., `TryAddSingleton`, `TryAddScoped`, `TryAddTransient`).
- Direct `AddSingleton`/`AddScoped`/`AddTransient` is PROHIBITED within building block extensions.
- **Exception**: Official framework extensions (e.g. `AddHttpContextAccessor`, `AddLogging`, `AddAuthentication`) that are known to be idempotent are allowed and preferred over manual `TryAdd` registrations.

### Rule 14 — Structural Organization

#### Depth-Based Visibility & Naming Convention

- **Level 1 (Public API Surface)**:
    - **Location**: Any `.cs` file in a first-level folder (e.g., `ApiKeys/`) MUST be declared as `public`.
    - **Namespace**: MUST use the library's flat root namespace (e.g., `namespace VK.Blocks.Authentication;`).
    - **Naming**: MUST use the **`VK` prefix** for all public types (e.g., `VKApiKeyOptions`, `IVKApiKeyStore`) to prevent naming collisions in the flattened namespace.
- **Level 2+ (Encapsulated Internals)**:
    - **Location**: Any `.cs` file in a second-level or deeper folder (e.g., `ApiKeys/Internal/`, `ApiKeys/Persistence/`) MUST be declared as `internal`.
    - **Namespace**: MUST use the exact matching folder namespace (e.g., `namespace VK.Blocks.Authentication.ApiKeys.Internal;`).
    - **Naming**: MUST **NOT use the `VK` prefix** (e.g., `ApiKeyValidator`, NOT `VKApiKeyValidator`). Internal classification is handled by the namespace and directory depth.
- **NO Type-Driven Folders**: Avoid grouping by technical type at the root level (e.g., separating all Handlers from Requirements).
- **Folder Naming**: Folder names MUST be noun-based and domain-driven.
  ✅ ApiKeys/Internal/
  ❌ Features/HandleApiKeys/

#### Constant Visibility

- **Single File Scope:** Use `private const` within the class.
- **Cross-file (Same Feature):** Extract to an `internal static class XxxConstants` inside the feature's folder.
- **Cross-feature (Global):** Extract to a `public static class` in a global `Constants/` folder or at the module's root.
- ALWAYS eliminate magic strings using this visibility hierarchy.
- Constants file MUST be named after its scope:
  ✅ WorkingHoursConstants.cs
  ❌ Constants.cs

#### Type Segregation

- **One File, One Type**: NEVER declare multiple primary `class`, `record`, or `interface` types in a single `.cs` file.
- **Navigation**: Extract nested or bundled types into their own files to maintain high cohesive navigation.
- **Exception**: Private nested types used exclusively within the same class MAY remain in the same file. e.g. private sealed record InternalResult(...)

### Rule 15 — Configuration Pattern (Zero-Reflection)

- ALL building block Options classes MUST implement `IVKBlockOptions` to support the zero-reflection pattern.
- Configuration sections MUST be resolved using the standardized `AddVKBlockOptions<TOptions>(configuration, transform)` primary wrapper.
- **Functional Transformation (ADR-016)**: To support immutable `init` properties and `record` types, all configuration delegates MUST use the **`Func<T, T>`** pattern instead of `Action<T>`. This allows the use of `with` expressions for non-destructive mutation: `services.AddVKBlockOptions<T>(config, options => options with { Prop = value })`.
- Section names MUST follow the `VKBlocksConstants.VKBlocksConfigPrefix + "{ModuleName}"` format (e.g., `VKBlocks:Authentication`).
- The **Idempotent Dual-Registration Pattern** (IOptions + Singleton) MUST be maintained to allow building blocks synchronous access to their options during startup.
- **Wrapper vs Core**: Modular registration MUST use the public `[WRAPPER]` pattern receiving `IConfiguration` to delegate to the internal `[CORE]` registration logic.

### Rule 21 — Hierarchical Configuration Pattern (Args Pattern)

- **Pattern**: Any behavioral setting that can change per-request (e.g., Timeout, TTL, Temperature) MUST follow the **"Global Default + Local Override"** pattern.
- **Components**:
    - **Global Defaults**: Defined in the module's `IVKBlockOptions` class (e.g., `VKAgentOptions.MaxIterations`).
    - **Local Overrides**: Defined in a dedicated `XxxArgs` record (e.g., `VKAgentArgs.MaxIterations`) and passed as an optional method argument.
- **Merging Priority**: The implementation MUST merge these values using the null-coalescing priority: **`args?.Property ?? _options.Property`**.
- **Naming**: Overriding records MUST be named with the **`Args` suffix** (e.g., `VKChatArgs`, `VKRagArgs`).
