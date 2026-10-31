# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Persistence.EFCore.Cosmos モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Independent Cosmos Db Persistence Block](./adr-001-independent-cosmos-db-persistence-block.md)

**Status**: ✅ Accepted  
**概要**: Azure Cosmos DB データベースの構成や NuGet 依存関係を他のデータベースプロバイダから分離し、依存関係を極小化した独立モジュールとしてパッケージングする設計。  
**キーワード**: Cosmos DB Provider, DbContext Configurator, Dependency Isolation

---

### Distributed Consistency & Performance

#### [ADR-002: Session Token Propagation for Distributed Read-Your-Writes Consistency](./adr-002-session-token-propagation-for-read-your-writes.md)

**Status**: ✅ Accepted  
**概要**: 分散マイクロサービス環境において、HTTP ヘッダーを介して Cosmos DB Session Token を伝播させることで、Strong 一致性の高コスト・高遅延を回避しつつ「読己所写（Read-Your-Writes）」の一致性を保証。  
**キーワード**: Session Token, Read-Your-Writes, Causal Consistency, Monotonic Reads

#### [ADR-003: Atomic In-Partition Batching via Cosmos TransactionalBatch](./adr-003-in-partition-acid-batching-via-transactional-batch.md)

**Status**: ✅ Accepted  
**概要**: 同一パーティションキー内の複数操作を Cosmos DB ネイティブの `TransactionalBatch` にバンドルし、単一のネットワーク往復で完全な ACID トランザクションと RU 最適化を実現。  
**キーワード**: TransactionalBatch, ACID Transactions, Partition Key, RU Optimization

---

## 🎯 ADR の読み方ガイド

1. **モジュール分離の理解**: **ADR-001** で、なぜ Cosmos DB 専用の独立 BuildingBlock を設けたかの背景を理解してください。
2. **分散整合性戦略の理解**: **ADR-002** で、ステートレスなマルチノード構成において Session Consistency を維持する伝播設計を把握してください。
3. **トランザクション性能の理解**: **ADR-003** で、同一パーティションキー内の複数エンティティ操作をアトミックに実行するバッチパターンを把握してください。

## 🔗 関連ドキュメント
- [Persistence Module Manifest](/src/BuildingBlocks/Persistence/.prompts/module-manifest.md)
- [Persistence.EFCore ADRs](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/README.md)

**Last Updated**: 2026-08-20  
**Total ADRs**: 3
