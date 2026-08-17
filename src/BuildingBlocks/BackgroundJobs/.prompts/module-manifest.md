---
layer: 3
id: backgroundjobs-manifest
scope: building-blocks/backgroundjobs
requires: CS.02, CS.06, AP.04, BB.07, BB.08, OR.03
---

# VK.Blocks BackgroundJobs Manifest (Layer 3)

Defines the scheduler-agnostic contract for fire-and-forget, delayed, recurring, and continuation jobs. This package MUST contain zero references to any concrete scheduler SDK (Hangfire, Quartz.NET, Azure Functions Timer); concrete providers are exclusively downstream packages (e.g. `BackgroundJobs.Hangfire`).

## Architectural Boundaries

### 1. Zero Scheduler Dependency

- This package MUST NOT reference any concrete scheduling library. All contracts (`IVKJob`, `IVKRecurringJob`, `IVKJobStore`) MUST be resolvable purely against BCL + Core.

### 2. Transactional Enqueue (Outbox)

- Enqueueing a job as a consequence of a business data write MUST go through the Outbox pattern integrated with Persistence's `IVKUnitOfWork`. Enqueueing a job via direct scheduler API calls inside a business transaction (risking enqueue/commit inconsistency) is PROHIBITED.

### 3. Explicit Tenant Context Restoration

- Every job payload MUST carry `VKTenantId` (Core) explicitly. Job execution MUST reconstruct `IVKUserContext` from the payload at the start of execution — jobs MUST NOT assume ambient user/tenant context is available, since none exists outside an HTTP request scope.

### 4. Idempotent Execution

- Jobs MUST support an Idempotency Key derived from business intent (not just a random job ID). Re-enqueueing the same logical unit of work MUST be detectable and MUST NOT result in duplicate side effects.

### 5. Standardized Error Surface

- Job execution failures MUST surface through `BackgroundJobErrors` constants and the `VKResult` pattern (R1). Raw scheduler-specific exceptions MUST NOT leak past the concrete provider package.

### 6. Retry Policy Alignment

- Retry count and backoff strategy MUST follow the same resilience philosophy as `OR.03` (Provider Resiliency). Exhausted retries MUST route the job to a dead-letter/failed state, never silently drop it.

### 7. Distributed Coordination for Recurring Jobs

- Recurring/Cron jobs MUST NOT rely on single-instance assumptions. In multi-instance deployments, execution MUST be coordinated via a distributed lock or the scheduler's native leader-election mechanism — duplicate concurrent firing of the same recurring job across instances is PROHIBITED.

### 8. Cancellation and Timeout Propagation

- `CancellationToken` MUST propagate through the full execution path. A job exceeding its configured timeout MUST be cancelled and marked failed — indefinite hangs are PROHIBITED.

### 9. Concurrency Throttling per Job Type

- Each job type MUST support a configurable maximum concurrent execution limit, to prevent overwhelming downstream resources (database, external APIs) shared with the request-serving path.

### 10. Priority Queue Segregation

- Job enqueueing MUST support routing to distinct priority queues (e.g. `critical`/`default`/`low-priority`). Low-priority job volume MUST NOT be able to starve critical job processing.

### 11. Graceful Shutdown

- On application shutdown, in-flight jobs MUST either complete or be safely re-queued — abrupt termination that leaves a job in an ambiguous "maybe executed" state is PROHIBITED.

### 12. Payload Serialization via Core Contract

- Job payloads MUST be serialized using Core's `IVKJsonSerializer`. Payload schema changes MUST be backward-compatible or versioned — deploying a payload shape change that breaks deserialization of already-enqueued jobs is PROHIBITED without a migration/versioning strategy.

### 13. Messaging Boundary

- BackgroundJobs governs scheduled/delayed/recurring internal units of work. Cross-service or cross-boundary event-driven communication belongs exclusively to the Messaging module. A job MAY publish a message as its side effect, but BackgroundJobs MUST NOT reimplement pub/sub semantics.

### 14. Modular DI Registration

- `VKBackgroundJobsBlock` MUST follow the standard 8-step DI registration order (R13). `IVKBackgroundJobsBuilder` is the sole extension point for concrete providers — providers MUST NOT bypass it via direct `IServiceCollection` manipulation.

### 15. Provider-Agnostic Diagnostics

- Job duration, success/failure rate, and queue backlog length MUST be emitted under the shared BackgroundJobs diagnostics namespace, consumable by Observability's dashboard integration.

### 16. Test-Friendly Synchronous Scheduler

- A synchronous, in-process fake scheduler MUST be available so unit tests can assert "was this job enqueued with these parameters" without requiring a real backend scheduler.
