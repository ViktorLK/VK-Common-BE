# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Authorization モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture (コアアーキテクチャ)

#### [ADR-001: Centralize Tenant Isolation in Authorization Pipeline](./adr-001-centralize-tenant-isolation-in-authorization-pipeline.md)

**Status**: ✅ Accepted  
**概要**: Authorization Pipeline 内に `TenantAuthorizationHandler` を導入し、テナント横断のデータ漏洩を防ぐゼロトラスト境界を確立  
**キーワード**: Tenant Isolation, Zero-Trust, Authorization Pipeline

#### [ADR-002: Establish Dynamic Policy Generation for Permission-Based Authorization](./adr-002-establish-dynamic-policy-generation-for-permission-based-authorization.md)

**Status**: ✅ Accepted  
**概要**: `IAuthorizationPolicyProvider` の拡張ポイントを活用し、静的登録に依存せず、実行時にポリシーを動的生成（On-the-fly）するアーキテクチャ  
**性能向上**: 起動パフォーマンスのペナルティ排除（Zero-Registration）  
**キーワード**: Dynamic Policy, PBAC, Startup Optimization

#### [ADR-011: Refactor Authorization Building Block Structure and Naming](./adr-011-refactor-authorization-building-block-structure-and-naming.md)

**Status**: ✅ Accepted  
**概要**: VK.Blocks 標準規約への準拠のため、名前空間のフラット化、VK プレフィックスの適用、および DI 登録パターンの標準化を実施  
**キーワード**: Refactoring, Naming Convention, Namespace Flattening, AP.02

#### [ADR-012: Authorization Library Architecture Normalization](./adr-012-authorization-library-architecture-normalization.md)

**Status**: ✅ Accepted  
**概要**: VK.Blocks 標準規約 (BB.01–BB.05) への完全準拠のため、ディレクトリ構造の再編、`IVKBlockMarker` の実装、およびカプセル化の深化を実施  
**キーワード**: Blueprint, IVKBlockMarker, Internal Isolation, BB.01

#### [ADR-013: Feature-Sliced Modular Registration Pattern](./adr-013-feature-sliced-modular-registration-pattern.md)

**Status**: ✅ Accepted  
**概要**: 認可機能を垂直スライスごとに分割し、BB.03 に準拠した個別の登録ロジックと Builder による fluent API を提供するアーキテクチャ  
**キーワード**: Modular Registration, BB.03, IVKAuthorizationBuilder, Encapsulation

#### [ADR-016: Authorization Block Normalization and Core Protocol Standardization](./adr-016-authorization-block-normalization-and-core-protocol-standardization.md)

**Status**: ✅ Accepted  
**概要**: 認可ブロックの全機能を AP.05 に適合させ、ドメインセマンティクスを優先したエバリュエータ設計と Core Protocols 規格を確立  
**キーワード**: Normalization, AP.05, Semantic-Priority, Protocols

---

### Extensibility & Declarative Design (拡張性と宣言的設計)

#### [ADR-003: Adopt Attribute Evaluator Pattern for Extensible Custom Requirements](./adr-003-adopt-attribute-evaluator-pattern-for-extensible-custom-requirements.md)

**Status**: ✅ Accepted  
**概要**: `IAttributeEvaluator` と `DynamicRequirement` を用いて、カスタム認可条件（役職、業務時間など）を属性メタデータ駆動で一元評価するパターン  
**キーワード**: Attribute Evaluator, Declarative Design, Metadata-Driven

#### [ADR-006: Modernize Dynamic Policy Evaluation with IAuthorizationRequirementData](./adr-006-iauthorizationrequirementdata.md)

**Status**: ✅ Accepted  
**概要**: .NET 8+ の `IAuthorizationRequirementData` を採用し、属性ベースの動的認可における複雑な文字列解析（PolicyProvider）を排除  
**キーワード**: ABAC, IAuthorizationRequirementData, Result Pattern, Modernization

#### [ADR-015: Tenant-Level Capability Authorization (Entitlements)](./adr-015-tenant-level-capability-authorization-entitlements.md)

**Status**: ✅ Accepted  
**概要**: テナントの契約プランや機能フラグに基づく認可 (Entitlements) を導入し、ユーザー権限とは別の「システム機能の利用可否」を属性ベースで制御  
**キーワード**: Entitlements, Tenant Capability, Declarative Security, SaaS

---

### Automation & DX (自動化と開発体験)

#### [ADR-004: Introduce Source Generators for Authorization Handlers and Permissions Catalog](./adr-004-introduce-source-generators-for-authorization-handlers-and-permissions-catalog.md)

**Status**: ✅ Accepted  
**概要**: C# Source Generators（`IIncrementalGenerator`）を導入し、ハンドラーのDI登録や権限カタログの定数クラス生成をコンパイル時に自動化  
**キーワード**: Source Generators, Compile-Time Safety, Boilerplate Reduction

