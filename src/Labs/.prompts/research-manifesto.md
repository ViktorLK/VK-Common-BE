---
layer: 2
id: vk-research-manifesto
scope: labs
# [Essential DNA Only] Maintain stability without the industrial overhead
requires: CS.01, CS.03, CS.06
---

# 🔬 VK.Blocks Research Manifesto (Labs)

You are in **Research Mode (Visionary)**. Your mission is to prioritize **conceptual purity** and **iteration velocity** over industrial boilerplate.

## LAB01. Exploration over Engineering

- **Permission to Deviate**: Labs are permitted to bypass **Type B (Industrial Habits)** rules to maintain research velocity:
    - **Architectural Freedom**: MAY skip **Vertical Slice Architecture (BB.01)**. You are free to experiment with Onion, Clean, Hexagonal, or N-Tier architectures to find the best fit for the research.
    - **Naming**: MAY skip `VK` prefix for internal types and block markers (e.g., `PwpBlock`).
    - **Structure**: MAY skip `Internal/` subfolder requirements (**AP.03**). Implementations reside directly in folders that match your mental model.
    - **Namespaces**: MAY use flattened or non-path-matching namespaces (e.g., `VK.Labs.{Module}`).
- **Model Fidelity**: Prioritize high-fidelity mapping between your chosen conceptual model (Mathematical, Physical, Biological, or Psychological) and the software architecture.
- **Process**: Type B rule violations are allowed and only require a `🚩` flag in audits. Type A (Logic Bottom Line) MUST still be followed with zero tolerance.
- **Goal**: Prove the concept first. Hardening to BuildingBlock standards is a separate future phase.

## LAB02. Stability at Boundaries (CS.01 Alignment)

- **Hard Boundaries**: `Result<T>` is mandatory at Feature/Service boundaries to ensure system stability.
- **Internal Freedom**: Low-level implementation details (SIMD kernels, custom memory pools) are exempt from `Result` wrapping — they are implementation details, not contracts.

## LAB03. Deterministic Foundations (CS.06)

- **No Static Side-Effects**: Use `TimeProvider` and `IVKGuidGenerator` to ensure experimental results are reproducible and testable. Prohibit direct use of `DateTime.UtcNow` or `Guid.NewGuid()`.

## LAB04. The Bridge (Harden & Integrate)

- **Path to Stability**: Design with "Hardening" in mind. Ensure that the core logic can be extracted and standardized into a BuildingBlock when the research is proven successful.
