---
id: "#119"
legacy_id: "LABS.PWP-003"
category: debt
parent: null
title: Implement Granular Message Management (Edit/Delete)
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
  - SillyTavern parity features
  docs: []
commits: []
---
# Task: Implement Granular Message Management (Edit/Delete)
**ID**: #119 *(Legacy: LABS.PWP-003)*
**Status**: 🟡 Medium | #Debt
**Target**: `VK.Labs.Pwp.Controllers.MessageController`
**Ref**: SillyTavern parity features

## 📝 Description
Implement specific message CRUD (Update/Delete) in IPwpChatHistoryStore and expose it via a MessageController. This allows users to retract AI responses or edit their own messages to correct typos.

## ✅ DoD (Definition of Done)
- [ ] Implement Granular Message Management (Edit/Delete)
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests