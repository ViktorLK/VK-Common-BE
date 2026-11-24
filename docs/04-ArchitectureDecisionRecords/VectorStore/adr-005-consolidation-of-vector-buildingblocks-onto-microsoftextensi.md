# ADR 005: Consolidation of Vector BuildingBlocks onto Microsoft.Extensions.VectorData and DataIngestion

**Date**: 2026-11-04
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: VectorStore

## 1. Context (背景)
VK.Blocks内のVectorStore, VectorSearch, VectorIngestは、独自定義のCRUDモデル、スパース・マルチベクトル、Capabilitiesフラグ、多重のRetrievalStore/Compressionなど、Microsoft.Extensions.VectorDataと重複または投機的（未実装）な抽象を抱えていた。
特にAI.Engram（記憶検索）の実装において、VectorIngestは未使用であり、VectorSearchとVectorStoreの間で二重のストレージ抽象（IVKRetrievalStore vs IVKVectorCollection）が存在していた。
また、BASE-06（Radical YAGNI）およびARCH-02（Port-and-Adapter）に基づき、公式SDK（Microsoft.Extensions.VectorData / DataIngestion）が存在するものは自作せず、VK固有のガバナンス（VKResult, テナント分離, エイリアス, 起動時検証, テレメトリ）のみを薄いFacadeとして提供する方針に合意した。

## 2. Decision (決定事項)
1. Microsoft.Extensions.VectorData および Microsoft.Extensions.DataIngestion を一次ストレージ・インジェスチョン基盤として正式採用する。
2. 自前のCRUD契約、投機的なスパース/マルチベクトル型（VKSparseVector, VKMultiVector）、静的Capabilitiesモデル、IVKBulkCapableVectorStore、IVKRetrievalStore、VectorSearch.Compressionを削除する。
3. VK固有のガバナンス（VKResult返却 [CS.01]、テナント述語強制注入 [OR.02]、論理コレクション名→物理エイリアス解決、起動時のEmbeddingModelId/Dimensions検証 [BASE-03]）は、VectorStoreの単一の薄いDecorator/Facade（VKVectorCollectionFacade）に集約する。
4. VectorSearchはRRF融合・Rerankポート・Relational Joinのみの軽量機能に絞り込み、VectorStoreへの垂直統合を視野に入れる。
5. VectorIngestはMicrosoft.Extensions.DataIngestionを土台とし、確定性Chunk ID生成・Embeddingモデル版数追跡・Hierarchicalチャンカー等のVK固有要件のみを保持する。

## 3. Alternatives Considered (代替案の検討)
- **Option 1**: Retain legacy status quo (Rejected: insufficient modularity and maintainability).

## 4. Consequences & Mitigation (結果と緩和策)
メリット:
1. 重複抽象（自前CRUD、二重のRetrievalStore、非推奨のCapabilitiesフラグ）が完全に排除され、保守コストと認知的負荷が大幅に低下する。
2. Microsoft公式の各VectorStoreコネクタ（InMemory, SqliteVec, Qdrant, pgvector）および将来のDataIngestionリーダー・チャンカーエコシステムを直接活用できる。
3. VKResult、テナント強制注入、論理エイリアスルーティング、モデル/次元整合性検証が単一の薄いガバナンス境界（VKVectorCollectionFacade）に集約される。

デメリット / 影響:
1. 破壊的変更（Breaking Change）となり、VKSparseVector, VKMultiVector, IVKRetrievalStore, VectorSearch.Compression等に依存するコードは修正または削除が必要。
2. 内部クラスで直接VectorDataの例外をキャッチせず、必ずFacade境界でVKResultに変換する規律が必要。

## 5. Implementation & Security (実装詳細とセキュリティ考察)
- VectorStoreOptionsにLogical Collection Registry（論理名 -> エイリアス/物理名, EmbeddingModelId, Dimensions）を導入。
- IValidateOptionsまたはホスト起動時フックでモデル/次元不整合をフェイルファスト検証。
- VectorDataCollectionAdapterをVKVectorCollectionFacadeに再構成し、テナント境界ガードとVKResult変換を集中化。
- AI.EngramのIVKRetrievalStore参照を新Facadeに移行。
