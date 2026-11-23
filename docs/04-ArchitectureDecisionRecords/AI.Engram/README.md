# Architecture Decision Records (ADR) - AI.Engram Index

このディレクトリには、VK.Blocks.AI.Engram 長期記憶およびコンテキストエンジニアリングエンジンの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### 3-Layer Memory Engine Architecture (3 層記憶エンジン)

#### [ADR-001: L0 Immutable Episode Store and Memory Strategy Separation](./adr-001-memory-consolidation-scoring-and-decay-strategies-for-long-term-dialogue.md)
**Status**: ✅ Accepted  
**概要**: 会話やツールの生ログを不変の L0 追記専用ログ（Episode）として保持し、各記憶戦略を疎結合に分離する設計。

---

#### [ADR-002: Tiered Chat History Compression and Separation of Forgetting vs. Erasure](./adr-002-tiered-chat-history-compression-and-eviction.md)
**Status**: ✅ Accepted  
**概要**: 定常的な刈り込み（Pruning）はホットベクトルインデックスの退避のみに限定し、GDPR/APPI による物理消去と明確に分離する多階層圧縮・退避設計。

---

#### [ADR-003: Asynchronous Strategy-Based Cognitive Memory Compression](./adr-003-asynchronous-strategy-based-cognitive-memory-compression.md)
**Status**: ✅ Accepted  
**概要**: リクエストスレッドをブロックしない非同期ジョブキューと、多様な圧縮アプローチのポリモーフィック切り替え設計。

---

#### [ADR-004: Single-Pass LLM Extraction, Scoring, and Contradiction Arbitration with Eidos](./adr-004-single-pass-eidos-extraction-and-scoring.md)
**Status**: ✅ Accepted  
**概要**: `VK.Blocks.AI.Eidos` による構造化 JSON バインディングを用い、事実抽出・重要度・感情・矛盾調停を 1 回の LLM 呼び出しに集約する設計。

---

#### [ADR-005: Persistent Tiered Compression Outbox, Session Serialization, and Prompt-Cache Stabilization](./adr-005-persistent-compression-outbox-and-prefix-cache.md)
**Status**: ✅ Accepted  
**概要**: べき等性キー付きの永続化 Outbox ジョブストア、セッション内順序保証ロック、およびプロンプトキャッシュ安定化のためのバッチ閾値制御。

---

#### [ADR-006: L2 Recall Engine, Side-Effect-Free Decay, and Human-Like Presentation Policy](./adr-006-l2-recall-and-human-like-presentation-policy.md)
**Status**: ✅ Accepted  
**概要**: データベースを破壊しない L2 想起時時間減衰計算と、追加 LLM 呼び出しなしでヘッジ表現や詳細度縮退を行う人間らしい提示ポリシー。

---

## 🔗 関連ドキュメント

- [AI.Engram Module Manifest (Tier 2)](/src/BuildingBlocks/AI.Engram/README.md)

**Last Updated**: 2026-11-05  
**Total ADRs**: 6
