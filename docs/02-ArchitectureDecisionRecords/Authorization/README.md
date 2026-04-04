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

---

### Extensibility & Declarative Design (拡張性と宣言的設計)

#### [ADR-003: Adopt Attribute Evaluator Pattern for Extensible Custom Requirements](./adr-003-adopt-attribute-evaluator-pattern-for-extensible-custom-requirements.md)

**Status**: ✅ Accepted  
**概要**: `IAttributeEvaluator` と `DynamicRequirement` を用いて、カスタム認可条件（役職、業務時間など）を属性メタデータ駆動で一元評価するパターン  
**キーワード**: Attribute Evaluator, Declarative Design, Metadata-Driven

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

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとセキュア設計の理解用

1. **ADR-001**: マルチテナントシステムにおける認可境界（Authorization Boundary）の設計思想
2. **ADR-002**: エンタープライズレベルでの権限ベースアクセス制御（PBAC）におけるポリシーの動的生成アプローチ
3. **ADR-003**: 宣言的でクリーンなコントローラーを保つための評価器（Evaluator）パターンの実装

### 開発体験(DX)と自動化の理解用

1. **ADR-004**: マジックストリングや手動DI登録の排除によるフェイルセーフの確立と、Source Generator の実践的導入
2. **ADR-005**: 抽象から具体（IPermissionEvaluator）へのリファクタリングと、自動化を維持しながらアーキテクチャの純度を高める手法

---

## 🔗 関連ドキュメント

- [Architecture Audit Report](/docs/04-AuditReports/Authorization/Authorization_20260306.md) - 包括的なアーキテクチャ評価

---

**Last Updated**: 2026-04-02  
**Total ADRs**: 5
