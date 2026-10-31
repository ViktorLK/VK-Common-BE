# ADR 002: Role-Segregated Repository Protocols (ISP & CQS Separation)

- **Date**: 2026-08-25
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence

---

## 1. Context (背景)

初期の設計では、標準的な `IRepository<T>` インターフェースにすべての CRUD メソッド、一括処理（Bulk）、および特殊なシステム処理が集約されていた。しかし、実際の業務マイクロサービス開発において以下の問題が発生した：

1. **インターフェース分離原則 (ISP) の違反**:
   読み取り専用のクエリハンドラーや参照系 API においても、更新（Update）、削除（Delete）、一括挿入（BulkInsert）などの不要な変更メソッドが可視化されていた。
2. **Change Tracking とパフォーマンスの相反**:
   読み取り処理ではパフォーマンス最大化のために No-Tracking（`AsNoTracking`）が原則であるのに対し、書き込み・更新処理ではエンティティの変更追跡（Change Tracker）が不可欠である。単一のインターフェースでは開発者が誤って追跡付きの参照を行い、メモリ枯渇や不要な同期オーバーヘッドを招くリスクがあった。
3. **システム・ワーカー処理の権限越境**:
   バックグラウンドワーカーやシステムバッチでは、マルチテナント分離（`TenantId`）や論理削除（`ISoftDelete`）のグローバルフィルターを意図的にバイパスして処理を行う必要がある。これを通常のリポジトリに混在させると、一般業務ロジックから誤ってフィルターバイパスが呼び出されるセキュリティ脆弱性につながる。

## 2. Problem Statement (問題定義)

CQS（コマンド・クエリ分離）の原則を徹底し、開発者の意図（読み取り、変更、一括処理、システム特権処理）に応じてコンパイル時（`BASE-03`）に操作面を制限できる、直交的で型安全なリポジトリプロトコル体系が必要であった。

## 3. Decision (決定事項)

リポジトリ契約を単一の肥大化したクラスから、役割に応じた 4 つの独立したプロトコルへ分離・階層化する。

### 1. 役割特化プロトコルの定義
- **`IVKEntityReadRepository<TEntity>`**:
  - 読み取り専用。仕様パターン（`ISpecification`）、ページネーション、射影クエリを提供。
  - 追跡（Tracking）はデフォルトで無効（`VKQueryTracking.NoTracking`）。
- **`IVKEntityWriteRepository<TEntity>`**:
  - 変更専用。`AddAsync`、`UpdateAsync`、`DeleteAsync` のみを公開。
  - 単一エンティティの変更ライフサイクルを Unit of Work の保護下で管理。
- **`IVKEntityBulkRepository<TEntity>`**:
  - 大量データの一括処理専用。`BulkInsertAsync`、`BulkUpdateAsync`、`BulkDeleteAsync` を提供。
  - Change Tracker をバイパスし、高スループットなバッチ書き込みを実現。
- **`IVKEntitySystemRepository<TEntity>`**:
  - 特権ワーカー専用。テナントフィルターや論理削除フィルターを解除した低レベルアクセスを提供。
  - 一般の Application サービスへの注入は禁止し、専用のシステムジョブからのみ注入。

### 2. 一般 CRUD 用の合成インターフェース
- 標準的な集約ルート（Aggregate Root）の管理で読み書き双方を必要とするユースケースのため、合成プロトコルを提供：
  ```csharp
  public interface IVKEntityRepository<TEntity> :
      IVKEntityReadRepository<TEntity>,
      IVKEntityWriteRepository<TEntity>
      where TEntity : class
  {
  }
  ```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 従来の単一 `IRepository<T>` を維持
- **Approach**: すべてのメソッドを 1 つのインターフェースにまとめ、オプションフラグ（`bool asNoTracking = true` 等）で制御する。
- **Rejected Reason**: 引数の誤指定によるヒューマンエラーを防げず、コードレビュー時に「このハンドラーがデータを変更するかどうか」を型シグネチャから判断できないため却下。

### Option 2: リポジトリを完全廃止し、直接 `IQueryable<T>` を公開
- **Approach**: Repository を介さず、Application 層で直接 `DbContext.Set<T>()` を操作する。
- **Rejected Reason**: ORM 実装への強い結合が生じ、`ARCH-02`（ポート＆アダプター）に反する。また、監査ログや論理削除のインターセプトが各所に分散するため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **コンパイル時の安全性 (`BASE-03`)**: 参照系ハンドラーに `IVKEntityReadRepository` を注入することで、誤った `Update` 呼び出しをコンパイルエラーとして検知。
- **ゼロアロケーション・パフォーマンス向上**: 参照系において安全に No-Tracking パスを強制できる。
- **システム特権の保護**: フィルターバイパス操作の呼び出し箇所を静的に監査可能。

### Negative
- **DI 登録インターフェースの増加**: DI コンテナに登録・解決する型が増加する。

### Mitigation
- `Persistence.EFCore` 側で `EfCoreRepository<T>` が複数インターフェースを一括実装し、DI 登録のボイラープレートを最小化する。

## 6. Implementation & Security
- **Security**: `IVKEntitySystemRepository` の使用箇所はコードレビューおよび静的解析で厳格に監査。
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence/Repositories/Protocols/`

