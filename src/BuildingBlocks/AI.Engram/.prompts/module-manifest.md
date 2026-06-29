---
layer: 3
id: ai-engram-manifest
scope: building-blocks/ai-engram
# [Specialized Rules for AI.Engram] Memory Consolidation, Forgetting Curve decay, and Deduplication
requires: CS.01, CS.03, CS.04, AP.01, AP.03, BB.01, BB.03, BB.07, DL.01
---

# 🧠 VK.Blocks AI.Engram Manifest (Layer 3)

The `AI.Engram` module is responsible for managing the cognitive memory lifecycle (Compression, Consolidation, Decay, and Pruning) in AI workflows.

## Specialized Rules for Engram

### 1. Memory Tier Transitions
- **Compression (L1 to L2)**: Short-term dialogue history (Echo/L1) exceeding configured turn thresholds or triggered by a timer MUST be summarized into L2 summary records.
- **Consolidation (L2 to L3)**: Consolidation operations saving summaries into the long-term Vector Store (L3) MUST run inside the `AfterStage` pipeline hook or asynchronously in the background. Engram components MUST ONLY call the vector storage abstraction via the `AI.Recall` API, maintaining complete decoupling from physical vector databases.

### 2. Forgetting Curve decay Modeling
- **Ebbinghaus Decay Formula**: Retention score updates MUST calculate decay based on the forgetting curve model, dynamically adjusting the half-life based on entry access frequency (`AccessCount`).
- **Score-based Pruning**: The pruning processor (triggered periodically via background timers) MUST implement strategy-driven actions (Delete, Archive, or Compress) according to `VKEngramOptions` configurations.

### 3. Asynchronous Execution Safety
- **Background Worker Resilience**: Periodic background tasks (Decay, Pruning, Deduplication) MUST be isolated from client request pipelines. Exceptions arising during background worker iterations MUST be caught, logged, and tracked without compromising application stability.
- **Deduplication Cost Mitigation**: Cosine similarity deduplication checks (merging close vector memories via LLM integration) are high-overhead and MUST only run at the end of the pruning phase.
