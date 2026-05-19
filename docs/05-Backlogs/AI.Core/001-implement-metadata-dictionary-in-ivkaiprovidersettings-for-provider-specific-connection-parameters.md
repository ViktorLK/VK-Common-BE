# Task: Implement Metadata dictionary in IVKAIProviderOptions for provider-specific connection parameters
**ID**: AI.CORE-001
**Status**: 🟡 Medium, reference: [AP.05] | #Debt
**Target**: `N/A`
**Ref**: N/A

## 📝 Description
To avoid polluting the core AI abstractions with provider-specific properties (like Azure's DeploymentName or OpenAI's OrgId), introduce a `Dictionary<string, string> Metadata` in `IVKAIProviderOptions`. This allows implementation-specific connectors (e.g., in AISK) to retrieve specialized parameters without modifying the core Options classes. This aligns with the 'Open-Closed Principle' and keeps the config schema clean for different AI vendors.

## ✅ DoD (Definition of Done)
- [ ] Implement Metadata dictionary in IVKAIProviderOptions for provider-specific connection parameters
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests