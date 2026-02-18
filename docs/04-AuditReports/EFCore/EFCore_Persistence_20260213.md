# タスク: 詳細アーキテクチャ監査報告書 - src\BuildingBlocks\Persistence\EFCore

## 📊 総合スコア (Total Score: 85/100)

**評価理由**:
本プロジェクトは、非常に堅牢な Entity Framework Core のカプセル化を示しており、柔軟性と抽象化のバランスが見事に取れています。コード構造は明確で、EF Core の最新機能（インターセプター、一括更新/削除など）を効果的に活用しています。主な減点要因は、一部のクラスへの責任の集中（God Class の傾向）、ファイル名のエラー、および潜在的な並行処理/状態管理のリスクにあります。これらは修正が必要ですが、全体的な品質は高いと判断されます。

---

## ✅ アーキテクチャの強み (Architectural Strengths)

### 1. 現代的なパフォーマンス最適化 (Modern Performance Optimizations)

- **原理/パターン**: CQRS / パフォーマンス最適化
- **ビジネス価値**:
    - `EfCoreRepository` は `ExecuteUpdateAsync` と `ExecuteDeleteAsync` を実装しており、EF Core 7 以降の一括操作機能を活用しています。これにより、従来の「取得して更新/削除」パターンを回避し、大規模データ処理のパフォーマンスを劇的に向上させています。
    - `AsNoTracking` の簡単な利用方法を提供しており、読み取り専用操作の高速化を実現しています。

### 2. 強力な横断的関心事の処理 (AOP via Interceptors)

- **原理/パターン**: インターセプターパターン / AOP (アスペクト指向プログラミング)
- **ビジネス価値**:
    - [`AuditingInterceptor`](/src/BuildingBlocks/Persistence/EFCore/Interceptors/AuditingInterceptor.cs) と [`SoftDeleteInterceptor`](/src/BuildingBlocks/Persistence/EFCore/Interceptors/SoftDeleteInterceptor.cs) により、監査と論理削除のロジックがビジネスロジックから分離されています。
    - これにより単一責任の原則 (SRP) と開放/閉鎖原則 (OCP) に準拠し、これらの機能が独立して進化でき、コアビジネスコードを汚染しません。

### 3. 型安全なメタデータキャッシュ (Type-Safe Metadata Caching)

- **原理/パターン**: リフレクション / メモ化
- **ビジネス価値**:
    - `EfCoreCache<T>` クラスは、`IsAuditable`、`IsSoftDelete` などのメタデータや、リフレクションで生成された `MethodInfo` を巧みにキャッシュしています。
    - 操作ごとのリフレクションオーバーヘヘッドを回避するこの実装は、非常に専門的な企業レベルの最適化であり、高負荷時のスループットを維持します。

### 4. 柔軟なカーソルページネーション (Cursor-based Pagination)

- **原理/パターン**: イテレータパターン / ページネーション戦略
- **ビジネス価値**:
    - `GetCursorPagedAsync` は効率的なカーソルページネーションを提供し、前後への移動をサポートします。
    - 従来のオフセットページネーションと比較して、大規模データセットにおいても一貫したパフォーマンスを提供します（`Skip` によるパフォーマンス低下を回避）。

### 5. モジュール化された依存性注入 (Modular Dependency Injection)

- **原理/パターン**: 依存性注入 / 拡張メソッド
- **ビジネス価値**:
    - `ServiceCollectionExtensions` は明確な `AddVKDbContext` メソッドを提供し、複雑な Context、Interceptor、Repository の登録ロジックをカプセル化しています。
    - .NET Core の標準的な慣習に従っており、使用者の構成の複雑さを大幅に低減し、開発効率を向上させます。

---

## ⚠️ アーキテクチャ上のリスク (Architectural Risks & Smells)

### 1. ファイル命名エラー (File Naming Error)

