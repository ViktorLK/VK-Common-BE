# ADR 002: Establish Dynamic Policy Generation for Permission-Based Authorization

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Authorization Module

## Context (背景)

エンタープライズシステムにおいて、ロール（Roles）は指数関数的に増加する傾向があり、静的なロールベースのアクセス制御（RBAC）はやがて管理不能（Role Explosion）となります。これを解決するために、よりきめ細かい権限文字列（例：`Users.Read`, `Invoices.Write`）を用いる権限ベースのアクセス制御（PBAC: Permission-Based Access Control）への移行が必要不可欠です。

しかし、ASP.NET Core の標準的な `Policy` ベース認可を用いてこれを実現しようとすると、一般的にアプリケーション起動時に数百もの静的ポリシーを一つ一つ登録しなければなりません（例：`options.AddPolicy("Users.Read", ...)`）。

## Problem Statement (問題定義)

- **起動オーバーヘッドとメモリ肥大化**: 数百〜数千におよぶすべてのポリシーを起動時に事前に構築・登録することは、Startup Time の大幅な遅延とメモリ（Memory Bloat）の浪費を招きます。
- **登録漏れのリスク**: 新しい権限を追加した際、定数や属性には追加しても、スタートアップでの `AddPolicy` を忘れると、実行時に「ポリシーが見つからない」というシステムエラーで停止してしまいます。
- **デベロッパーエクスペリエンス (DX) の低下**: コントローラーに `[AuthorizePermission]` を追加するだけでなく、設定ファイルも触らなければならないという二度手間が発生します。

## Decision (決定事項)

`IAuthorizationPolicyProvider` の拡張ポイントを活用し、**動的ポリシー生成アーキテクチャ（Dynamic Policy Generation）** を実装することを決定しました。

カスタムプロバイダーである `PermissionPolicyProvider` を作成し、特定のプレフィックス（例：`Permission:`）でフォーマットされたポリシーの要求をインターセプトします。
事前に登録されたリストに依存するのではなく、リクエストされたポリシー名から実行時にオンザフライ（on-the-fly）で権限文字列をパースし、`PermissionRequirement` を含む `AuthorizationPolicy` を動的に生成して返却します。

## Alternatives Considered (代替案の検討)

### Option 1: 起動時の全ポリシー静的登録 (Static Registration)

- **Approach**: アプリケーション起動時に、リフレクションや Source Generator を用いてすべての存在する権限をリストアップし、ループで `options.AddPolicy(...)` を呼び出す。
- **Rejected Reason**: 結局すべてのポリシーオブジェクトがメモリ上に常駐するため、メモリ使用量の問題が解決しません。また、実際にアプリケーションのライフサイクル中に一度も呼ばれない権限ポリシーまでインスタンス化されるのは非効率です。

### Option 2: 属性内でのみに完結するカスタムフィルタ (Custom IAsyncAuthorizationFilter)

- **Approach**: ポリシーシステムを使用せず、独自の MVC/API フィルタを作成して `[AuthorizePermission]` を直接処理する。
- **Rejected Reason**: ASP.NET Core の標準 Authorization パイプライン（DI, `IAuthorizationService`, Razor Pages などの統合）周りから外れるため、全体としてのアーキテクチャの統一性が失われ、単体テスト・結合テストも難しくなります。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 開発者は `[Authorize(Policy = "Permission:Users.Read")]` や強型付けされたカスタム属性（`[AuthorizePermission]`）を追加するだけで即座に利用可能となり、スタートアップでの登録は一切不要になります（DXの大幅向上）。
    - ポリシーは要求された時にだけ生成（およびキャッシュ）されるため、起動時のオーバーヘッドが実質ゼロになり、メモリ効率が最大化されます。
- **Negative**:
    - 初回のリクエスト時にオンザフライで生成するわずかな遅延（マイクロスレッドレベル）が発生します。
- **Mitigation**:
    - ASP.NET Core の基底クラス `DefaultAuthorizationPolicyProvider` 自体がポリシーのキャッシュメカニズムを備えているため、同一定義のポリシー生成は初回の一度きりであり、パフォーマンス上の懸念は払拭されます。

## Implementation & Security (実装詳細とセキュリティ考察)

- `PermissionPolicyProvider` は、プレフィックスが一致しない場合は本来のベースプロバイダー（`base.GetPolicyAsync`）にフォールバックするため、標準の機能（組み込みの Role ポリシーなど）を一切妨害しません。
- **セキュリティの担保**: ポリシーが存在するかどうかを事前に検証する必要がなく、「指定された権限を持つか」という本質的な問い（`PermissionRequirement` の評価）にのみ集中できるため、セキュリティの抜け漏れを防ぎます。
