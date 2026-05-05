# VK.Blocks Backlog Dashboard

Welcome to the central backlog for the VK.Blocks ecosystem. This directory tracks all technical debt, upcoming features, and architectural refinements categorized by module.

## 📊 Status Summary

| Module | Active Tasks | High Priority | Status |
|---|:---:|:---:|---|
| **[Code Scanning](active/00-Code-Scanning.md)** | 8 | 1 | 🔍 Scanning |
| **[Core](active/01-Core.md)** | 8 | 2 | 🟢 Healthy |
| **[Authorization](active/02-Authorization.md)** | 5 | 1 | 🟡 Normalizing |
| **[MultiTenancy](active/03-MultiTenancy.md)** | 2 | 0 | 🟢 Stable |
| **[Persistence](active/04-Persistence.md)** | 2 | 2 | 🔴 Refactoring |

## 🛠️ Management Guide

- **New Tasks**: Add to the respective module file in `active/`.
- **Completion**: Move the task to `archive/` or mark as completed in the module file.
- **Large Shifts**: If a task requires a significant design change, trigger an **ADR** (Rule 11).

## 📂 Directory Structure

- `active/`: Ongoing and approved tasks.
- `archive/`: Historical record of completed work.
- `ideas/`: Future visions and unrefined proposals.

---
*Last Updated: 2026-05-05*
