---
layer: 3
id: ai-manifest
scope: building-blocks/ai
# [Specialized Rules for AI] Resilience, Performance, and Testing
requires: CS.04, OR.03, AP.05, DL.01
---

# 🤖 VK.Blocks AI Manifest (Layer 3)

The AI module orchestrates non-deterministic LLM calls. It must be shielded by robust resiliency and strict configuration patterns.

## AI Specific Extensions (L3)
1. **Resiliency (OR.03)**: Specifically, ALL provider-bound calls (LLM/Embeddings) MUST be wrapped in a Polly resilience pipeline.
2. **Streaming Performance (CS.04)**: For token streaming, MUST use `IAsyncEnumerable<T>` and optimize buffer allocations via `ArrayPool`.
3. **Behavioral Overrides (AP.05)**: Standardize on the `XxxArgs` pattern (e.g., `VKChatArgs`) to allow per-request model parameter tuning.
4. **Mocking Strategy (DL.01)**: Unit tests MUST use mock providers to ensure deterministic behavioral testing without real endpoint calls.
