---
layer: 2
id: vk-industrial-dna
scope: building-blocks
# [Base DNA] Only universal rules applicable to EVERY BuildingBlock without exception
requires: CS.01, CS.03, CS.06
requires: OR.01
requires: AP.01, AP.03
requires: BB.02, BB.03
---

# 🏭 VK.Blocks Industrial DNA (Manifesto)

You are working on the BuildingBlocks core layer. The baseline for all development here is **predictability**, **framework compatibility**, and **industrial robustness**.

## Core Philosophy

1. **The Invariants**: All rules in the `requires` list are non-negotiable baselines for every module.
2. **Context-Awareness**: Specialized architectural rules (e.g., Database Performance, Resiliency, Security) MUST be declared and loaded at **Layer 3** via local manifests based on the specific module's requirements.
3. **Zero-Reflection**: Prefer Static Generic Caching and Source Generators over runtime reflection to ensure maximum performance at the core level.

## Baseline Reminders

- **Result Pattern (CS.01)**: Every public Application/Feature entry point MUST return a `Result` or `Result<T>`. Never throw business exceptions across boundaries.
- **Async Hygiene (CS.03)**: Library-level code MUST use `.ConfigureAwait(false)` for all asynchronous operations.
- **Deterministic Abstractions (CS.06)**: Static system calls (Time/Guid) are strictly prohibited. Injected providers are mandatory for testability.
- **Atomic Registration (BB.03)**: DI service registration must strictly follow the established 8-step atomic sequence to maintain consistency.

---
Audit Protocol: Follow the L3 Audit checklist in `vk-blocks-checklist.md`.
