# Task: Support Opt-In Compiled Queries Generation in [VKPersistEntity] Source Generator
**ID**: TOOLS.SOURCEGENERATORS-001
**Status**: 🟡 Medium | #Debt
**Target**: `src/Tools/SourceGenerators/Persist/ & src/BuildingBlocks/Persistence.EFCore/`
**Ref**: AI.Psyche.EFCore_20260901.md

## 📝 Description
Extend VK.Tools.SourceGenerators to support opt-in generation of static EF Core Compiled Queries (EF.CompileAsyncQuery) for high-frequency hot-spot lookups (such as GetById, FindByUniqueKey, Exists) when [VKPersistEntity(CompiledQueries = true)] is enabled on entities. This avoids runtime LINQ expression tree compilation and optimizes memory allocation on critical paths while preventing overengineering on low-frequency CRUD entities.

## ✅ DoD (Definition of Done)
- [ ] Support Opt-In Compiled Queries Generation in [VKPersistEntity] Source Generator
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests