---
id: "#008"
legacy_id: "CORE-008"
category: debt
parent: null
title: Implement Architectural Rule Retrieval Tool in MCP Server
module: Core
type: debt
priority: P1
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
# Task: Implement Architectural Rule Retrieval Tool in MCP Server
**ID**: #008 *(Legacy: CORE-008)*
**Status**: 🔴 High | #Debt
**Target**: `VK.Tools.McpServer`
**Ref**: N/A

## 📝 Description
Implement a new MCP tool 'vk_get_architectural_rule' that allows the AI to fetch detailed rule specifications (CS.xx, OR.xx, etc.) from the .agents/rules/ directory by ID. This enables a thinner system prompt where only the index (checklist) is stored, while the full 'Industrial DNA' is fetched on-demand.

## ✅ DoD (Definition of Done)
- [x] Implement Architectural Rule Retrieval Tool in MCP Server
- [x] **Assess if an ADR is required (Rule 11)**
- [x] Verify changes
- [x] Run tests