# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.VectorStore モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture

#### [ADR-001: Promotion of Vector Store to Root-Level Building Block](./adr-001-promotion-of-vector-store-to-root-level-building-block.md)

**Status**: ✅ Accepted  
**概要**: ベクトルデータベース抽象を AI 特有の論理的制約から完全に解放し、一般的な高次元データストアとして独立に再利用可能にするため、独立したルートレベルの Building Block `VK.Blocks.VectorStore` へ昇格・再編成する設計。  
**キーワード**: Namespace Reorganization, Decoupling, Vector Storage, Core Promotion

---

#### [ADR-002: Bulk Vector Operations Abstraction](./adr-002-bulk-vector-operations-abstraction.md)

**Status**: ✅ Accepted  
**概要**: 大量データのインジェストに伴うネットワーク・ディスク I/O のオーバーヘッドを極小化するため、一括挿入・更新を可能にする `IVKBulkCapableVectorStore` インターフェースおよび一括登録（UpsertBatchAsync）の標準実装を定義する設計。  
**キーワード**: Batch Upsert, Bulk Operations, Database Optimization, Thread-Safe Ingestion

---

## 🎯 ADR の読み方ガイド

### 共通ベクトルストア抽象の理解用
1. **ADR-001**: なぜベクトルデータベースを AI ドメインから切り離してルート層で定義すべきなのか、その背景と再利用性のトレードオフを理解するために最初に読んでください。
2. **ADR-002**: 一件ずつの更新によるデータベースロックやボトルネックを解消し、スループットを数十倍に加速するバルク（一括）書き込み制御の仕組みを理解するために読んでください。

## 🔗 関連ドキュメント
- [VectorStore Module Manifest](../../../src/BuildingBlocks/VectorStore/README.md)

**Last Updated**: 2026-06-24  
**Total ADRs**: 2
