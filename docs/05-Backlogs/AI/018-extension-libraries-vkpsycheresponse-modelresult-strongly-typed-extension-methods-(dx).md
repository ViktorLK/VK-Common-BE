# Task: Extension Libraries VKPsycheResponse ModelResult Strongly-Typed Extension Methods (DX)
**ID**: AI-018
**Status**: 🟡 Medium | #Debt
**Target**: `VKPsycheResponseExtensions`
**Ref**: AP.03 / DL.04

## 📝 Description
Extension libraries (e.g., AI.Eidos, AI.Engram) MUST provide self-documenting extension methods (e.g., GetToolCallResult(), GetEngramResult()) over VKPsycheResponse.GetModelResult<T>() to avoid undocumented object? casting pain and improve DX.

## ✅ DoD (Definition of Done)
- [ ] Extension Libraries VKPsycheResponse ModelResult Strongly-Typed Extension Methods (DX)
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests