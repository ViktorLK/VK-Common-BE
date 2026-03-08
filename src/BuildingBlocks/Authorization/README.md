# VK.Blocks.Authorization

## はじめに

**VK.Blocks.Authorization** は、現代的な .NET 10 アプリケーション向けの高機能な認可（Authorization）フレームワークを提供するビルディングブロックです。
最小特権の原則（Least Privilege）とテナント分離を開発の中核に据え、単一テナントから大規模なマルチテナントシステムまで、堅牢かつ柔軟なアクセス制御を実現することを目的としています。

## アーキテクチャ

本モジュールは、VK.Blocks の設計思想に基づき、以下のアーキテクチャパターンを採用しています：

- **Policy-Based Authorization**: ASP.NET Core 標準の認可ポリシーを高度に抽象化し、ビジネス要件に基づいた柔軟なポリシー定義が可能です。
- **Dynamic Attribute Evaluation**: リクエストコンテキストやユーザー属性に基づき、認可ロジックを実行時に動的に評価するエンジン（`IAttributeEvaluator`）を備えています。
- **Feature-Driven (Vertical Slice)**: 機能（Feature）ごとに独立したフォルダ構造を採用し、開発の並行性とメンテナンス性を向上させています。
- **Enterprise Security Standards**: テナント分離（Tenant Isolation）などのエンタープライズ特有のセキュリティ要件を標準機能として統合しています。

## 主な機能

- **動的ポリシー評価 (Dynamic Policies)**:
    - クレームやカスタム属性に基づいた柔軟な条件判定。
    - `DynamicRequirement` による高度な条件分岐のサポート。

- **パーミッションベースの管理 (Permission-Based System)**:
    - 属性 `AuthorizePermissionAttribute` を用いた宣言的なアクセス制御。
    - パーミッションのレジストリ（`IPermissionRegistry`）による権限の一元管理。

- **マルチテナント分離 (Multi-Tenant Isolation)**:
    - 同一テナント内でのデータアクセスを強制する `SameTenantRequirement`。
    - クロステナント攻撃を未然に防ぐ強力なガードレール。

- **時間帯ベースのアクセス制限 (Working Hours)**:
    - `WorkingHoursRequirement` により、業務時間外のクリティカルな操作を遮断。

- **ネットワーク制限 (Network-Based Security)**:
    - 社内ネットワークからのアクセスのみを許可する `InternalNetworkOnly` ポリシーの提供。

- **ランクベースの認可自動生成 (Rank-Based Automation)**:
    - `EmployeeRank` 枚挙から認可ポリシーと属性を Source Generator で自動生成。
    - `[MinimumRank(EmployeeRank.Senior)]` のような強型付けされた属性による安全な認可定義。
    - `RankPolicies` 定数による、入力ミスを防ぐポリシー指定。

## 採用技術

- **Core**: .NET 10 / ASP.NET Core Authorization
- **Dependency**: MediatR (Orchestration context)
- **Validation**: FluentValidation
- **Observability**: System.Diagnostics / OpenTelemetry
- **Standards**: RFC 7807 (Problem Details for Errors)

## 開始方法

### 1. サービスの登録

`IServiceCollection` に対して、認可機能を登録します。

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddVKAuthorization(options =>
    {
        // カスタム設定（必要に応じて）
    });
}
```

### 2. コントローラーでの利用

属性を使用して、アクションレベルでの認可を適用します。

```csharp
[AuthorizePermission("financial:write")]
[Authorize(Policy = VKPolicies.WorkingHoursOnly)]
public IActionResult ProtectedAction()
{
    return Ok();
}
```

## 今後の展望

本モジュールをより堅牢でスケーラブルなエンタープライズ対応のセキュリティ基盤へと進化させるため、以下のロードマップを予定しています：

- **認証/認可監査ログ (Security Audit Logging)**:
    - **背景**: 現状の `AuthorizationFailureReason` は主に開発者向けの診断情報ストリームです。運用環境において「同一 IP から 1 分間に 50 回の異なる TenantId アクセス試行」といった異常な振る舞いを検知するためには、これを独立したセキュリティイベントストリームとして扱う必要があります。
    - **対策**: `AuthorizationHandler` の基底クラスに統一されたフックを追加し、認可の成否（特に異常な失敗）を非同期イベントとして `VK.Blocks.Observability` モジュールへ送信し、システムレベルでのセキュリティアラート（Security Alarm）をトリガーする仕組みを導入します。

- **ポリシー解決のパフォーマンス最適化 (Policy Caching/Cost)**:
    - **背景**: `PermissionHandler` は `IPermissionProvider` に依存していますが、すべてのリクエストで都度データベースや外部サービスへ権限を問い合わせる構成は、システム全体の深刻なパフォーマンスボトルネックとなる潜在的なリスクを孕んでいます。
    - **対策**: 権限クエリの頻度を監査し、`IPermissionProvider` のデフォルト実装に `IMemoryCache` 等を用いたキャッシュロジックを統合します。権限メタデータのキャッシュライフサイクルをプロトコルとして明確に規定し、高速かつ低コストなポリシー解決を実現します。

- **開発者体験の向上 (DX - Custom Attributes)**:
    - **背景**: マジックストリングの排除（Rule 13）は進んでいますが、`[Authorize(Policy = "MinimumRank_Junior")]` のような指定方法は依然として呼び出し側でのタイポのリスクを残しています。
    - **対策** (一部導入済): Source Generator を活用し、`EmployeeRank` 枚挙などのドメイン知識から `[MinimumRank(EmployeeRank.Junior)]` のような強型付けされた特性（Attribute）を自動生成するアーキテクチャを標準化します。これにより、開発者はコンパイル時の型安全性を享受し、堅牢なポリシー定義を手間なくシームレスに行うことが可能になります。

- **AI-Based Policy Generator**:
    - 管理画面からの自然言語入力による認可ポリシーの自動生成機能の導入。
