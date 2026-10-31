# ADR 001: ORM-Agnostic Persistence Abstractions and Clean Domain Decoupling

- **Date**: 2026-02-14
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence

---

## 1. Context (背景)

当初、データアクセス、トランザクション管理、リポジトリ契約、およびエンティティライフサイクルに関する基本機能は `VK.Blocks.Persistence.EFCore` 内に直接配置されていた。しかし、アーキテクチャの進化に伴い以下の問題が顕在化した：

1. **ORM 依存の漏洩 (`ARCH-02`, `ARCH-03`)**:
   エンティティモデルやドメインロジックが `Microsoft.EntityFrameworkCore` の型（`DbContext`, `EntityEntry` 等）に暗黙的に結合し、Clean Architecture の Core Purity（隔離テスト）を満たせなくなっていた。
2. **依存関係の強制と肥大化 (`ARCH-04`, `BASE-06`)**:
   EF Core を使用せず、Dapper、Cosmos DB 直接接続、または Redis/InMemory で完結させたい軽量マイクロサービスに対しても、不要な EF Core パッケージ群が強制的に参照されていた。
3. **テスタビリティの低下**:
   ドメイン層の単体テストにおいて、EF Core のモックやインメモリ DB の振る舞いの差異に依存せざるを得ず、純粋なインターフェースによる高速な検証が困難であった。

## 2. Problem Statement (問題定義)

物理的な ORM（EF Core）、データベースドライバ、またはクラウド SDK に一切依存せず、ドメイン層およびアプリケーション層が安全に依存できる、純粋な永続化ポート（Ports）を提供する独立した Building Block が必要であった。

## 3. Decision (決定事項)

物理 ORM から独立した永続化抽象レイヤー **「`VK.Blocks.Persistence`」** を新設し、データアクセスのポート（Port）と基底契約を完全に分離する。

### 1. ゼロ外部依存の純粋コア設計 (`ARCH-03`)
- `VK.Blocks.Persistence` は BCL（.NET 基本クラスライブラリ）および `VK.Blocks.Core` のみに依存し、外部 ORM やサードパーティ SDK の参照を一切排除する。
- 永続化操作の成否は例外ではなく `VKResult<T>`（`CS.01`）および `VKPersistenceErrors` により表現する。

### 2. ポート契約の集約 (`ARCH-02`)
- **Unit of Work & トランザクション**: `IVKUnitOfWork`、`IVKUnitOfWork<TContext>`、`IVKTransaction` を定義し、コミット責務を一元管理。
- **監査ポート**: `IVKAuditProvider` を定義し、実行コンテキスト（`IVKUserContext`）および `TimeProvider` と連携（`BASE-02`, `CS.06`）。
- **アウトボックス契約**: 分散トランザクション・イベント送信用に `IVKOutboxStore` および `VKOutboxMessage` を定義。

### 3. 中立的フォールバック実装の同梱 (`ARCH-05`)
- アプリケーションが監査プロバイダーを明示的に登録しない場合でも起動失敗を防ぐため、中立的な `NoOpAuditProvider` を同アセンブリ内に同梱し、`TryAdd` セマンティクスで自己完結登録する。

```
[ Domain / Application Layer ]
            |
            v
[ VK.Blocks.Persistence ]  (Ports: IVKUnitOfWork, IVKEntityRepository<T>, IVKAuditProvider)
            ^
            | (Implements / Adapts)
+-----------+-----------------------+
|                                   |
[ VK.Blocks.Persistence.EFCore ]   [ VK.Blocks.Persistence.EFCore.Cosmos / Sqlite ]
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: `Persistence.EFCore` の中に Abstractions フォルダとして同居
- **Approach**: 単一プロジェクト内で名前空間のみを分け、インターフェースを定義する。
- **Rejected Reason**: パッケージ参照の強制を排除できず、Core レイヤーへの ORM 汚染を防ぐ物理的ガードレール（コンパイル時強制 `BASE-03`）が働かないため却下。

### Option 2: すべての抽象を `VK.Blocks.Core` に集約
- **Approach**: 永続化関連のインターフェースもすべて最下層の `Core` に配置する。
- **Rejected Reason**: `Core` が不要に肥大化し（God Block 化）、永続化を必要としない純粋なユーティリティやアルゴリズムブロックまで不要なリポジトリ抽象を抱え込むことになるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全なポート＆アダプター分離 (`ARCH-02`)**: ドメインロジックは `VK.Blocks.Persistence` のインターフェースのみに依存し、将来の ORM 差し替えやモックが容易になる。
- **バイナリフットプリントの極小化**: EF Core を使わないサービスは、軽量な抽象契約のみを参照できる。

### Negative
- **プロジェクト数とパッケージ管理の増加**: プロジェクトが 1 つ増えることによる管理コスト。

### Mitigation
- `Directory.Build.props` および中央パッケージ管理（CPM）によりビルド設定とバージョンを一元化する。

## 6. Implementation & Security
- **Security**: 監査情報（`CreatedBy`, `UpdatedBy`）は `IVKUserContext` から取得し、クライアントによる改ざんを防御（`FS-07`）。
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence`

