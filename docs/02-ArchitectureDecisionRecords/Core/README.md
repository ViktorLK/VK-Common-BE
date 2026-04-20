# ADR Index — Core

このディレクトリには、`VK.Blocks.Core` モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

#### [ADR-001: Expanding Global ErrorType for Enterprise-Grade Error Handling](/docs/02-ArchitectureDecisionRecords/Core/adr-001-expanding-global-errortype-for-enterprise-grade-error-handling.md)

**Status**: ✅ Accepted  
**概要**: 全局 `ErrorType` 枚举を拡張し、429, 503, 502/504 等の工業級 HTTP マッピングをサポート。  
**キーワード**: Result Pattern, RESTful, Enterprise Error Handling

---

#### [ADR-002: Idempotent BuildingBlock Options Registration in DI Container](/docs/02-ArchitectureDecisionRecords/Core/adr-002-idempotent-buildingblock-options-registration-in-di-container.md)

**Status**: ✅ Accepted  
**概要**: `AddVKBlockOptions` において、`Any()` チェックと `TryAddSingleton` を組み合わせ、重複した検証ロジックと DI 記述子の登録を防止。  
**キーワード**: Idempotency, DI Container, Startup Performance

---

#### [ADR-003: Introducing Service Marker Pattern for BuildingBlock Modularization](/docs/02-ArchitectureDecisionRecords/Core/adr-003-introducing-service-marker-pattern-for-buildingblock-modularization.md)

**Status**: ✅ Accepted  
**概要**: オプション型に基づいた間接的な依存性チェックを廃止し、`AddVKBlockMarker<T>` を用いたセマンティックなサービス登録確認を導入。  
**キーワード**: Service Marker, Modularity, Dependency Validation

---

#### [ADR-004: Unified Synchronization State Abstraction in Core](/docs/02-ArchitectureDecisionRecords/Core/adr-004-unified-synchronization-state-abstraction-in-core.md)

**Status**: ✅ Accepted  
**概要**: メタデータ同期用のハッシュ指紋を管理する `ISyncStateStore` 抽象を導入。全 Building Block で一貫した同期プロトコルを確立。  
**キーワード**: Synchronization, Abstraction, Idempotency, No-Op Fallback

---
#### [ADR-005: Use Static Abstract Interface for Configuration Section Resolution](./adr-005-use-static-abstract-interface-for-configuration-section-resolution.md)

**Status**: ✅ Accepted  
**概要**: `IVKBlockOptions` に `static abstract SectionName` を導入し、リフレクションなしでの構成セクション自動解決を実現。  
**キーワード**: Static Abstract, Configuration, Zero-Reflection

---

#### [ADR-006: Establishing Granular Capability-Based Structure and Namespace Alignment in Core](./adr-006-restructuring-core-module-into-semantic-pillars.md)

**Status**: ✅ Accepted  
**概要**: 大雑把な「5つの柱」への集約案を却下し、厳密な機能・責務単位での高解像度なアプローチ（12のCapabilityエリア）とフォルダ・Namespaceの1対1対応を採用。  
**キーワード**: Refactoring, Capability Boundaries, Explicit Namespaces, Anti-Utility-Bucket

---

#### [ADR-007: Unified BuildingBlock Identification and Source-Generated Zero-Reflection Validation](./adr-007-unified-buildingblock-identification-and-source-generated-zero-reflection-validation.md)

**Status**: ✅ Accepted  
**概要**: プロパティを `IVKBlockMarker` に集約し、Source Generator による `Instance` シングルトン注入を導入。再帰的な依存関係検証をリフレクションなしで高速化。  
**キーワード**: Source Generator, Singleton Pattern, Zero-Reflection, Dependency Tree
#### [ADR-008: Robust BuildingBlock Dependency Validation through Pre-order Traversal and Cycle Detection](./adr-008-robust-building-block-dependency-validation.md)

**Status**: ✅ Accepted  
**概要**: 親の検証を先行させる Pre-order 走査と、`HashSet` による循環参照検知を導入。依存関係エラーの特定を容易にし、`StackOverflow` を防止。  
**キーワード**: Pre-order Traversal, Circular Dependency, Cycle Detection

---

**Last Updated**: 2026-04-20  
**Total ADRs**: 8
