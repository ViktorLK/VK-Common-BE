# ADR 015: Deterministic Physical Column Ordering Convention

- **Date**: 2026-09-16
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence.EFCore

---

## 1. Context (背景)

EF Core の既定の動作では、マイグレーション実行時に生成されるテーブルの列順（Column Order）は、C# クラス内のプロパティ宣言順およびクラス継承階層の走査順に依存する。

これにより、以下のような問題が生じていた：
1. **スキーマレイアウトの不統一**:
   テーブルごとに主キー（`Id`）やテナント ID（`TenantId`）の物理的な並び順がばらつき、SQL クエリツール（DBeaver, SSMS 等）や DB 管理者による手動確認時の視認性が低下していた。
2. **監査・論理削除フィールドの散在**:
   基底クラス（`VKEntityBase`）を継承している場合でも、エンティティの派生クラス側で定義された業務カラムの途中に `CreatedAt` や `IsDeleted` が混入するなど、整然としたテーブルレイアウトが維持できなかった。
3. **手動アトリビュート管理の破綻**:
   `[Column(Order = n)]` をすべてのエンティティプロパティに手動で付与する方法は、保守コストが膨大でヒューマンエラーの温床となる（DRY 違反）。

## 2. Problem Statement (問題定義)

すべてのエンティティにおいて、個別のアトリビュート定義を要求することなく、コンパイル時および EF Core マイグレーション生成時に決定論的（Deterministic）かつ直感的な物理列順序を一律で強制する仕組みが必要であった。

## 3. Decision (決定事項)

`IVKModelCreatingContributor`（`ADR-013`）を実装した **`ColumnOrderingModelContributor`** を導入し、全エンティティのプロパティに対して標準列順序インデックスを自動算出・付与する。

### 1. 標準列順序レイアウト規約
物理テーブルにおけるカラム順を以下の 6 つの論理スロット（ゾーン）に分類し、決定論的に整列する：

| ゾーン | 対象プロパティ / 役割 | 優先度 (Column Order) |
| :--- | :--- | :---: |
| **Zone 1: Primary Key** | 主キー（`Id`） | `0` |
| **Zone 2: Tenant Context** | マルチテナント識別子（`TenantId`） | `1` |
| **Zone 3: Domain Properties** | 業務ドメイン固有のプロパティ群（名前順またはアルファベット順） | `100` 〜 |
| **Zone 4: Audit Metadata** | 監査プロパティ（`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`） | `1000` 〜 `1003` |
| **Zone 5: Soft Delete Flags** | 論理削除プロパティ（`IsDeleted`, `DeletedAt`, `DeletedBy`） | `1010` 〜 `1012` |
| **Zone 6: Concurrency Token** | 楽観的排他制御トークン（`RowVersion`, `ConcurrencyToken`） | `2000` |

### 2. メタデータモデルへの自動適用
```csharp
internal sealed class ColumnOrderingModelContributor : IVKModelCreatingContributor
{
    public void Configure(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            ApplyOrdering(entityType);
        }
    }
}
```
- 各プロパティのメタデータに対して `property.SetColumnOrder(calculatedOrder)` を呼び出すことで、EF Core のマイグレーション生成エンジンに標準列順序を反映する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: 個別エンティティクラスへの `[Column(Order = n)]` 手動付与
- **Approach**: 各開発者がモデル定義時にアトリビュートを記述する。
- **Rejected Reason**: 列順序番号の重複や抜け漏れ、基底クラスとの競合が頻発するため却下。

### Option 2: データベース側の列順序を不問（成り行き任せ）とする
- **Approach**: 物理列順序を制御せず、EF Core のデフォルトの並び順を受け入れる。
- **Rejected Reason**: エンタープライズ RDBMS において、頻繁にアクセスされる主キーやテナントキーがテーブル先頭にまとまっていることによる物理 I/O の効率化、およびデータ監査・運用メンテナンス性の観点から却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全なスキーマ統一性**: システム内の全テーブル（数十〜数百テーブル）で、`Id`, `TenantId` が常に先頭に、監査・削除・バージョン管理が常に末尾に配置される。
- **開発者の認知負荷ゼロ**: アプリケーション開発者は列順を一切意識せず、ビジネスプロパティの定義に集中できる。

### Negative
- 既存のマイグレーションが存在するテーブルに対して適用する場合、列順変更のための `ALTER TABLE` またはテーブル再構築スクリプトが生成される可能性がある。

### Mitigation
- 新規テーブル作成時に即座に適用し、既存テーブルについては破壊的影響のない DBMS（PostgreSQL 等）または新規環境構築時から段階的に導入する。

## 6. Implementation & Status
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence.EFCore/Database/Internal/ColumnOrderingModelContributor.cs`

