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

#### [ADR-003: Microsoft.Extensions.VectorData Abstraction Boundary and Backend Roles](./adr-003-vectordata-abstraction-boundary-and-backend-roles.md)

**Status**: ✅ Accepted  
**概要**: Microsoft.Extensions.VectorData.Abstractions を公式接続基盤として取り込み、VK 側は VKResult・テナント分離・診断を担うガバナンス層に特化。バックエンドを専用エンジン、リレーショナル列、組み込みの 3 分類に再定義する設計。  
**キーワード**: VectorData, Governance Adapter, Backend Roles, Schema Validation, EF Core Separation

---

#### [ADR-004: Capability Negotiation and Reciprocal Rank Fusion Pushdown](./adr-004-capability-negotiation-and-rrf-pushdown.md)

**Status**: ✅ Accepted  
**概要**: Reciprocal Rank Fusion (RRF) を I/O フリー純関数として独立させ、バックエンド能力に応じたネイティブ下推（Pushdown）とクライアント側 RRF 兜底（Fallback）の動的交渉メカニズムを定義する設計。  
**キーワード**: RRF, Core Purity, Capability Negotiation, Pushdown, Hybrid Search

---

## 🎯 ADR の読み方ガイド

### 共通ベクトルストア抽象の理解用
1. **ADR-001**: なぜベクトルデータベースを AI ドメインから切り離してルート層で定義すべきなのか、その背景と再利用性のトレードオフを理解するために最初に読んでください。
2. **ADR-002**: 一件ずつの更新によるデータベースロックやボトルネックを解消し、スループットを数十倍に加速するバルク（一括）書き込み制御の仕組みを理解するために読んでください。
3. **ADR-003**: Microsoft 公式標準の VectorData と VK のガバナンス層の役割分担、およびリレーショナル DB（pgvector）と専用エンジン（Qdrant）の責務分離を把握するために読んでください。
4. **ADR-004**: ハイブリッド検索における RRF の純関数設計と、エンジンのネイティブ機能下推によるパフォーマンス最大化方針を理解するために読んでください。

## 🔗 関連ドキュメント
- [VectorStore Module Manifest](/src/BuildingBlocks/VectorStore/.prompts/module-manifest.md)
- [RFC-001: Vector Ecosystem Modernization](/docs/02-RFCs/VectorStore/rfc-001-v2-vector-ecosystem-modernization.md)

**Last Updated**: 2026-10-30  
**Total ADRs**: 4
