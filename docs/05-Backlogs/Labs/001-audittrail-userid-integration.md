# Task: AuditTrail UserId Integration
**ID**: LABS-001
**Status**: 🟡 Medium | #Debt
**Target**: `src/Labs/TaskNexusOrbit` -> `AuditTrailInterceptor.cs`
**Ref**: N/A

## 📝 Description
The `AuditTrailInterceptor` currently hardcodes "System" as the user ID. It should be integrated with `ICurrentUserContext` to capture the actual user making the changes.

## ✅ DoD (Definition of Done)
- [ ] Inject `ICurrentUserContext` into `AuditTrailInterceptor`
- [ ] Replace hardcoded "System" with `currentUserContext.UserId`
- [ ] Verify audit log entries have correct user IDs
