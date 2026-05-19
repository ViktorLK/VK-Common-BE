# ADR 006: Semantic Retrieval Alignment

- **Date**: 2026-05-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: AI.SemanticKernel Module Terminology Alignment

## 2. Context (背景)

Semantic Kernel モジュールでは、ベクトル検索操作に対して従来の SK の用語に従い `Memory` という用語を使用していました。しかし、基盤となる `AI` ブロックでは、現代的な RAG（Retrieval Augmented Generation）パターンとの整合性を保つために `Retrieval` という用語を採用しています。この不一致は、抽象化層と実装層の間での混乱を招き、アーキテクチャの一貫性を損なっていました。

## 3. Problem Statement (問題定義)

- **用語の不一致**: 開発者が `AI` ブロックの `Retrieval` を探している際に、`AI.SemanticKernel` では `Memory` を探さなければならず、認知負荷が高まっていました。
- **将来的な拡張性の制限**: SK 自体も将来的に `Memory` から `Vector Store` や `Retrieval` への移行を計画しており、古い用語に固執することは技術的負債となります。

## 4. Decision (決定事项)

すべての `Memory` 関連コンポーネントを `Retrieval` にリネームし、統一します：

1.  **ディレクトリ名の変更**: `Memory/` フォルダを `Retrieval/` にリネームします。
2.  **クラス名の変更**: 内部エンジンを `AISKRetrievalEngine` に、設定クラスを `VKRetrievalOptions` にリネームします。
3.  **インターフェースの整合**: `AI` ブロックの `IVKRetrievalOptions` 等との命名規則を完全に一致させます。

## 5. Alternatives Considered (代替案の検討)

### Option 1: 両方の用語を維持する
- **Approach**: フォルダ名は `Memory` のままにし、エイリアス等で対応する。
- **Rejected Reason**: 二重の定義は混乱を助長し、コードのクリーンさを損ないます。

### Option 2: SK の用語 (Memory) に統一する
- **Approach**: 基盤の `AI` ブロック側を `Memory` に変更する。
- **Rejected Reason**: `Retrieval` は RAG パターンの業界標準用語であり、将来性を考えると `Retrieval` の方が適切です。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 全モジュールを通じて用語が統一され、RAG パターンの実装意図が明確になります。
    - 開発者の学習コストとミスが低減されます。
- **Negative**:
    - 既存の `Memory` 参照箇所でのリネーム作業が必要となります。
- **Mitigation**:
    - 影響範囲を特定し、一括置換とビルド検証により整合性を確保しました。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **主要コンポーネント**:
    - `Retrieval/Internal/AISKRetrievalEngine.cs`
    - `DependencyInjection/VKRetrievalOptions.cs`
- **セキュリティ**: 検索クエリ（Query）のログ出力時に、機密情報が含まれないようフィルタリングを検討する基盤を整えました。

---
**Last Updated**: 2026-05-10
