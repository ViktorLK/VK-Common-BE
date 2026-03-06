# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Authentication モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture & Refactoring (コアアーキテクチャとリファクタリング)

#### [ADR-001: Remove Dead Code and Unused Abstractions](./adr-001-remove-dead-code-and-unused-abstractions.md)

**Status**: 📝 Draft  
**概要**: 未使用の `AuthResult` クラスおよびコメントアウトされた不要な DI 登録を削除し、コードベースのノイズを排除する  
**キーワード**: Dead Code Elimination, Clean Code

---

### Standards & Compliance (標準とコンプライアンス)

#### [ADR-002: RFC 7807 Compliant Error Responses in Authentication Pipeline](./adr-002-rfc-7807-compliant-error-responses.md)

**Status**: 📝 Draft  
**概要**: 認証パイプラインでの 401 Unauthorized エラーを RFC 7807 (Problem Details) 形式に統一し、運用監視用の `TraceId` を付与する  
**キーワード**: RFC 7807, Problem Details, HTTP API, Observability

---

### Security & Resilience (セキュリティと耐障害性)

#### [ADR-003: Consolidate Token Blacklist Check Responsibility](./adr-003-consolidate-token-blacklist-check-responsibility.md)

**Status**: 📝 Draft  
**概要**: JWT リクエストにおけるトークン失効（Blacklist）チェックを `JwtBearerEventsFactory.OnTokenValidated` に一元化し、冗長な分散キャッシュ I/O を排除  
**性能向上**: 分散キャッシュへの I/O コスト削減  
**キーワード**: Security, JWT Revocation, Caching Optimization

#### [ADR-004: Comprehensive Refactoring of Authentication Module (Phase 1-3.5)](./adr-004-comprehensive-refactoring-of-authentication-module-phase-1-35.md)

**Status**: ✅ Accepted  
**概要**: 認証モジュールの全面的なリファクタリング（エラー定数の集約、Token/API Key の Blacklist 分離、Source Generator を用いた可観測性の自動化）  
**キーワード**: Refactoring, Tech Debt, Observability, Separation of Concerns (SoC)

#### [ADR-005: Introduce Atomic Rate Limiting for API Keys](./adr-005-introduce-atomic-rate-limiting-for-api-keys.md)

**Status**: 📝 Draft  
**概要**: 並行リクエスト下での競合状態（Race Condition）を防ぐため、Redis の Lua スクリプト (`INCR` + `EXPIRE`) を用いたアトミックな API Key レートリミッターを導入  
**キーワード**: Rate Limiting, Atomic Operations, Redis, Lua Script, Race Condition

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとリファクタリングの理解用

1. **ADR-004**: Authentication モジュールの全体的な進化の方向性と、技術的負債（エラーハンドリング、可観測性の欠如）をどのように解消したかのロードマップ。
2. **ADR-001**: 日常的なコードクリーンアップ（破れ窓理論の防犯）の重要性。

### セキュリティ強化と並行処理の理解用

1. **ADR-005**: 高トラフィックな環境下で発生する状態同期バグに対処するための、Redis アトミック操作の高度なユースケース。
2. **ADR-003**: 認証フローにおける防御ポイント（Defense-in-Depth vs DRY）のアーキテクチャ的トレードオフの考察。

### API 標準化の理解用

1. **ADR-002**: 認証レイヤーのようなミドルウェアの深い位置から、エンドポイントと同じ標準エラーレスポンス（Problem Details）を安全に返却する手法。

---

## 🔗 関連ドキュメント

- [Architecture Audit Report](/docs/04-AuditReports/Authentication/Authentication_20260305.md) - 包括的なアーキテクチャ評価

---

**Last Updated**: 2026-03-06  
**Total ADRs**: 5
