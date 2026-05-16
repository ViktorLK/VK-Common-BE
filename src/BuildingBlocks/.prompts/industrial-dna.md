---
layer: 2
id: vk-industrial-dna
scope: building-blocks
# [Base DNA] Only universal rules applicable to EVERY BuildingBlock without exception
requires: CS.01, CS.03, CS.06, OR.01, AP.01, AP.03, BB.01, BB.02, BB.03, BB.07, BB.08, DL.01, AP.05
---

# 🏭 VK.Blocks Industrial DNA (Manifesto)

You are working on the BuildingBlocks core layer. The baseline for all development here is **predictability**, **framework compatibility**, and **industrial robustness**.

## Core Philosophy

1. **The Invariants**: All rules in the `requires` list are non-negotiable baselines for every module.
2. **Context-Awareness**: Specialized architectural rules (e.g., Database Performance, Resiliency, Security) MUST be declared and loaded at **Layer 3** via local manifests.
3. **Physical Organization (BB.07)**: Options records MUST reside in their own dedicated files at the functional root. Nesting is STRICTLY PROHIBITED.
4. **Reliability by Design (BB.08)**: Modular features MUST implement implicit parent registration pull-up to ensure dependency integrity.
5. **Configuration Pattern (AP.05)**: Standardize on the **Global -> Feature -> Request** hierarchical merge strategy for all dynamic overrides.
6. **Testing Maturity (DL.01)**: Unit tests MUST use deterministic mocking for all external or non-deterministic dependencies.
7. **Zero-Reflection**: Prefer Static Generic Caching and Source Generators over runtime reflection for core-level performance.

---
Audit Protocol: Follow the L1/L2 Audit checklist in `vk-blocks-checklist.md`.
