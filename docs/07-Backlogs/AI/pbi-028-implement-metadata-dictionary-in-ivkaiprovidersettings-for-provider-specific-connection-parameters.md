---
id: "#028"
legacy_id: "AI.CORE-001"
category: debt
parent: null
title: Implement Metadata dictionary in IVKAIProviderOptions for provider-specific connection parameters
module: AI
type: debt
priority: P2
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules: []
  docs: []
commits: []
---
# Task: Implement Metadata dictionary in IVKAIProviderOptions for provider-specific connection parameters
**ID**: #028 *(Legacy: AI.CORE-001)*
**Status**: 🟢 Completed | #Debt
**Target**: `IVKAIProviderOptions`
**Ref**: N/A

## 📝 Description
To avoid polluting the core AI abstractions with provider-specific properties (like Azure's DeploymentName or OpenAI's OrgId), introduce a `Dictionary<string, string> Metadata` in `IVKAIProviderOptions`. This allows implementation-specific connectors (e.g., in AISK) to retrieve specialized parameters without modifying the core Options classes. This aligns with the 'Open-Closed Principle' and keeps the config schema clean for different AI vendors.

## ✅ DoD (Definition of Done)
- [x] Implement Metadata dictionary in IVKAIProviderOptions for provider-specific connection parameters
- [x] **Assess if an ADR is required (DL.03)** (Not required; non-breaking default interface implementation)
- [x] Verify changes
- [x] Run tests