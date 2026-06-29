# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.VectorSearch モジュールの主要な設計決定を記録した ADR が参考として含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Standalone Vector Search Rank Fusion Engine](./adr-001-standalone-vector-search-rank-fusion-engine.md)

**Status**: ✅ Accepted  
**概要**: 密ベクトル類似度検索と疎テキスト検索（BM25等）のスケールの異なる結果を統一ソートするため、相互ランク融合（RRF）および加重スコア融合（Weighted Score Fusion）をカプセル化した独立したフュージョンエンジン `VK.Blocks.VectorSearch` を導入する設計。  
**キーワード**: Rank Fusion, Reciprocal Rank Fusion (RRF), Hybrid Search, Composable Search

---

#### [ADR-002: Pipeline-Based Vector Search Orchestration](./adr-002-pipeline-based-vector-search-orchestration.md)

**Status**: ✅ Accepted  
**概要**: 検索の前処理（クエリ書き換え）、中間処理（セマンティックキャッシュ、安全フィルタリング）、後処理（Cross-Encoder リランキング）を統一制御するため、Core パイプラインを基礎としたモジュール化検索実行エンジンを導入する設計。  
**キーワード**: Vector Search Pipeline, Semantic Cache, Query Rewriter, Reranking Stage

---

#### [ADR-003: Multi-Strategy Retrieval and Context Compression Framework](./adr-003-multi-strategy-retrieval-and-context-compression-framework.md)

**Status**: ✅ Accepted  
**概要**: 検索処理をプロバイダから抽象化する多様な検索戦略（ベクトル、キーワード、ハイブリッド）を `IVKSearchStrategy` でカプセル化し、検索結果の肥大化によるトークン超過を防ぐ文脈圧縮（Context Compression）ステージを導入する設計。  
**キーワード**: Search Strategy, Hybrid Search, Context Compression, Token Management

---

## 🎯 ADR の読み方ガイド

### ハイブリッド検索融合の理解用
1. **ADR-001**: 異なる次元や性質の検索エンジン（ベクトルデータベースとテキストインデックス）の結果を、いかに数学的一貫性をもってマージ・順位再計算するかを学ぶために最初に読んでください。
2. **ADR-002**: 単一のクエリ検索に留まらず、前後のキャッシュ、クエリ書き換え、セキュリティ検閲、およびリランキングを高可用かつコンポーザブルに実現するオーケストレーションの流れを学ぶために読んでください。
3. **ADR-003**: ストレージエンジンから独立した検索実行戦略と、検索結果をプロンプトバジェットに収めるための文脈圧縮メカニズムを理解するために読んでください。

## 🔗 関連ドキュメント
- [VectorSearch Module Manifest](../../../src/BuildingBlocks/VectorSearch/README.md)

**Last Updated**: 2026-06-27  
**Total ADRs**: 3
