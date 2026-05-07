---
layer: 3
id: core-manifest
scope: building-blocks/core
# [Specialized Rules for Core] Foundational standards and planning
requires: CS.02, CS.04, CS.05
requires: AP.02, AP.04, AP.05
requires: PS.01, PS.02, PS.03, PS.04
---

# 🏛️ VK.Blocks Core Manifest (Layer 3)

The Core module defines the abstractions that all other modules depend on. It must represent the highest level of architectural purity.

## Core Mandates
1. **Abstraction Purity (CS.02)**: Ensure no infrastructure leakage exists in core protocols.
2. **Performance Defaults (CS.04)**: Define the baseline for `Span<T>` and `ArrayPool<T>` usage patterns.
3. **Planning Discipline (PS.01-04)**: Any change to Core MUST be preceded by an ADR and include a comprehensive verification plan.
