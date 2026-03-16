# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Observability モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture (コアアーキテクチャ)

#### [ADR-001: Separation of Concerns: Core Observability vs. OpenTelemetry Integration](./adr-001-separation-of-concerns-core-observability-vs-opentelemetry.md)

**Status**: ✅ Accepted  
**概要**: オブザーバビリティの標準API（`System.Diagnostics`）と送信用SDK（`OpenTelemetry`）のプロジェクトを物理的に分離し、コア層からインフラへの依存を排除するアーキテクチャ  
**キーワード**: Clean Architecture, Zero Dependency, Vendor Lock-in Avoidance

---

### Component Lifecycle & Dependency Injection (ライフサイクルとDI管理)

#### [ADR-002: Do Not Register Static ActivitySource/Meter in DI Container](./adr-002-do-not-register-static-activitysourcemeter-in-di-container.md)

**Status**: ✅ Accepted  
**概要**: `ActivitySource` と `Meter` は `static` フィールドとして直接アクセスさせ、寿命管理のミスマッチやパフォーマンスオーバーヘッドを防ぐためDIコンテナには登録しない  
**性能向上**: DI Allocation & Resolution Overhead Exclusion  
**キーワード**: Performance Optimization, Singleton, IHostApplicationLifetime

---

#### [ADR-003: Adopt Wildcard Telemetry Registration and Remove Common Diagnostic Class](./adr-003-adopt-wildcard-telemetry-registration.md)

**Status**: ✅ Accepted  
**概要**: 共通の `Diagnostic` クラスを廃止し、OpenTelemetry の監視登録をワイルドカード（`"VK.Blocks.*"`）で行うことで、各モジュールの自律的なテレメトリ発行（ゼロコンフィグレーション）を実現  
**キーワード**: OCP, Wildcard Registration, Zero Configuration

---

### Data Propagation & Error Handling (データ伝播とエラーハンドリング)

#### [ADR-004: Automatic Trace Context Propagation Bridge (Result to Activity)](./adr-004-automatic-trace-context-propagation-bridge.md)

**Status**: ✅ Accepted  
**概要**: `Result<T>` パターンで返されたビジネスロジックのエラー情報を、自動的に `Activity`（Span）のタグやイベントとしてマッピングするブリッジ拡張メソッドを導入  
**キーワード**: Result Pattern, Tracing Context, Automatic Mapping

---

### Security & Privacy Compliance (セキュリティとプライバシー保護)

#### [ADR-005: Protection of PII (Personally Identifiable Information) in Telemetry](./adr-005-protection-of-pii-in-telemetry.md)

**Status**: ✅ Accepted  
**概要**: トレースのユーザーメタデータ収集において、ユーザー名などの個人情報（PII）はデフォルトで無効（`false`）とし、設定による明示的なオプトインを要求するSecure by Default設計  
**キーワード**: PII Protection, Secure by Default, Opt-in Data Collection

---

## 🎯 ADR の読み方ガイド

### アーキテクチャとセキュア設計の理解用

1. **ADR-001**: モジュール分割のクリーンアーキテクチャ境界と、特定ベンダー（OpenTelemetry）に依存しない設計思想
2. **ADR-005**: 稼働時の監視データから個人情報漏洩事故を防ぐための、フレームワークレベルでの防御的設計

### パフォーマンスと自動化の理解用

1. **ADR-002**: 高頻度で呼ばれるテレメトリ周辺での、DIコンテナを使わないことによる最適化アプローチ
2. **ADR-003**: OCP（開放閉鎖の原則）に従い、構成の追加無しに新規モジュールの監視を始める全体構造
3. **ADR-004**: 開発者が毎回トレースへのエラー書き込みを手動で行う手間と漏れを防ぐ、シームレスな統合手法

---

## 🔗 関連ドキュメント

- [Architecture Audit Report](/docs/04-AuditReports/Observability/Observability_20260311.md) - 包括的なアーキテクチャ評価

---

**Last Updated**: 2026-03-11  
**Total ADRs**: 5
