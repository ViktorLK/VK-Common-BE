---
layer: 3
id: ai-manifest
scope: building-blocks/ai
# [Specialized Rules for AI] Resilience, Performance, and Testing
requires: CS.04
requires: OR.03
requires: AP.05
requires: DL.01
---

# 🤖 VK.Blocks AI Manifest (Layer 3)

The AI module orchestrates non-deterministic LLM calls. It must be shielded by robust resiliency and strict configuration patterns.

## AI Mandates
1. **Resiliency (OR.03)**: ALL LLM/Embedding provider calls MUST be wrapped in a Polly resilience pipeline (Retry + Circuit Breaker).
2. **Streaming Performance (CS.04)**: Use `IAsyncEnumerable<T>` and optimize buffer allocations for high-throughput token streaming.
3. **Behavioral Overrides (AP.05)**: Every model interaction MUST support per-request overrides (e.g., Temperature, MaxTokens) via the `XxxArgs` pattern.
4. **Mocking Strategy (DL.01)**: Unit tests must never call real AI endpoints. Use robust mocks for deterministic behavioral testing.
