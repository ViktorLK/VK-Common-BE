# Architecture Decision Records (ADR) - Web Index

このディレクトリには、VK.Blocks.Web モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Design (コア設計)

#### [ADR-001: Result Monad と Railway-Oriented Programming](./adr-001-result-monad-rop.md)

**Status**: ✅ Accepted
**概要**: 例外ベースの制御フローに代わり、Result Monad + ROP パターンで型安全なエラーハンドリングを実現。`Bind`, `Map`, `Tap`, `Ensure`, `Match` の5演算子を実装。
**キーワード**: Result Monad, ROP, Functional Programming, Error Handling

---

### Performance Optimization (性能最適化)

#### [ADR-002: Expression Tree コンパイルキャッシュ](./adr-002-expression-tree-caching.md)

**Status**: ✅ Accepted
**概要**: `ValidationFailureCache` で `Lazy<T>` + `ConcurrentDictionary` + `Expression.Compile()` を使用し、リフレクションコストを排除。
**性能向上**: ~5,000倍 (MethodInfo.Invoke ~5μs → Compiled Delegate ~1ns)
**キーワード**: Expression Tree, Lazy Initialization, Reflection Optimization

---

### Design Patterns (設計パターン)

#### [ADR-003: CorrelationId の Strategy パターン抽象化](./adr-003-correlation-id-strategy.md)

**Status**: ✅ Accepted
**概要**: `ICorrelationIdProvider` + `CorrelationIdOptions` で ID 生成戦略を DI で差し替え可能に設計。ヘッダー > TraceId > GUID の優先順位を設定で制御。
**キーワード**: Strategy Pattern, DIP, OCP, Distributed Tracing

---

## 🎯 ADR の読み方

### 面接準備用

1. **ADR-001**: 関数型プログラミング概念 (Monad, ROP) の C# への適用
2. **ADR-002**: Expression Tree と CLR の深い理解、パフォーマンス最適化

### アーキテクチャ理解用

1. **ADR-001**: CQRS + MediatR パイプラインとの統合設計
2. **ADR-003**: Strategy パターンと Options パターンの併用

### 横断的関心事の設計用

1. **ADR-003**: 分散トレーシングの CorrelationId 戦略

---

## 🔗 関連ドキュメント

- [Architecture Audit Report](/docs/04-AuditReports/Web/Web_20260219.md) - 最新の監査報告書 (87/100)
- [EFCore ADRs](/docs/02-ArchitectureDecisionRecords/EFCore/README.md) - EFCore モジュールの ADR

---

**Last Updated**: 2026-02-19
**Total ADRs**: 3
