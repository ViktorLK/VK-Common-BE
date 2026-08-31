# Task: Implement Read-Write Splitting and Read Replica Connection Routing
**ID**: PERSISTENCE.EFCORE-001
**Status**: 🟡 Medium | #Debt
**Target**: `src/BuildingBlocks/Persistence.EFCore/Database/Repositories/VKEFCoreReadRepository.cs`
**Ref**: analysis_results.md

## 📝 Description
Implement dynamic read-write database connection routing in EFCore repository/UnitOfWork when VKQueryOptions.UseReadReplica is enabled. Support secondary connection strings or DbConnectionInterceptor-based routing to offload read queries to read replicas.

## ✅ DoD (Definition of Done)
- [ ] Implement Read-Write Splitting and Read Replica Connection Routing
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests