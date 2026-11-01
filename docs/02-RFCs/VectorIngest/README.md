# VectorIngest RFCs — Evolution Wave Index

このディレクトリには、`VK.Blocks.VectorIngest` モジュールのアーキテクチャ設計書（RFC）が格納されています。

---

## 🌊 概念設計書一覧

| RFC 番号 / ファイル | コアテーマ & 概念メタファー | 統合・包含された ADR 群 | 状態 |
| :--- | :--- | :--- | :---: |
| [rfc-001-v1-modular-document-ingestion-parsing-and-multi-strategy-chunking.md](./rfc-001-v1-modular-document-ingestion-parsing-and-multi-strategy-chunking.md) | **全自動製材・木材パルプ加工ライン**<br/>生ドキュメント（PDF/Word 等）の動的ファイル解析（`IVKDocumentParserResolver`）、4 つの高度テキスト分割戦略（固定・再帰・階層・セマンティック）、およびパイプライン制御された一括インデックス登録。 | `VectorIngest`:<br/>`ADR-001` | ✅ Approved<br/>*(Implemented)* |

---

## 🔗 関連ドキュメント

- **Architecture Decision Records (ADR)**:
  - [VectorIngest ADR Index](/docs/04-ArchitectureDecisionRecords/VectorIngest/README.md) — インジェスト設計決定原典
- **System Architecture**:
  - [VectorIngest Architecture Specification](/docs/03-Architecture/VectorIngest/README.md) — モジュール仕様書

