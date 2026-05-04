# ADR 015: Tenant-Level Capability Authorization (Entitlements)

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization

## 1. Context (背景)

マルチテナント SaaS 環境において、アクセス制御は二つの次元で考える必要がある：一つは「ユーザーが何をできるか（User Permissions）」、もう一つは「テナントの現在のプランや契約でどの機能が利用可能か（Tenant Capabilities/Features）」である。従来、これら二つの関心事は混同されることが多く、テナントレベルのチェックがコントローラーやビジネスロジック内にアドホックに実装されていた。これは、宣言的セキュリティの原則に反し、保守性や一貫性を損なう原因となっていた。

## 2. Problem Statement (問題定義)

アドホックなテナント機能チェックには以下の問題があった：
1. **コードの散在**: 多くのコントローラーで `if (tenant.HasFeature("X"))` のようなチェックが繰り返され、DRY 原則に違反していた。
2. **宣言的でない設計**: 属性ベースの認可（PBAC）と、命令的な機能チェックが混在し、API の認可要件を一目で把握することが困難であった。
3. **テストの複雑化**: ビジネスロジック内にテナントチェックが入り込むことで、純粋なビジネスルールのユニットテストが困難になっていた。
4. **不整合のリスク**: ある API ではチェックが行われ、別の類似 API ではチェックが漏れるといった不整合が発生しやすかった。

## 3. Decision (決定事项)

認可 BuildingBlock 内に、テナントレベルの機能認可に特化した「Entitlements」機能を導入した：

1. **VKRequireTenantFeatureAttribute の導入**:
   - 開発者がコントローラーやアクションに対して、宣言的に必要なテナント機能を指定できるようにした。
2. **VKTenantFeatureRequirement の実装**:
   - 特定の機能 ID を保持する認可要件クラスを定義した。
3. **TenantFeatureAuthorizationHandler による一元評価**:
   - `ITenantProvider`（MultiTenancy ブロック）から現在のテナント情報を取得し、要求された機能が有効かどうかを評価するハンドラーを実装した。
4. **Result パターンの採用**:
   - 評価結果を `Result` オブジェクトで返し、認可失敗時のエラー理由を明確にした。
5. **モジュール登録への統合**:
   - `AddEntitlements()` メソッドを通じて、簡単に認可パイプラインへ統合できるようにした。

### 使用例：
```csharp
[ApiController]
[Route("api/[controller]")]
[VKRequireTenantFeature("AdvancedAnalytics")] // このテナントで AdvancedAnalytics が有効な場合のみアクセス可能
public class AnalyticsController : ControllerBase
{
    [HttpGet]
    [VKAuthorizePermission("Analytics.View")] // かつ、ユーザーが表示権限を持っていること
    public IActionResult Get() => Ok();
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: ミドルウェアによる一括チェック
- **Approach**: すべてのリクエストに対して、パスやメタデータに基づいてミドルウェアでチェックする。
- **Rejected Reason**: 細粒度な制御（特定のアクションのみチェック等）が難しく、また認可パイプライン（ASP.NET Core Authorization）から外れるため、他の認可ルールとの組み合わせが困難になる。

### Option 2: 既存の Permission システムの拡張
- **Approach**: テナント機能を「システム権限」として扱い、ユーザー権限と同じ仕組みで評価する。
- **Rejected Reason**: ユーザーが「持っている」権限と、システムが「提供している」機能は概念的に別物であり、混同すると将来的なロール管理や課金モデルの変更に対応しづらくなる。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **宣言的セキュリティの強化**: API の認可要件が属性として明確に表現され、視認性と保守性が向上した。
- **関心の分離**: ビジネスロジックからテナントレベルの制限が分離され、コードがクリーンになった。
- **再利用性**: 一度定義した機能 ID を、システム全体で一貫した認可ルールとして再利用できる。

### Negative
- **MultiTenancy ブロックへの依存**: 認可ブロックが MultiTenancy ブロックの `ITenantProvider` に依存することになる。

### Mitigation
- 依存関係はインターフェースを介して抽象化し、疎結合を維持する。また、Entitlements 機能自体をオプショナルなモジュール（`AddEntitlements()`）として提供することで、マルチテナントでない環境では登録をスキップできるようにした。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ**: テナントの隔離は `ITenantProvider` によって保証されており、ハンドラーはその結果を信頼して機能チェックを行う。機能 ID のタイポを防ぐため、定数クラスを用いた指定を推奨する。
- **エラーハンドリング**: 機能が無効な場合は `Forbidden` (403) ではなく、場合によっては `ServiceUnavailable` (503) や特定のビジネスエラーコードを返すように、RFC 7807 に基づくエラーレスポンスを構成できる。

**Last Updated**: 2026-04-24
**Status**: ✅ Accepted
