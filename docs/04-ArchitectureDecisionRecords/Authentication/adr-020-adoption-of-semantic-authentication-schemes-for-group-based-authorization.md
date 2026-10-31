# ADR 020: Adoption of Semantic Authentication Schemes for Group-Based Authorization

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authentication Modularization

## 2. Context (背景)

ASP.NET Core の標準的な認証システムでは、コントローラーやアクションに対して `[Authorize(AuthenticationSchemes = "Bearer,ApiKey")]` のように具体的なスキーム名を指定する必要があります。しかし、アプリケーションが成長し、認証方式が追加・変更（例：JWT から OAuth、あるいは mTLS への移行）されるたびに、ソースコード内の多数の箇所を修正しなければならず、保守性と柔軟性に課題がありました。

## 3. Problem Statement (問題定義)

1. **密結合**: 認可の要件（誰がアクセスするか）と、それを実現する技術（どのスキームを使うか）が密結合している。
2. **マジックストリング**: スキーム名の文字列が分散し、スペルミスや不整合の原因となる。
3. **拡張の困難さ**: 新しい認証方式を導入した際、既存の全てのコントローラーの属性を更新する必要がある。

## 4. Decision (決定事項)

技術的な認証スキームを「User」「Service」「Internal」という 3 つの意味論的なグループ（Semantic Groups）に分類して管理する設計を採用しました。

### 核心的なメカニズム
1. **IVKSemanticSchemeProvider**: 各認証機能（JWT, ApiKey 等）が、自身がどのグループに属するかを表明するためのインターフェースを導入。
2. **VKAuthGroupAttribute**: 開発者は具体的なスキーム名ではなく、セマンティックなポリシー名（例：`AuthPolicies.GroupUser`）を指定。
3. **動的スキーム解決**: 実行時に `IVKSemanticSchemeProvider` を介して、指定されたグループに対応する全技術スキームを自動的に収集・適用。

```csharp
// 開発者の使用例
[VKAuthGroup(AuthPolicies.GroupUser)] // 人間ユーザー（JWT, OAuth 等）を許可
public class MyController : ControllerBase { ... }
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 静的なポリシー定義**: 全てのポリシーを `Startup.cs` で静的に定義する。
    - **Rejected Reason**: 認証ブロックがプラグイン式（モジュール式）であるため、どのモジュールが有効化されているかに応じて動的にポリシーを構成する必要があり、静的定義では不十分。
- **Option 2: 共通のベースクラス**: 認証を処理するベースコントローラーを作成する。
    - **Rejected Reason**: 継承による制約が強く、既存の Web API との統合が困難になる。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 認証技術の変更がビジネスロジック（コントローラー）に影響を与えない。
    - 新しい認証方式（例：Biometrics）を「User」グループに追加するだけで、既存の `[VKAuthGroup(GroupUser)]` が付与された全エンドポイントで自動的に有効になる。
- **Negative**:
    - 認証スキームの解決ロジックが一段階抽象化されるため、デバッグ時に「実際にどのスキームが呼ばれているか」を確認する手順が一つ増える。
- **Mitigation**:
    - `AuthenticationMetadataProvider` を通じて、有効なセマンティックスキームの一覧を診断ログおよび OpenTelemetry メトリクスに出力する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Fail-Safe**: グループに有効なスキームが一つも登録されていない場合、セキュリティの観点から「全員拒否（401/403）」として動作させる。
- **優先順位**: 同一グループ内に複数のスキームがある場合、構成（Options）で指定された優先順位に従って評価を行う。

**Last Updated**: 2026-04-24
