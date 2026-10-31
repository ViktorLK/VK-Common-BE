# ADR 001: Modular Document Ingestion Parsing and Multi-Strategy Chunking Pipeline

- **Date**: 2026-06-27
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/VectorIngest

## 1. Context (背景)

ベクトルデータベース（`VectorStore`）を利用した類似度検索や RAG（Retrieval-Augmented Generation）システムでは、ドキュメントの登録（Ingestion）フェーズが精度を大きく左右する。
生のファイルを直接ベクトルストアに放り込むことはできず、以下の手順を順次実行しなければならない：
1. **ドキュメントの解析 (Parsing)**: PDF、Word、HTML、Markdown などの多様な物理フォーマットを解析し、クリーンなプレーンテキストに抽出する。
2. **チャンク分割 (Chunking)**: 抽出したテキストを、LLM の文脈や類似度の計算に適した適切なサイズ（数百文字程度）のフラグメント（チャンク）に分割する。
3. **埋め込みベクトルの生成とインデックス登録 (Indexing)**: チャンクごとにテキストエンベッディングを生成し、メタデータとともにベクトルストアへ一括して書き込む。

従来の設計では、これらの工程を各アプリケーションが独自にスクリプト化していたため、チャンク分割アルゴリズムの最適化や、ファイル形式ごとのパース処理が標準化されていなかった。

## 2. Problem Statement (問題定義)

ドキュメントの取り込み・解析・分割・格納という一連のライフサイクルを統一的かつ柔軟（異なるファイル形式、多様なチャンク戦略のサポート）に処理可能とする、再利用性の高いデータ取り込み（Ingest）パイプライン基盤が必要であった。

## 3. Decision (決定事項)

新規の Building Block **「`VK.Blocks.VectorIngest`」**を導入し、モジュール化された解析（Parsing）、多戦略分割（Chunking）、および統一インジェストパイプラインを構築する。

### 1. ドキュメント解析の動的解決 (`Parsing`)
- `IVKDocumentParser` を定義し、各物理拡張子に対応する解析器を抽象化する。
- 実行時にファイル形式に応じて最適な解析器を動的に解決する `IVKDocumentParserResolver` (`DefaultDocumentParserResolver`) を導入する。

### 2. 多様なテキスト分割戦略の標準化 (`Chunking`)
- `IVKTextChunker` インターフェースを定義し、以下の 4 つの分割戦略を実装する：
  - `DefaultFixedSizeChunker`: 単純な固定長文字数での分割。
  - `DefaultRecursiveChunker`: 段落、文、単語の優先順位に基づく再帰的分割（文脈の途切れを極小化）。
  - `DefaultHierarchicalChunker`: 親チャンクと子チャンクのツリー構造を保持する階層的分割。
  - `DefaultSemanticChunker`: 埋め込みベクトルの類似度の急激な変化（トピック遷移）を検知して境界を決定するセマンティック分割。

### 3. Core 管道をベースとする `IngestPipeline` の定義
- `Core` のパイプラインランタイムを利用した `IngestPipelineExecutor` を実装。
- `DocumentLoadStage`（読み込みとパース）および `DocumentWriteSinkStage`（分割とバルクインデックス登録）を組み合わせた一連のフェーズをシーケンシャルに制御する。

```
[Raw Document File]
       |
       v
[DocumentLoadStage]  ---> Resolves IVKDocumentParser via Resolver -> Text
       |
       v
[DocumentWriteSinkStage] ---> Splits Text via IVKTextChunker (Strategy)
       |                     ---> Bulk Upserts to Vector Store
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: AI 核心ライブラリ内に Ingestion ロジックを同梱する
- **Approach**: `VK.Blocks.AI` 内のユーティリティとしてドキュメント処理コードを追加する。
- **Rejected Reason**: ファイルのバイナリ解析（PDF 解析ライブラリ等）やテキストチャンキングの数学的処理は、実行時のプロンプトオーケストレーション（AI）とは異なる責務であり、アセンブリサイズや依存関係を分離するために別 Block とすべきと判断した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **インジェストの高品質化**: 再帰的分割やセマンティック分割の標準化により、RAG の検索精度（ヒット率）が大幅に向上する。
- **プラグイン構造**: 新しいファイル形式のパース処理や、より高度なチャンキングアルゴリズムをインターフェースの実装だけで簡単に追加できる。

### Negative
- **ライブラリ依存の増加**: PDF 解析やセマンティック解析用エンジンの導入により、`VectorIngest` ブロックが依存するパッケージアセンブリが増える。

### Mitigation
- 解析器やチャンカーを Feature 単位（`ParsingFeature`, `ChunkingFeature`）でトグル可能にし、不要な機能がメモリやランタイムに与える影響を制御する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 悪意ある PDF や超巨大ファイルがロードされた場合のリソース枯渇（DoS 攻撃）を防ぐため、`VKParsingOptions` やガード条件によってファイルサイズの上限およびメモリバッファの上限を厳格にバリデーションする。

## 7. Status
✅ Accepted
