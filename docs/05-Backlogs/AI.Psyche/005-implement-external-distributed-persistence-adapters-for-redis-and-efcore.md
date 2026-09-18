# Task: Implement external distributed persistence adapters for Redis and EFCore
**ID**: AI.PSYCHE-005
**Status**: 🔵 Low | #Debt
**Target**: `src/BuildingBlocks/AI.Psyche.Store.Redis, src/BuildingBlocks/AI.Psyche.Persistence.EFCore`
**Ref**: AP.07, BB.01, CS.08

## 📝 Description
Develop dedicated external adapter packages (VK.Blocks.AI.Psyche.Store.Redis and VK.Blocks.AI.Psyche.Persistence.EFCore) implementing IVKEchoStore (via Redis list RPUSH/LRANGE sliding window) and IVKPsycheSessionRepository (via EF Core DbContext), ensuring production-grade distributed persistence without polluting the core AI.Psyche library.

## ✅ DoD (Definition of Done)
- [ ] Implement external distributed persistence adapters for Redis and EFCore
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests