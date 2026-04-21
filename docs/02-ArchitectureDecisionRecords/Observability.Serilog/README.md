# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Observability.Serilog モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

#### [ADR-001: Eliminate BuildServiceProvider Anti-pattern in DI Configuration](/docs/02-ArchitectureDecisionRecords/Observability.Serilog/adr-001-eliminate-buildserviceprovider-anti-pattern-in-di-configuration.md)

**Status**: 📝 Draft  
**概要**: DIコンテナの構築フェーズにおける不要なシングルトン複製やメモリリークを防ぐため、`BuildServiceProvider()` の呼び出しを排除し、ファクトリパターンによる設定方針に移行する設計決定。  
**キーワード**: Dependency Injection, Anti-pattern, Memory Leak, IServiceProvider

---

#### [ADR-002: Adopt ISinkConfigurator Pattern with Static Abstract Members](/docs/02-ArchitectureDecisionRecords/Observability.Serilog/adr-002-adopt-isinkconfigurator-pattern-with-static-abstract-members.md)

**Status**: 📝 Draft  
**概要**: 中央集権的な拡張メソッドの肥大化を防ぎ、将来のログ出力先（Sink）追加に柔軟に対応するため、C# 12の`static abstract`インターフェースメンバーを用いた型安全なプラグインアーキテクチャを導入する設計決定。  
**キーワード**: Open-Closed Principle, static abstract, C# 12, Sink Configuration

---

#### [ADR-003: Implement Partial-Match Lazy-Evaluated PII Masking Strategy](/docs/02-ArchitectureDecisionRecords/Observability.Serilog/adr-003-implement-partial-match-lazy-evaluated-pii-masking-strategy.md)

**Status**: 📝 Draft  
**概要**: ログを通じた機密情報（PII）の漏洩を防ぐため、完全一致ではなく部分一致（OrdinalIgnoreCase）を採用し、かつマスキング不要時の不要なメモリアロケーションを防ぐ遅延評価（Lazy Evaluation）を実装する設計決定。  
**キーワード**: Security, PII Masking, Performance, Lazy Evaluation

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとセキュア設計の理解用

1. **ADR-001**: ASP.NET Core の DI コンテナにおける最も典型的なアンチパターン（BuildServiceProvider）とその解決策を学ぶための最適な基礎資料です。
2. **ADR-002**: モダン C#（静的抽象メンバー）を活用して、実行時のオーバーヘッドなしにコンパイル時の多態性と拡張性（OCP）を担保する設計技法を理解するのに役立ちます。
3. **ADR-003**: ログのセキュリティと高スループット環境下でのパフォーマンス（アロケーションの最小化）をどのように両立させるかの実践的なトレードオフ判断を示しています。

## 🔗 関連ドキュメント

- [Architecture Audit Report (2026-03-15)](/docs/04-AuditReports/Observability.Serilog/Observability.Serilog_20260315.md)

---

**Last Updated**: 2026-03-15  
**Total ADRs**: 3
