# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Authentication モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture & Refactoring (コアアーキテクチャとリファクタリング)

#### [ADR-006: Refactor Authentication Core for Zero-Dependency and InMemory Defaults](./adr-006-refactor-authentication-core-for-zero-dependency.md)

**Status**: ✅ Accepted  
**概要**: 認証コアパッケージから `IDistributedCache` 等の外部依存を完全排除し、単一ノード向けの高速な `InMemory` 実装をデフォルト化して、分散環境向けロジックを専用の Redis 拡張パッケージへ分離する  
**キーワード**: Zero-Dependency, Modularity, Separation of Concerns (SoC), InMemory Cache

#### [ADR-007: Optimize High-Frequency Auth Interfaces with ValueTask](./adr-007-optimize-high-frequency-auth-interfaces-with-valuetask.md)

**Status**: ✅ Accepted  
**概要**: リクエストごとに高頻度で実行されるプロバイダーインターフェイス（Rate Limiter 等）の戻り値を `ValueTask` に変更し、同期完了時のヒープアロケーションをゼロにして GC 負荷を下げる  
**キーワード**: Performance, Zero-Allocation, ValueTask, High-Traffic Optimization

#### [ADR-001: Remove Dead Code and Unused Abstractions](./adr-001-remove-dead-code-and-unused-abstractions.md)

**Status**: 📝 Draft  
**概要**: 未使用の `AuthResult` クラスおよびコメントアウトされた不要な DI 登録を削除し、コードベースのノイズを排除する  
**キーワード**: Dead Code Elimination, Clean Code

#### [ADR-008: Centralized Identity Claims Resolution as SSOT](./adr-008-centralized-identity-claims-resolution-as-ssot.md)

**Status**: ✅ Accepted  
**概要**: 認証済みユーザーの属性（UserId, Roles等）抽出ロジックを `ClaimsPrincipalExtensions` に一元化し、`VKClaimTypes` を単一の真実の情報源 (SSOT) として確立する  
**キーワード**: SSOT, Claims, Identity Parsing, Clean Architecture

#### [ADR-009: Strict Eager Validation for Authentication Configuration](./adr-009-strict-eager-validation-for-authentication-configuration.md)

**Status**: ✅ Accepted  
**概要**: 認証構成クラスから nullable プロパティを排除し、`IValidateOptions<T>` と `.ValidateOnStart()` を用いて起動時の防御的検証 (Fail-Fast) を強制する  
**キーワード**: Eager Validation, Fail-Fast, Configuration, IValidateOptions

#### [ADR-010: Dynamic Dictionary-Driven OAuth Configuration](./adr-010-dynamic-dictionary-driven-oauth-configuration.md)

**Status**: ✅ Accepted  
**概要**: ハードコードされていた OAuth プロバイダープロパティを辞書形式にリファクタリングし、設定駆動型の動的 DI 登録を実現することで OCP に関する技術的負債を解消する  
**キーワード**: OCP, Dictionary-Driven, Dynamic Configuration, OAuth, Keyed Services

#### [ADR-012: Authentication Module Standardization and Idempotent DI Registration](./adr-012-authentication-module-standardization.md)

**Status**: ✅ Accepted  
**概要**: DI登録のべき等性確保（TryAddへの移行）、スキーム名の抽象化、および登録パイプラインのフェーズ分離により、モジュールの再利用性と拡張性を向上させる  
**キーワード**: Idempotency, Dependency Injection, Scheme Abstraction, Pipeline Ordering

#### [ADR-013: Autonomous Lifecycle Management for Cleanup Tasks](./adr-013-autonomous-cleanup-lifecycle-management.md)

**Status**: ✅ Accepted  
**概要**: インフラ構成（In-Memory vs Distributed）を自律的に検知し、不要なクリーンアップ用バックグラウンドタスクを自己停止（Hard Exit）させることでリソースを最適化する  
**キーワード**: Self-Adaptive, Background Service, Resource Optimization, Lifecycle Management

#### [ADR-018: Standardization of Authentication Opt-in Model and Idempotent Validation Patterns](./adr-018-standardization-of-authentication-opt-in-model-and-idempotent-validation-patterns.md)

**Status**: ✅ Accepted  
**概要**: 認証ライブラリをデフォルト無効（Enabled: false）のオプトイン方式へ移行し、IValidateOptions と TryAddEnumerable を用いた厳格かつべき等なバリデーション構成を標準化する  
**キーワード**: Explicit Opt-in, Fail-Fast, Idempotency, Validation Patterns

#### [ADR-019: Decoupling Authentication and Authorization via Dependency Inversion](./adr-019-decoupling-authentication-and-authorization-via-dependency-inversion.md)

**Status**: ✅ Accepted  
**概要**: Authentication ブロックから認可ライブラリへの直接依存を排除し、`ISemanticSchemeProvider` を介した依存性の逆転 (Dependency Inversion) によって認証と認可を疎結合化する  
**キーワード**: Dependency Inversion, Decoupling, Semantic Policies, Pure Module

#### [ADR-020: Adoption of Semantic Authentication Schemes for Group-Based Authorization](./adr-020-adoption-of-semantic-authentication-schemes-for-group-based-authorization.md)

**Status**: ✅ Accepted  
**概要**: 技術的な認証スキーム（JWT, ApiKey等）を「User」「Service」「Internal」というセマンティックなグループに分類し、認可ポリシーとの疎結合化を実現する  
**キーワード**: Semantic Groups, Abstraction, Authorization Coupling, IVKSemanticSchemeProvider

#### [ADR-021: Zero-Reflection Auto-Discovery of Authentication Extensions via Source Generators](./adr-021-zero-reflection-auto-discovery-of-authentication-extensions-via-source-generators.md)

