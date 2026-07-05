---
layer: 3
id: ai-engram-manifest
scope: building-blocks/ai-engram
requires: CS.01, CS.03, CS.04, AP.01
---

# VK.Blocks AI.Engram Manifest (Layer 3)

The cognitive memory lifecycle module — manages Compression, Consolidation, Decay, and Pruning of AI conversation memories.

## Architectural Boundaries

### 1. Memory Tier Transitions

- L1 (short-term) → L2 (summary): Triggered by turn threshold or timer. Summarization is mandatory.
- L2 → L3 (long-term vector store): Consolidation MUST run in `AfterStage` or background. Access to vector storage MUST go through `VectorSearch` API only — no direct vector DB coupling.

### 2. Decay & Pruning

- Retention scores MUST follow the Ebbinghaus forgetting curve model with access-frequency-adjusted half-life.
- Pruning actions (Delete, Archive, Compress) are strategy-driven via configuration.
- Deduplication (cosine similarity) is high-overhead and MUST only run at the end of the pruning phase.

### 3. Background Isolation

- All periodic tasks (Decay, Pruning, Deduplication) MUST be isolated from client request pipelines. Background exceptions MUST be caught and logged — never propagated to callers.
