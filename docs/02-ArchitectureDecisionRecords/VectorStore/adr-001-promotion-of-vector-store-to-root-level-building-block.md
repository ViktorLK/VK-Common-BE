# ADR 001: Promotion of Vector Store to Root-Level Building Block

- **Date**: 2026-06-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/VectorStore

## 1. Context (背景)

ベクトルデータストア（Vector Store）は、高次元の多次元ベクトルデータをインデックス・管理し、類似度検索（Cosine 類似度、内積など）を高速に実行するためのデータストレージシステムである。
これまで、本ソリューションにおけるベクトルストアの実装は、`AI.VectorStore` という名前空間およびアセンブリ配下に配置されていた。
しかし、この構造では以下の課題があった：
1. **ドメインの混同**: ベクトル検索は一般の検索、キャッシュ、データの重複排除など、AIの生成推論（Generation）を伴わない多種多様なシステム要件でも利用される。これを `AI` という上位ドメインの配下に固定することは、責務の分類として不自然であった。
2. **モジュールの再利用性の制限**: ベクトル操作のみを使いたい別レイヤーや他システムに対して、不要な AI コグラフィックランタイム（AI.dll 等）へのプロジェクト参照を強制することになり、モジュールの独立性が低くなっていた。

## 2. Problem Statement (問題定義)

ベクトルストアを、AI ドメイン特有の論理的制約から完全に解放し、一般的な高次元データストアとして独立に再利用可能にするための物理パッケージングおよび名前空間の刷新が必要であった。

## 3. Decision (決定事項)

従来の `VK.Blocks.AI.VectorStore` を廃止し、独立したルートレベルの Building Block **「`VK.Blocks.VectorStore`」**として昇格・再編成する。

### 1. ルート名前空間の分離
- 関連するすべての API、モデル、インターフェースの名前空間を `VK.Blocks.AI.VectorStore` から `VK.Blocks.VectorStore`（およびその配下サブネームスペース）へと移行し、物理的な分離を明確にする。

### 2. コアインターフェースの抽出 (`VecEngine`)
- データベース全体のCRUDを司る `IVKVectorStore` および、特定インデックス/テーブルに対応する `IVKVectorCollection` 抽象を定義する。
- メタデータフィルタリングのための共通スキーマ `VKMetadataFilter` や、ベクトルレコードを表す `VKVectorRecord` を共通モデルとしてパッケージ化する。

### 3. デフォルトインメモリストアの提供 (`InMemoryVectorStore`)
- 外部データベースを用意せずとも即時動作するスレッドセーフな `InMemoryVectorStore` と `InMemoryVectorCollection` を標準同梱する。

```
[Old Structure]
  VK.Blocks.AI  --> Covers (AI.Core + AI.VectorStore)

[New Structure]
  VK.Blocks.AI (Pure LLM Orchestration)
  VK.Blocks.VectorStore (Root-Level Generic Vector Storage)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: AI 配下のサブ名前空間として維持するが、プロジェクトファイルを分ける
- **Approach**: アセンブリは分けるが、名前空間は `VK.Blocks.AI.VectorStore` のまま維持する。
- **Rejected Reason**: 名前空間に `AI` が含まれ続ける限り、利用者はこれが AI 関連専用のツールであると誤認しやすく、汎用的なインデックスキャッシュ等としての普及の障壁となるため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な疎結合化**: ベクトルデータストアが単一の独立インフラになり、任意のマイクロサービスが AI の機能有無にかかわらず独立して高速な類似度検索のインフラを利用できるようになった。
- **名前空間と物理ディレクトリのクリーン化**: 依存ツリーが単純化され、アーキテクチャ監査における違反リスクが低減した。

### Negative
- **大規模な移行コストの発生**: 既存プロジェクトにおける `using VK.Blocks.AI.VectorStore` の参照がすべてエラーとなるため、コードの一括置換が必要になる。

### Mitigation
- 段階的移行を支援するため、旧名前空間にエイリアスや一時的なフォワード用属性を付与するか、移行用正規表現スクリプトを共有して置換作業を自動化する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- ベクトルデータおよびメタデータ内の機密データは、ベクトルの類似度に影響を与えない形でハッシュ化またはマスクして格納することを推奨する。
- バリデータにより、登録されるベクトルの次元数の一貫性が厳密に担保される。

## 7. Status
✅ Accepted
