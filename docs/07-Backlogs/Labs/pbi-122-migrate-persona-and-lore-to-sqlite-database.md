---
id: "#122"
legacy_id: "LABS.PWP-006"
category: debt
parent: null
title: Migrate Persona and Lore to SQLite Database
module: Labs
type: debt
priority: P2
status: icebox
sprint: 
estimate: 
created: 2026-05-01
closed: 
refs:
  rules:
  - PWP-006
  docs: []
commits: []
---
# Task: Migrate Persona and Lore to SQLite Database
**ID**: #122 *(Legacy: LABS.PWP-006)*
**Status**: 🟡 Medium | #Debt
**Target**: `PwpSqliteChatHistoryStore.cs`
**Ref**: PWP-006

## 📝 Description
Migrate Persona and Lorebook storage from local JSON file system to central SQLite database. Implement PwpSqlitePersonaCodex and PwpSqliteLorebookManager to enable ACID transactions, Full-Text Search (FTS5) for lore entries, and multi-tenant isolation. Maintain SillyTavern compatibility via JSON import/export endpoints.

## ✅ DoD (Definition of Done)
- [ ] Migrate Persona and Lore to SQLite Database
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests