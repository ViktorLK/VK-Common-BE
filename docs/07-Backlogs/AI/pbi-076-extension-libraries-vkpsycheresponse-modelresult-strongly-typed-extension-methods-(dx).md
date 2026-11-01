---
id: "#076"
legacy_id: "AI-018"
category: debt
parent: null
title: Extension Libraries VKPsycheResponse ModelResult Strongly-Typed Extension Methods (DX)
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
  - AP.03 / DL.04
  docs: []
commits: []
---
# Task: Extension Libraries VKPsycheResponse ModelResult Strongly-Typed Extension Methods (DX)
**ID**: #076 *(Legacy: AI-018)*
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