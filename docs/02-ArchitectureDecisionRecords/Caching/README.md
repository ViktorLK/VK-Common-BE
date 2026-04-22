# Architecture Decision Records (ADR) - Caching

このディレクトリには、VK.Blocks.Caching モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

#### [ADR-001: Adopting Result Pattern in Caching Module](/docs/02-ArchitectureDecisionRecords/Caching/adr-001-adopting-result-pattern-in-caching-module.md)

**Status**: ✅ Accepted  
**概要**: ICacheBlock および ICacheProvider に Result パターンを採用し、明示的なエラー処理と Application Layer との一貫性を確保。  
**キーワード**: Result Pattern, Error Handling, Consistency

---

## 🎯 ADR の読み方ガイド

### アーキテクチャと堅牢性の理解用
1. **ADR-001**: なぜキャッシュ操作に Result パターンが必要なのかを理解するために読んでください。

## 🔗 関連ドキュメント
- [Caching Architecture Audit (2026-03-20)](/docs/04-AuditReports/Caching/Caching_20260320.md)

**Last Updated**: 2026-03-20  
**Total ADRs**: 1
