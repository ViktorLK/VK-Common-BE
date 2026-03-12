# ADR 001: Transition to Tenant Resolution Pipeline Architecture

## 1. Meta Data

- **Date**: 2026-03-09
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: MultiTenancy Module Refactoring

## 2. Context (背景)

VK.Blocks の MultiTenancy モジュールにおいて、テナント識別はマルチテナントシステムを構築する上での根幹となる機能である。初期の実装では、テナントIDをHTTPヘッダー（`X-Tenant-Id`）からのみ取得する単一の `HttpContextTenantProvider` に依存していた。しかし、複数の製品群や環境（開発、本番）を束ねるエンタープライズアーキテクチャにおいては、JWT Claim（認証トークン）、サブドメイン名（`tenant.yourdomain.com`）、開発時のクエリ文といった複数のソースからの動的なテナント解決が必要不可欠となる。特定の一つのソースに限定された設計は、システムの拡張性および柔軟性のボトルネックになっていた。

## 3. Problem Statement (問題定義)

現在の `HttpContextTenantProvider` 実装が抱える課題は以下の通りである。

1. **拡張性の欠如 (OCP 違反)**: 新しいテナント解決手段（例：ホスト名ベース、トークンベース）を追加する度にモジュールのコアロジックを直接変更する必要があり、Open-Closed Principle に反している。
2. **保守性の低下**: 複数の解決手段を単一のクラスに詰め込むと、コードの肥大化と責務の混同（Single Responsibility Principle の違反）が発生する。
3. **柔軟な優先順位制御の困難さ**: 複数のソースからテナント情報を取得し得る場合（例：ヘッダーにもクエリにも存在する場合）、動的かつ一元的に優先順位を制御・変更するスケーラブルなメカニズムが存在しない。

```csharp
// 悪い例：単一責務の崩壊、ハードコードされたロジック
public string? GetCurrentTenantId()
{
    // ヘッダーから取得...
    // 将来的にここへクエリから取得やClaimsから取得を追記していくと
    // 巨大なメソッドになり保守性が破綻する
}
```

## 4. Decision (決定事項)

テナントの解決において、**戦略パターン（Strategy Pattern）**を用いた**リゾルバパイプラインアーキテクチャ（`TenantResolutionPipeline`）**を採用した。

### 設計詳細

1. **ITenantResolver と Pipeline の導入**:
   各テナント解決方式を独立したリゾルバクラスとして実装する。これらは `TenantResolutionPipeline` によって設定された優先度順（`Order`）で反復試行される。
2. **解決方式の分離**:
    - `HeaderTenantResolver` (Order: 100)
    - `ClaimsTenantResolver` (Order: 200)
    - `DomainTenantResolver` (Order: 300)
    - `QueryStringTenantResolver` (开发用, Order: 900)
3. **TenantContext ベースのキャッシュ**:
   解決されたテナント情報は、リクエストの早期段階（`TenantResolutionMiddleware`）で一度だけ計算され、スコープ付きの `ITenantContext` にキャッシュされる。これにより、後続のアプリケーション層は `ITenantProvider` または `ITenantContext` を介して一貫したパフォーマンスでテナント情報を取得できる。

```csharp
// 抽象化された解決戦略インターフェース
public interface ITenantResolver
{
    int Order { get; }
    Task<TenantResolutionResult> ResolveAsync(HttpContext context, CancellationToken cancellationToken = default);
}

// Result<T> に準拠した構造的レスポンス
public sealed record TenantResolutionResult
{
    public static TenantResolutionResult Success(string tenantId) => ...
    public static TenantResolutionResult Fail(string error) => ...
}
```

## 5. Alternatives Considered (代替案の検討)

### Option 1: 従来の Provider クラスのメソッド拡張

**Approach**: 既存の `HttpContextTenantProvider` 内部に全てのロジックを分岐（`if / else if`）として記述する。
**Rejected Reason**: 容易に実装できるものの、テストが困難になり、将来の拡張時にクラスの修正が必要となるため却下（OCP / SRP 違反）。

### Option 2: DI による単一の Resolver の切り替え

**Approach**: アプリケーション起動時に `ITenantProvider` に注入する具象クラスを一つだけ選択する方式。
**Rejected Reason**: 複数の提供元フォールバック（例：基本はヘッダーだが、開発時のみクエリを許可）といった複合的なシナリオに対応できず、要件を満たせない。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive

- **拡張性**: 新しい解決方法（例：GeoIPベース）を追加する際、既存コードを変更せず新しいリゾルバを追加しDIに登録するだけで済む。
- **保守性**: 各リゾルバは単一の責務を持つため、ユニットテストが極めて容易に行える。
- **柔軟な制御**: `Order` プロパティと DI 登録時のオプション設定により、パイプラインの順序と有効/無効を環境ごとに柔軟に変更可能。

### Negative

- クラス数とファイル数が増大し、初期の学習コストや認知的負荷がわずかに高まる。

### Mitigation

- パイプラインの全体像と個別のリゾルバの挙動を明確に示すドキュメント（本 ADR およびソースコードの XML コメント）を付与する。オプションによる構成のデフォルト値を適切に設定する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Result Pattern の徹底**: 全ての解決処理は独自のドメイン拡張例外を投げるのではなく、`TenantResolutionResult.Fail(error)` を介して構造化されたエラーを返す。これを一元的にミドルウェアにて RFC 7807 (Problem Details) 様式の HTTP 401 へ変換する。
- **データ不変性の保証**: 解決結果およびテナントの表現には `sealed record` 型（`TenantInfo`, `TenantResolutionResult`）を使用し、不要な変更を防ぐ不変なデータを提供する。
- **セキュリティとテナンシーの強制**: `MultiTenancyOptions.EnforceTenancy` が `true` の場合、テナントが特定できないリクエストは後続のパイプラインに流さずに直ちにはねのける（Fail-Fast）。これは情報漏洩および認可ミスの重大なリスクをシステム境界で防ぐ設計である。
- **ログの可観測性**: ミドルウェアおよびパイプラインでは、VK.Blocks のルールに準拠し `{TenantId}` および `{TraceId}` のプレースホルダーを用いた構造化ログを出力し、トレーサビリティを確保している。
