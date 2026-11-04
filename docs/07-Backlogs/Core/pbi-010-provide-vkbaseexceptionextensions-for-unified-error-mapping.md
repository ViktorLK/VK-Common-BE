---
id: "#010"
legacy_id: "CORE-012"
category: debt
parent: null
title: Provide VKBaseExceptionExtensions for Unified Error Mapping
module: Core
type: debt
priority: P2
status: done
sprint: 
estimate: 
created: 2026-05-01
closed: 2026-05-05
refs:
  rules:
  - ADR-011
  docs: []
commits: []
---
# Task: Provide VKBaseExceptionExtensions for Unified Error Mapping
**ID**: #010 *(Legacy: CORE-012)*
**Status**: ✅ Completed | #Debt
**Target**: `VK.Blocks.Core -> Exceptions/`
**Ref**: ADR-011

## 📝 Description
Implement a set of extension methods to facilitate the conversion between standard .NET exceptions, 'VKResult' objects, and the new 'VKBaseException' hierarchy. This helper is essential for migrating legacy modules to the standardized fault modeling defined in ADR-011.

## ✅ DoD (Definition of Done)
- [x] Provide VKBaseExceptionExtensions for Unified Error Mapping
- [x] **Assess if an ADR is required (DL.03)**
- [x] Verify changes
- [x] Run tests