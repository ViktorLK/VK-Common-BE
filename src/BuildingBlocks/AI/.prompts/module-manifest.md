---
layer: 3
id: ai-manifest
scope: building-blocks/ai
# [Specialized Rules for AI] Resilience, Performance, and Testing
requires: CS.04, OR.03
---

# 🤖 VK.Blocks AI Manifest (Layer 3)

The AI module orchestrates non-deterministic LLM calls and handles high-throughput token streams.

## AI Specialized Extensions (L3)
1. **Provider Resiliency (OR.03)**: ALL provider-bound calls (LLM/Embeddings) MUST be wrapped in a Polly resilience pipeline to handle transient external failures.
2. **Streaming Efficiency (CS.04)**: For token streaming, MUST use `IAsyncEnumerable<T>` and strictly optimize buffer allocations via `ArrayPool` to minimize GC pressure.
3. **Contractual Args Mapping (AP.05 Specialized)**:
    - AI `Args` records MUST be source-generated strictly from `IVK...Overrides` interfaces to ensure security and surface area control.
    - **Security by Default**: Properties in Options NOT present in a matching Overrides interface are automatically excluded from the generated Args surface area.
4. **Contextual Instrumentation**: AI orchestration (Chat/Agents) MUST implement hierarchical tracing and metric recording using the `AiDiagnostics` hub.


