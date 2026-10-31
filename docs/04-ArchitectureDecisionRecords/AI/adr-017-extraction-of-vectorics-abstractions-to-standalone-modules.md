# ADR 017: Extraction of Vectorics Abstractions to Standalone Modules

- **Date**: 2026-06-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI

## 1. Context (背景)

VK.Blocks.AI は、LLM（大規模言語モデル）の推論呼び出しやチャットセッションの抽象化を行うための中心的な Building Block である。
これまで、このライブラリの中には、テキスト埋め込み生成（Embeddings）、検索結果のリランキング（ReRanking）、ベクトルストアからのRAGオーケストレーション（Retrieval）、およびセマンティックキャッシュ（SemanticCache）といった、ベクトル空間操作を伴う機能群（総称して「Vectorics」）が同梱されていた。
しかし、この密結合な構成には以下の問題があった：
1. **責務の過負荷**: AI 実行モジュールが、ベクトルデータベースや次元数の計算、リランキング数学など、ストレージ・インデックス寄りのインフラ技術に直接依存する形になり、単一責任の原則（SRP）に違反していた。
2. **依存関係の汚染**: 単純な LLM チャット機能のみを使いたいクライアントアプリに対しても、ベクトル検索に必要な複雑な抽象やスキーマ群が強制的に依存関係として取り込まれていた。

## 2. Problem Statement (問題定義)

ベクトル空間インデクシングと生成推論（Generation）のアーキテクチャ境界をクリーンに分離し、双方を独立したアセンブリとして再利用可能にするためのモジュール再編を行う必要があった。

## 3. Decision (決定事項)

VK.Blocks.AI から **「Vectorics（ベクトル空間・検索関連）の全アセンブリ・クラス群を廃止・抽出し、それぞれ独立した独立 Building Block へ移行する」**。

### 1. Vectorics フォルダの廃止
- `VK.Blocks.AI` プロジェクト内に存在した `Vectorics/` ディレクトリ配下の全コード（Embeddings, ReRanking, Retrieval, SemanticCache）を削除する。

### 2. 独立した Building Block への再編
- **`VK.Blocks.VectorStore`** （新規アセンブリ）: テキスト埋め込み（Embeddings）抽象、ベクトルコレクション（`IVKVectorCollection`）、インメモリストアなどをカプセル化する。
- **`VK.Blocks.VectorSearch`** （新規アセンブリ）: 検索結果の複数スコア融合アルゴリズム（Weighted Fusion, RRF）およびリランキング制御を集約する。

### 3. 名前空間の整理
- 移行に伴い、関連クラスの名前空間を `VK.Blocks.AI.Vectorics` から、それぞれのルートモジュールに対応する `VK.Blocks.VectorStore` および `VK.Blocks.VectorSearch` へと刷新する。

```
[Old Assembly Structure]
  VK.Blocks.AI
    +-- Generation (LLM Core)
    +-- Vectorics (Embeddings, Rerank, RAG etc.)  <-- DEPRECATED

[New Assembly Structure]
  VK.Blocks.AI (Pure Generation & Orchestration)
  VK.Blocks.VectorStore (Embeddings & Database Operations)
  VK.Blocks.VectorSearch (Fusion Engine & Search Logic)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: フォルダ構造のみで分離し、同一アセンブリ（AI.dll）で配布し続ける
- **Approach**: フォルダを `Vectorics/` に分けたまま、名前空間の分離だけで済ませる。
- **Rejected Reason**: アセンブリとしての物理的な依存分離がなされないため、NuGet パッケージとしての配信時に不要なストレージ機能が AI コアに引きずられ続け、肥大化問題が解決しないため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **単一責任の追求**: AI コアモジュールは純粋な LLM 生成とオーケストレーションのみに集中でき、テストが格段に容易になった。
- **プラグイン可能なベクトルインフラ**: 埋め込みエンジンやベクトルコレクションが独立したため、AI コアの存在を前提としない、純粋な埋め込み生成処理やデータパイプラインでの再利用が容易になった。

### Negative
- **破壊的変更の発生**: 既存の `IVKEmbeddingsEngine` や `VKSemanticCache` などを参照していたクライアントコードにおいて、プロジェクト参照の追加（`VK.Blocks.VectorStore` 等）と名前空間の using 宣言の書き換えが必要になる。

### Mitigation
- 移行用ドキュメントを明記し、DI 登録時に `AddVKAIBlock` から自動で必要な `VectorStore` や `VectorSearch` の依存も（必要に応じて）マーク・追加バインドする後方互換ヘルパーを一時的に提供する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 物理的な境界の再定義に伴い、機密性の高い API キーや接続文字列がアセンブリ間をまたいで暗黙的に引き渡されないよう、各アセンブリごとにオプション設定（Options）のスコープを厳格に独立させる。

## 7. Status
✅ Accepted
