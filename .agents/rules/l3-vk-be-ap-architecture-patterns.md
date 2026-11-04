---
trigger: manual
---

<ruleset category="AP" name="Architecture & Design Patterns">

<rule id="AP.01" type="A" name="Modern C# Semantics">
- **Sealed by Default**: ALL Application and Infrastructure classes (Handlers, Providers, Evaluators, Attributes) MUST be declared as `sealed class` unless polymorphism is explicitly required. `sealed partial class` is permitted when Source Generator integration requires it (e.g., `[VKBlockMarker]`, `[VKFeatureMarker]`, `[VKBlockDiagnostics]`).
- **Immutable Data**: Use `sealed record` for all DTOs, domain settings, and authorization requirements instead of plain classes to guarantee immutability and value equality. Use `with` expressions for non-destructive mutation instead of manual copy constructors.
- **Required Properties**: Use `required` keyword for all non-nullable properties in `record` or DTO types to ensure compile-time safety. STRICTLY PROHIBIT the use of `default!` for property initialization.
- **Modern C# Idioms**: Use C# 12+ features (Collection expressions `[]`, Primary constructors) where appropriate. STRICTLY ADHERE to the project's `.editorconfig` for formatting rules (e.g., preference for explicit types over `var` for built-in types).
- **Pattern Matching**: Prefer `is` and `switch` expressions over `if`/`else` chains and type casting for concise, readable branching.
- **Null Handling**: Prefer `??` / `??=` / `?.` over explicit null checks. Use `is null` / `is not null` over `== null` to avoid operator overload side-effects and ensure pattern consistency.
- **Defensive Programming (VKGuard)**:
    - **Mandatory Boundary Checks**: ALL method and constructor boundaries MUST use `VKGuard` to enforce preconditions. Manual `if (x == null) throw` patterns are STRICTLY PROHIBITED.
    - **Guard Selection**: Use appropriate `VKGuard` methods (`NotNull`, `NotNullOrWhiteSpace`, `NotEmpty`, `NotEmptyGuid`, `EnumDefined`).
    - **Fluent Assignment**: Leverage the return value of `VKGuard` for single-line field initialization (e.g., `_service = VKGuard.NotNull(service);`) or expression-bodied members.
