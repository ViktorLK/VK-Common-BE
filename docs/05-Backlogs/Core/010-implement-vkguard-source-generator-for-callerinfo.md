# Task: Implement VKGuard Source Generator for CallerInfo
**ID**: CORE-010
**Status**: 🟡 Medium | #Debt
**Target**: `VK.Blocks.Core -> Guards/`
**Ref**: Core README (Future Outlook), ADR-010

## 📝 Description
Implement a Source Generator to resolve 'CallerArgumentExpression' and parameter metadata at compile-time for 'VKGuard' methods. This aims to eliminate runtime reflection/metadata retrieval and further reduce the overhead of high-frequency guard clauses.

## ✅ DoD (Definition of Done)
- [ ] Implement VKGuard Source Generator for CallerInfo
- [ ] **Assess if an ADR is required (DL.03)**
- [ ] Verify changes
- [ ] Run tests