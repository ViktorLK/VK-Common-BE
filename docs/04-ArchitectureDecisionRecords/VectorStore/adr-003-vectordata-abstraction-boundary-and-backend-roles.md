# ADR-003: Microsoft.Extensions.VectorData Abstraction Boundary and Backend Roles

- **Status**: ✅ Accepted
- **Date**: 2026-10-30
- **Deciders**: Architecture Team
- **Technical Story**: VectorStore Ecosystem Modernization (`PS.01`, `ARCH-01`, `ARCH-02`)

---

## 1. Context (背景)

.NET 9+ において、Microsoft はベクトルデータアクセスの公式標準として `Microsoft.Extensions.VectorData.Abstractions`（`IVectorStore`, `IVectorStoreRecordCollection<TKey, TRecord>` 等）を策定した。Qdrant、Azure AI Search、Redis、PostgreSQL 等の主要ベンダーも同抽象向けの公式コネクタを提供している。  
一方で、従来の `VK.Blocks.VectorStore`（ADR-001, ADR-002）は独自の `IVKVectorStore` / `IVKVectorCollection<T>` を持っており、公式エコシステムとの統合や最新コネクタの活用において二重管理の課題が生じていた。

---

## 2. Problem Statement (問題定義)

1. **エコシステム乖離と多重車輪の再発明**: 独自の検索・コレクション抽象を作り続けると、公式コネクタの進化（Qdrant の最新 Query API 等）に追従するメンテナンスコストが極大化する。
2. **リレーショナルDBと専用エンジンの責務衝突 (Brain-Split)**: 
   - 専用エンジン（Qdrant など）はコレクションの動的作成・Alias 管理をベクトルストアが管轄する。
   - リレーショナル DB（PostgreSQL pgvector, Azure SQL）ではベクトルはテーブルの一列に過ぎず、スキーマと HNSW 索引は `VK.Blocks.Persistence.EFCore` のマイグレーションが管轄する。`VectorStore` がリレーショナル側でコレクション作成を試みると、責任境界が破綻する。
3. **ガバナンスの欠落**: 公式の `VectorData` は例外送出を前提としており、`VKResult<T>` 統一エラーモデル（`CS.01`）、強制テナント分離 `VKTenantId`（`OR.02`）、および診断メトリクス（`BB.04`）を持たない。

---

## 3. Decision (決定事項)

1. **VectorData 上のガバナンス・デコレータとしての位置づけ**:
   - `Microsoft.Extensions.VectorData.Abstractions` を公式接続基盤として採用する。
   - VK 側は `VectorData` を直接アプリケーションに漏洩させず、`VectorDataCollectionAdapter` を通じて `VKResult<T>`、`VKTenantId` 境界検証、および `VKDiagnosticsConstants` による計装を被せる。
2. **3 分類バックエンドロールの導入 (`VKVectorStoreBackendRole`)**:
   - `DedicatedEngine`: コレクション・Alias・バックアップの完全なライフサイクル（`IVKVectorCollectionLifecycle`）を VectorStore が管轄する。
   - `RelationalColumn`: スキーマ作成を禁止し、起動時検証（`IVKVectorSchemaValidator`）と LINQ ベクトル検索拡張のみを提供する。
   - `Embedded`: SQLite / InMemory によるローカルテスト・エッジ向け軽量構成。

---

## 4. Alternatives Considered (代替案の検討)

- **Option 1: VectorData を無視して完全独自抽象を継続**
  - *Approach*: 既存の独自 `IVKVectorCollection` に各社 SDK を独自実装し続ける。
  - *Rejected Reason*: メンテナンスコストが高すぎ、ベンダーの新機能対応が遅延する（`BASE-06` 違反）。
- **Option 2: VK 独自抽象を全廃し、Microsoft.Extensions.VectorData を直結**
  - *Approach*: アプリケーション層に `IVectorStoreRecordCollection` を直接 DI する。
  - *Rejected Reason*: `VKResult` 統制が崩壊し、テナント漏洩のリスクが高まる。

---

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - 各種 DB ベンダー公式の VectorData コネクタを再利用可能になり、実装コストが激減する。
  - リレーショナル DB と EF Core の責務衝突が完全に解消される。
  - テナント分離・診断・Result パターンのエンタープライズ品質が担保される。
- **Negative**:
  - `VectorData` のバージョン互換性・抽象仕様の変更に追従するアダプター層の保守が必要。
- **Mitigation**:
  - `VectorDataCollectionAdapter` にて外部例外を `VKVecEngineErrors` に厳格に変換・隔離する（Anti-Corruption Layer パターン）。

---

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `VKGuard.NotNull` によりメソッド境界での引数検証を徹底（`AP.01`）。
- テナント分離: すべてのクエリおよび Upsert は `VKTenantId` を強制し、Adapter 内でフィルター式を自動結合する（`OR.02`）。

