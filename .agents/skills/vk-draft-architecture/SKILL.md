---
name: vk-draft-architecture
description: Generate a module architecture specification (README.md) in docs/03-Architecture/ using docs/00-Blueprints/03-ArchitectureBlueprint.md (BASE-12, ARCH-01).
---

# Skill: Draft Module Architecture Specification (`BASE-12`, `ARCH-01`)

Use this skill when bootstrapping a new BuildingBlock (Phase A audit requirement) or documenting the boundary principles, C4 Context & Container, and Mermaid runtime sequences of an existing module.

## Steps

1. **Bootstrap Context (`PS.04`)**:
   - Call `vk_be_get_module_context` on `src/BuildingBlocks/{ModuleName}`.
   - Fetch architectural rules `BB.01, AP.03, AP.06` via `vk_be_get_architectural_rule`.

2. **Load the Single Source of Truth Blueprint**:
   - Read `docs/00-Blueprints/03-ArchitectureBlueprint.md`.

3. **Generate & Save Artifacts**:
   - Create or update `docs/03-Architecture/{ModuleName}/README.md` following `03-ArchitectureBlueprint.md` (System Context, Container Diagram, Runtime Sequence, Layering & Drivers).
   - Ensure `src/BuildingBlocks/{ModuleName}/.prompts/module-manifest.md` is referenced as the SSOT for L3 boundary rules.
