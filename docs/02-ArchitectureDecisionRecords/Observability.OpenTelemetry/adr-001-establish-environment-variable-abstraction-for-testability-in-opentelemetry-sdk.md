# ADR 001: Establish Environment Variable Abstraction for Testability in OpenTelemetry SDK

**Date**: 2026-03-12
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Observability.OpenTelemetry Resource Detection Improvements

---

## 1. Context (背景)

`VK.Blocks.Observability.OpenTelemetry` モジュールにおけるクラウドリソース（Azure App Service, Kubernetes 等）の自動検出ロジック (`VkResourceBuilder`) は、これまで `System.Environment.GetEnvironmentVariable()` を直接呼び出すことで実装されていました。
対象となっていた環境変数は `WEBSITE_SITE_NAME`、`K8S_CLUSTER_NAME`、`HOSTNAME` などです。

## 2. Problem Statement (問題定義)

OSの環境変数への強固な構造的結合（Hard Dependency）が存在することにより、以下の重大な問題が発生していました。

*   **テスト容易性の欠如 (Lack of Testability)**: ユニットテスト実行時に特定のクラウド環境をシミュレートするためには、テスト・プロセス自身の環境変数を書き換える必要がありました。
*   **テストの脆弱性と汚染 (Test Fragility & State Pollution)**: プロセスレベルの環境変数を変更することは、テストの並列実行を阻害し、他のテストケースに波及する状態汚染（State Pollution）を引き起こすリスクがあります。

```csharp
// 悪い例 (Bad Practice): 強い結合によるテスト困難性
var podName = System.Environment.GetEnvironmentVariable("K8S_POD_NAME")
           ?? System.Environment.GetEnvironmentVariable("HOSTNAME");
```

## 3. Decision (決定事項)

環境変数へのアクセスを抽象化する `IEnvironmentProvider` インターフェースを導入し、`System.Environment` に直接依存しない設計（Dependency Inversion）を採用することを決定しました。

### 設計詳細 (Design Details)

1.  **抽象化の導入**:
    ```csharp
    public interface IEnvironmentProvider
    {
        string? GetVariable(string name);
    }
    ```
2.  **デフォルト実装の提供**: `DefaultEnvironmentProvider` として本番用の実装を提供。
3.  **Method Injection の適用**: `VkResourceBuilder.Build` メソッドや各検出ヘルパーメソッドに対して、引数として `IEnvironmentProvider` を注入（Method Injection）する設計に変更。
4.  **構成の起点**: Fluent API の起点である `VkObservabilityBuilder` のコンストラクタ内で `DefaultEnvironmentProvider` を初期化して渡す。

今後は、VK.Blocks の SDK ロジックやドメインロジックにおいて、`System.Environment.GetEnvironmentVariable` を直接使用することは非推奨（Discouraged）となります。

## 4. Alternatives Considered (代替案の検討)

*   **Option 1: テストフレームワーク側での環境変数モックライブラリの使用**
    *   **Approach**: アプリケーションコードは変更せず、テスト時に特定のスレッドコンテキストでのみ環境変数をオーバーライドするライブラリを導入する。
    *   **Rejected Reason**: .NET 固有のスレッド・コンテキストや AsyncLocal との相性問題が発生しやすく、テストコードが不必要に複雑化するため。設計の根本解決（関心事の分離）になっていない。
*   **Option 2: `VkObservabilityOptions` へのプロパティ追加**
    *   **Approach**: `VkObservabilityOptions` に `K8sClusterName` 等のプロパティを追加し、アプリケーション起動時に `IConfiguration` 経由でバインドさせる。
    *   **Rejected Reason**: SDK として「自動検出（Auto Detection）」を行うという責務が失われ、利用側のアプリケーション開発者に設定の負担を強いることになるため。

## 5. Consequences & Mitigation (結果と緩和策)

*   **Positive**:
    *   クラウド検出ロジックが 100% ユニットテスト可能になった。
    *   並列実行可能な堅牢なテストスイートを構築できるようになった。
    *   システム環境への依存という「副作用」が明示的なインターフェース契約として可視化された。
*   **Negative**:
    *   引数に `IEnvironmentProvider` を引き回すため、メソッドシグネチャがわずかに長くなる。
*   **Mitigation**:
    *   メソッドインジェクションに留め、不必要な DI コンテナへの登録（`IServiceCollection.AddSingleton<IEnvironmentProvider>()`）は控えることで、全体的な構成の複雑化を避ける。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

*   **セキュリティ重点**:
    *   環境変数の読み取り（Read-only）に特化したインターフェースに限定しているため、意図しない環境変数の書き換え（改ざん検知・防止）を防ぐセキュアな設計となっている。
    *   機密情報（トークンやキー）の読み取りには使用せず、あくまでクラウドリソース識別子のみにスコープを限定している。

---
**Last Updated**: 2026-03-12
**Total ADRs**: 1
