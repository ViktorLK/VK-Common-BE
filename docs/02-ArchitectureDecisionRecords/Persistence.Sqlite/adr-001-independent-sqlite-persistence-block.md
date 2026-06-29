# ADR 001: Independent Sqlite Persistence Block

- **Date**: 2026-06-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Persistence.Sqlite

## 1. Context (背景)

VK.Blocks の永続化レイヤー（`Persistence.EFCore`）は、データアクセスやトランザクション、監査（Auditing）などのコア機能を提供する。しかし、特定の物理データベース（SQL Server、PostgreSQL、Cosmos DB、SQLite 等）への接続定義や設定ロジックをこの共通コアプロジェクトに混在させると、以下の問題が生じる：
1. **不要なバイナリ依存関係の強制**: ローカル開発用のインメモリ/ファイルベースの SQLite のみを使用したいマイクロサービスに対しても、不要な SQL Server や Cosmos DB のパッケージライブラリが依存関係として強制的に取り込まれる。
2. **構成コードの肥大化と複雑化**: 異なるデータベース特有の設定条件分岐（接続文字列の形式、プロバイダ固有のマイグレーションアセンブリ指定など）が一つの DI 拡張メソッドに集中し、保守性が低下する。

## 2. Problem Statement (問題定義)

各マイクロサービスがそれぞれの動作環境（ローカル開発、オンプレミス、クラウド）に合わせた最適なデータベースプロバイダを選択でき、かつ不要なライブラリ依存関係の汚染を完全に排除する、疎結合なデータベースプラグインアーキテクチャが必要であった。

## 3. Decision (決定事項)

SQLite データベースを使用するための独立した Building Block **「`VK.Blocks.Persistence.Sqlite`」**を新規に定義し、構成と依存関係を分離する。

### 1. 独立したアセンブリ設計
- コアの永続化モデルや基底 DbContext は `Persistence.EFCore` に維持し、SQLite 固有の EF Core プロバイダ参照 (`Microsoft.EntityFrameworkCore.Sqlite`) や接続ロジックを本ブロックに完全に封じ込める。

### 2. 冪等な DI 登録とバリデーション (BB.03 の適用)
- `VKPersistenceSqliteBlockExtensions` を用意し、SQLite 専用のビルダ・オプション群を登録する。
- 起動時にオプション設定を検証する `SqliteOptionsValidator` を定義し、無効な接続文字列やパスが指定されている場合に早期にプロセスを失敗させる。

### 3. SQLite 専用構成器 (`SqliteDbContextOptionsConfigurator`)
- `IDbContextOptionsConfigurator` を実装し、外部（`Persistence.EFCore`）からインジェクションされることで、動的に DbContext に対して `.UseSqlite()` を安全に適用する。

```
[Microservice App]
       |
       +--> Reference: VK.Blocks.Persistence.Sqlite (Only SQLite packages imported)
       |
       +--> Call: AddVKPersistenceSqliteBlock()
                   |
                   +--> Registers SqliteDbContextOptionsConfigurator
                   +--> Applies .UseSqlite() to VKBaseDbContext
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 共通 Persistence.EFCore にすべて内包
- **Approach**: `Persistence.EFCore` 内で `#if` プリプロセッサや実行時オプション判定を用いて、すべてのデータベースプロバイダの構成を一枚岩で提供する。
- **Rejected Reason**: SQLite 以外の余分な NuGet パッケージ（Cosmos DB 等）がすべてのプロジェクトに強制インポートされることになり、モジュール分離の原則に反するため却下。

### Option 2: アプリケーション側で DbContext 設定を手動記述
- **Approach**: Building Block としてプロバイダを提供せず、各アプリの `Startup.cs` / `Program.cs` で直接 `.UseSqlite()` などを呼び出して設定する。
- **Rejected Reason**: システム全体で統一すべき監査インターセプタやマイグレーションアセンブリ指定などの標準設定が各アプリに散散し、実装漏れや重複コードの温床となるため却下。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **依存の極小化**: SQLite しか使わないアプリは、Cosmos DB や他の巨大なクラウドストレージ用 SDK に一切依存しなくなる。
- **プラグイン化の実現**: データベース構成が疎結合になり、開発環境は SQLite、本番環境は SQL Server や Cosmos DB といった環境切り替えが Building Block の差し替えだけで可能になる。

### Negative
- **新規 Block プロジェクトの増加**: プロバイダごとにプロジェクト数が増え、管理すべき NuGet パッケージの管理簿（`Directory.Packages.props`）が肥大化する。

### Mitigation
- 中央の `Directory.Packages.props` ですべてのバージョンを一元管理し、アセンブリ間の依存関係を厳密に監査する。

## 6. Implementation & Security (実装详细とセキュリティ考察)

- SQLite の接続文字列に含まれるパスワードやセンシティブな認証情報は環境変数や Secret Manager 等から動的に注入する。
- 接続文字列の妥当性は、起動時に `SqliteOptionsValidator` によって徹底的に検証される。

## 7. Status
✅ Accepted
