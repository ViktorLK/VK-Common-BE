---
id: "#066"
legacy_id: "AI.EIDOS-004"
category: debt
parent: null
title: Support Streaming Partial Materialization and JSON Patch Incremental Push
module: AI
type: debt
priority: P2
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - RFC-StreamingPartialMaterialization
  docs: []
commits: []
---
# Task: Support Streaming Partial Materialization and JSON Patch Incremental Push
**ID**: #066 *(Legacy: AI.EIDOS-004)*
**Status**: 🟡 Medium | #Feature
**Target**: `src/BuildingBlocks/AI.Eidos/Streaming/`
**Ref**: RFC-StreamingPartialMaterialization

## 📝 Description
Incorporate JSON Patch (RFC 6902) delta generation into the streaming pipeline. Emit incremental property-level updates as soon as valid tokens arrive, enabling frontends to render partial state updates in real-time and drastically reducing interaction latency for complex schemas.

## ✅ DoD (Definition of Done)
- [ ] Support Streaming Partial Materialization and JSON Patch Incremental Push
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests
