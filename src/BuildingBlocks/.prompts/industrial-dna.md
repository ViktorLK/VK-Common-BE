<dna requires="CS.01, CS.03, CS.06, CS.07, AP.01, AP.02, AP.03, BB.01, BB.02, BB.03, OR.01">

# VK.Blocks BuildingBlocks Baseline DNA

Shared root manifest inherited by every module under `src/BuildingBlocks/{Module}`. Module manifests (`module-manifest.md`) declare only module-specific ownership and invariants; they MUST NOT restate rules defined here.

## 1. Ownership Baseline
- **Owns**: Reusable technical mechanisms, domain-agnostic protocols, and in-process fallback implementations (`NoOp` / `InMemory`).
- **Delegates to Host**: Business entity lifecycles (`AP.07`), secret storage and rotation, and durable persistence of audit/state unless the module itself is a Tier 3 persistence adapter.

## 2. Package Layer & Dependency Matrix
Each module's `<manifest allowed="..." forbidden="...">` MUST conform to its tier:
- **Tier 0 — Primitives (`Core`)**: BCL and `Microsoft.Extensions.*` abstractions only. Zero external NuGet SDKs, zero I/O (`ARCH-03`).
- **Tier 1 — Foundations (`Caching`, `Resilience`, `Security`, `Observability`, etc.)**: Cross-cutting ports and neutral fallbacks. Allowed: Tier 0 + `Microsoft.Extensions.*`. Forbidden: Tier 2/3, vendor SDKs.
- **Tier 2 — Capabilities (`AI`, `AI.*`, `Vector*`, `Web.*`, etc.)**: Vertical capability orchestration and domain ports. Allowed: Tier 0, Tier 1, peer contract packages (e.g., `VK.Blocks.AI`). Forbidden: concrete ORM/vendor/cloud SDKs (`ARCH-02`).
- **Tier 3 — Concrete Adapters (`*.MEAI`, `*.EFCore`, `*.Redis`, `*.Sqlite`, `*.Azure`, etc.)**: Vendor/ORM implementations of Tier 1/2 ports. Allowed: target Tier 1/2 port package + vendor SDK. Forbidden: lateral dependencies on sibling adapters.

## 3. Public Surface
- Public types are restricted to `Common/Models/`, `Common/Protocols/`, root `VK{Module}Block`, `VK{Module}Options`, and DI builder extensions, placed in the flat root namespace `VK.Blocks.{Module}` with `VK` / `IVK` prefixes (`AP.03`, `BB.01`).
- All slice implementations reside under `Internal/` or `Common/Shared/` as `internal sealed` without the `VK` prefix (`AP.01`, `AP.03`).
- Fallible operations return `VKResult` / `VKResult<T>`; public async contracts (`IVK*`) return `Task<...>` or `IAsyncEnumerable<...>` with `CancellationToken` (`CS.01`, `CS.03`).

## 4. Defaults & Overrides
- **Fallback Tier (Tier 1/2)**: Co-locate `NoOp` or bounded `InMemory` defaults in the contract package via idempotent `TryAdd` (`ARCH-05`, `AP.02`). Fallbacks represent explicit disabled/standalone states only — never mask invalid configuration (`AP.06`).
- **Provided Tier (Tier 3 / Host)**: Replace fallbacks deterministically via explicit `IVK{Module}Builder` extension methods, independent of registration order (`AP.02`, `AP.06`).

## 5. Baseline Invariants
- **DNA-01** MUST inject `TimeProvider`, `IVKGuidGenerator`, and `IVKJsonSerializer`; never call `DateTime.UtcNow`, `Guid.NewGuid()`, or `System.Text.Json` directly (`CS.06`).
- **DNA-02** MUST use Roslyn Source Generators (`[VKBlockMarker]`, `[VKFeature]`, `[VKBlockDiagnostics]`, `[LoggerMessage]`, `[VKOptions]`) instead of runtime reflection on hot paths.
- **DNA-03** MUST treat tenant, user, and scope identifiers as opaque context passed from inbound boundaries; never read ambient globals or embed tenant business semantics (`ARCH-09`).
- **DNA-04** MUST define error codes as centralized constants (`VK{Feature}Errors`) composing `VKError`; never throw exceptions for foreseeable failures or use inline error strings (`CS.01`).
- **DNA-05** MUST bound all in-memory default stores and caches (capacity cap or eviction policy); unbounded in-process collections are prohibited.
- **DNA-06** MUST resolve required dependencies via `GetRequiredService<T>()` and validate options at startup (`IValidateOptions<T>`), failing fast before the first request (`CS.07`, `BB.05`).

## 6. Registration & Diagnostics
- Follow the synchronous 6-step SG registration pipeline (`BB.03`) with immutable `sealed record` options (`BB.05`, `BB.07`); `Core` is the sole module exempt from `Options` and `Enabled` toggling.
- Consolidate all `[LoggerMessage]` definitions and `[VKBlockDiagnostics<TBlock>]` instruments in `{Feature|Common}/Diagnostics/Internal/*Diagnostics.cs`, scrubbing PII and secrets before emission (`BB.04`, `OR.01`).

</dna>
