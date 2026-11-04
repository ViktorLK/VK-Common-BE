---
id: "#018"
legacy_id: "CORE-017"
category: debt
parent: null
title: Migrate all BuildingBlock Options to VKOptionsAttribute
module: Core
type: debt
priority: P2
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - DL.04
  - AP.05
  - ADR-CORE-020
  docs: []
commits: []
---
# Task: Migrate all BuildingBlock Options to VKOptionsAttribute
**ID**: #018 *(Legacy: CORE-017)*
**Status**: 🟢 Done | #Debt
**Target**: `VKOptionsAttribute`
**Ref**: DL.04, AP.05, ADR-CORE-020

## 📝 Description
Refactor all BuildingBlock Options classes to decorate [VKOptions] directly on Options records and clean up ArgsGenerationMode from [VKFeature] attributes across all modules.

## ✅ DoD (Definition of Done)
- [x] Migrate all BuildingBlock Options to VKOptionsAttribute
- [x] **Assess if an ADR is required (DL.03)**: [ADR-CORE-020](/docs/04-ArchitectureDecisionRecords/Core/adr-020-decoupling-args-generation-from-vkfeature-to-vkoptions.md)
- [x] Verify changes (Fixed generator extraction order and validation reference check)
- [x] Run tests / full build (All 83 projects built successfully)