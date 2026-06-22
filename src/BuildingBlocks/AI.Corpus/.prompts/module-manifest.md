---
layer: 3
id: ai-corpus-manifest
scope: building-blocks/ai-corpus
# [Specialized Rules for AI.Corpus] Pluggable Filter Pipeline, Cooldown & Recency Decays
requires: CS.01, CS.03, CS.04, AP.01, AP.03, BB.01, BB.03, BB.07, DL.01
---

# 📊 VK.Blocks AI.Corpus Manifest (Layer 3)

The `AI.Corpus` module manages the knowledge injection lifecycle (Gathering, Filtering, Tracking) within prompt orchestration pipelines.

## Specialized Rules for Corpus

### 1. In-Memory Store Fallbacks & Graceful Degradation
- **Local Store Fallbacks**: Provide standard `InMemoryKnowledgeLifecycleStore` and `InMemoryKnowledgeInjectionStore` to enable zero-infrastructure startup.
- **Fault-Tolerant Tracking**: Operations on `IVKKnowledgeInjectionStore` during the `Tracking` stage MUST NOT block or fail the execution pipeline. Exceptions from injection logging MUST be caught, logged via diagnostics, and degraded gracefully to return `Result.Success`.

### 2. Multi-Stage Filter Chain Ordering
- **Deterministic Filtering Sequence**: Filter executions inside `DefaultFilteringStage` MUST strictly follow the prioritized category sequence:
  1. *Stickiness* (Force-retention filters like `StickinessFilter`).
  2. *Static Metadata* (e.g., `ScheduleFilter`, `FreshnessFilter`, `PersonaFilter`).
  3. *Behavioral Gates* (e.g., `CooldownFilter`, `EmotionGatedFilter`, `ProbabilityFilter`, `DependencyFilter`).
  4. *Mutex & Pruning* (e.g., `ConflictResolutionFilter`, `GroupTopNFilter`).
  5. *Budget & Decay* (e.g., `TokenBudgetFilter`, `RecencyBiasFilter`).
- **Feature Toggles**: Every filter implementation MUST inspect its corresponding `VKFilteringOptions` property before executing. A disabled filter must immediately bypass execution and pass candidates to the next node.

### 3. High-Performance Decay & Cooldown Calculation
- **Recency Decay Math**: Recency bias calculation (`RecencyBiasFilter`) MUST optimize float operations to minimize CPU cycles.
- **Thread-Safety on State**: Candidate evaluations and state transfers between stages (`Gathering` -> `Filtering` -> `Tracking`) MUST use thread-safe data structures (`VKCorpusContext` and `VKKnowledgeCandidatesState`).
