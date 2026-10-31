# ADR 004: Introduce Source Generators for Authorization Handlers and Permissions Catalog

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Authorization Module

## Context (背景)

Authorization モジュールにおいて、開発者はこれまで新しい `IAuthorizationHandler` または `IVKAuthorizationHandler` の実装を手動で DI コンテナ（`IServiceCollection`）に登録し、かつ `[AuthorizePermission("...")]` のような属性内でマジックストリングを使用する必要がありました。

## Problem Statement (問題定義)

各ハンドラーを手動で DI 登録するアプローチ（`services.AddScoped<IAuthorizationHandler, ...>()`）は、人為的なミスを誘発しやすく、誤って登録を忘れた場合、必要な認可チェックが実行されずにセキュリティ脆弱性（権限バイパス）に直結するリスクがあります。
また、権限名をマジックストリングとして多用することは、DRY（Don't Repeat Yourself）原則に違反します。タイポによるバグの発見が遅れ、将来的な仕様変更やリファクタリングが極めて困難になります。システムの規模が拡大するにつれ、この手動管理は保守性の大きなボトルネックとなっていました。

## Decision (決定事項)

C# Source Generators（`IIncrementalGenerator`）を導入し、これらの横断的関心事（Cross-cutting concerns）にまつわるボイラープレートタスクをコンパイル時に自動化することを決定しました。

具体的には以下の2つの Generator を導入します：

1. **AuthorizationHandlersGenerator**
   プロジェクト内の `IAuthorizationHandler` または `IVKAuthorizationHandler` を実装するすべてのクラスをコンパイル時に走査し、DI コンテナへの登録拡張メソッド（`AddGeneratedAuthorizationHandlers`）を自動生成します。
   重複登録や手動登録との競合を防ぐため、登録には `TryAddEnumerable` を使用します。

2. **PermissionsCatalogGenerator**
   コード内の `[AuthorizePermission]` および `[AuthorizeRoles]` 属性の利用箇所を走査し、使用されている権限文字列を抽出・重複排除した上で、強型付けされた定数文字列を保持する `PermissionsCatalog` クラスを自動生成します。

## Alternatives Considered (代替案の検討)

### Option 1: Reflection at Startup (起動時のリフレクション)

- **Approach**: アプリケーション起動時にリフレクションを用いてアセンブリをスキャンし、ハンドラーを自動登録、および権限のカタログを構築する。
- **Rejected Reason**: 起動時間（Startup Time）のペナルティが大きく、クラウドネイティブ環境やスケーリング時に不利。また、マジックストリングの問題に対するコンパイル時の型安全性（Compile-time safety）を提供できない。

### Option 2: 従来の手動管理の継続 + Linter/Analyzer の導入

- **Approach**: 手動での登録を継続しつつ、Roslyn Analyzer を独自に作成して「登録漏れ」や「未定義の権限文字列」を警告する。
- **Rejected Reason**: エラーは防げるものの、ボイラープレートの記述量自体は減らないため、開発体験（DX）の向上に寄与しない。より根本的な解決策としてコード生成が優位と判断。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 手動登録エラーの排除により、フェイルオープンなどのセキュリティリスクを低減。
    - 権限文字列のコンパイル時安全性が確保され、マジックストリングが排除される。
    - 実行時のパフォーマンス劣化なし（すべての解析・生成負荷がコンパイル時にシフトされるため）。
- **Negative**:
    - Source Generator の開発および保守という新たな技術的複雑性の導入。
    - 大規模プロジェクトにおいて、コンパイル時間や IDE の応答性が悪化するリスク。
- **Mitigation**:
    - Source Generator のパイプライン内で `CompilationProvider.Combine` などのフルコンパイルに依存するアンチパターンを避け、`WhereNotNull` などを駆使した純粋な Incremental 処理を徹底することで、ビルドおよび IDE パフォーマンスへの影響を極小化する。

## Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ重点**: 本アーキテクチャの導入により、開発者の単純な「ハンドラー登録忘れ」による認可パイプラインの欠落を構造的に防ぐことができます。
- 自動生成された DI 登録処理では、`TryAddEnumerable` を用いることで、同一ハンドラーの多重実行（副作用の原因）や、開発者が特殊な意図で手動登録した実装との予期せぬ衝突を安全に回避します。
