# ADR 006: Modernize Dynamic Policy Evaluation with IAuthorizationRequirementData

- **Date**: 2026-04-06
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization

## 1. Context (背景)

エンタープライズアプリケーションにおいて、属性ベースのアクセス制御（ABAC）を実現するために動的な認可要件（`DynamicRequirement`）が導入されました。以前の ADR-003 では `IAttributeEvaluator` パターンを採用しましたが、これを実際の ASP.NET Core の認可パイプラインに接続する際、以下の課題が残っていました：

- 文字列ベースのポリシー解析（`PolicyProvider`）による実装の複雑化。
- `IAttributeEvaluator` が本来所属すべき `DynamicPolicies` ではなく、`MinimumRank` スライスに配置されていたことによる機能間の結合。
- 評価失敗時の詳細なコンテキストが Result Pattern に統合されていなかったこと。
- `MinimumRankGenerator` が生成する属性や、標準の `AuthorizePermission` / `AuthorizeRoles` が固定の文字列ポリシー（Policy String）に依存しており、実行時の評価プロセスが「ブラックボックス」化していたこと。これにより、Grafana 等での細粒度な診断（どの権限が、どのロールが拒否されたか）が不可能でした。
- 独自のランク列挙型（Enum）を利用する際、DIコンテナへのポリシー事前手動登録が強制される開発者体験（DX）の悪化。

## 2. Problem Statement (問題定義)

1. **認可パイプラインの複雑性**: カスタム属性を認可要件に変換するために、通常 `IAuthorizationPolicyProvider` による文字列パースが必要でしたが、これはマジックストリングに依存し、実装の保守性を下げていました。
2. **Result Pattern (Rule 1) への非遵守**: 動的な評価結果が単純な `bool` で返されていたため、操作符（Operator）のミス設定などのエラーを構造化データとして返せず、診断が困難でした。
3. **垂直スライスの不全 (Rule 12)**: 属性評価のコアロジックが別の機能フォルダに混在しており、モジュール性が損なわれていました。
4. **不変性の欠如 (Rule 15)**: `DynamicRequirement` のプロパティが必須入力として強制されておらず、ランタイムエラーの温床となっていました。
5. **Role/Rank の密結合と静的ポリシーの限界**: `MinimumRankRequirement` が特定の `EmployeeRank` にハードコードされており、下流のマイクロサービスが独自のランク列挙型（例: `VipLevel`）を定義した際、パイプラインが機能不全に陥っていました。さらに、SG が生成する文字列ポリシーは、実行時に手動で `AddPolicy` を登録する必要があるため、「ゼロコンフィグ（Zero Config）」が達成できていませんでした。
6. **観測可能性の欠如**: 標準の `Roles` プロパティや文字列ベースの `Permission` 判定では、どの具体的なロールや権限が原因で認可が失敗したかをメトリクスとして抽出できず、運用監視上の大きな課題となっていました。

## 3. Decision (決定事項)

1. **IAuthorizationRequirementData の採用**:
   .NET 8+ で導入された `IAuthorizationRequirementData` を `DynamicAuthorizeAttribute` に実装。これにより、`PolicyProvider` による文字列解析を完全にバイパスし、属性から直接 `IAuthorizationRequirement` への変換を可能にしました。
2. **IAttributeEvaluator の Result Pattern 統合**:
   戻り値を `Result<bool>` にアップグレードし、`AuthorizationErrors.InvalidOperator` などのエラーコンテキストを付与。
3. **機能スライスの純粋化**:
   `IAttributeEvaluator` およびその実装を `Features/DynamicPolicies` フォルダへ移動し、他機能との結合を排除。
4. **不変性と型の安全性**:
   `DynamicRequirement` に `required` キーワードを導入し、不完全な要件定義をコンパイル時に防止。
5. **「Resolver」命名規約の確立**:
   外部データを提供する「Provider」と区別し、内部的な変換・解析を行うコンポーネントには「Resolver」という名称（または直接的な統合）を使用することを決定。
6. **MinimumRank の Integer 抽象化**:
   `MinimumRankRequirement` を `int` 比較へ抽象化させ、任意の列挙型（Enum）を受け入れられるようリファクタリング。JWT クレームのパースにおいても、数字または動的 Enum Type に基づくフォールバック解析を導入し、`EmployeeRank` へのハードコードを解消。
7. **Source Generator の IAuthorizationRequirementData 化**:
   `MinimumRankGenerator` においても文字列 Policy の生成と制約を完全廃止。生成される動的 Attribute (`Require{EnumName}Attribute`) に `IAuthorizationRequirementData` を実装させることで、利用者は `AddPolicy` 登録作業なしに、属性を付与するだけで即座にカスタムランク認可を実現可能にした（ゼロコンフィグ化）。
8. **標準属性（Permissions/Roles）の全面刷新**:
   `AuthorizePermissionAttribute` と `AuthorizeRolesAttribute` も `IAuthorizationRequirementData` インターフェースを実装するようにリファクタリング。ASP.NET Core のデフォルト処理をバイパスし、独自の `PermissionHandler` / `RoleHandler` を介して評価を行うことで、**「どの権限/ロールで失敗したか」を Tag 付けした細粒度メトリクス**の収集を可能にしました。
9. **不要な Provider の廃止**:
   属性自体が Requirement を提供可能になったため、複雑な `PermissionPolicyProvider` などの動的プロバイダを削除し、コードベースを簡素化。

## 4. Alternatives Considered (代替案の検討)

### Option 1: カスタム PolicyProvider による文字列解析

- **Approach**: 文字列（例: `"Dynamic:Attr:Op:Val"`) を解析して Requirement を生成する。
- **Rejected Reason**: .NET 8+ の `IAuthorizationRequirementData` が提供するネイティブな統合と比較して、ランタイムオーバーヘッドが大きく、実装が冗長であるため。

### Option 2: IAttributeEvaluator を Abstractions に移動

- **Approach**: グローバルなフォルダに抽象を置く。
- **Rejected Reason**: Rule 12（垂直スライス）に反し、このインターフェースは `DynamicPolicies` 機能に特化しているため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive

- 認可ロジックからマジックストリングと冗長な解析コードが排除され、保守性が大幅に向上。
- エラーハンドリングが Result Pattern に統一され、安全なデフォルト拒否（Default Deny）が実現。
- メモリ割り当ての少ない高性能な認可チェックを実現。

### Negative

- `IAuthorizationRequirementData` という比較的新しい .NET の機能に依存する（.NET 8 未満では動作しない）。

### Mitigation

- 本プロジェクトは最新の .NET をターゲットとしているため、この依存は許容範囲内。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **セキュアな評価**: `AttributeEvaluator` は正向の強断言（`Equals`, `Exists`, `Contains`）のみをサポートし、否定の断言（`NotEquals`, `NotContains`）を意図的に排除することで、属性欠落による意図しないアクセス許可を防止。
- **診断**: `DynamicRequirementHandler` は `AuthorizationDiagnostics` を通じて評価の成功/失敗を記録し、トレーサビリティを確保。

---

**Last Updated**: 2026-04-06
