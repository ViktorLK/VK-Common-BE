<dna id="vk-building-blocks-dna" scope="src/BuildingBlocks" requires="AP.01, AP.02, AP.03, BB.01, BB.02, BB.03, BB.05, BB.07">

# 🏭 VK.Blocks — BuildingBlocks Baseline DNA

All modules under `src/BuildingBlocks` are reusable framework packages governed by **L0–L3** rules.

## 1. Package Layer Taxonomy (ARCH-01, ARCH-03)
Every BuildingBlock manifest MUST declare its `package_layer` from one of the following tiers (dependencies flow strictly downward):
- **Tier 0 — Primitives (`Core`)**: Zero I/O, zero external SDKs. Pure contracts, `Result<T>`, guards, and base abstractions (`ARCH-03`).
- **Tier 1 — Foundations (`Caching`, `Resilience`, `Security`, `Observability`, etc.)**: Cross-cutting infrastructure ports and neutral `InMemory`/`NoOp` fallbacks (`ARCH-05`). Depends only on Tier 0.
- **Tier 2 — Domain Capabilities (`AI.Engram`, `AI.Psyche`, `VectorStore`, etc.)**: High-level vertical capabilities orchestrating Tier 0/1 ports. Free of concrete database/vendor SDKs (`ARCH-02`).
- **Tier 3 — Concrete Adapters (`*.EFCore`, `*.Redis`, `*.Sqlite`, `*.Qdrant`)**: Vendor/ORM-specific implementations of Tier 1/2 ports.

## 2. Invariant Principles (BASE-12)
- **Zero-Reflection Runtime**: Use Roslyn Source Generators (`[VKBlockMarker]`, `[VKBlockDiagnostics]`, `[LoggerMessage]`) and static generic caching.
- **Co-Located Fallbacks**: Tier 1 and Tier 2 packages ship with idempotent `TryAdd` neutral defaults (`InMemory` / `NoOp`); Tier 3 adapters override them deterministically via explicit builder APIs (`ARCH-05`, `AP.06`).

</dna>
