# ADR 003: Introducing Service Marker Pattern for BuildingBlock Modularization

- **Date**: 2026-04-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core / Modularity & Dependency Validation

## 1. Context (背景)

VK.Blocks の BuildingBlock システムが拡張されるにつれ、モジュール間の依存関係を確認する必要性が増している。例えば、`Authentication.OpenIdConnect` 拡張モジュールは、`Authentication` コアモジュールが正しく登録されていることを前提としている。

従来は `IOptions<TOptions>` の登録有無を確認することで代用してきたが、これは以下の理由で不十分であった：
- **概念の混同**: 「構成（Options）が存在すること」は必ずしも「関連するサービス群が DI コンテナに登録され、機能が有効化されていること」を保証しない。
- **抽象化の欠如**: 共通のインターフェースや機能ブロック（例：キャッシュ）に対して、どの実装が選択されているかを統一的に判断する手段がなかった。

## 2. Problem Statement (問題定義)

Options 依存のチェックには以下の「坏味道 (Bad Smell)」が存在する：

1.  **セマンティクスの欠如**: `services.Any(d => d.ServiceType == typeof(VKAuthenticationOptions))` というコードは、アーキテクチャ上の意図（認証機能が有効か？）を直接的に表現していない。
2.  **疎結合の阻害**: 拡張モジュールがコアモジュールの具体的な Options 型を詳細まで知る必要があり、依存関係が強まってしまう。
3.  **検証の不完全性**: 構成だけが読み込まれ、実際のハンドラーやサービスが追加されていない不完全な状態を検知できない。

## 3. Decision (決定事項)

**「サービスマーカーパターン (Service Marker Pattern)」** を導入し、BuildingBlock の登録状態をドメイン駆動的に管理する。

### 核心設計
`VK.Blocks.Core` に以下の拡張メソッドを追加する：

```csharp
public static class VKBlockServiceExtensions
{
    // マーカーの有無をチェック
    public static bool IsVKBlockRegistered<TMarker>(this IServiceCollection services)
        where TMarker : class;

    // マーカーを登録
    public static IServiceCollection AddVKBlockMarker<TMarker>(this IServiceCollection services)
        where TMarker : class;
}
```

### 運用ルール（標準エントリパターン）
各 BuildingBlock の登録メソッドでは、以下の **「Check-Self, Check-Prerequisite, Mark-Self」** パターンを厳守し、冪等性とFail-Fastを両立させる：

1.  **Check-Self (Idempotency)**: `IsVKBlockRegistered<OwnBlock>()` で既に登録済みかを確認し、済みなら即座にリターンする。
2.  **Check-Prerequisite (Fail-Fast)**: 依存する基礎ブロック（例：`AuthenticationBlock`）の存在を確認し、なければ `InvalidOperationException` をスローする。
3.  **Actual Registration**: `TryAdd` 系メソッドや `AddVKBlockOptions` を用いて、実際のサービス登録を行う。
4.  **Mark-Self (Success Commit)**: メソッドの最後に `AddVKBlockMarker<OwnBlock>()` を呼び出し、これ以降の重複処理を防止すると同時に、登録の成功を証明する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: 属性 (Attribute) によるスキャン
- **Approach**: 各モジュールに関連属性を付与し、アセンブリをスキャンする。
- **Rejected Reason**: 実行時のパフォーマンスコストが高く、柔軟な条件付き登録（Runtime Decision）に対応しにくいため。

### Option 2: 共通のベースクラス/インターフェース
- **Approach**: すべての Block に共通の `IBaseBlock` 等を実装させる。
- **Rejected Reason**: 型安全性に欠け、特定のブロック（例：Auth ではなく Cache が必要）を特定するために結局追加の型情報が必要になるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **セマンティックな依存性チェック**: `IsVKBlockRegistered<AuthenticationBlock>()` のように、コードの意図が明確になる。
- **堅牢性の向上**: 前提条件が満たされていない場合に、早期に（Fail-Fast）エラーを出すことが容易になる。
- **疎結合の維持**: 具体的な設定クラス (Options) ではなく、論理的なブロック名 (Marker) に依存させることができる。

### Negative
- **マーカークラスの定義が必要**: 各モジュールに中身が空のマーカークラスが必要になるが、これは Block 定義クラス自体を再利用することで緩和可能。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

### 実装
`TryAddSingleton<TMarker>()` を使用することで、複数の拡張モジュールが同じマーカーを追加しようとした場合でも安全に（冪等に）動作する。

### セキュリティ
モジュール間の依存関係を厳格化することで、認証や認可などの重要なセキュリティブロックが「未登録のままバイパスされる」といった構成ミスを未然に防ぐことができる。
