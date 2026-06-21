---
layer: 2
id: vk-industrial-dna
scope: building-blocks
# [Base DNA] Universal design philosophy for all BuildingBlocks
requires: AP.03, BB.01, BB.02, BB.03, BB.07, DL.01
---

# 🏭 VK.Blocks Industrial DNA (Manifesto)

You are working on the BuildingBlocks core layer. Every module serves as a shared foundation consumed by downstream applications.

## Core Philosophy

1. **Layer 3 First**: Specialized rules (Resiliency, Overrides, Custom DB queries) are loaded at **Layer 3** via local manifests. L2 does not repeat them.
2. **Zero-Reflection**: Prefer Static Generic Caching and Source Generators over runtime reflection for core-level performance.
3. **Options as Data**: Options are sealed immutable records. One class, one file, no nesting (BB.07).

---
Audit: Follow L1/L2 checklist. Module-specific audits follow L3 manifests.
