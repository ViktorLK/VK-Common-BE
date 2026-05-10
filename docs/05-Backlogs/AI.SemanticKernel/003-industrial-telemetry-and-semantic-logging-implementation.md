# Task: Industrial Telemetry and Semantic Logging Implementation
**ID**: AI.SEMANTICKERNEL-003
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