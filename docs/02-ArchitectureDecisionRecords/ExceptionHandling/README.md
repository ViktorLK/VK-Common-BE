# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.ExceptionHandling モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

#### [ADR-001: Standardizing ProblemDetails Factory and ExceptionHandling DI Builder](./adr-001-standardizing-problemdetails-factory-and-exceptionhandling-di-builder.md)

**Status**: ✅ Accepted  
**概要**: `ExceptionHandling` モジュール内の依存性の逆転（DIP）とテスト容易性を確保するため、ファクトリのインターフェース化とDI登録における型安全なBuilderパターンの導入を決定しました。  
**キーワード**: Builder Pattern, Dependency Injection, Type Safety, Unit Testing

---

#### [ADR-002: Chain of Responsibility Pattern for Exception Handling](./adr-002-chain-of-responsibility-pattern-for-exception-handling.md)

**Status**: ✅ Accepted  
**概要**: 例外処理の中央集権的な `switch` 文を排除し、拡張性（OCP）と保守性（SRP）の高い Chain of Responsibility パターンに基づくミドルウェアのアーキテクチャを採用しました。  
**キーワード**: Chain of Responsibility, OCP, Pipeline, Extensibility

---

#### [ADR-003: Standardizing Error Responses with RFC 7807 ProblemDetails](./adr-003-standardizing-error-responses-with-rfc7807-problemdetails.md)

**Status**: ✅ Accepted  
**概要**: 全てのAPIエラー形式を RFC 7807 (ProblemDetails) 標準に統一し、デバッグトレース用の `TraceId` と国際化用の `ErrorCode` を拡張属性として必須化しました。  
**キーワード**: RFC 7807, ProblemDetails, Error Handling, Observability

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとセキュア設計の理解用
1. **ADR-003**: なぜエラーの形式がこのJSONオブジェクトとして返されるのか、ログ（TraceId）とどう連携しているのかを理解したいクライアント・フロントエンド開発者向け。
2. **ADR-002**: ドメイン固有の例外や外部システムの特殊な例外が追加された際に、どのように共通基盤へ自作例外ハンドラを組み込むのかを理解したいバックエンド開発者向け。
3. **ADR-001**: 共通ライブラリを設計する際、DI コンテナへの拡張メソッドの提供はどうあるべきか（IXXXBuilderパターン）について設計方針を学ぶアーキテクト向け。

## 🔗 関連ドキュメント

- `ExceptionHandling_20260316.md` (アーキテクチャ監査レポート)

**Last Updated**: 2026-03-17
**Total ADRs**: 3
