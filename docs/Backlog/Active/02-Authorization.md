# Authorization Module Backlog

Tasks related to the `VK.Blocks.Authorization` library.

## 🔴 High Priority (Normalization)

- [ ] **[Rule 18] DI Normalization**: Ensure all Authorization sub-features (Roles, Permissions, etc.) strictly follow the Wrapper -> Core registration sequence without any async-over-sync calls.

## 🟡 Medium Priority (New Features)

- [ ] **[Feature] Policy Synthesis Engine**: Implement a declarative way to combine multiple authorization policies using AND/OR logic.
- [ ] **[Observability] Audit Log Integration**: Automatically capture authorization decisions (Allowed/Denied) into the audit trail using the `Audit` block if present.

## 🔵 Low Priority (Scaling)

- [ ] **[Feature] Distributed Cache Integration**: Provide an optional Redis-backed implementation for `IPermissionStore` and `IRoleStore` to support high-scale distributed environments.
- [ ] **[Feature] API Gateway Support**: Add extensions for YARP or Ocelot to propagate authorization decisions to downstream microservices.
- [ ] **[Feature] Real-time Hot-Reload**: Implement a SignalR-based mechanism to invalidate authorization caches across instances when policies change.