#### [ADR-005: Refactor Authorization Evaluation to Specialized Permission Evaluator Pattern and Retain Automation](./adr-005-refactor-authorization-evaluation-to-specialized-permission-evaluator-pattern-and-retain-automation.md)

**Status**: ✅ Accepted  
**概要**: 汎用的な `IVKAuthorizationHandler` を廃止し、Result Pattern を採用した `IPermissionEvaluator` へ移行。同時に自動登録 SG を新契約に適合。  
**キーワード**: Result Pattern, Specialization, Low Friction, Vertical Slice

#### [ADR-007: Definition-First Permission Discovery Strategy](./adr-007-definition-first-permission-discovery-strategy.md)

**Status**: ✅ Accepted  
**概要**: 定数クラスからメタデータ（表示名・説明）を抽出し、強タイプ属性を自動生成する「定義優先」のスキャン戦略  
**キーワード**: Source Generators, Metadata-First, Compile-Time Safety

#### [ADR-008: Hash-Based Metadata Synchronization for Startup Performance](./adr-008-hash-based-metadata-synchronization.md)

**Status**: ✅ Accepted  
**概要**: メタデータの指紋（Hash）を比較することで、起動時の冗長な DB 同期処理をスキップし、起動速度を劇的に向上させる最適化  
**キーワード**: Startup Performance, FNV-1a Hashing, Differential Sync

#### [ADR-009: Permission Naming and Isolation Strategy](./adr-009-permission-naming-and-isolation-strategy.md)

**Status**: ✅ Accepted  
**概要**: 属性生成時に Module プレフィックスを強制し、未管理権限に Misc を付与することで、大規模統合時の命名競合を根絶する治理戦略  
**キーワード**: Namespacing, Governance, Conflict Prevention

#### [ADR-010: Standardization of Authorization Vertical Slice Structure and Delegated Registration](./adr-010-standardization-of-authorization-vertical-slice-structure-and-delegated-registration.md)

**Status**: ✅ Accepted  
**概要**: 各機能の構造を [Root/Metadata/Internal] に標準化し、DI 登録を中央から各機能へ委譲することで、カプセル化と保守性を向上させるアーキテクチャ  
**キーワード**: Vertical Slice, Encapsulation, Delegated Registration, Metadata Separation

#### [ADR-014: Automated Block Identity & Metadata via Source Generation](./adr-014-automated-block-identity--metadata-via-source-generation.md)

**Status**: ✅ Accepted  
**概要**: `IVKBlockMarker` を起点とした Source Generator により、モジュール識別子 (URN) とバージョン情報を自動合成し、テレメトリの一貫性と型安全性を確保  
**キーワード**: Source Generators, Metadata Synthesis, Type Safety, IVKBlockMarker

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとセキュア設計の理解用

1. **ADR-016**: 認可ブロックの最新の標準化（AP.05）と Core Protocols による横断的な規格設計
2. **ADR-012**: モジュールの標準ディレクトリ構造 (Blueprint) とマーカーパターンによる自動化の基礎
3. **ADR-013**: 機能ごとの独立した登録フロー (BB.03.2) と Builder による拡張性の確保
4. **ADR-015**: テナントレベルの機能認可 (Entitlements) による多次元のアクセス制御
5. **ADR-011**: 初期のリファクタリングと命名規則の統一（歴史的経緯）
6. **ADR-001**: マルチテナントシステムにおける認可境界（Authorization Boundary）の設計思想
7. **ADR-002**: エンタープライズレベルでの権限ベースアクセス制御（PBAC）におけるポリシーの動的生成アプローチ
8. **ADR-003**: 宣言的でクリーンなコントローラーを保つための評価器（Evaluator）パターンの実装
9. **ADR-006**: 最新の .NET 機能を活用した、複雑さを排除した ABAC アーキテクチャの実現

### 開発体験(DX)と自動化の理解用

1. **ADR-014**: マーカーパターンと Source Generator による識別子管理の自動化
2. **ADR-004**: ママジックストリングや手動DI登録の排除によるフェイルセーフの確立と、Source Generator の実践的導入
3. **ADR-007**: 権限のメタデータ管理と強タイプ属性生成による「コンパイラを味方につける」開発手法
4. **ADR-009**: 多人数開発における権限の命名競合をシステマティックに防ぐ方法
5. **ADR-010**: 複雑化したモジュールの保守性を維持するための、垂直スライスの標準化と登録の委譲手法

### 性能と信頼性の理解用

1. **ADR-008**: 起動時のオーバーヘッドを最小限に抑えつつ、コードと DB の同期を整合させるための指紋同期テクニック

---

## 🔗 関連ドキュメント

- [Architecture Audit Report](/docs/04-AuditReports/Authorization/Authorization_20260409.md) - 包括的なアーキテクチャ評価

---

**Last Updated**: 2026-05-05  
**Total ADRs**: 16


