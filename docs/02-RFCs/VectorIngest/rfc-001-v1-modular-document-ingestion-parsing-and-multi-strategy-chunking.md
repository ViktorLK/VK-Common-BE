# RFC-001: Modular Document Ingestion Parsing & Multi-Strategy Chunking

- **Status**: ✅ Approved *(Implemented)*
- **Author**: Architecture Team
- **Date**: 2026-06-27
- **Target Module(s)**: `VK.Blocks.VectorIngest`
- **Related ADRs / RFCs**: [ADR-001](/docs/04-ArchitectureDecisionRecords/VectorIngest/adr-001-modular-document-ingestion-parsing-and-multi-strategy-chunking-pipeline.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **現状の課題 (Current Pain Points)**:
  1. ベクトルデータベースへの生ドキュメント（PDF, Word, Markdown, HTML 等）登録において、ファイル解析・テキスト構造化・チャンク分割・埋め込み生成という一連の処理が各アプリケーションで独自にスクリプト化されていた。
  2. 単純な固定長文字数分割しか行えず、文脈や意味の境界が途切れて検索精度（ヒット率）が著しく悪化していた。
  3. 大量ファイルの一括取り込み時に、メモリ不足やパイプライン中断時の整合性担保が不十分であった。
- **なぜ今やるのか (Why Now / Consumer)**:
  - エンタープライズ RAG システムおよびナレッジベース構築において、多形式ファイル解析と高精度なチャンキングを標準化するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - ドキュメント取り込み・解析・分割・インデックス登録を一元制御する `VK.Blocks.VectorIngest` を導入する。
  - ファイル形式に応じた動的解析器を解決する `IVKDocumentParser` および `IVKDocumentParserResolver` を提供する。
  - 4 つのテキスト分割戦略（固定長 `FixedSize`、再帰的 `Recursive`、階層的 `Hierarchical`、セマンティック `Semantic`）を標準実装する。
  - `Core` のパイプラインランタイムを利用した `IVKIngestPipeline`（`DocumentLoadStage` ➡️ `DocumentWriteSinkStage`）を構築する。
  - 疎ベクトル（BM25/SPLADE）および多ベクトル（ColBERT）の選択的生成オプション（`VKIndexingOptions`）をサポートする。
- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - ベクトルデータの永続化インデックス管理（`VectorStore` に委譲）。
  - 自然言語プロンプトの組み立てや推論（`AI` ドメインに委譲）。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

- **Metaphor (概念メタファー / メンタルモデル)**:
  - **The Automated Timber Sawmill & Wood Pulp Factory (全自動製材・木材パルプ加工ライン)**:
    - 森林から届く原木（PDF や Word などの生ファイル）の皮を剥ぎ（Document Parser）、純粋な繊維（プレーンテキスト）を抽出。
    - 用途に合わせて、荒削り（固定長）、年輪・節に沿った精密裁断（再帰的・セマンティック分割）を行い、最適なチップ（チャンク）へと加工。
    - 最後に規格化されたパレットに結束（埋め込みベクトル生成）して、中央高次元倉庫（VectorStore）へ出荷する。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| 原木の皮剥ぎ・樹種判別 | `IVKDocumentParserResolver` | Public Protocol | 拡張子に応じた最適なパルプ抽出器の動的解決 |
| 木材繊維の精密裁断機 | `IVKTextChunker` | Public Protocol | 4 つの分割戦略（固定・再帰・階層・セマンティック） |
| 自動コンベア搬送ライン | `IVKIngestPipeline` | Public Protocol | 読み込み・解析・分割・書き込みのシーケンス統制 |
| 高次元倉庫への出荷ゲート | `IVKIndexingService` | `Internal/` | 埋め込みベクトル生成と VectorStore へのバッチ投入 |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

```csharp
namespace VK.Blocks.VectorIngest;

public interface IVKDocumentParser
{
    bool CanParse(string extensionOrMimeType);
    Task<VKResult<string>> ParseAsync(Stream content, CancellationToken cancellationToken = default);
}

public interface IVKTextChunker
{
    VKChunkerType ChunkerType { get; }
    Task<VKResult<IReadOnlyList<VKChunk>>> ChunkAsync(string text, IVKChunkingOptions options, CancellationToken cancellationToken = default);
}

public interface IVKIngestPipeline
{
    Task<VKResult<VKIngestResponse>> IngestAsync(VKIngestRequest request, CancellationToken cancellationToken = default);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance & Security**:
  - 悪意ある巨大 PDF による DoS（Zip Bomb 型攻撃）を防ぐため、`VKParsingOptions` でファイルサイズ上限（デフォルト 50MB）および展開後文字数上限を強制バリデーション。
  - セマンティックチャンキングは Embedding API を複数回呼ぶため、コストと処理時間が増加（固定長・再帰的分割をデフォルト推奨）。

---

## 6. 📋 Decomposition & Implementation Plan

- [x] **Phase 1 (Parsing & Resolvers)**: `IVKDocumentParser` と `DefaultDocumentParserResolver`（`ADR-001`）
- [x] **Phase 2 (Chunking Strategies)**: 固定、再帰、階層、セマンティックの 4 大チャンカー実装
- [x] **Phase 3 (Pipeline & Sink)**: `DefaultIngestPipeline` と `DocumentWriteSinkStage` の構築
- [x] **Phase 4 (Sparse & Multi-Vector)**: `VKIndexingOptions` によるコスト保護付き多次元ベクトル拡張

