# ADR 013: Feature-Sliced Modular Registration Pattern

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization

## 1. Context (背景)

Authorization ライブラリは、Roles, Permissions, WorkingHours, TenantIsolation など多岐にわたる機能を提供している。従来、これらの機能の DI 登録は `AddAuthorizationBlock` 内の単一のモノリシックな拡張メソッドで管理されていた。しかし、機能の増加に伴い、登録ロジックが複雑化し、特定の機能のみを有効化したり、機能ごとの依存関係を制御したりすることが困難になっていた。また、Rule 13 および Rule 18 で規定されている「べき等な登録シーケンス（Check-Self, Options, Mark-Self, Validation, Feature Toggle, Core Services）」を各機能に対して厳格に適用する必要があった。

## 2. Problem Statement (問題定義)

モノリシックな登録方式には以下の問題があった：
1. **保守性の低下**: すべての機能の登録ロジックが 1 つのファイルに集中しており、変更の影響範囲が広かった。
2. **テストの困難さ**: 特定の機能だけを分離して DI コンテナに登録し、ユニットテストや統合テストを行うことが難しかった。
3. **柔軟性の欠如**: 特定の機能だけを無効化（Feature Toggle）する際、モノリシックなメソッド内での条件分岐が複雑になり、可読性が低下していた。
4. **標準規約への違反**: 各機能が独立して `IsVKBlockRegistered<T>` によるチェックや `AddVKBlockMarker<T>` によるマーキングを行うべきところ、中央で一括処理されていたため、機能ごとの独立性が保たれていなかった。

## 3. Decision (決定事項)

認可機能の登録を、機能ごとに分割されたモジュール方式（Feature-Sliced Modular Pattern）へ移行した：

1. **機能別登録クラスの導入**:
   - 各機能（Roles, Permissions 等）に対して、`Internal/{FeatureName}Registration.cs` を作成した。
   - 各クラスは、Rule 18 に基づく 8 ステップの登録シーケンスをカプセル化して実装する。
2. **IVKAuthorizationBuilder による fluent インターフェース**:
   - `AddAuthorizationBlock` が `IVKAuthorizationBuilder` を返すように変更。
   - `AddRoles()`, `AddPermissions()` などのメソッドを Builder の拡張メソッドとして定義し、チェーン呼び出しを可能にした。
3. **機能マーカー (Feature Marker) の活用**:
   - 各機能に対して `xxxFeature` クラスを定義し、`IsVKBlockRegistered<xxxFeature>()` を用いて二重登録を防止した。

### 登録コードの例：
```csharp
// 利用側
services.AddAuthorizationBlock(configuration)
    .AddRoles()
    .AddPermissions()
    .AddWorkingHours();

// 内部実装 (RolesRegistration.cs)
internal static class RolesRegistration
{
    public static IVKAuthorizationBuilder Register(IVKAuthorizationBuilder builder)
    {
        var services = builder.Services;
        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<RolesFeature>()) return builder;
        
        // 2. Options, 3. Mark-Self, 4. Validation ...
        services.AddVKBlockMarker<RolesFeature>();
        
        // 7. Core Services
        services.TryAddScoped<IVKRoleProvider, DefaultRoleProvider>();
        // ...
        
        return builder;
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: モノリシックなメソッド内で switch/if 分岐
- **Approach**: `AddAuthorizationBlock` にオプションを渡し、その中で機能ごとに if 文で登録を制御する。
- **Rejected Reason**: オプションクラスが肥大化し、登録ロジックの凝集度が下がる。また、新しい機能を追加するたびに中央のメソッドを修正する必要があり、OCP（開放閉鎖の原則）に違反する。

### Option 2: リフレクションによる自動スキャン
- **Approach**: 特定のインターフェースを実装したクラスをリフレクションでスキャンして自動登録する。
- **Rejected Reason**: 起動パフォーマンスへの影響があること、および VK.Blocks の「Zero-Reflection」方針に反するため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **高い凝集度**: 各機能の登録ロジックがその機能のフォルダ内に閉じられ、保守性が劇的に向上した。
- **構成の柔軟性**: 必要な機能だけを明示的に `AddXxx()` することで、最小限のサービス構成を実現できる。
- **規約の強制**: Rule 18 のシーケンスを機能単位で適用できるため、起動時のバリデーションや二重登録防止がより堅牢になった。

### Negative
- **コード量の増加**: 各機能に Registration クラスと Builder 拡張が必要になるため、ファイル数が増加する。

### Mitigation
- 標準的なテンプレート（Blueprint）に従うことで、新しい機能追加時の実装パターンを固定化し、迷いを排除した。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **セキュリティ**: 各機能の有効化（Enabled）フラグが、Registration クラス内の Rule 18 シーケンスによって厳格に評価される。無効化された機能のハンドラーは DI コンテナに登録されないため、意図しない認可ロジックの実行を構造的に防ぐことができる。
- **依存関係**: `IVKAuthorizationBuilder` を通じて `IServiceCollection` と `IConfiguration` にアクセスできるため、各機能が個別に設定セクション（`VKBlocks:Authorization:Roles` 等）を読み取ることが可能。

**Last Updated**: 2026-04-24
**Status**: ✅ Accepted
