# ADR 011: Refactor Authorization Building Block Structure and Naming

- **Date**: 2026-04-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization

## 1. Context (背景)

VK.Blocks プロジェクト全体で策定された新しい標準規約（CS.01–BB.05）に基づき、BuildingBlock 各モジュールの構造的整合性と命名の統一性が求められている。先行してリファクタリングが完了した Core および Authentication モジュールのパターンを Authorization モジュールにも適用し、保守性と視認性を向上させる必要がある。

## 2. Problem Statement (問題定義)

リファクタリング前の `VK.Blocks.Authorization` には以下の問題が存在していた：

1. **名前空間の断片化**: `Features/` 配下に深くネストされた名前空間（例: `VK.Blocks.Authorization.Features.Permissions.Internal`）により、利用側の `using` ステートメントが肥大化していた。
2. **命名規則の不一致**: 公開型と内部型の区別が不明確であり、`VK` プレフィックスの適用基準が曖昧であった。
3. **DI 登録の不備**: `IAuthorizationHandler` が具象型のみで登録されており、ASP.NET Core の標準的な認可パイプラインから自動的に認識されない構成になっていた。
4. **不変性の制限**: オプションクラスのプロパティが `init` のみであったため、標準的な DI コンテナによる動的な構成変更に対応しづらかった。

## 3. Decision (決定事項)

以下の設計変更を実施し、アーキテクチャを標準化した：

1. **名前空間のフラット化**: 全ての公開型をルート名前空間 `VK.Blocks.Authorization` に配置し、`Features/` フォルダを物理的・論理的に廃止した。
2. **命名規則の厳格化**: 
   - 全ての公開クラス、レコード、列挙型に `VK` プレフィックスを付与。
   - 全ての公開インターフェースに `IVK` プレフィックスを付与。
   - 内部実装クラスからは `VK` プレフィックスを除去し、`Internal/` サブ名前空間に隔離した。
3. **DI 登録の標準化**: 
   - `AddVKAuthorizationBlock` を AP.02 (Idempotent registration) に準拠させ、`VKAuthorizationBlock` マーカー型による二重登録防止を実装。
   - 各機能のハンドラーを `IAuthorizationHandler` インターフェースとして `TryAddEnumerable` で登録するように修正した。
4. **オプションモデルの改善**: `VKAuthorizationOptions` のプロパティを `set` に変更し、`IOptions` パターンにおける柔軟性を確保した。

### 核心的な DI 登録パターンの例：

```csharp
public static IServiceCollection AddRolesFeature(this IServiceCollection services)
{
    services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKRoleProvider, DefaultRoleProvider>());
    services.TryAddScoped<VKRoleHandler>();
    // ASP.NET Core 認可システムへの統合
    services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, VKRoleHandler>(sp => sp.GetRequiredService<VKRoleHandler>()));
    // プログラムによる評価用
    services.TryAddScoped<IVKRoleEvaluator>(sp => sp.GetRequiredService<VKRoleHandler>());

    return services;
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 既存の名前空間を維持し、エイリアスで対応
- **Approach**: 名前空間は変更せず、`using` エイリアスや型名変更のみを行う。
- **Rejected Reason**: 根本的な構造的複雑さが解消されず、新しい BuildingBlock 開発ガイドライン（BB.01）に抵触するため。

### Option 2: すべてのハンドラーを Singleton で登録
- **Approach**: パフォーマンス向上のため、状態を持たないハンドラーを Singleton にする。
- **Rejected Reason**: 一部のハンドラー（TenantIsolation 等）が Scoped サービス（HttpContext, DbContext 等）に依存しており、キャプティブ依存（Captive Dependency）の問題が発生するため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **利用体験の向上**: ルート名前空間一つでほとんどの機能にアクセス可能になり、開発効率が向上した。
- **堅牢な DI**: 標準的な認可パイプラインへの確実な統合により、ランタイムエラーのリスクが低減した。
- **一貫性**: 他の BuildingBlock と全く同じルールでコードを読み書きできるようになった。

### Negative
- **破壊的変更**: 既存のクライアントコードにおいて、名前空間のインポートや型名の参照（例: `PermissionHandler` -> `VKPermissionHandler`）の修正が必要となる。

### Mitigation
- 変更内容をこの ADR および Walkthrough ドキュメントに詳細に記録し、移行ガイドとして機能させる。
- ユニットテストを 100% 同期させ、ロジックのデグレードが発生していないことを保証する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **階層化の排除**: `Abstractions/`, `Common/`, `Contracts/` などの技術的なフォルダ分けを廃止し、ドメイン駆動のフォルダ構造（例: `Permissions/`, `Roles/`）に統一した。
- **セキュリティ**: テナント隔離やネットワーク制限などの重要な認可ロジックが、新しい `IAuthorizationHandler` 登録により、ASP.NET Core の認可ミドルウェアから確実かつ暗黙的に呼び出されることを保証した。

**Last Updated**: 2026-04-22


