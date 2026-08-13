# VK.Blocks.AI.Eidos Building Block

`VK.Blocks.AI.Eidos` handles AI Response Contract management, format negotiation, schema validation, self-healing repair, streaming partial parsing, DTO binding, and integration with `AI.Psyche` via `VKAIEidosPsycheMiddleware`.

## Architecture

This building block follows **Vertical Slice Architecture** with 3 high-cohesion feature slices:

1. **Contract** (`Contract/`):
   - Metadata containers (`VKAIEidosResponseContract`, `VKAIEidosSchema`).
   - Contract versioning (`VKAIEidosContractVersion`) & migration (`IVKContractMigrator`).
   - Hierarchical override registry (`IVKContractRegistry`: Tenant -> Persona -> Scenario).
   - Context-aware contract router (`IVKContractResolver`).

2. **Negotiation** (`Negotiation/`):
   - Capability detector (`IVKProviderCapabilityDetector`).
   - Format negotiator (`IVKContractNegotiator`: Structured Output vs Tool Call vs Prompt JSON).
   - Fallback policy (`IVKContractFallbackPolicy`).
   - Agnostic representation projector (`IVKContractProjector`).

3. **Parsing** (`Parsing/`):
   - Markdown JSON extractor (`IVKContractExtractor`).
   - JSON Schema & field validator (`IVKContractValidator`).
   - Self-healing repair instruction builder (`IVKContractRepairService`).
   - Incremental streaming parser (`IVKContractStreamParser`).
   - Strongly-typed DTO binder (`IVKContractBinder`).
   - **Psyche Pipeline Middleware** (`VKAIEidosPsycheMiddleware`).

## Registration

```csharp
services.AddVKAIEidosBlock(configuration);
```