- **違反**: 一般的なコーディング規約
- **リスク**:
    - ファイル `IUnsafeContext .cs` が存在します（`.cs` の前のスペースに注意）。
    - **ビジネス影響**: 一部のファイルシステムツール、スクリプト、またはビルドシステムで予測不可能なエラーが発生する可能性があり、プロフェッショナルさに欠ける印象を与えます。

### 2. リポジトリの状態保持リスク (Stateful Repository Risk)

- **違反**: ステートレスサービス原則
- **リスク**:
    - `EfCoreRepository.Unsafe()` メソッドは `_isUnsafeMode` フィールドを `true` に設定します。
    - **ビジネス影響**: Repository は通常 Scoped ライフサイクルです。同一スコープ内（例: 同一リクエスト）で `Unsafe()` と通常のクエリが続けて呼び出されると、後続の通常クエリが意図せずグローバルフィルター（論理削除など）を無視する可能性があり、深刻なデータ漏洩や論理エラーにつながります。

### 3. リポジトリの過剰な責務 (Large Class / God Object Check)

- **違反**: 単一責任の原則 (SRP)
- **リスク**:
    - `EfCoreRepository` は CRUD、一括操作、2種類のページネーション（Offset/Cursor）、生 SQL、ストリーミング処理などを実装しています。
    - **ビジネス影響**: クラスの肥大化により、保守コストが増加し、バグの温床になりやすくなります。ジェネリックリポジトリでは一般的ですが、注意が必要です。

### 4. UnitOfWork の実装の複雑さ (Complex UnitOfWork Disposal)

- **違反**: KISS 原則 (Keep It Simple, Stupid)
- **リスク**:
    - `UnitOfWork` は `IDisposable` と `IAsyncDisposable` を実装しており、内部で Repository の Dispose を手動管理しています。
    - **ビジネス影響**: この二重 Dispose パターンはエラーが発生しやすく、EF Core の `DbContext` がすでに接続ライフサイクルを管理しているため過剰設計です。「オブジェクトは既に破棄されています」という実行時例外を引き起こすリスクがあります。

---

## 💡 演進ロードマップ (Evolutionary Roadmap)

### 1. [Critical] ファイル名の修正

- **提案**: `IUnsafeContext .cs` を即座に `IUnsafeContext.cs` にリネームしてください。
- **コスト/効果**: 低コストで即座にリスクを排除できます。

### 2. [High] `Unsafe()` パターンのリファクタリング

- **提案**: 状態を持つ `Unsafe()` メソッドを廃止してください。
- **推奨案**:
    - **案 A**: 現在のインスタンスの状態を変更するのではなく、新しい `UnsafeEfCoreRepository` ラッパーまたはインスタンスを返す。
    - **案 B**: 具体的なメソッドの引数で `QueryOptions` を通じてフィルター無視を伝達し、リポジトリをステートレスに保つ。

### 3. [Medium] リポジトリインターフェースの分割 (Interface Segregation)

- **提案**: `IBaseRepository` を `IReadRepository`, `IWriteRepository`, `IPagedRepository` などの小さなインターフェースに分割することを検討してください。
- **メリット**: `EfCoreRepository` はすべてを実装し続けますが、利用者は必要な機能にのみ依存することで、結合度を下げることができます。

### 4. [Medium] UnitOfWork の簡素化

- **提案**: `UnitOfWork` から Repository のキャッシュと Dispose 管理を削除してください。
- **メリット**: 依存性注入コンテナ (DI) が Repository のライフサイクルを適切に管理します。`UnitOfWork` はトランザクション管理（`BeginTransaction`, `Commit`, `Rollback`）に集中させるべきです。

### 5. [Low] カーソルページネーションの堅牢化

- **提案**: `GetCursorPagedAsync` 内の `cursorSelector.Compile()` は呼び出しごとに式ツリーをコンパイルしています。
- **最適化**: `EfCoreCache` と同様のメカニズムでコンパイル済みのデリゲートをキャッシュするか、EF Core の翻訳メカニズムを活用して可能な限りデータベース側で処理を行うことを検討してください。
