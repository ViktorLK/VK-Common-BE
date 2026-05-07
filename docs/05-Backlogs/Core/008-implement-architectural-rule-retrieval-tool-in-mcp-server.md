# Task: Implement Architectural Rule Retrieval Tool in MCP Server
**ID**: CORE-008
**Status**: 🔴 High | #Debt
**Target**: `VK.Tools.McpServer`
**Ref**: N/A

## 📝 Description
Implement a new MCP tool 'vk_get_architectural_rule' that allows the AI to fetch detailed rule specifications (CS.xx, OR.xx, etc.) from the .agents/rules/ directory by ID. This enables a thinner system prompt where only the index (checklist) is stored, while the full 'Industrial DNA' is fetched on-demand.

## ✅ DoD (Definition of Done)
- [ ] Implement Architectural Rule Retrieval Tool in MCP Server
- [ ] **Assess if an ADR is required (Rule 11)**
- [ ] Verify changes
- [ ] Run tests