# ADR 003: Atomic In-Partition Batching via Cosmos TransactionalBatch

- **Date**: 2026-08-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence.EFCore.Cosmos

---

## 1. Context (背景)

EF Core の Cosmos DB プロバイダは、エンティティのコレクション保存時、内部的に複数の個別の HTTP/REST または TCP リクエストを Cosmos DB に発行する。

しかし、同一の論理パーティションキー（Logical Partition Key）に属する複数のドキュメント（例: 親エンティティと複数の子エンティティ、または集約ルートとトランザクションログ）を更新する場合、以下の問題が発生する：
1. **アトミック性の欠如**:
   複数の書き込みのうち一部が成功し、ネットワーク障害等で後続が失敗した場合、部分書き込み（Partial Write）が発生してデータが破損するリスクがある。
2. **ネットワーク往復遅延 (Round-trip Latency) の増大**:
   $N$ 件のドキュメント操作に対して $N$ 回のネットワーク I/O が発生し、Cosmos DB の RU 消費と処理時間が線形に増加する。

## 2. Problem Statement (問題定義)

同一パーティションキー内の複数操作に対して、単一のネットワーク呼び出しで完全な ACID トランザクション（All-or-Nothing）を保証する高速なバッチ実行機能が必要であった。

## 3. Decision (決定事項)

Cosmos DB ネイティブの **`TransactionalBatch` API を抽象化・公開する `IVKCosmosTransactionalBatchFactory`** を導入する。

### 1. `IVKCosmosTransactionalBatchFactory` の提供
- 同一パーティションキーに限定した操作をグループ化し、Cosmos DB のサーバーサイドで 1 つのアトミックなトランザクションとして実行するファクトリ契約を定義。
- 作成（Create）、置換（Replace）、削除（Delete）の操作を単一リクエストペイロードにバンドル。

```csharp
services.TryAddSingleton<IVKCosmosTransactionalBatchFactory, CosmosTransactionalBatchFactory>();
```

### 2. Result パターンによるエラー制御 (`CS.01`)
- バッチ内のいずれか 1 件でも競合や制約違反（409 Conflict, 412 Precondition Failed 等）を起こした場合、バッチ全体がロールバックされ、失敗した操作のインデックスとエラー詳細を `VKResult<T>` で返却する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: Cosmos DB のストアドプロシージャ (Stored Procedures / JavaScript)
- **Approach**: サーバー側 JS ストアドプロシージャをデプロイし、その中でトランザクションを実行する。
- **Rejected Reason**: JS コードのバージョン管理、デプロイ自動化の困難さ、および型安全性の欠如により却下。

### Option 2: EF Core の `SaveChangesAsync` に完全に依存
- **Approach**: EF Core に任せ、トランザクション境界は設けない。
- **Rejected Reason**: Cosmos DB EF Core プロバイダは真の分散トランザクションをサポートしておらず、同一パーティション内でも障害時の整合性を保証できないため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な ACID 保証**: 同一パーティション内での更新が原子的に実行され、不整合状態を完全に排除。
- **高スループット & RU 削減**: 単一リクエストで複数操作を完了するため、ネットワークレイテンシと RU 課金効率が劇的に改善。

### Negative
- 操作対象が「同一のパーティションキー」に属していなければならないという Cosmos DB の物理制約（異なるパーティションキーを跨ぐバッチは不可）。

### Mitigation
- ドメインモデリング段階で集約（Aggregate）境界とパーティションキー設計を一致させる（DDD ガイドラインの徹底）。

## 6. Implementation & Status
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence.EFCore.Cosmos/Connection/Internal/CosmosTransactionalBatchFactory.cs`

