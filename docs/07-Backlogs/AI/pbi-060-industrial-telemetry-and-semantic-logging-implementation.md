---
id: "#060"
legacy_id: "AI.SEMANTICKERNEL-003"
category: debt
parent: null
title: Industrial Telemetry and Semantic Logging Implementation
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
# Task: Industrial Telemetry and Semantic Logging Implementation
**ID**: #060 *(Legacy: AI.SEMANTICKERNEL-003)*
**Status**: 🔴 High | #Debt
**Target**: `Diagnostics/Internal/AISKDiagnosticsConstants.cs`
**Ref**: OR.01, BB.04

## 📝 Description
Ensure all AI engines (Chat, Embedding, Retrieval) use structured logging via [LoggerMessage] source generators as per OR.01. Implement detailed semantic tokens in AISKDiagnosticsConstants and verify OTel trace propagation across the kernel pipeline. Remove any legacy logger.LogXxx calls.

## ✅ DoD (Definition of Done)
- [ ] Industrial Telemetry and Semantic Logging Implementation
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests