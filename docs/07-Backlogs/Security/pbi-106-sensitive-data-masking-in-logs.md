---
id: '#106'
title: Sensitive Data Masking in Logs
module: Security
category: debt
parent: 
legacy_id: SECURITY-001
type: debt
priority: P1
status: todo
sprint: S-2026-44
points: 3
rank: 
started: 
carry_over_count: 0
ready: true
estimate: 
created: 2026-05-01
closed: 
refs:
  rules: []
  docs: []
commits: []
---
# Task: Sensitive Data Masking in Logs
**ID**: #106 *(Legacy: SECURITY-001)*
**Status**: 🔴 High | #Debt
**Target**: `N/A`
**Ref**: N/A

## 📝 Description
Implement a standard sensitive data masking processor for logs to ensure PII (like emails, phone numbers) are masked according to OR.02.

### 1. Problem / Context (現状の課題と背景)
Logging output currently does not automatically mask sensitive PII (emails, phone numbers, tokens), violating OR.02 compliance requirements.

### 2. Proposed Changes (具体的な改修方針)
- Implement a centralized PII masking processor / formatter for logs.
- Provide configurable masking rules and integrate into Security / Observability pipelines.
- Verify that sensitive fields are replaced by redaction tokens.

## ✅ DoD (Definition of Done)
- [ ] Sensitive Data Masking in Logs
- [ ] Verify changes
- [ ] Run tests
