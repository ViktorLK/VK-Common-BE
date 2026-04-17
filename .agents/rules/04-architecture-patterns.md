---
trigger: always_on
---

# VK.Blocks: Architecture & Design Patterns

### Rule 12 — Modern C# Semantics

- **Sealed by Default**: ALL Application and Infrastructure classes (Handlers, Providers, Evaluators, Attributes) MUST be declared as `sealed class` unless polymorphism is explicitly required.
- **Immutable Data**: Use `sealed record` for all DTOs, domain settings, and authorization requirements instead of plain classes to guarantee immutability and value equality. Use `with` expressions for non-destructive mutation instead of manual copy constructors.
- **Required Properties**: Use `required` keyword for all non-nullable properties in `record` or DTO types to ensure compile-time safety. STRICTLY PROHIBIT the use of `default!` for property initialization.
- **Pattern Matching**: Prefer `is` and `switch` expressions over `if`/`else` chains and type casting for concise, readable branching.
- **Null Handling**: Prefer `??` / `??=` / `?.` over explicit null checks. Use `is null` / `is not null` over `== null` to avoid operator overload side-effects and ensure pattern consistency.
- **Collection Expressions**: Use `[]` initializer syntax (C# 12+) over `new List<T>()` or `new T[] {}` where applicable.

### Rule 13 — Service Registration Pattern

- Each BuildingBlock module MUST define a dedicated marker type (e.g. `public sealed class AuthenticationBlock;`).
- Each registration method MUST implement the **"Check-Self, Check-Prerequisite, Actual Registration, Mark-Self"** pattern:
    1.  Check for self-registration via `IsVKBlockRegistered<OwnBlock>()` and return early if true.
    2.  Validate prerequisites using `IsVKBlockRegistered<BaseBlock>()` and throw `InvalidOperationException` if missing.
    3.  Perform actual service registration using idempotent patterns (see below).
    4.  Register the self-marker using `services.AddVKBlockMarker<OwnBlock>()` immediately after options registration, but BEFORE any early returns related to feature-toggle (`Enabled`) checks. This ensures the block is recognized for dependency resolution.
- ALL idempotency checks (including `TOptions` registration) MUST use the semantic `IsVKBlockRegistered<T>()` helper instead of manual `Any()` checks.
- Every individual service or provider MUST be registered using the **`TryAdd`** pattern (e.g., `TryAddSingleton`, `TryAddScoped`, `TryAddTransient`).
- Direct `AddSingleton`/`AddScoped`/`AddTransient` is PROHIBITED within building block extensions.
- **Exception**: Official framework extensions (e.g. `AddHttpContextAccessor`, `AddLogging`, `AddAuthentication`) that are known to be idempotent are allowed and preferred over manual `TryAdd` registrations.

### Rule 14 — Structural Organization

#### Folder Layout

- **Feature-Driven (Vertical Slice)**: Group related Handlers, Requirements, Attributes, and Models into a single feature folder (e.g. `Features/WorkingHours/`).
- **NO Type-Driven Folders**: Avoid grouping by technical type (e.g., separating all Handlers from Requirements).
- **Core Separation**: Only place globally shared abstractions, DI extensions, or cross-cutting constants in root or `Abstractions/` directories.
- **Naming**: Feature folder names MUST be noun-based and domain-driven.
  ✅ Features/WorkingHours/
  ❌ Features/HandleWorkingHours/

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
- Configuration sections MUST be resolved using the standardized `AddVKBlockOptions<TOptions>(configuration)` primary wrapper.
- Section names MUST follow the `VKBlocks:Category:Feature` hierarchy (e.g., `VKBlocks:Web:Cors`).
- The **Idempotent Dual-Registration Pattern** (IOptions + Singleton) MUST be maintained to allow building blocks synchronous access to their options during startup.
- **Wrapper vs Core**: Modular registration MUST use the `[WRAPPER]` pattern receiving `IConfiguration` to delegate to the `[CORE]` implementation receiving `IConfigurationSection`.
