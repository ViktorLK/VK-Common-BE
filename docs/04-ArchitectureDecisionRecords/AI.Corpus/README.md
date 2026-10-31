# Architecture Decision Records (ADR) - AI.Corpus Index

このディレクトリには、VK.Blocks.AI.Corpus ナレッジライフサイクル管理モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Lifecycle Pipeline (コアパイプライン)

#### [ADR-001: Three-Phase Knowledge Lifecycle Management Pipeline](./adr-001-three-phase-knowledge-lifecycle-management-pipeline.md)

**Status**: ✅ Accepted  
**概要**: ベクトル検索による動的召回と静的ルール取得を行う「Gathering」、多角的なルール選別を行う「Filtering」、実際に注入された実績を追跡する「Tracking」の3段階ライフサイクルパイプライン設計の確立。  
**キーワード**: Knowledge Sourcing, 3-Phase Lifecycle, Usage Tracking

---

#### [ADR-002: Chain of Filters Architecture for Rule Based Knowledge Selection](./adr-002-chain-of-filters-architecture-for-rule-based-knowledge-selection.md)

**Status**: ✅ Accepted  
**概要**: 17種に及ぶ多様な知識選別ルール（Cooldown、Stickiness等）をそれぞれ単一責任のクラスに分離し、優先度順に並び替えたチェーン形式で順次実行する拡張性の高い選別アーキテクチャ。  
**キーワード**: Chain of Filters, IVKKnowledgeLifecycleFilter, Open-Closed Principle

---

#### [ADR-003: Standardization of Sourcing, Filtering, and Tracking Features](./adr-003-standardization-of-sourcing-filtering-and-tracking-features.md)

**Status**: ✅ Accepted  
**概要**: 命名規則を `AICorpus` へと統一し、曖昧なフィルター名称を実際のセマンティクス（EntryMaxCount、GroupTopN 等）に適合するように刷新。さらにテスト容易性向上のため、オンメモリ追跡ストア `InMemoryKnowledgeInjectionStore` を標準追加する設計。  
**キーワード**: Code Cleanup, Refactoring, Zero-Infrastructure Testing

---

#### [ADR-004: Standardization of Memory Store Boundary Validation and Diagnostics](./adr-004-standardization-of-memory-store-boundary-validation-and-diagnostics.md)

**Status**: ✅ Accepted  
**概要**: インメモリデータストアの引数検証ロジックから例外スローを排除して `VKGuard` 防御へ統一し、Source Generator を用いた `[LoggerMessage]` パターンによる構造化ログと診断用の定数・ロガー機構を追加する設計。  
**キーワード**: Boundary Validation, VKGuard, Structured Logging, DiagnosticsConstants

---

#### [ADR-005: Asynchronous Corpus Ingestion Service and Job Status Engine](./adr-005-asynchronous-corpus-ingestion-service-and-job-status-engine.md)

**Status**: ✅ Accepted  
**概要**: 巨大なコーパスファイルをインポートする際のタイムアウト回避と進捗可視化のため、処理をバックグラウンドへ逃がす非同期インジェストエンジンと、スレッド安全なインメモリ進行状況ストアを提供するアーキテクチャ。  
**キーワード**: Asynchronous Ingestion, Job Status Engine, InMemoryStatusStore, Background Task

---

## 🎯 ADR の読み方ガイド

### ナレッジライフサイクル全体の理解用

1. **ADR-001**: 知識の収集からLLM実行後の永続化追跡まで、データと状態がどのように流れていくかの全体ライフサイクルを理解するために読んでください。
2. **ADR-002**: CooldownやStickinessなど、対話のテンポと状態を制御するフィルターがどのように独立して機能し、結合を避けながら実行されているかを理解するために読んでください。
3. **ADR-003**: 命名の表記揺れ解消の経緯と、モックテストやインフラ不要（InMemory）で追跡機能をテスト可能にした設計を理解するために読んでください。
4. **ADR-004**: 例外の排除による堅牢化と、BuildingBlock 独自の診断機構 (`CorpusDiagnostics`)・構造化ロギングの実装パターンを理解するために読んでください。
5. **ADR-005**: 長時間実行される RAG データのパース・インポート処理を、非同期ワーカープロセスとジョブステータス追跡ストアでどのように切り離しているかを理解するために読んでください。

## 🔗 関連ドキュメント

- [AI.Corpus Module Manifest (Layer 3)](/src/BuildingBlocks/AI.Corpus/README.md)

**Last Updated**: 2026-06-27  
**Total ADRs**: 5
