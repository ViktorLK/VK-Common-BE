# ADR 019: Decoupling Authentication and Authorization via Dependency Inversion

- **Date**: 2026-04-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authentication, VK.Blocks.Authorization

## 1. Context (背景)

VK.Blocks.Authentication ブロックはこれまで、セマンティックポリシー（User, Service, Internal グループ）を登録するために `Microsoft.AspNetCore.Authorization` パッケージに直接依存していました。

この設計は、認証（Authentication）と認可（Authorization）の境界を曖昧にし、認証機能のみを必要とするクライアントに対しても認可ロジック（およびその依存関係）の導入を強いていました。また、認証モジュールの内部で認可ポリシーを登録することで、依存関係グラフが複雑化し、モジュールの再利用性や拡張性が制限される要因となっていました。

## 2. Problem Statement (問題定義)

現在の実装には以下の課題がありました：

1.  **Tight Coupling（密結合）**: Authentication ブロックが上位レイヤーの Authorization ライブラリに依存しており、アーキテクチャの階層（Layers）が逆転していました。
2.  **Violation of SoC（関心事の分離の違反）**: 認証モジュールの役割は「誰であるかを確認する」ことですが、旧設計では「そのユーザーをどのグループ（Policy）に含めるか」という認可の判断まで抱え込んでいました。
3.  **Core Abstraction の欠如**: セマンティックグループ名（`VK.Group.User` 等）が認証側に閉じていたため、認可モジュールやアプリケーション層がこれらを一貫して利用するための共有契約が存在しませんでした。

## 3. Decision (決定事項)

**依存性の逆転（Dependency Inversion）** パターンを採用し、`ISemanticSchemeProvider` を介した「パブリッシャー（認証）」と「コンシューマー（認可）」の分離を実現しました。

具体的な決定事項は以下の通りです：

1.  **Shared Contract の導入**:
    - `VK.Blocks.Core.Security` に `ISemanticSchemeProvider` インターフェースを定義。認証方式が自身のアクティブなスキーム（Bearer, ApiKey 等）を「提供」する契約を確立。
    - 同ディレクトリに `AuthPolicies` 定数クラスを配置し、システム全体で共有されるポリシー名の真実の情報源（SSOT）としました。

2.  **Publishing Model (Authentication Side)**:
    - 各認証機能（JWT, ApiKey, OAuth）に `ISemanticSchemeProvider` の実装（例：`JwtSemanticSchemeProvider`）を追加。
    - これらは DI コンテナに `TryAddEnumerableSingleton` で登録され、自身の構成オプションに基づいて動的にスキーム情報を公開します。

3.  **Consumption Model (Authorization Side)**:
    - `VKAuthorizationPolicyProvider` を Authorization ブロックへ移動。
    - DI から全ての `IEnumerable<ISemanticSchemeProvider>` を解決し、それらが提供するスキームを統合して `AuthorizationOptions` にセマンティックポリシーを動配置するように設計変更しました。

4.  **Dependency Cleanup**:
    - `VK.Blocks.Authentication.csproj` から `Microsoft.AspNetCore.Authorization` のパッケージ参照を完全に削除。

## 4. Alternatives Considered (代替案の検討)

### Option 1: 現状維持（認証側でのポリシー登録）
- **Approach**: 従来通り Authentication 内で `AddAuthorization()` を呼び出し、ポリシーを登録する。
- **Rejected Reason**: アーキテクチャの純粋性を損ない、認証モジュールの軽量化（Zero-Authorization dependency）が達成できないため却下。

### Option 2: イベントベースのポリシー登録
- **Approach**: 起動時にイベントを発火し、認可側がそれをフックしてポリシーを追加する。
- **Rejected Reason**: ASP.NET Core の `IConfigureOptions<AuthorizationOptions>` パイプラインとの整合性が取りづらく、DI によるサービス収集の方が堅牢かつ同期的な初期化に適しているため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive (メリット)
- **Architectural Purity**: 認証ブロックが認可から完全に独立し、ドメインの関心事が分離されました。
- **Modularity**: 新しい認証方式を追加する際、`ISemanticSchemeProvider` を実装するだけで、認可側のコードを変更することなく自動的にグループポリシーに組み込まれます。
- **Reduced Footprint**: 認証のみを利用するプロジェクトの依存関係パッケージが削減されます。

### Negative (デメリット)
- **Abstraction Complexity**: 短絡的な設計に比べ、インターフェースを介すため初期のコード量が増加します。

### Mitigation (緩和策)
- **Standard Registration Helpers**: `TryAddEnumerableSingleton` 等の拡張メソッドを提供し、認証機能側での登録コードを 1 行で済むように定型化しました。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

### Implementation
```csharp
// ISemanticSchemeProvider の実装例 (JWT 側)
internal sealed class JwtSemanticSchemeProvider(IOptionsMonitor<JwtOptions> options) : ISemanticSchemeProvider
{
    public IEnumerable<string> GetUserSchemes() => options.CurrentValue.Enabled ? [options.CurrentValue.SchemeName] : [];
    public IEnumerable<string> GetServiceSchemes() => options.CurrentValue.Enabled ? [options.CurrentValue.SchemeName] : [];
    public IEnumerable<string> GetInternalSchemes() => [];
}
```

### Security Considerations
- **No Null Policy**: スキームプロバイダーは null ではなく空のリストを返すことを保証し、認可側のポリシー生成時に例外が発生しないようにしています。
- **Fail-Fast**: 無効な構成（Enabled=true だがスキーム名が空など）は、既存の `IValidateOptions` によって起動時にトラップされます。

## 7. Status
✅ Accepted
