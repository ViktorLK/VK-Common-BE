---
layer: 3
id: core-manifest
scope: building-blocks/core
# [Specialized Rules for Core] Foundational standards and planning
requires: CS.02, CS.04, CS.05, AP.02, AP.04, AP.05, PS.01, PS.02, PS.03
---

# 🏛️ VK.Blocks Core Manifest (Layer 3)

The Core module defines the abstractions that all other modules depend on. It must represent the highest level of architectural purity.

## Core Specific Extensions (L3)
1. **Abstraction Purity (CS.02)**: Specifically, any new contract in Core MUST be zero-dependency relative to external libraries.
2. **Performance Defaults (CS.04)**: Core MUST provide the baseline implementations for `IVKJsonSerializer` and memory-efficient utilities.
3. **Planning Discipline (PS.01-04)**: Architectural changes to Core MUST include a formal ADR and a full regression-test plan in the Walkthrough.
