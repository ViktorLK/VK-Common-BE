---
layer: 3
id: messaging-manifest
scope: building-blocks/messaging
requires: CS.02, CS.06, AP.04, BB.07, BB.08, OR.03
---

# VK.Blocks Messaging Manifest (Layer 3)

Defines the broker-agnostic contract for event-driven, cross-service communication — publish/subscribe, queuing, and delivery guarantees. This package MUST contain zero references to any concrete broker SDK (Azure.Messaging.ServiceBus, RabbitMQ.Client, Confluent.Kafka); concrete providers are exclusively downstream packages (e.g. `Messaging.AzureServiceBus`, `Messaging.RabbitMQ`).

Scope is cross-service/cross-boundary event-driven communication where the publisher does not know or control who consumes. Scheduled/delayed/recurring internal units of work belong exclusively to BackgroundJobs — see Rule 14.

## Architectural Boundaries

### 1. Zero Broker Dependency

- This package MUST NOT reference any concrete messaging SDK. All contracts (`IVKMessage`, `IVKEvent`, `IVKCommand`, `IVKMessagePublisher`, `IVKMessageSubscriber`) MUST be resolvable purely against BCL + Core.

### 2. Standardized Envelope

- Every message MUST be wrapped in a standard Envelope (`MessageId`, `CorrelationId`, `CausationId`, `Timestamp`, `VKTenantId`, `SchemaVersion`, `Payload`). Business payload and transport metadata MUST remain separated — publishers/subscribers MUST NOT inline metadata fields into the payload type.

### 3. Shared Outbox Infrastructure

- Transactional publish (business write + message dispatch atomicity) MUST use the same shared Outbox infrastructure consumed by BackgroundJobs, integrated with Persistence's `IVKUnitOfWork`. Messaging MUST NOT implement an independent, parallel Outbox table/polling mechanism.

### 4. Mandatory Inbox for Non-Idempotent Consumers

- Any consumer whose handling logic is not naturally idempotent MUST use an Inbox pattern (processed-message-ID tracking) to deduplicate under At-Least-Once delivery. Assuming exactly-once delivery from the broker is PROHIBITED.

### 5. Explicit Tenant Context Restoration

- Every message Envelope MUST carry `VKTenantId`. Consumers MUST reconstruct `IVKUserContext` from the Envelope at the start of handling — consumers MUST NOT assume ambient tenant context.

### 6. Distributed Trace Propagation

- W3C Trace Context MUST propagate through message headers across the publish/consume boundary, per `observability-manifest` Rule 10. A message pipeline that breaks trace continuity is a defect, not an acceptable gap.

### 7. Dead-Letter on Exhausted Retry

- Consumption failures MUST retry with backoff aligned to `OR.03`. Exceeding the maximum retry count MUST route the message to a dead-letter queue — indefinite reprocessing of a poison message is PROHIBITED.

### 8. Ordering Is Opt-In, Not Assumed

- Message types requiring strict ordering MUST be explicitly declared as such and MUST use a consistent Partition/Session Key for that aggregate. The contract MUST NOT assume global ordering as a default guarantee, since most broker backends do not provide it.

### 9. Backward-Compatible Schema Evolution

- Payload schema changes MUST be additive/backward-compatible, or explicitly versioned via the Envelope's `SchemaVersion`. Deploying a breaking payload change without a version bump that consumers can branch on is PROHIBITED.

### 10. Backpressure Awareness

- The consumption contract MUST support configurable concurrency/prefetch limits so a consumer can throttle intake when processing capacity is exceeded — unbounded concurrent handling of an inbound burst is PROHIBITED.

### 11. Standardized Error Surface

- Publish/consume failures MUST surface through `MessagingErrors` constants and the `VKResult` pattern (R1). Raw broker-specific exceptions MUST NOT leak past the concrete provider package.

### 12. Modular DI Registration

- `VKMessagingBlock` MUST follow the standard 8-step DI registration order (R13). `IVKMessagingBuilder` is the sole extension point for concrete providers — providers MUST NOT bypass it via direct `IServiceCollection` manipulation.

### 13. Provider-Agnostic Diagnostics

- Publish latency, consume latency, queue backlog, and dead-letter count MUST be emitted under the shared Messaging diagnostics namespace, consumable by Observability.

### 14. BackgroundJobs Boundary

- Messaging governs cross-service, publisher-agnostic-of-consumer event communication. Scheduled, delayed, recurring, or explicitly-targeted internal work belongs to BackgroundJobs. Messaging MUST NOT implement job-scheduling semantics (cron, delay-until, single-target dispatch); BackgroundJobs MUST NOT implement pub/sub fan-out. A BackgroundJob MAY publish a message as a side effect; a message handler MAY enqueue a job — the two remain contractually distinct and neither reimplements the other.

### 15. Test-Friendly In-Memory Bus

- An in-memory message bus implementation MUST be available for unit/integration tests without requiring a real broker.
