---
id: "#061"
legacy_id: "AI.EIDOS-003"
category: debt
parent: null
title: Integrate OpenTelemetry Native Instrumentation and Metrics Observability
module: AI
type: debt
priority: P1
status: todo
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - OR.01
  - BB.04
  docs: []
commits: []
---
# Task: Integrate OpenTelemetry Native Instrumentation and Metrics Observability
**ID**: #061 *(Legacy: AI.EIDOS-003)*
**Status**: 🔴 High | #Observability
**Target**: `src/BuildingBlocks/AI.Eidos/Diagnostics/`
**Ref**: OR.01, BB.04

## 📝 Description
Wire up ActivitySource and Meter instrumentation across DefaultMaterializationMiddleware, DefaultStreamingParser, and DefaultContractNegotiator. Collect and export key metrics including materialization self-healing repair rate (`ai.eidos.materialization.repair_rate`), contract negotiation downgrade count (`ai.eidos.negotiation.downgrades`), and time-to-first-envelope materialization duration.

## ✅ DoD (Definition of Done)
- [ ] Integrate OpenTelemetry Native Instrumentation and Metrics Observability
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests
