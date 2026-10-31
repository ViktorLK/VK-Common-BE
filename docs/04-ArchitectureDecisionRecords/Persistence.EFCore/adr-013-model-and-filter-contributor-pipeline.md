# ADR 013: Pluggable Model Configuration and Query Filter Contributor Pipeline

- **Date**: 2026-08-23
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence.EFCore

---

## 1. Context (背景)

当初の `VKBaseDbContext` では、`OnModelCreating` メソッド内に以下のようなモデル構成やグローバルフィルターがハードコードされていた：
- マルチテナント（`TenantId`）のグローバルクエリフィルター
- 論理削除（`ISoftDelete`）のフィルター適用
- 楽観的排他制御トークン（`RowVersion`）の設定
- 監査プロパティの列マッピング

この一枚岩アプローチには以下のアーキテクチャ上の課題があった：
1. **開放閉鎖原則 (OCP, `ARCH-11`) の違反**:
   新しい横断的関心事（例: 外部の `MultiTenancy` モジュールや特定 DB 固有の規約）を追加するたびに、基底の `VKBaseDbContext` を修正しなければならなかった。
2. **モジュール間の横方向結合 (`ARCH-01`)**:
   `VK.Blocks.Persistence.EFCore` が本来知るべきでない他の BuildingBlock の具象型やロジックを参照せざるを得ず、単方向レイヤリング原則に反していた。
3. **柔軟なオプトイン・オプトアウトの欠如**:
   特定のマイクロサービスで論理削除を無効化したり、独自のグローバルフィルターを追加したりすることが困難であった。

## 2. Problem Statement (問題定義)

`VKBaseDbContext` を変更することなく、各モジュールやアプリケーションが DI コンテナ経由で独立してモデル構成、データ型規約、およびグローバルクエリフィルターを注入・合成できる、プラガブルなパイプライン機構が必要であった。

## 3. Decision (決定事項)

**Contributor パイプラインパターン**を採用し、モデル生成ライフサイクルを 3 つの直交したインターフェースに分解・抽象化する。

### 1. Contributor プロトコルの新設
- **`IVKModelCreatingContributor`**:
  - `ModelBuilder` を受け取り、エンティティのマッピング、インデックス、テーブル構成を拡張。
  - 例: `ConcurrencyModelContributor`, `OutboxModelContributor`, `ColumnOrderingModelContributor`
- **`IVKModelConventionContributor`**:
  - `ModelConfigurationBuilder` を受け取り、データ型レベルの既定変換規約（Conventions）を注入。
  - 例: SQLite の `DateTimeOffsetToStringConverter` 適用など。
- **`IVKGlobalFilterContributor`**:
  - 動的 LINQ 式を構築し、特定のエンティティインターフェース（`ISoftDelete`, `IMultiTenant`）に対するグローバルクエリフィルターを宣言的に注入。
  - 例: `SoftDeleteFilterContributor`

```csharp
public interface IVKModelCreatingContributor
{
    void Configure(ModelBuilder modelBuilder);
}

public interface IVKModelConventionContributor
{
    void ConfigureConventions(ModelConfigurationBuilder configurationBuilder);
}

public interface IVKGlobalFilterContributor
{
    void ApplyFilter(ModelBuilder modelBuilder, Type entityType);
}
```

### 2. `VKBaseDbContext` でのパイプライン実行
`VKBaseDbContext` のライフサイクルにおいて、DI から解決された全 Contributor を順序制御付きで反復実行する：
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // 1. 各モジュールから提供された ModelCreating Contributor の実行
    foreach (var contributor in _modelCreatingContributors)
    {
        contributor.Configure(modelBuilder);
    }

    // 2. Global Filter Contributor による動的フィルター適用
    ApplyGlobalFilters(modelBuilder);
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: EF Core Interceptor での代用
- **Approach**: モデル定義もインターセプター側で動的に制御する。
- **Rejected Reason**: EF Core のインターセプターは実行時（Command/Connection/SaveChange）のフックであり、メタデータモデル（`IModel`）の構築フェーズには介入できないため不可。

### Option 2: `VKBaseDbContext` の仮想メソッド（Template Method）による継承
- **Approach**: 各アプリが `ConfigureModelOverride` などの仮想メソッドをオーバーライドする。
- **Rejected Reason**: サードパーティの Building Block（例: `VK.Blocks.MultiTenancy`）が自己完結型で DI 登録を通じてモデル定義を注入できず、アプリケーション側での手動記述が漏洩するため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な疎結合とプラグイン化**: `MultiTenancy` や `Persistence.EFCore.Sqlite` などの周辺ブロックが、自身の DI 拡張（`BB.03`）で Contributor を登録するだけで透過的に EF Core に統合される。
- **高いテスタビリティ**: テスト環境では特定の Contributor のみを選択して `DbContext` を構成可能。

### Negative
- DI 解決される Contributor の数が増加することによる、初回モデルビルド（Cold Start）時の軽微なオーバーヘッド。

### Mitigation
- EF Core のコンパイル済みモデル（Compiled Models）や、CLR 静的メタデータキャッシュ（`ADR-002`）と併用することで、ランタイムクエリへの影響はゼロ（$0$ ms）。

## 6. Implementation & Status
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence.EFCore/Database/Protocols/`