- **Collection Expressions**: Use `[]` initializer syntax (C# 12+) over `new List<T>()` or `new T[] {}` where applicable.
</rule>

<rule id="AP.02" type="B" name="Service Registration Policy">
- **Idempotency**: All building block registrations MUST be strictly idempotent. Registering the same block multiple times must be safe and have no side effects.
- **Safe Registration**: Every individual service or provider MUST be registered using the **`TryAdd`** pattern (e.g., `TryAddSingleton`, `TryAddScoped`, `TryAddTransient`). Direct `AddSingleton` is PROHIBITED.
  - **Exception — Infrastructure Override**: Infrastructure Provider modules (e.g., `AI.Psyche.EFCore`) whose sole purpose is to replace a parent module's default implementation (e.g., InMemory → EFCore) MAY use `AddScoped` / `AddSingleton` to ensure deterministic override regardless of registration order. The parent module MUST use `TryAdd` so that the infrastructure provider's `Add` registration takes precedence.
- **Marker & Dependency Validation**: Via `[VKBlockMarker]`. See **BB.02** for marker spec, **BB.03** for execution order.
- **Provider Overrides**: Strongly-typed override interfaces via builder (e.g., `.AddXxxProvider<T>()`). See **BB.03** for implementation.
</rule>

<rule id="AP.03" type="B" name="Structural Organization">
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
- **Public API Surface (`Common/Models/`, `Common/Protocols/` & Foundations)**:
    - **Location**: Dedicated for types exposed to other BuildingBlocks or the Application layer. Public models reside in `Common/Models/`, public interfaces in `Common/Protocols/`.
    - **Visibility**: MUST be declared as `public`.
    - **Namespace**: MUST use the library's flat root namespace (e.g., `namespace VK.Blocks.AI;`).
    - **Naming**: MUST use the **`VK` prefix** (e.g., `VKAIUsage`).
- **NO Type-Driven Folders**: Avoid grouping by technical type **at the building block's first-level root directory**. Inside a vertical slice feature or `Common/`, technical grouping folders are permitted.
- **Folder Naming**: Folder names MUST be noun-based and domain-driven (e.g. `ApiKeys/Internal/`).

#### Implementation Naming Taxonomy
| Prefix | Visibility | Intent & Context |
| :--- | :--- | :--- |
| **`Default`** | `internal sealed` | **Official Recommendation**. Production-grade, high-performance implementation. |
| **`Basic`** | `internal sealed` | **Foundational / Lightweight**. In-Memory / Single-node. |
| **`NoOp`** | `internal sealed` | **Graceful Disablement**. Zero-allocation fallback. |
| **`Composite`** | `internal sealed` | **Mediator / Aggregator**. Coordinates across multiple providers. |
| **`{Vendor}`** | `internal sealed` | **External Coupling**. Anti-Corruption Layer coupled to vendor SDKs. |

#### Interface Versioning (Public API)
- **Backward Compatibility**: Once an interface is published as a Level 1 Public API, breaking changes to consumers MUST be avoided.
- **Default Interface Methods (DIM)**: Use C# 8.0+ Default Interface Methods when adding new functionality to an existing public interface.
- **ADR Trigger**: Any unavoidable breaking change to a Level 1 public interface REQUIRES an immediate ADR (DL.03).

#### Constant Visibility
- **Single File Scope:** Use `private const` within the class.
- **Cross-file (Same Feature):** Extract to an `internal static class XxxConstants` inside the feature's folder.
- **Cross-feature (Global):** Extract to a `public static class` in a global `Constants/` folder or at the module's root.
- Constants file MUST be named after its scope (e.g. `WorkingHoursConstants.cs`).

#### Type Segregation
- **One File, One Type**: NEVER declare multiple primary `class`, `record`, or `interface` types in a single `.cs` file.
- **Exception**: Private nested types used exclusively within the same class MAY remain in the same file.
</rule>

<rule id="AP.04" type="B" name="Configuration Policy (Zero-Reflection)">
- **Strict Contracts**: ALL building block Options classes MUST implement `IVKBlockOptions` to support the zero-reflection pattern.
- **Immutability**: Configuration objects MUST be immutable after initialization.
- **Dual-Registration**: The framework MUST maintain an **Idempotent Dual-Registration Pattern** (IOptions + Singleton) to allow synchronous access to options during startup.
- **Implementation Delegation**: Follow **BB.05**.
</rule>

<rule id="AP.05" type="B" name="Request-Scoped Args Pattern (SG-Automated)">
- **Pattern**: Behavioral settings that change per-request MUST follow a **Source-Generator-driven Args pattern**.
- **ArgsMode** (`[VKOptions]` attribute on Options record):
    - **`None` (0, default)**: No Args record generated.
    - **`Explicit` (1)**: Only properties decorated with `[VKRequestOverride]` are included.
    - **`Implicit` (2)**: All public non-static, non-readonly properties are included unless decorated with `[VKNoRequestOverride]`.
- **Convention**: Generated `{BaseName}Args` record implements `IVKArgs<TArgs>`.
- **Naming**: Args records MUST use the **`Args` suffix** (e.g., `VKChatOptions` → `VKChatArgs`).
- **Static Empty**: Every generated Args record includes `public static {ArgsName} Empty { get; } = new();`.
</rule>

<rule id="AP.06" type="A" name="Explicit API & Override Pattern (Anti-Overprotection)">
- **Anti-Overprotection / Fallback Rule**: Do NOT register silent `Null/NoOp` fallbacks in the DI container to mask developer configuration omissions. Fallback registrations are ONLY allowed to represent an explicit, documented "disabled" config state. Missing mandatory configurations MUST fail fast at startup.
- **No Heuristic Priority**: Do NOT write heuristics to guess whether a user's implementation should take priority over defaults.
- **Explicit Builder Overrides**: Expose overrides via explicit chain methods on the builder (e.g., `builder.OverrideAuditProvider<T>()`).
- **Deterministic Replacement**: Inside builder overrides, using `services.Replace()` is permitted to eliminate registration order sensitivity.
</rule>

<rule id="AP.07" type="A" name="Non-Intrusive Capability Boundary">
- **No Implicit Side-Effects**: Core BuildingBlocks MUST NOT implicitly create or persist domain entities when request context lacks explicit identifiers.
- **App Ownership & Fallback**: Business entity lifecycles belong exclusively to the App layer. Omitted optional identifiers MUST fallback smoothly to stateless execution.
</rule>

</ruleset>
