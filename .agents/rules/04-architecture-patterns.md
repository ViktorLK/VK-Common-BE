---
trigger: manual
---

# VK.Blocks: Architecture & Design Patterns (AP)

### AP.01 — Modern C# Semantics

- **Sealed by Default**: ALL Application and Infrastructure classes (Handlers, Providers, Evaluators, Attributes) MUST be declared as `sealed class` unless polymorphism is explicitly required. `sealed partial class` is permitted when Source Generator integration requires it (e.g., `[VKBlockMarker]`, `[VKFeatureMarker]`, `[VKBlockDiagnostics]`).
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
- **Safe Registration**: Every individual service or provider MUST be registered using the **`TryAdd`** pattern (e.g., `TryAddSingleton`, `TryAddScoped`, `TryAddTransient`). Direct `AddSingleton` is PROHIBITED.
- **Marker & Dependency Validation**: Via `[VKBlockMarker]`. See **BB.02** for marker spec, **BB.03** for execution order.
- **Provider Overrides**: Strongly-typed override interfaces via builder (e.g., `.AddXxxProvider<T>()`). See **BB.03** for implementation.

### AP.03 — Structural Organization

#### Semantic Visibility & Naming Convention

- **Internal Scoping (`Internal/`)**:
    - **Location**: Any `.cs` file within an `Internal/` folder at any depth (e.g., `ApiKeys/Internal/`).
    - **Visibility**: MUST be declared as `internal`.
    - **Namespace**: MUST use the exact matching folder namespace.
    - **Naming**: MUST **NOT use the `VK` prefix**.
- **Internal Shared Foundation (`Common/Shared/`)**:
    - **Location**: Dedicated for types shared across multiple features within the same block.
    - **Visibility**: MUST be declared as `internal`.
    - **Namespace**: SHOULD use the library's flat root namespace (e.g., `namespace VK.Blocks.AI;`).
    - **Naming**: MUST **NOT use the `VK` prefix**.
- **Public API Surface (`Common/Contracts/` & Foundations)**:
    - **Location**: Dedicated for types exposed to other BuildingBlocks or the Application layer.
    - **Visibility**: MUST be declared as `public`.
    - **Namespace**: MUST use the library's flat root namespace (e.g., `namespace VK.Blocks.AI;`).
    - **Naming**: MUST use the **`VK` prefix** (e.g., `VKAIUsage`).
- **NO Type-Driven Folders**: Avoid grouping by technical type at the root level (e.g., separating all Handlers from Requirements).
- **Folder Naming**: Folder names MUST be noun-based and domain-driven.
  ✅ ApiKeys/Internal/
  ❌ Features/HandleApiKeys/

#### Implementation Naming Taxonomy

For concrete implementation classes, strictly adhere to the following semantic prefixes to clearly communicate their capability, performance baseline, and engineering intent:

| Prefix | Visibility | Performance & Technical Baseline | Engineering Intent & Context |
| :--- | :--- | :--- | :--- |
| **`Default`** | `internal sealed` | Production-grade, high-performance (e.g., `FrozenDictionary`, `ExpressionCompiler`). | **The Official Recommendation**. Use this for the standard, production-ready implementation. |
| **`Basic`** | `internal sealed` | In-Memory / Single-node / No distributed protection mechanisms. | **Foundational / Lightweight**. Not designed to withstand high concurrency or distributed scale. |
| **`NoOp`** | `internal sealed` | Zero-allocation, immediately returns `Result.Failure`. | **Graceful Disablement**. The feature is toggled off, but ensures the DI container remains stable. |
| **`Composite`** | `internal sealed` | Aggregation of multiple implementations. | **Mediator / Aggregator**. Acts as a coordinator or conflict resolver across multiple providers. |
| **`{Vendor}`** (e.g., `SK`, `Ef`) | `internal sealed` | Carries breaking-change risks aligned with vendor SDK upgrades. Focuses on the Anti-Corruption Layer. | **External Coupling**. Deeply coupled to an external dependency. Must be swappable via interfaces. |

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
- **Implementation Delegation**: For exact structure (`sealed record` + `init`), naming (`VK` prefix), transform (`Func<T,T>`), and validation (`IValidateOptions`), strictly follow **BB.05**.

### AP.05 — Strict Overrides Contract (Mode B)

- **Pattern**: Behavioral settings that change per-request MUST follow a **"Strict Contractual Isolation"** model to prevent accidental exposure of system-level configurations.
- **Components**:
    - **Global Settings (`IVK...Settings`)**: Defined on `Options` classes. Groups all configuration parameters.
    - **Local Overrides (`IVK...Overrides`)**: A separate interface defining ONLY the subset of properties permitted for request-level modification.
    - **Generated Args (`XxxArgs`)**: A source-generated record that strictly implements the Overrides interface.
- **Contract-First Automation**:
    - The Source Generator MUST automatically identify the relationship: `IVK...Settings` -> `IVK...Overrides`.
    - **Strict Subset**: `Args` properties MUST be derived EXCLUSIVELY from the Overrides interface members.
    - **Security by Default**: Properties present in `Options` but absent in the `Overrides` interface are automatically excluded. Manual `[VKIgnoreArgs]` is prohibited in favor of this protocol-based exclusion.
- **Merging Priority**: The implementation MUST use the null-coalescing merge: **`args?.Property ?? _options.Property`**.
- **Naming**: Overriding records MUST use the **`Args` suffix** (e.g., `VKChatArgs`).
