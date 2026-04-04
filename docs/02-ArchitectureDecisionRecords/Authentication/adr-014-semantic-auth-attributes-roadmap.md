# ADR 014: Strategic Roadmap for Semantic Authentication Attributes and Groups

**Date**: 2026-03-31  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Authentication UX / Developer Experience

## 2. Context (背景)

現在の ASP.NET Core の標準的な認証指定（スキーム名やポリシー名の文字列指定）は、大規模開発においてメンテナンス性の低下やタイポによるバグを引き起こすリスクがあります。これを解決し、より直感的な開発体験を提供するための戦略的ロードマップを策定しました。

## 3. Problem Statement (問題定義)

現在の実装には以下の課題があります：
- **マジックストリングへの依存**: `[Authorize(AuthenticationSchemes = "Bearer")]` のような記述が必要で、スキーム名が変更された際の影響範囲が大きい。
- **認可ポリシーの複雑化**: 特定の「ユーザーグループ」に対してどの認証方式が有効かを開発者が個別に把握し、設定する必要があり、ヒューマンエラーが発生しやすい。
- **Fail-Late な検証**: 認証モジュールが有効化されているにもかかわらず、どの戦略（JWT/ApiKey等）も設定されていない場合に、リクエスト受信時までエラーが顕在化しない。

## 4. Decision (決定事項)

開発効率と安全性を高めるため、以下の機能を順次導入する戦略的なロードマップを決定しました：

1.  **[Done] セマンティック认证属性の引入**: スキーム名をラップした専用属性（`[JwtAuthorize]`, `[ApiKeyAuthorize]`）を導入し、コントローラーでの宣言を型安全にする。
2.  **[Done] 论理的な「认证グループ」の提供**: `User`, `Service`, `Internal` 等の論理名で、適切な認証・認可ポリシーをセットで適用できるヘルパーメソッドを導入。
3.  **[Done] 启动时 Fail-Fast 验证**: `AuthenticationBlockExtensions` において、無効なポリシー使用を未然に防ぐ動的ポリシー登録ロ逻辑を実装。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 標準の Authorize 属性のみを利用**
  - **Rejected Reason**: 大規模開発においては「何のためにこの認証が必要か」というセマンティクス（意味論）が欠如し、コードの意図が不透明になるため、ドメイン駆動な独自の属性ベースアプローチが最適と判断した。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: 開発者はインフラの詳細（スキーム名など）を意識せず、ビジネスロジックに基づいた認証指定が可能になる。設定ミスによる実行時エラーが起動時に検出される。
- **Negative**: オリジナルの ASP.NET Core 属性に加えて独自の属性を管理するコストが発生する。
- **Mitigation**: 独自の属性は標準の `AuthorizeAttribute` を継承またはラッピングすることで、基盤側の挙動との互換性を完全に維持する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- このロードマップにより、認証（誰か）と認可（Policy）の責務を分離しつつ、開発者が「安全な選択肢」を自然に選べる環境を構築します。

## 8. Enterprise-Grade Features (企業向け高度な機能)

さらに、エンタープライズレベルの要求に応えるため、以下の機能を将来的な拡張ポイントとして定義します：

### 1. 拡張型分布式セッション管理 (Dynamic Session Revocation)
`IJwtTokenRevocationProvider` を深掘りし、より高度なセッション制御を実現します。
- **全デバイスログアウト (Global Logout)**：特定のユーザーに関連付けられたすべてのトークンを一括で失効させる機能。
- **セッション単位の撤回**: JWT に `sid` (Session ID) を埋め込み、デバイス管理画面から特定のデバイス（紛失した端末など）のみをリモートで強制的に無効化させる機能。

### 2. 精細な API Key 権限管理 (Scoped API Keys)
API Key に対して、リソース単位のアクセス制御（Scope）を導入します。
- **原理**: `ApiKeyContext` に権限検証ロジックを導入。
- **効果**: サードパーティパートナーに対して「読み取り専用」の API Key を発行するなど、最小権限の原則（PoLP）を適用。Key が漏洩した場合の損害を最小限に抑制。

### 3. 多テナント認証の隔離 (Multi-Tenant Auth Isolation)
複数のテナントがそれぞれ独自の OIDC プロバイダー（Auth0, Azure AD 等）を利用するシナリオに対応します。
- **機能**: `X-Tenant-Id` ヘッダー等に基づき、実行時に JWT の `Authority` や検証用公開鍵を動的に切り替える。
- **アプローチ**: `IOptionsMonitor` と `JwtBearerHandler` を高度にカスタマイズし、テナントごとの独立した認証コンテキストを提供。

### 4. アイデンティティ・エンリッチメント・パイプライン (Advanced Claims Enrichment)
`VKClaimsTransformer` を拡張し、プロファイル情報の高度な統合を実現します。
- **シナリオ**: キャッシュやデータベースから RBAC ロール、地理位置情報、サブスクリプションレベルを自動取得。
- **最適化**: 分布式キャッシュ（Redis）を活用し、リクエストごとにデータベースへ問い合わせることなくリッチな身元情報を保持。

### 5. セキュリティ監査と可観測性 (Security Observability)
- **異常モニタリング**: `AuthenticationDiagnostics` を通じて、短時間に異なる IP から大量の 401 エラーを発生させている API Key を検知し、自動的なアラート発行や一時的なブロックを実行。
- **セキュリティログ**: **RFC 5424** 標準に準拠し、監査に必要な「認証失敗」の機微な詳細情報を構造化ログとして記録。

## 9. Implementation Status (2026-04-01)

以下の機能が実装され、`VK.Blocks.Authentication` モジュールに統合されました：

- **提供属性**:
    - `[JwtAuthorize]`: JWT 認証を型安全に要求。
    - `[ApiKeyAuthorize]`: ApiKey 認証を型安全に要求。
    - `[AuthGroup(AuthGroups.User)]`: JWT/OIDC のマルチ認証をサポート。
- **動的ポリシー登録**:
    - 設定ファイル（`appsettings.json`）の `Enabled` 状態に基づき、必要な Authorization Policy のみを DI 時に動的に登録する Fail-Fast 構造を採用。
- **物理的分離**:
    - OIDC 固有のプロバイダー（Azure B2C, Google）を `Authentication.OpenIdConnect` モジュールへ移管し、コアライブラリの疎結合（ADR-006）を維持。
