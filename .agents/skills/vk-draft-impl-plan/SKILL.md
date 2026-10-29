---
name: vk-draft-impl-plan
description: Create a Contract-First implementation plan and specification (PS.01 audit, BB.01 vertical slice layout, CS.01 error matrix, DI plan) using docs/00-Blueprints/Plan-ContractAndImplementationBlueprint.md.
---

# Skill: Draft Implementation Plan & Contract Specification (`BASE-01`, `PS.01`)

Use this skill before writing implementation code for a feature or refactoring task to lock down contracts, error codes, vertical slice layout, and `PS.01` Architecture Decision Audit.

## Steps

1. **Bootstrap Context (`PS.04`)**:
   - Call `vk_be_get_module_context` on `src/BuildingBlocks/{ModuleName}`.
   - Fetch architectural rules `PS.01, BB.01, BB.03, BB.05, CS.01, CS.03, AP.01, AP.03` via `vk_be_get_architectural_rule`.

2. **Load the Single Source of Truth Blueprint**:
   - Read `docs/00-Blueprints/Plan-ContractAndImplementationBlueprint.md`.

3. **Produce the Specification**:
   - Output the complete specification following all 6 sections of `Plan-ContractAndImplementationBlueprint.md` (Architectural Decision Audit, Vertical Slice File Layout, Contract-First Protocols & Error/Acceptance Matrix, DI & Source Generator Hooks, Non-Functional & Security Guardrails, Verification Checklist).
   - If Enums are added or modified, run `VK.Tools.DocsGenerator` after code changes to keep `docs/05-DataDictionary/` synchronized.

