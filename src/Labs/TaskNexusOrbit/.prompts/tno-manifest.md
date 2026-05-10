---
layer: 3
id: tno-manifest
scope: module
extends: vk-research-manifesto
# [TNO Specific Requirements] Supplementing the Research Manifesto
requires: CS.01
requires: CS.04
requires: AP.05
requires: BB.01
---

# TNO Rule Set: TaskNexusOrbit

## TNO01: Dependency-Aware Status Transitions [uses: CS.01]

Status transitions to **InProgress** or **Done** MUST be blocked if any prerequisite tasks (dependencies) are not in the **Done** state. This check MUST occur within the `VKTask` aggregate root via `ChangeStatus` and be validated by the Application layer fetching the dependent tasks.

## TNO02: Multi-Tenant Boundary Enforcement [uses: OR.02]

All TNO entities MUST implement `IVKMultiTenantEntity`. The `TenantId` MUST be propagated from the `IVKUserContext` to all commands and queries. Direct repository access without a `TenantId` filter (where applicable) is strictly prohibited.

## TNO03: Time Logging Integrity [uses: CS.01, CS.06]

Time entries logged via `LogTime` MUST have a positive duration. Every `VKTimeEntry` is an immutable record of effort; corrections should be made via compensating entries or specific adjustment logic, never by direct mutation of historical records.

## TNO04: Project-Task Sovereignty [extends: BB.01]

A `VKTask` cannot exist in an orphaned state; it MUST be associated with a valid `VKProject` at creation. Moving a task between projects is a high-impact operation that requires explicit permission checks and project-level event emission.

## TNO05: Mandatory Domain Event Emission [uses: CS.01]

Every significant state change (Status Change, User Assignment, Detail Update) MUST raise a corresponding Domain Event (e.g., `TaskStatusChangedEvent`). These events are the primary triggers for side effects like notifications and audit logging.

## TNO06: Attachment Management [standalone]

Task attachments MUST be handled as metadata records pointing to external blob storage. The application layer MUST NOT handle raw byte streams directly but instead use signed URLs or abstraction-based upload/download flows.

## TNO07: Audit Traceability [uses: CS.05]

All Aggregate Roots in TNO MUST implement `IVKAuditable`. Interceptors MUST be used to automatically populate `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy` fields. Manual setting of these fields in domain logic is prohibited except for seed data.

## TNO08: Assignment Logic [uses: CS.01]

Task assignment to a user MUST validate that the user has the necessary permissions within the target Project. A task can only be assigned to one primary user at a time to ensure clear accountability.

## TNO09: Result-Wrapped Contract Flow [extends: CS.01]

All Application Layer services and MediatR handlers MUST return `VKResult` or `VKResult<T>`. Error codes MUST use the `Task.NotFound` or `Task.InvalidStatusTransition` format defined in `TaskErrors.cs`.

## TNO10: Notification Triggering [standalone]

Event handlers for Task/Project events MUST be idempotent. Notification delivery (Email, Teams, etc.) should be treated as a "best-effort" secondary operation that does not block the primary transaction.

---

## TNO Audit Checklist (Phase C: Domain Audit)

- [ ] **Dependency Check (TNO01)**: Status transition validated against dependent tasks.
- [ ] **Tenant Isolation (TNO02)**: IVKMultiTenantEntity implemented and TenantId propagated.
- [ ] **Time Integrity (TNO03)**: Hours > 0 and record immutability respected.
- [ ] **Project Linkage (TNO04)**: All tasks belong to a project; no orphans.
- [ ] **Event Emission (TNO05)**: Domain events raised for all state changes.
- [ ] **Auditable Flow (TNO07)**: IVKAuditable implemented and fields populated via interceptors.
- [ ] **Error Patterns (TNO09)**: VKResult returned with standardized TaskErrors.
- [ ] **Notification Handlers (TNO10)**: Handlers are idempotent and non-blocking.