**Status**: ✅ Accepted  
**概要**: 実行時のアセンブリ走査とリフレクションを排除し、Source Generator を用いてコンパイル時に認証プロバイダーを自動検出・登録することで起動性能と Native AOT 対応を向上させる  
**キーワード**: Zero-Reflection, Source Generators, Native AOT, Startup Performance

#### [ADR-023: Modular Feature-Sliced Registration Pattern for Authentication Sub-Blocks](./adr-023-modular-feature-sliced-registration-pattern-for-authentication-sub-blocks.md)

**Status**: ✅ Accepted  
**概要**: JWT, ApiKey, OAuth 等の各機能を独立した垂直スライスとして管理し、Feature 単位でのカプセル化された DI 登録とビルダーパターンによる宣言的なオプトインを実現する  
**キーワード**: Vertical Slice, Feature-Sliced, DI Orchestration, Modular Design

---

### Standards & Compliance (標準とコンプライアンス)

#### [ADR-002: RFC 7807 Compliant Error Responses in Authentication Pipeline](./adr-002-rfc-7807-compliant-error-responses.md)

**Status**: 📝 Draft  
**概要**: 認証パイプラインでの 401 Unauthorized エラーを RFC 7807 (Problem Details) 形式に統一し、運用監視用の `TraceId` を付与する  
**キーワード**: RFC 7807, Problem Details, HTTP API, Observability

#### [ADR-022: Standardization of Building Block API Surface via VK-Prefixing](./adr-022-standardization-of-building-block-api-surface-via-vk-prefixing.md)

**Status**: ✅ Accepted  
**概要**: パブリック API 層の全型に `VK` プレフィックスを付与することを義務付け、名前空間の衝突回避、識別性の向上、およびライブラリとしてのブランド一貫性を確保する  
**キーワード**: Naming Convention, Branding, Namespace Safety, Public API

---

### Security & Resilience (セキュリティと耐障害性)

#### [ADR-003: Consolidate Token Blacklist Check Responsibility](./adr-003-consolidate-token-blacklist-check-responsibility.md)

**Status**: 📝 Draft  
**概要**: JWT リクエストにおけるトークン失効（Blacklist）チェックを `JwtBearerEventsFactory.OnTokenValidated` に一元化し、冗長な分散キャッシュ I/O を排除  
**キーワード**: Security, JWT Revocation, Caching Optimization

#### [ADR-004: Comprehensive Refactoring of Authentication Module (Phase 1-3.5)](./adr-004-comprehensive-refactoring-of-authentication-module-phase-1-35.md)

**Status**: ✅ Accepted  
**概要**: 認証モジュールの全面的なリファクタリング（エラー定数の集約、Token/API Key の Blacklist 分離、Source Generator を用いた可観測性の自動化）  
**キーワード**: Refactoring, Tech Debt, Observability, Separation of Concerns (SoC)

#### [ADR-005: Introduce Atomic Rate Limiting for API Keys](./adr-005-introduce-atomic-rate-limiting-for-api-keys.md)

**Status**: 📝 Draft  
**概要**: 並行リクエスト下での競合状態（Race Condition）を防ぐため、Redis の Lua スクリプト (`INCR` + `EXPIRE`) を用いたアトミックな API Key レートリミッターを導入  
**キーワード**: Rate Limiting, Atomic Operations, Redis, Lua Script, Race Condition

#### [ADR-011: Dual Mode JWT Validation Strategy](./adr-011-dual-mode-jwt-validation-strategy.md)

**Status**: ✅ Accepted  
**概要**: 対称鍵 (Symmetric) と OIDC ディスカバリーのハイブリッドなトークン認証をサポートするため、構成によって署名鍵の検証方式を動的に切り替えるアプローチを採用する  
**キーワード**: JWT, OIDC, Asymmetric Encryption, Security Strategy

#### [ADR-014: Strategic Roadmap for Semantic Auth Attributes and Enterprise Features](./adr-014-semantic-auth-attributes-roadmap.md)

**Status**: ✅ Accepted  
**概要**: マジックストリングを排除するセマンティック属性の導入、および「全デバイスログアウト」「マルチ租戸隔離」「セキュリティ監査」といったエンタープライズ機能への戦略的ロードマップ  
**キーワード**: Semantic Attributes, Scoped API Keys, Multi-Tenant Isolation, Security Observability, Session Revocation

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとリファクタリングの理解用

1. **ADR-004 / ADR-012**: Authentication モジュール全体的な進化の方向性と、DI登録の標準化による堅牢な基盤構築。
2. **ADR-020**: 技術とビジネスセマンティクスを分離する高度な抽象化設計。
3. **ADR-021**: Source Generator による Native AOT 最適化と Zero-Reflection 戦略。
4. **ADR-023**: 複雑な認証基盤を保守可能にする垂直スライス型設計。

### セキュリティ強化とエンタープライズ機能の理解用

1. **ADR-014**: 単なる認証の実装を超え、セッション管理、権限管理、多租戸対応といった高度なセキュリティ要件への対応方針。
2. **ADR-011 / ADR-003**: 柔軟な検証戦略と効率的な失効チェックのトレードオフ。

### API 標準化の理解用

1. **ADR-022**: ライブラリとしてのプロフェッショナルな命名規則とブランド一貫性の維持。

---

## 🔗 関連ドキュメント

- [Architecture Audit Report](/docs/04-AuditReports/Authentication/Authentication_20260305.md) - 包括的なアーキテクチャ評価

---

**Last Updated**: 2026-04-24  
**Total ADRs**: 23
