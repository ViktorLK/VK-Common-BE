# Architecture Decision Records (ADR) - Index

このディレクトリには、VK.Blocks.Persistence モジュールの主要な設計決定を記録した ADR が含まれています。

## 📚 ADR 一覧

### Core Architecture (コアアーキテクチャ)

#### [ADR-001: ORM-Agnostic Persistence Abstractions and Clean Domain Decoupling](./adr-001-orm-agnostic-persistence-abstractions.md)

- **Status**: ✅ Accepted
- **概要**: 物理 ORM や外部 SDK に依存しない純粋な永続化ポート（Unit of Work、トランザクション、監査、アウトボックス）を独立モジュールとして分離策定。
- **キーワード**: Port and Adapter, Clean Architecture, Dependency Isolation, Core Purity

---

### Protocol & Interface Design (プロトコル・インターフェース設計)

#### [ADR-002: Role-Segregated Repository Protocols (ISP & CQS Separation)](./adr-002-role-segregated-repository-protocols.md)

- **Status**: ✅ Accepted
- **概要**: 単一の巨大リポジトリを排し、参照専用（Read）、更新専用（Write）、一括処理（Bulk）、特権処理（System）に直交分離した型安全なリポジトリプロトコル体系。
- **キーワード**: ISP, CQS, Read-Write Separation, Bulk Operations, System Security

---

## 🎯 ADR の読み方ガイド

1. **基本設計の理解**: まず **ADR-001** を通じて、永続化レイヤーがどのように ORM から独立し、Clean Architecture のポート（Ports）として機能しているかを把握してください。
2. **リポジトリの利用指針**: アプリケーション開発やハンドラー実装時は **ADR-002** を参照し、ユースケース（参照 / 更新 / 一括 / 特権）に応じた適切なリポジトリインターフェースを選択してください。

## 🔗 関連ドキュメント
- [Persistence Module Manifest](/src/BuildingBlocks/Persistence/.prompts/module-manifest.md)
- [Persistence.EFCore ADRs](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore/README.md)
- [Persistence.EFCore.Cosmos ADRs](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore.Cosmos/README.md)
- [Persistence.EFCore.Sqlite ADRs](/docs/04-ArchitectureDecisionRecords/Persistence.EFCore.Sqlite/README.md)

**Last Updated**: 2026-10-30  
**Total ADRs**: 2

