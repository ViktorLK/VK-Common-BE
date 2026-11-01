---
id: "#064"
legacy_id: "AI.SEMANTICKERNEL-004"
category: debt
parent: null
title: Complete Governance Filters and Token Counting Logic
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
  - VKAIAuditSettings
  - VKAISafetySettings
  docs: []
commits: []
---
# Task: Complete Governance Filters and Token Counting Logic
**ID**: #064 *(Legacy: AI.SEMANTICKERNEL-004)*
**Status**: 🟡 Medium | #Debt
**Target**: `Governance/, Filters/`
**Ref**: VKAIAuditSettings, VKAISafetySettings

## 📝 Description
Complete the logic for VKSensitiveContentFilter (integrating with Moderation services) and VKTokenCounter (integrating with Tiktoken or similar tokenizer). Currently these are placeholders in the Governance/Filters directory.

## ✅ DoD (Definition of Done)
- [ ] Complete Governance Filters and Token Counting Logic
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests