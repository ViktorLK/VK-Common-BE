# Coding Standards (開発標準)

本プロジェクトで採用しているコーディング規約とアーキテクチャ標準について記述します。

## 1. Code Style (コードスタイル)

- **C# Version**: C# 12.0 最新機能を積極的に採用する。
- **Namespace**: File-scoped namespace (`namespace MyNamespace;`) を使用する。
- **Nullable Reference Types**: 全プロジェクトで `Enabled` に設定し、`null` 安全性を保証する。
- **Async/Await**:
    - すべての I/O 操作は非同期とする。
    - ライブラリコードでは `ConfigureAwait(false)` を徹底する。
    - 可能であれば `ValueTask` を使用してアロケーションを削減する。

## 2. Architecture (アーキテクチャ)

### Clean Architecture & DDD

- **Dominant Layer**: ドメイン層（Core）が中心であり、インフラストラクチャ層（Persistence）はドメインに依存する。
- **Repository Pattern**:
    - `IReadRepository` (Query) と `IWriteRepository` (Command) を分離する (CQS)。
    - `IQueryable` は Repository 外部に公開しない（将来的な方針）。
- **Dependency Injection**:
    - コンストラクタ注入を原則とする。
    - `IServiceCollection` 拡張メソッドを提供し、利用者が 1 行で登録できるようにする (`AddVKDbContext`)。

## 3. Testing (テスト)

- **Unit Testing**: xUnit を使用。
- **Integration Testing**: Testcontainers または SQLite In-Memory を使用して、実際のデータベース挙動に近いテストを行う。
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` 形式を推奨。

## 4. Documentation (ドキュメント)

- **Language**: 原則として英語（コードコメント）または日本語（設計書）。
- **XML Documentation**: 公開 API (`public` メソッド/クラス) には必ず `<summary>` を記述する。
