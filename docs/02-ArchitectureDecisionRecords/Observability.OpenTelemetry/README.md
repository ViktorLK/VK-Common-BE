# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Observability.OpenTelemetry モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

#### [ADR-001: Establish Environment Variable Abstraction for Testability in OpenTelemetry SDK](/docs/02-ArchitectureDecisionRecords/Observability.OpenTelemetry/adr-001-establish-environment-variable-abstraction-for-testability-in-opentelemetry-sdk.md)

**Status**: ✅ Accepted  
**概要**: テスト容易性を確保するため、OpenTelemetry におけるクラウドリソース自動検出を行う際に `System.Environment` への直接アクセスを排除し、`IEnvironmentProvider` による抽象化（Dependency Inversion）を導入する設計決定。  
**キーワード**: Testability, Method Injection, Abstraction, OpenTelemetry SDK

---

#### [ADR-002: Adopt Fluent Builder Pattern for SDK Initialization](/docs/02-ArchitectureDecisionRecords/Observability.OpenTelemetry/adr-002-adopt-fluent-builder-pattern-for-sdk-initialization.md)

**Status**: 📝 Draft  
**概要**: OpenTelemetryの初期化設定において、柔軟なオプトイン制御と内部複雑性の隠蔽を実現するため、拡張メソッドによる直接構成からFluent Builderパターン（`VkObservabilityBuilder`）へ移行する設計決定。  
**キーワード**: Fluent Builder, SDK Initialization, Separation of Concerns

---

#### [ADR-003: Implement Custom Cloud Resource Detection Strategy](/docs/02-ArchitectureDecisionRecords/Observability.OpenTelemetry/adr-003-implement-custom-cloud-resource-detection-strategy.md)

**Status**: 📝 Draft  
**概要**: AzureやKubernetesなど多様な環境間で一貫したリソース属性を付与し、外部パッケージへの依存を減らすため、標準Detectorに頼らず環境変数ベースのカスタムリソース検出機構を導入する設計決定。  
**キーワード**: Resource Detection, Cloud Provider, Standardization

---

#### [ADR-004: Enforce ParentBased Sampling as Default Strategy](/docs/02-ArchitectureDecisionRecords/Observability.OpenTelemetry/adr-004-enforce-parentbased-sampling-as-default-strategy.md)

**Status**: 📝 Draft  
**概要**: 分散トレーシングにおけるデータ量制御とトレース連続性の両立を図るため、全マイクロサービスで`ParentBasedAlwaysOn`サンプリング戦略を既定の動作として強制する設計決定。  
**キーワード**: Sampling Strategy, ParentBased, Trace Continuity

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとセキュア設計の理解用

1. **ADR-001**: ユニットテスト時におけるOS環境変数等との強い結合（Hard Dependency）を、インターフェースによる抽象化でどう解決するか（関心事の分離）を理解するために一読を推奨します。
2. **ADR-002**: 複雑なSDKの初期化ロジックをFluent APIによっていかに隠蔽し、型安全な構成オプションを提供するかの設計プラクティスを理解するために役立ちます。
3. **ADR-003**: クラウド環境のリソース自動検出において、標準ライブラリ（外部依存）と独自実装（軽量化・標準化）のトレードオフをどう評価したかを示す事例です。
4. **ADR-004**: マイクロサービス全体での過剰なログ量制御（コスト管理）とトレーサビリティの確保を両立させる、サンプリング戦略の実践的アプローチを概説しています。

## 🔗 関連ドキュメント

- [Architecture Audit Report (2026-03-12)](/docs/04-AuditReports/Observability.OpenTelemetry/Observability.OpenTelemetry_20260312.md)

---

**Last Updated**: 2026-03-12  
**Total ADRs**: 4
