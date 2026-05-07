# RFC: [Project Name / Feature Name]

**Status**: 📝 Draft / 🔍 Review / ✅ Approved / 🛑 Rejected
**Author**: [Author Name]
**Date**: [YYYY-MM-DD]
**Module**: [e.g. AI.Cognitive]

## 1. 🧬 Metaphor (隐喻)
> **The biological or physical model that inspires this design.**

Describe the mental model. For example: "The Cold/Hot Synapse model simulates how human brains move short-term memories to long-term storage based on access frequency and emotional intensity."

## 2. 🗺️ Mapping (映射)
> **How the metaphor translates into system components.**

| Metaphor Component | Technical Implementation | Rationale |
| :--- | :--- | :--- |
| Cold Synapse | Redis / Low-TTL Cache | Ephemeral, fast access |
| Hot Synapse | Vector DB / Persistent | Permanent, associative |
| Consolidation | Background Job (Worker) | Periodic pruning and migration |

## 3. 🏗️ Code Blueprint (蓝图)
> **Core interfaces and data structures.**

```csharp
public interface IVKSynapseManager { ... }
public record SynapseContext(Guid Id, float Weight);
```

## 4. 🧪 Implementation Strategy
> **High-level steps for development.**

- [ ] Prototype phase
- [ ] Integration with Core
- [ ] Performance benchmarks
