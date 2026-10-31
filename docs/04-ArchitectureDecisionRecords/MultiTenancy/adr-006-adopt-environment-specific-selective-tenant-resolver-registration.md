# ADR 006: Adopt Environment-Specific Selective Tenant Resolver Registration

## 1. Meta Data

- **Date**: 2026-03-12
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: MultiTenancy Module Refactoring

## 2. Context (背景)

（参照: ADR 001）
テナントを解決するための複数のリゾルバ（Header, Claims, Domain, QueryString）が定義された。これらのうち、例えば `QueryStringTenantResolver`（`?tenantId=abc`）は、ローカル開発時や結合テスト環境でのデバッグにおいて非常に効果的である。しかし、このような単純なオーバーライド機構を本番環境（Production）で有効にした場合、悪意のあるユーザーが意図的にクエリ文字列に他テナントのIDを付与することで、容易に認証・認可コンテキストを突破し、テナントスプーフィング（なりすまし）を引き起こす重大な脆弱性となる。

## 3. Problem Statement (問題定義)

本番環境において、開発用の危険なテナントリゾルバが実行されることを技術的にどのように確実に防ぐか。

## 4. Decision (決定事項)

**DI 登録フェーズにおいて、環境変数/Options ベースでのインプトジェクト選択的登録（Selective Registration）メカニズムを採用する。**

- `MultiTenancyOptions.EnabledResolvers` リストの設定を導入する。
- `MultiTenancyServiceCollectionExtensions` 内での DI 登録時に、このリストを評価する。
- 実行時に条件分岐（`if (environment.IsProduction)` コードの埋め込み）を行うのではなく、構成プロバイダー（`appsettings.json` など）で明示的に有効化されたリゾルバのみを `IServiceCollection` に `ITenantResolver` として登録する。
- 本番環境用の設定では、`QueryString` をリストから除外することで、本番の Dependency Injection コンテナ内に危険な具象クラスが配置されることすら防ぐ。

## 5. Alternatives Considered (代替案の検討)

### Option 1: リゾルバ内部での環境チェック
- **Approach**: `QueryStringTenantResolver.ResolveAsync` メソッドの内部で `IWebHostEnvironment.IsDevelopment()` をチェックし、本番環境の場合は即座に失敗させる。
- **Rejected Reason**: インフラストラクチャーに対する依存（`IWebHostEnvironment`）がリゾルバクラス内に漏れ出し、クリーンアーキテクチャの関心の分離に反する。また、DI コンテナには使われないクラスが登録され続けるためスマートな設計とは言えないため却下。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- 攻撃対象領域（Attack Surface）が本番環境で確実に最小化される。
- 各リゾルバは純粋に自らの解決ロジックに集中でき、環境の条件分岐を内部に持つ必要がなくなる（SRP の遵守）。

### Negative
- 開発者が新しい環境を構築する際、構成ファイル（`appsettings.json`）の `EnabledResolvers` を正しく設定し忘れると、意図したリゾルバが動作せず混乱を招く。

### Mitigation
- `EnabledResolvers` が空の場合は、自動的に構成の緩い「全リゾルバ有効化」フォールバックを使用する設計とする（または、ドキュメントに強力な構成テンプレートを提供する）等の措置を適用する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- `QueryStringTenantResolver` のコメントにて「Intended for use in development environments only」と明示されている警告に対し、DI 登録レベルのフィルタリングは強力な技術的エンフォースメント（強制手段）を提供する。
- 本番設定ファイル（`appsettings.Production.json`）には、`Header`, `Claims`, `Domain` のみが明示的にアローリストとして許可される。
- これにより、テナントスプーフィング・権限昇格攻撃の主たる脆弱性ベクトルの1つが根絶される。
