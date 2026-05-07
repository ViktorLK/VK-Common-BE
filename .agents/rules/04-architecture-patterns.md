---
trigger: model_decision
---

# VK.Blocks: Architecture & Design Patterns (AP)

### AP.01 — Modern C# Semantics

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

### AP.02 — Service Registration Policy

- **Idempotency**: All building block registrations MUST be strictly idempotent. Registering the same block multiple times must be safe and have no side effects.
- **Dependency Validation**: Modules MUST declare and automatically validate their prerequisite blocks (e.g., Core, Infrastructure) before registering themselves.
- **Marker Types**: Each module MUST use a strongly typed marker (`[VKBlockMarker]`) to track its registration state and dependencies.
- **Safe Registration**: Every individual service or provider MUST be registered using the **`TryAdd`** pattern (e.g., `TryAddSingleton`, `TryAddScoped`, `TryAddTransient`). Direct `AddSingleton` is PROHIBITED.
- **Implementation Delegation**: For the exact execution order and implementation sequence of this policy, you MUST strictly follow **BB.03** in `05-block-blueprint.md`.

### AP.03 — Structural Organization

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

#### Interface Versioning (Public API)

- **Backward Compatibility**: Once an interface (e.g., `IVK...`) is published as a Level 1 Public API, breaking changes to consumers MUST be avoided.
- **Default Interface Methods (DIM)**: Use C# 8.0+ Default Interface Methods when adding new functionality to an existing public interface to maintain backward compatibility.
- **ADR Trigger**: Any unavoidable breaking change to a Level 1 public interface REQUIRES an immediate Architectural Decision Record (ADR) and explicit team approval (DL.03).

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

### AP.04 — Configuration Policy (Zero-Reflection)

- **Strict Contracts**: ALL building block Options classes MUST implement `IVKBlockOptions` to support the zero-reflection pattern.
- **Immutability**: Configuration objects MUST be immutable after initialization.
- **Dual-Registration**: The framework MUST maintain an **Idempotent Dual-Registration Pattern** (IOptions + Singleton) to allow synchronous access to options during startup.
- **Implementation Delegation**: For the exact structure, naming conventions, and validation setup of Options classes, you MUST strictly follow **BB.05** in `05-block-blueprint.md`.

### AP.05 — Hierarchical Configuration Pattern (Args Pattern)

- **Pattern**: Any behavioral setting that can change per-request (e.g., Timeout, TTL, Temperature) MUST follow the **"Global Default + Local Override"** pattern.
- **Components**:
    - **Global Defaults**: Defined in the module's `IVKBlockOptions` class (e.g., `VKAgentOptions.MaxIterations`).
    - **Local Overrides**: Defined in a dedicated `XxxArgs` record (e.g., `VKAgentArgs.MaxIterations`) and passed as an optional method argument.
- **Merging Priority**: The implementation MUST merge these values using the null-coalescing priority: **`args?.Property ?? _options.Property`**.
- **Naming**: Overriding records MUST be named with the **`Args` suffix** (e.g., `VKChatArgs`, `VKRagArgs`).


