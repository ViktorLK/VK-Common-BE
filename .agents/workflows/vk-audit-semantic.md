---
description: Perform a deep semantic audit on a BuildingBlock to ensure it carries the VK.Blocks "Industrial DNA" (Markers, DI Overloads, Immutability, and Result flow).
---

# Workflow: Semantic Architecture Audit (DNA Check)

## Goal

Evaluate the target module beyond its physical structure. This audit focuses on functional contracts, C# 12+ idiomatic correctness, and architectural consistency with VK.Blocks standards (Industrial DNA).

## Steps

### 1. Identify the Target
- Determine the module path and `moduleName` (e.g., "Authentication", "Storage").
- Read core files in the following locations:
    - Module Root (Marker)
    - `DependencyInjection/` (Registration logic)
    - `Features/` or `Internal/` (Application logic)
    - `Diagnostics/` (Observability contracts)

### 2. Marker DNA Audit (Rules 14, 17)
Verify the "Face" of the module:
- [ ] **Definition**: Ensure the marker file `VK{ModuleName}Block.cs` exists at the root.
- [ ] **Signature**: Must be declared as `public sealed partial class`.
- [ ] **Naming**: Must use the `VK` prefix (e.g., `VKAuthenticationBlock`).
- [ ] **Attribute**: Must use `[VKBlockMarker]` with explicit `Dependencies` if applicable.

### 3. DI Registration Integrity (Rules 13, 18 & ADR-016)
Verify how the module integrates into the `IServiceCollection`:
- [ ] **Dual Overloads**: Ensure exactly two public entry points exist:
    - **A (Config-based)**: `Add{ModuleName}Block(this IServiceCollection services, IConfiguration configuration)`
    - **B (Code-based)**: `Add{ModuleName}Block(this IServiceCollection services, Func<VK{ModuleName}Options, VK{ModuleName}Options> transform)`
- [ ] **Functional Transform**: Overload B MUST use `Func<T, T>` to support record `with` expressions (ADR-016). `Action<T>` is PROHIBITED.
- [ ] **Idempotency**: Starts with `if (services.IsVKBlockRegistered<{ModuleName}Block>()) return builder;`.
- [ ] **Marker Placement**: `services.AddVKBlockMarker<{ModuleName}Block>()` MUST be called BEFORE the `Enabled` toggle early return.
- [ ] **No Blocking**: Ensure NO `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` calls exist within the registration flow.

### 4. Options Immutability Audit (Rules 15, 20)
Verify the configuration contract:
- [ ] **Type**: `VK{ModuleName}Options` must be a `sealed record`.
- [ ] **Properties**: Must use `get; init;`. `set;` is STRICTLY PROHIBITED.
- [ ] **Safety**: Use `required` for mandatory properties. `default!` is PROHIBITED.
- [ ] **Validation**: Ensure `IValidateOptions<T>` is registered and validation is triggered.

### 5. Application Layer & Args Pattern (Rules 1, 3, 12, 21)
Sample a Handler/Service to check "DNA" in action:
- [ ] **Result Pattern**: Business logic must return `Result<T>` or `Result.Failure`. `throw` for expected failures is PROHIBITED.
- [ ] **Args Pattern (Rule 21)**: Methods with behavioral overrides must accept an `XxxArgs` record and merge settings using `args?.Prop ?? _options.Prop`.
- [ ] **Context Retention**: Exceptions caught at boundaries must be logged via internal loggers before mapping to `Result.Failure`.
- [ ] **Async Hygiene**: Library code MUST use `.ConfigureAwait(false)` on all awaits.
- [ ] **Defensive Guard**: Use `VKGuard` for all boundary/constructor validations.
- [ ] **CancellationToken**: All async methods must accept and propagate a `CancellationToken`.

### 6. Industrial Leakage & Performance (Rules 2, 4)
- [ ] **Contract Purity (Rule 2)**: Level 1 (Public API) interfaces MUST NOT leak infrastructure-specific types (e.g., `Microsoft.EntityFrameworkCore`, `Azure.Storage.Blobs`).
- [ ] **DB Performance (Rule 4)**: Ensure read-only queries use `.AsNoTracking()` and prefer projection over full entity materialization.
- [ ] **Memory Hygiene (Rule 4)**: For high-throughput logic, check for the use of `Span<T>`, `ArrayPool<T>`, or `stackalloc` where appropriate to minimize allocations.

### 7. Observability Signature (Rules 6, 19)
Ensure the module is "Transparent" and follows high-performance telemetry standards:
- [ ] **Diagnostics Class**: Must exist a `Diagnostics/Internal/` class decorated with `[VKBlockDiagnostics(typeof({ModuleName}Block))]`.
- [ ] **Logging Pattern**: Loggers must use `[LoggerMessage]` Source Generator. DIRECT calling of `logger.LogInformation()` etc. in business logic is STRICTLY PROHIBITED.
- [ ] **Telemetry**: Verify if `ActivitySource` or `Meter` names follow OpenTelemetry semantic conventions (e.g. `VKBlocks.{ModuleName}`).

### 8. Deterministic Logic (Rule 5.1)
Ensure the module is testable and deterministic:
- [ ] **No Static Time**: Prohibit `DateTime.UtcNow` or `DateTimeOffset.Now`. MUST use `TimeProvider` (injected).
- [ ] **No Static Guids**: Prohibit `Guid.NewGuid()`. MUST use `IVKGuidGenerator` (injected).
- [ ] **No Static JSON**: Prohibit `JsonSerializer.Serialize/Deserialize`. MUST use `IVKJsonSerializer` (injected).

### 9. Resiliency Signature (Rule 8)
Ensure the module is "Defensive" against external failures:
- [ ] **External Protection**: For all asynchronous I/O operations (HttpClient, Azure SDK, SQL), verify if they are wrapped with `ResiliencePipeline` (Polly) or built-in SDK retries. "Naked" external calls are PROHIBITED.

### 10. Resource Hygiene (Rule 4)
Ensure the module is "Responsible" with memory resources:
- [ ] **Pool Discipline**: When using `ArrayPool<T>`, verify that a `finally` block is used to `Return()` the buffer. "Borrow and forget" is STRICTLY PROHIBITED.

## Reporting

Output a **"Semantic DNA Report"** with the following status:

- 🧬 **Marker DNA**: [Pass/Fail] (Focus: partial/sealed/VK-prefix)
- 💉 **DI Integrity**: [Pass/Fail] (Focus: dual overloads/idempotency/transform/no-blocking)
- 🧊 **Immutability**: [Pass/Fail] (Focus: record/init/required)
- 🧊 **Industrial DNA**: [Pass/Fail] (Focus: Args Pattern/Contract Purity/Performance/CancellationToken)
- 👁️ **Observability**: [Pass/Fail] (Focus: LoggerMessage/Diagnostics attribute/OpenTelemetry)
- 🎯 **Determinism**: [Pass/Fail] (Focus: TimeProvider/GuidGenerator/JsonSerializer)
- 🛡️ **Resiliency**: [Pass/Fail] (Focus: Polly/SDK retries)
- 🧹 **Hygiene**: [Pass/Fail] (Focus: ArrayPool Return/finally blocks)
- 🌊 **Logic Flow**: [Pass/Fail] (Focus: Result/ConfigureAwait/VKGuard)

For every **[Fail]**, provide a specific code snippet and a **Refactoring Suggestion** aligned with VK.Blocks standards.
