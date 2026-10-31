# ADR 002: Standardized DateTimeOffset Storage via Model Convention (Abandoning EF1001 Internal APIs)

- **Date**: 2026-09-04
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence.EFCore.Sqlite

---

## 1. Context (背景)

SQLite はネイティブの `DATETIME` や `TIMESTAMP WITH TIME ZONE` 型を持たず、日付・時刻データを `TEXT`（ISO 8601 文字列）、`REAL`（ユリウス日）、または `INTEGER`（Unix タイムスタンプ）として格納する。

一方、エンタープライズ共通基盤（`VK.Blocks`）では、監査日時やビジネスイベント時刻にタイムゾーン情報を含む `DateTimeOffset` を標準型として採用している（`FS-05`）。

初期の実装では、EF Core の LINQ クエリにおいて `DateTimeOffset` の比較やソートを正しく SQL に変換するため、EF Core の内部プライベート API である `IQueryableMethodTranslatingExpressionVisitorFactory` を拡張した `VKSqliteQueryableMethodTranslatingExpressionVisitorFactory` を登録していた。

```csharp
// ❌ 過去の内部 API 依存（EF1001 警告を抑制）
#pragma warning disable EF1001
internal sealed class VKSqliteQueryableMethodTranslatingExpressionVisitorFactory
    : RelationalQueryableMethodTranslatingExpressionVisitorFactory
{
    // EF Core の内部式ツリーをフックして式を動的に書き換えていた
}
```

このアプローチには以下の重大な課題が存在した：
1. **EF Core バージョンアップ耐性の喪失**: EF Core のマイナーバージョンアップでも内部式トランスレーターの仕様変更によりコンパイル不能やランタイム例外を引き起こすリスクがあった。
2. **コンパイラ警告の常態化**: `EF1001`（Internal EF Core API usage）の警告抑制が必須となり、コード品質ゲートの信頼性を低下させていた。

## 2. Problem Statement (問題定義)

EF Core の内部非公開 API に一切依存せず、SQLite 上で `DateTimeOffset` の保存、SQL `ORDER BY` ソート、およびカーソルページネーション（`ADR-003`）が安全かつ決定論的に機能する標準的な変換機構が必要であった。

## 3. Decision (決定事項)

内部式ツリー訪問器を全廃し、EF Core 8/9/10 公式の **モデル構成規約（Model Convention）と `DateTimeOffsetToStringConverter`** による標準化へ移行する。

### 1. `SqliteModelConventionContributor` の導入 (`AP.01`, `CS.08`)
- `IVKModelConventionContributor` を実装し、すべての `DateTimeOffset` および `DateTimeOffset?` プロパティに対して一律で `DateTimeOffsetToStringConverter` を適用する。
- 保存される形式は ISO 8601 互換の文字列となり、SQL レベルの文字列辞書順ソート（Lexicographical Order）と時系列順序が完全に一致する（UTC 基準）。

```csharp
internal sealed class SqliteModelConventionContributor : IVKModelConventionContributor // [AP.01] [CS.08]
{
    public void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToStringConverter>();
    }
}
```

### 2. カーソル式プロバイダーの正規化 (`VKOffsetDateTimeCursorExpressionProvider`)
- カーソルページネーション時の動的ラムダ式書き換えにおいて、内部トランスレーターではなく EF Core 公式の `EF.Property<string>(param, memberName)` 構文への動的バインドを採用する。

### 3. 内部 API の完全削除
- `VKSqliteQueryableMethodTranslatingExpressionVisitorFactory` をコードベースから完全に削除し、`ReplaceService` 呼び出しを撤廃する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: 従来の `EF1001` 内部 API 訪問器を維持
- **Approach**: 内部トランスレーターのパッチを当て続け、EF Core バージョンアップのたびに保守する。
- **Rejected Reason**: .NET 10 への移行時における破壊的変更リスクが高すぎ、フレームワークの安定性原則（`BASE-03`, `BASE-07`）に反するため却下。

### Option 2: Unix タイムスタンプ（ミリ秒数値）として保存
- **Approach**: `long` 数値として保存し、数値比較でソートする。
- **Rejected Reason**: SQLite データベースファイルを直接閲覧・保守（GUI ツール）する際に日時の視認性が著しく失われ、タイムゾーンオフセット情報が欠落するため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **`EF1001` 警告の完全排除**: サポート対象の公式 API（`ModelConfigurationBuilder`）のみを使用するため、今後の .NET / EF Core アップグレードが極めて安全になる。
- **後方互換性の維持**: 既存の SQLite データベース内に保存されている ISO 8601 文字列データと完全な互換性を保持。
- **クエリの予測可能性**: 文字列形式の比較により、インデックススキャンおよび `ORDER BY` が高速かつ決定論的に実行される。

### Negative
- 文字列保存によるストレージサイズが、数値型（`INTEGER`）保存に比べてわずかに増加する。

### Mitigation
- SQLite の主たる利用用途はローカル開発、単体テスト、エッジ環境であり、ディスク容量よりもデバッグ容易性と安定性を最優先とする。

## 6. Implementation & Status
- **Status**: ✅ Accepted
- **Implemented In**: `src/BuildingBlocks/Persistence.EFCore.Sqlite/Internal/SqliteModelConventionContributor.cs`

