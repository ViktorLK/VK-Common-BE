# ADR 005: Refactor Authorization Evaluation to Specialized Permission Evaluator Pattern and Retain Automation

- **Date**: 2026-04-02
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization

## 1. Context (背景)

`VK.Blocks.Authorization` モジュールでは、これまで `IVKAuthorizationHandler` という汎用的なインターフェースを拡張ポイントとして提供してきましたが、このインターフェースは役割が曖昧であり、実質的に「権限チェック（Permission Check）」専用として使用されていました。また、ASP.NET Core の `IAuthorizationHandler` と名称が酷似しており、開発上の混乱を招く要因となっていました。

さらに、既存のメソッドは同期的な `bool` または `Task<bool>` を返しており、エンタープライズ級の **Result Pattern (Rule 1)** に準拠しておらず、エラーの詳細コンテキストを呼び出し側に伝えることが困難でした。

## 2. Problem Statement (問題定義)

1.  **単一責任原則の不徹底**: `IVKAuthorizationHandler` が「認可ハンドラー」なのか「権限評価器」なのかが不明確。
2.  **Result Pattern への非準拠**: 認可失敗時に「なぜ失敗したか（例：テナント不一致なのか、権限不足なのか）」を構造化データとして返せない。
3.  **垂直スライスの不全 (Rule 12)**: 権限関連の抽象がグローバルな `Abstractions` フォルダに散在しており、特定の Feature への凝集度が低い。
4.  **手動 DI 登録の摩擦**: Source Generator を排除して手動登録に切り替えた場合、"Low Friction" というプロジェクトの哲学に反し、ヒューマンエラーを誘発する。

## 3. Decision (決定事項)

1.  **インターフェースの廃止と再定義**:
    - `IVKAuthorizationHandler` を廃止。
    - 新たに `IPermissionEvaluator` インターフェースを `Features/Permissions` フォルダ内に導入。
2.  **Result 模式の導入**:
    - 戻り値を `Task<Result<bool>>` に変更。
3.  **Source Generator の継続利用とアップグレード**:
    - `AuthorizationHandlersGenerator` をアップグレードし、`IPermissionEvaluator` を自動認識して DI 登録 (`Scoped`) を行うようリファクタリング。
4.  **低摩擦（Low Friction）の堅持**:
    - 開発者がインターフェースを実装するだけで、DI 登録を意識せずに済む自動化を維持。

### 核心の設計 (Interface Definition)

```csharp
namespace VK.Blocks.Authorization.Features.Permissions;

public interface IPermissionEvaluator
{
    Task<Result<bool>> HasPermissionAsync(
        ClaimsPrincipal user, 
        string permission, 
        CancellationToken ct = default);
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 手動登録 (Explicit DI Registration)
- **Approach**: Source Generator を削除し、すべてのハンドラーを `ServiceCollectionExtensions` で手動登録する。
- **Rejected Reason**: ハンドラーが増えるたびに手動更新が必要になり、Low Friction 原則に反する。

### Option 2: IVKAuthorizationHandler の維持と名前空間変更
- **Approach**: 名前はそのままに、所属フォルダと戻り値だけを変更する。
- **Rejected Reason**: `AuthorizationHandler` という言葉が強すぎて、依然として「権限評価」という特定の責務を表現しきれない。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- 権限評価の責任が明確になり、コードの読みやすさが向上。
- Result Pattern により、認可失敗時の詳細な診断が可能。
- Vertical Slice アーキテクチャに従い、機能ごとの独立性が高まる。

### Negative
- 既存の `IVKAuthorizationHandler` を利用しているプロジェクトに破壊的変更が発生する。

### Mitigation
- Walkthrough ドキュメントにおいて、移行ガイド（名前の置換方法）を明示する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **自動登録**: SG は `IPermissionEvaluator` を実装したクラスを検出し、`IAuthorizationHandler` と `IPermissionEvaluator` の両方として登録する。
- **セキュリティ**: `HasPermissionAsync` は内部で `IPermissionProvider` を呼び出す際、常に `Result` をラップし、例外を境界でキャッチして安全な `Result.Failure` に変換する。

---
**Last Updated**: 2026-04-02
