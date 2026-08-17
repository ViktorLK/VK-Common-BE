---
layer: 3
id: caching-manifest
scope: building-blocks/caching
requires: CS.02, CS.06, AP.04, BB.07, OR.03
---

# VK.Blocks Caching Manifest (Layer 3)

Defines the provider-agnostic contract for business-object caching — get-or-set, invalidation, and multi-level composition. This package MUST contain zero references to any concrete cache backend (StackExchange.Redis, `IDistributedCache`, `IMemoryCache` implementations); concrete providers are exclusively downstream packages (e.g. `Caching.Redis`, `Caching.InMemory`).

Scope is strictly business-object caching (DTOs, aggregate query results, computed values). HTTP-semantic caching (ETag, Cache-Control, Output Caching) belongs exclusively to `Web.Caching` — see Rule 13.

## Architectural Boundaries

### 1. Zero Backend Dependency

- This package MUST NOT reference any concrete cache SDK. All contracts (`IVKCache`, `IVKDistributedCache`) MUST be resolvable purely against BCL + Core.

### 2. Get-or-Set as the Primary API

- `GetOrSetAsync<T>(key, factory, ttl)` MUST be the primary consumption pattern. Bare `GetAsync`/`SetAsync` pairs are permitted but MUST NOT be the only API surface, to avoid every call site reimplementing null-check-then-fetch logic.

### 3. Single Key Authority

- Cache keys MUST be generated exclusively through a shared Key Builder enforcing `{TenantId}:{Category}:{Id}` format. Ad-hoc string concatenation of cache keys anywhere outside this builder is PROHIBITED — this is a direct cross-tenant collision risk, identical in nature to Storage's path-authority rule.

### 4. Cache Failure Degrades, Never Breaks

- Cache backend unavailability MUST degrade to direct pass-through to the underlying data source. A cache failure MUST NOT surface as a business-operation failure. This mirrors Observability's principle that a telemetry/optimization subsystem must never become a single point of failure.

### 5. Request Coalescing for Hot Keys

- Concurrent requests for the same expired/missing key MUST be coalesced into a single factory invocation (SingleFlight pattern) — concurrent cache-miss stampedes triggering N duplicate downstream calls for the same key are PROHIBITED.

### 6. TTL Jitter for Bulk Expiration

- Bulk-set operations (e.g. cache-warming a category of keys) MUST apply randomized TTL jitter to avoid synchronized mass expiration causing a downstream load spike.

### 7. Negative Caching for Penetration Protection

- The contract MUST support caching "not found" results (or an equivalent negative-cache marker) with a distinct, typically shorter TTL, to prevent repeated lookups of non-existent keys from bypassing the cache entirely.

### 8. Event-Driven Invalidation Preferred

- Cache invalidation SHOULD be triggered via domain event subscription or a persistence-layer hook (e.g. post-`SaveChanges`), not scattered manual `RemoveAsync` calls at arbitrary call sites. Manual invalidation is permitted only where no natural domain event exists.

### 9. Tag-Based Bulk Invalidation

- The contract MUST support associating multiple keys with a shared tag and invalidating by tag, for scenarios requiring bulk invalidation (e.g. "all product caches for a tenant") without key enumeration.

### 10. L1/L2 Consistency via Broadcast

- When a multi-instance deployment uses a local L1 cache layered over a distributed L2, invalidation MUST broadcast to all instances (e.g. via pub/sub) to prevent stale L1 reads after an L2/source-of-truth update.

### 11. Serialization via Core Contract, Version-Tagged

- Cached values MUST be serialized using Core's `IVKJsonSerializer` and MUST carry a schema version marker. A version mismatch on read MUST be treated as a cache miss, not a deserialization failure.

### 12. Bounded Memory Footprint

- Any in-process (L1) cache implementation MUST enforce a configurable maximum size/entry count with an eviction policy (e.g. LRU). Unbounded in-memory caching is PROHIBITED.

### 13. Web.Caching Boundary

- This module governs business-object caching only. `Web.Caching` (HTTP-semantic caching: ETag, Cache-Control, Output Caching) MUST NOT depend on this module's Key Builder or cache business objects through it — the two remain contractually independent even if they share the same physical backend.

### 14. Modular DI Registration

- `VKCachingBlock` MUST follow the standard 8-step DI registration order (R13). `IVKCachingBuilder` is the sole extension point for concrete providers — providers MUST NOT bypass it via direct `IServiceCollection` manipulation.

### 15. Provider-Agnostic Diagnostics

- Hit rate, key count, and memory footprint MUST be emitted under the shared Caching diagnostics namespace, consumable by Observability.

### 16. Test-Friendly In-Memory Fake

- A pure in-memory fake implementation of `IVKCache` MUST be available for unit tests without requiring a real Redis instance.
