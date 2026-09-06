# Task: Raw Response Audit Storage and Contract Schema Master Registry
**ID**: AI.EIDOS-007
**Status**: 🔵 Low | #Debt
**Target**: `BuildingBlocks/AI.Eidos.EFCore`
**Ref**: CS.08, AP.07

## 📝 Description
Design and implement an orthogonal 1:1 append-only Raw Response Audit table (VK_AI_Eidos_Echo_Raw / VK_AI_Psyche_Echo_Raw) to store raw LLM JSON outputs and execution metadata for debugging and dataset curation without polluting core conversation memory. Also introduce a Contract Schema Master registry table (VK_AI_Eidos_Contract) with schema fingerprinting and versioning to prevent schema drift and enable time-travel debugging.

## ✅ DoD (Definition of Done)
- [ ] Raw Response Audit Storage and Contract Schema Master Registry
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests