# ADR-004: Capability Negotiation and Reciprocal Rank Fusion Pushdown

- **Status**: ✅ Accepted
- **Date**: 2026-10-30
- **Deciders**: Architecture Team
- **Technical Story**: VectorSearch Pipeline & Performance Optimization (`ARCH-03`, `ARCH-11`)

---

## 1. Context (背景)

ハイブリッド検索（セマンティック・ベクトル検索 ＋ キーワード・全文検索）は、現代の検索・RAG システムにおける業界標準となっている。その中核を担うアルゴリズムが、複数系統の順位スコアを統合する Reciprocal Rank Fusion (RRF: $\sum \frac{1}{k + \text{rank}}$) である。  
現行の `VK.Blocks.VectorSearch` は、パイプライン内部の `ReciprocalRankFusion.cs` でクライアント側のメモリ内計算を行っている。

---

## 2. Problem Statement (問題定義)

1. **二重 I/O とネットワーク・メモリの浪費**: Azure AI Search、Qdrant (Query API)、PostgreSQL (pgvector ＋ 全文検索) などの最新エンジンは、エンジン内部でネイティブにハイブリッド融合や RRF を実行可能である。クライアントが常にベクトルとキーワードで個別に全件問い合わせて自前融合すると、往復遅延と転送データ量が倍増する。
2. **純粋性の欠如 (`ARCH-03`)**: RRF の数学的ロジックがパイプラインの特定の内部クラスに結合しており、テスト性や単体での再利用性が損なわれている。
3. **能力差の無視**: プロバイダーが何を実行できるか（ネイティブ RRF、疎ベクトル、多ベクトル、フィルター下推）をフレームワークが認識できないため、最適な実行計画を組めない。

---

## 3. Decision (決定事項)

1. **純関数としての RRF の抽出 (`VKReciprocalRankFusion`)**:
   - `ARCH-03 Core Purity`（脱離 I/O テスト）に基づき、I/O を一切持たない純粋な数学関数として `VKReciprocalRankFusion.Compute<TKey>(...)` を公開契約として定義する。
2. **能力交渉メカニズム (`VKVectorStoreCapabilities`)**:
   - 各 Provider は `VKVectorStoreCapabilities` を宣言する。
   - `SupportsNativeHybridSearch` および `SupportsNativeRrf` が true の場合、検索パイプラインはエンジン側のネイティブクエリへ処理を下推（Pushdown）する。
   - ネイティブ未対応のプロバイダー（SQLite、InMemory 等）に対しては、自動的に複数候補を並列取得し、クライアント側 `VKReciprocalRankFusion` でマージする兜底（Fallback）パイプラインを適用する。

---

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 常にクライアント側で RRF を実行する**
  - *Approach*: 既存実装のまま、エンジンに関係なくクライアントでマージ。
  - *Rejected Reason*: Azure AI Search や Qdrant の強力なネイティブエンジン性能を無駄にし、スケール時にボトルネックとなる。
- **Option 2: プロバイダーごとに全く異なる検索 API を公開する**
  - *Approach*: Qdrant 用の検索クラス、Postgres 用の検索クラスを個別に作成。
  - *Rejected Reason*: `ARCH-01` / `ARCH-11` に違反し、上位アプリケーションがプロバイダーの具象に強く結合してしまう。

---

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - 先進的なベクトルエンジンのネイティブ最適化性能を最大限に引き出せる。
  - 統一された API（`SearchAsync` / `HybridSearchAsync`）のまま、バックエンドの差異を透過的に吸収できる。
  - RRF ロジックの決定論的テストが容易になる。
- **Negative**:
  - パイプライン内でプロバイダー能力に応じた分岐制御が追加され、内部の複雑性がやや増す。
- **Mitigation**:
  - `CapabilityAwareHybridStage` に分岐ロジックを集約し、Strategy パターンで明確に責務を分離する。

---

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `VKReciprocalRankFusion.Compute` は配列・ディクショナリの事前キャパシティ確保を行い、ゼロアロケーション指向で実装（`CS.04`）。
- ネイティブ下推時もクライアント側兜底時も、`TenantId` の厳格なフィルタリング条件が維持される（`OR.02`）。

