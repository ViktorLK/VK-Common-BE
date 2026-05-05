# Persistence Module Backlog

Tasks related to `VK.Blocks.Persistence` and `Persistence.EFCore`.

## 🔴 High Priority (Critical)

- [ ] **[Rule 18] Registration Cleanup**: Finalize the removal of any legacy registration methods. Ensure `AddPersistenceBlock` is the sole entry point and correctly handles marker registration.
- [ ] **[Rule 4] Default NoTracking**: Audit all repositories to ensure `.AsNoTracking()` is the default for read operations unless explicitly overridden.
