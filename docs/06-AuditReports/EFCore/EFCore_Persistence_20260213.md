# アーキテクチャ監査レポート: EFCore Persistence (2026-02-13)

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 85/100点
- **対象レイヤー判定**: Infrastructure Layer (Persistence Implementation)
- **総評 (Executive Summary)**: EF Coreの最新機能を活用したパフォーマンス最適化（一括更新など）と、インターセプターによる横断的関心事の分離は見事です。一方で、Repositoryの実装が肥大化しており（God Objectの傾向）、状態を持つ設計（Stateful Repository）やファイル命名のミスなど、プロフェッショナルな品質基準に達していない点が散見されます。

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[State Management]**: `EfCoreRepository.Unsafe()` - Repository インスタンス内の状態フラグ（`_isUnsafeMode`）を変更する設計になっています。Repositoryは通常Scopedライフサイクルであり、同一リクエスト内でこのメソッドが呼ばれると、後続の無関係なクエリまで「Unsafe（論理削除無視）」モードで実行される危険性があります。
- ❌ **[File Naming]**: `IUnsafeContext .cs` - ファイル名の拡張子の前にスペースが含まれています。ビルドスクリプトやファイルシステムによっては予期せぬエラーを引き起こす可能性があります。

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[パフォーマンス]**: `EfCoreCache<T>` によるリフレクション情報のキャッシュ戦略（型安全なメタデータキャッシュ）は非常に優秀で、高負荷時のスループット維持に貢献しています。
- 🔒 **[パフォーマンス]**: `GetCursorPagedAsync` における `cursorSelector.Compile()` は、呼び出しごとに式木をコンパイルしており、パフォーマンスの劣化を招く可能性があります。

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[疎結合性]**: `ServiceCollectionExtensions` によるDI登録のカプセル化は良好であり、利用者が複雑な内部構成を知る必要がない点は評価できます。

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[情報なし]**: 特記事項なし。

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[SRP (単一責任の原則)]**: `EfCoreRepository` が肥大化しており、CRUD、一括操作、ページネーション、生SQL実行など多くの責務を負っています。保守性が低下するリスクがあります。
- ⚠️ **[KISS (複雑性)]**: `UnitOfWork` が `Repository` のライフサイクル（Dispose）を手動管理しようとしており、二重Disposeのリスクがあります。DIコンテナのライフサイクル管理に任せるべきです。

## ✅ 評価ポイント (Highlights / Good Practices)

- **Modern Performance Optimizations**: `ExecuteUpdateAsync` / `ExecuteDeleteAsync` を活用し、メモリへのロードなしでデータベース操作を行うCQRS的な最適化が図られています。
- **AOP via Interceptors**: 監査と論理削除ロジックがインターセプターに分離されており、ビジネスロジックの汚染を防いでいます。
- **Type-Safe Metadata Caching**: リフレクション情報を静的にキャッシュする手法は、パフォーマンスへの配慮が行き届いています。

## 💡 改善ロードマップ (Evolutionary Roadmap)

1. **最優先対応 (Immediate Action)**:
    - `IUnsafeContext .cs` のファイル名を即座に修正する。
    - `Unsafe()` メソッドの状態依存を廃止し、新しいRepositoryインスタンスを返すか、引数オプションとして渡す設計に変更する。

2. **リファクタリング提案 (Refactoring)**:
    - `IBaseRepository` を `IReadRepository`, `IWriteRepository` などに分割（Interface Segregation）し、巨大なインターフェースを避ける。
    - `UnitOfWork` からRepositoryのDispose管理ロジックを削除し、単純化する。
    - カーソルページネーションの式コンパイルをキャッシュ化する。

3. **推奨される学習トピック (Learning Suggestions)**:
    - **Interface Segregation Principle (ISP)**: 巨大なRepositoryインターフェースを避けるための設計原則。
    - **Stateless Service Design**: DIにおけるScopedサービスの正しい設計と、状態保持のリスクについて。
