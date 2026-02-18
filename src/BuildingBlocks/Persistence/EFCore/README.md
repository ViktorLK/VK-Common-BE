# VK.Blocks.Persistence.EFCore

![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)

## はじめに

**VK.Blocks.Persistence.EFCore** は、モダンな .NET アプリケーションにおけるデータ永続化層のベストプラクティスを探求・実装するために設計された、堅牢かつ柔軟なライブラリです。

ドメイン駆動設計 (DDD) の原則に基づき、保守性、テスト容易性、そしてパフォーマンスを重視したアーキテクチャを提供します。Entity Framework Core の強力な機能を最大限に活用しつつ、複雑なビジネスロジックを支えるための抽象化とユーティリティを含んでいます。

## アーキテクチャ

本プロジェクトは、以下のアーキテクチャ原則とパターンを採用し、スケーラブルで疎結合なシステム設計を実現しています。

- **Design Principles**: SOLID原則、DRY (Don't Repeat Yourself)、KISS (Keep It Simple, Stupid)
- **Architectural Patterns**:
    - **Repository Pattern (Generic)**: データアクセスロジックを抽象化し、ビジネスロジックとの結合度を低減。
    - **CQS (Command Query Separation)**: `IReadRepository` (読み取り専用) と `IWriteRepository` (書き込み用) を明確に分離し、責務の明確化とパフォーマンス最適化（NoTrackingの強制等）を実現。
    - **Unit of Work**: 複数のデータベース操作を単一のトランザクションとして扱い、整合性を保証。
    - **Interceptor Pattern**: 監査 (Auditing) や論理削除 (Soft Delete) などの横断的関心事を通常操作において透過的に処理。
- **Domain-Driven Design (DDD)**: エンティティライフサイクルの管理とドメインイベントの統合（準備中）。

## 主な機能

本ライブラリは、エンタープライズグレードのアプリケーション開発に必要な以下の機能を提供します。

### 1. 高度な Repository 実装

- **CRUD操作の完全サポート**: 非同期処理を前提とした `Add`, `Update`, `Delete` およびその Range 操作。
- **柔軟なクエリ**: `Expression<Func<T, bool>>` による型安全な検索、`Include` による関連データの読み込み、`AsNoTracking` による読み取り最適化。
- **バルク操作**: EF Core 7+ の `ExecuteUpdateAsync`, `ExecuteDeleteAsync` をラップし、大量データの高速処理を実現。

### 2. 高性能なページネーション

- **Offset-based Pagination**: 標準的なページ番号とサイズによるページネーション。
- **Cursor-based Pagination**: 大規模データセットに対しても高速で安定したページ送り（前方/後方スクロール対応）を実現するカーソルページネーション。
- **Cursor Serializer Abstraction**: `ICursorSerializer` インターフェースにより、シリアライズ戦略を DI で差し替え可能。開発・テスト用の `SimpleCursorSerializer` と、HMAC-SHA256 署名・スキーマバージョン管理・有効期限を備えた本番用 `SecureCursorSerializer` を提供。

### 3. 横断的関心事の自動化 (Interceptors)

- **Auditing (監査ログ)**: エンティティの作成者、作成日時、更新者、更新日時を自動的に記録。
- **Soft Delete (論理削除)**: データを物理的に削除せず、削除フラグにより不可視化。クエリ時には自動的に除外 (Global Query Filter)。

### 4. その他

- **IAsyncEnumerable サポート**: `StreamAsync` により、大量データをメモリ効率よくストリーミング処理可能。
- **Concurrency Control**: 楽観的同時実行制御のサポート。

## 実装のハイライト

本ライブラリは、アーキテクチャ監査において以下の点が特に高く評価されています。

- **Advanced Bulk Operations (Hybrid Auditing)**: 通常の CRUD 操作は `Interceptors` で、ChangeTracker をバイパスするバルク操作は `IEntityLifecycleProcessor` で明示的に処理する「ハイブリッド戦略」を採用。これにより、パフォーマンスを犠牲にすることなく、すべての操作で監査ログと論理削除の整合性を保証しています。
- **High-Performance Metadata Caching**: `EfCoreTypeCache` などの静的ジェネリックキャッシュを活用し、リフレクションのオーバーヘッドを最小化。スレッドセーフかつロックフリーな実装により、高並列環境下でも安定したパフォーマンスを発揮します。
- **Dynamic Global Query Filters**: リフレクションを活用し、`ISoftDelete` などのインターフェースを実装したエンティティに対してグローバルクエリフィルタを自動適用。手動設定の手間を省き、設定漏れによるデータ漏洩リスクを排除しています。
- **Secure Cursor Serialization**: `ICursorSerializer` の Strategy パターンにより、開発環境と本番環境でシリアライズ実装を DI で切り替え。`SecureCursorSerializer` は HMAC-SHA256 署名と `CryptographicOperations.FixedTimeEquals` によるタイミング攻撃対策を実装しています。
- **Async Best Practices**: `ValueTask` や `ConfigureAwait(false)` の徹底、および `CancellationToken` の完全な伝播により、高負荷時でもスレッドプールを効率的に利用する設計となっています。

## 採用技術

本ライブラリは、**「依存関係の最小化 (Minimal Dependencies)」** を原則として設計されています。
サードパーティ製ライブラリへの依存を排除することで、導入時のバージョン競合リスクを抑え、プロジェクトのクリーンなアーキテクチャを維持します。

- **Core Framework**: .NET 8.0 / 9.0
- **ORM**: Entity Framework Core 8.0 / 9.0
- **Abstractions**:
    - `Microsoft.Extensions.Logging.Abstractions`
    - `Microsoft.Extensions.DependencyInjection.Abstractions`

※ MediatR, FluentValidation, Serilog, OpenTelemetry 等は、本ライブラリ自体には含まれていませんが、これらを使用するアプリケーションとシームレスに統合できるように設計されています。

## 開始方法

### 1. インストール

本ライブラリは現在 NuGet で公開されていません。ソリューション内のプロジェクト参照として追加してください。

```bash
# プロジェクトファイルがあるディレクトリに移動して実行
dotnet add [<PROJECT_NAME>.csproj] reference <PATH_TO_VK_BLOCKS>/VK.Blocks.Persistence.EFCore.csproj
```

### 2. サービスの登録

`Program.cs` または `Startup.cs` にて、`AddVKDbContext` を使用して DbContext を登録します。

```csharp
using VK.Blocks.Persistence.EFCore.DependencyInjection;

services.AddVKDbContext<MyDbContext>(
    persistenceOptions =>
    {
        persistenceOptions.EnableAuditing = true;   // 監査機能を有効化
        persistenceOptions.EnableSoftDelete = true; // 論理削除を有効化
    },
    dbContextOptions =>
    {
        dbContextOptions.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
    });
```

### 3. カーソルシリアライザーの設定（本番環境）

デフォルトでは `SimpleCursorSerializer`（署名なし）が登録されます。**本番環境では必ず `AddSecureCursorSerializer` を呼び出してください。**

```csharp
// 本番環境: HMAC-SHA256 署名 + 有効期限付きカーソルトークン
services.AddSecureCursorSerializer(opts =>
{
    opts.SigningKey = configuration["CursorSerializer:SigningKey"]; // Azure Key Vault 推奨
    opts.DefaultExpiry = TimeSpan.FromHours(1);
});
```

> ⚠️ `AddSecureCursorSerializer` を呼び忘れると、本番環境でカーソルが改ざん可能な状態になります。

### 4. リポジトリの使用

コンストラクタインジェクションにより `IBaseRepository<TEntity>` を注入して使用します。

```csharp
public class ProductService(IBaseRepository<Product> repository)
{
    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await repository.GetListAsync(x => x.IsActive);
    }
}
```

### 5. 実践的な使用例

#### Cursor-based Pagination

大規模なデータセットを効率的にページングします。

```csharp
// Products テーブルを Price の昇順でカーソルページネーション
var result = await repository.GetCursorPagedAsync(
    predicate: x => x.Category == "Electronics",
    cursorSelector: x => x.Price,
    cursor: null, // 初回は null, 2ページ目以降は cursor 値を指定
    pageSize: 20
);

var nextCursor = result.NextCursor;
```

#### Bulk Operations with Auditing

一括更新・削除を行いながら、監査ログ (UpdatedBy, UpdatedAt) や論理削除 (SoftDelete) を自動的に処理します。

```csharp
// 条件に一致する製品の価格を一括更新（監査情報は自動記録されます）
await repository.ExecuteUpdateAsync(
    predicate: x => x.Category == "OldModels",
    setPropertyAction: setter => setter.SetProperty(p => p.Price, p => p.Price * 0.9m)
);

// 条件に一致する製品を一括論理削除（SoftDelete有効時）
await repository.ExecuteDeleteAsync(
    predicate: x => x.StockQuantity == 0
);
```

## 今後の展望

アーキテクチャ監査で指摘された課題に基づき、以下の改善を計画・実施しています。

**✅ 実装済み**

- ~~**Robust Cursor Implementation**~~: `ICursorSerializer` インターフェースの導入により、カーソルのバージョン管理・HMAC-SHA256 署名・有効期限管理を実装済み。([ADR-009](../../../docs/02-ArchitectureDecisionRecords/EFCore/adr-009-cursor-serializer-abstraction.md))

**📋 計画中**

- **Strengthen Abstractions**: Specification パターンを導入することで `IQueryable` への直接的な依存を排除し、インフラの詳細がドメイン層に漏れ出さない「防腐層」としての機能を強化する。
- **Complex Bulk Logic Simplification**: バルク更新ロジックの内部実装をカプセル化し、外部ライブラリの破壊的変更に対する耐性を高める。
- **Multi-Tenancy Support**: テナントごとのデータ分離機能の拡充。
- **Second Level Cache**: Redis 等を使用したクエリキャッシュの統合。
- **Event Sourcing**: ドメインイベントの永続化とイベントソーシングパターンの探索。
- **Resilient Execution Strategy**: クラウド環境における一時的な接続障害（Transient Faults）を考慮し、EF Core の `IExecutionStrategy` を活用した自動再試行メカニズムを統合。
- **Unsafe Mode (Admin Access)**: 論理削除フィルタや監査ログの自動挿入を一時的にバイパスする API を提供。データ復旧・管理者メンテナンス・特殊バッチ処理に対応する。
