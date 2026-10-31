# ADR 002: Idempotent BuildingBlock Options Registration in DI Container

- **Date**: 2026-04-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core / Options Optimization

## 1. Context (背景)

VK.Blocks の各BuildingBlock モジュール（Authentication, Redis, Caching 等）は、`ConfigureServices` フェーズにおいて自身の構成情報を即座に参照し、それに基づいた動的な DI 登録や条件付きインターセプターの有効化などを行う必要がある。このため、`AddVKBlockOptions` 拡張メソッドを通じて、Options システムへの登録と、コンテナへの Singleton 登録を同時に行う「Dual-Registration Pattern」を採用している。

しかし、現在の実装では `AddVKOidcBlock` のような拡張モジュールがコアモジュールと同じ設定セクションを参照する場合、同一の `TOptions` 型が複数回 `AddSingleton` されるという問題が発生している。

## 2. Problem Statement (問題定義)

従来の無条件な `AddSingleton` 登録には以下の技術的リスクが存在する：

1.  **DI コンテナの冗長性**: `IServiceCollection` 内に同一型のサービス記述子が複数存在することになり、メモリの無駄やライフサイクル管理時の混乱を招く。
2.  **起動パフォーマンスの低下 (🔴 Critical)**: `ValidateOnStart()` を呼び出すたびに `IStartupValidator` がコンテナに追加される。同一型に対して 5 回呼び出された場合、アプリケーション起動時に全く同じ検証ロジックが 5 回実行され、大規模システムにおいて起動時間のボトルネックとなる。
3.  **不整合の露呈**: 重複した登録の場合、DI コンテナは「最後に登録されたもの」を優先するが、開発者が意図しないインスタンスが注入されるリスクを排除できない。

## 3. Decision (決定事項)

`VK.Blocks.Core` のオプション登録ロジックを **「冪等な二重登録パターン (Idempotent Dual-Registration Pattern)」** にアップグレードする。

具体的には、以下のガード句を導入する：

```csharp
// [IDEMPOTENCY CHECK]
if (services.Any(d => d.ServiceType == typeof(TOptions)))
{
    // 既に登録済みの場合は、システム登録（AddOptions/ValidateOnStart等）をスキップし、
    // 現在のコンテキスト用の Eager-bound 実行インスタンスのみを返す
    return options;
}
```

また、シングルトン登録には `AddSingleton` ではなく `TryAddSingleton` を使用し、サービス記述子レベルでの安全性を担保する。

## 4. Alternatives Considered (代替案の検討)

### Option 1: `TryAddSingleton` のみの使用
- **Approach**: 手動の `Any` チェックを行わず、標準の `TryAddSingleton` のみに頼る。
- **Rejected Reason**: `TryAddSingleton` は記述子の重複を防げるが、その前に呼び出される `AddOptions<T>().ValidateOnStart()` による検証器の重複登録を防ぐことができないため。

### Option 2: 拡張モジュール側にチェックを委ねる
- **Approach**: 各モジュールの `AddXxx` メソッド内で重複チェックを行う。
- **Rejected Reason**: 各開発者に冪等性の担保を強いることになり、実装されるコードで重複が発生しやすいため、Core 側で隠蔽すべき責務であると判断。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **起動の高速化**: 重複した検証ロジックが排除され、モジュール数が増えても起動時間が安定する。
- **DI コンテナの健全性**: コンテナ内が整理され、`IEnumerable<TOptions>` を解決した際の予期せぬ挙動が防止される。
- **開発体験 (DX) の向上**: 開発者が重複登録を気にすることなく、必要な場所で `AddVKBlockOptions` を安全に呼び出せる。

### Negative
- **初回登録の依存性**: 初めて `AddVKBlockOptions` を呼び出した際の Configuration Section がシステム全体の Options インスタンスを決定する。

### Mitigation
- 慣習として、メイン of `AddVKAuthenticationBlock` 等を最初に行うことをドキュメントで推奨する。

## 6. Implementation & Security (実装詳細とセキュリティ考慮事項)

### 実装詳細
`Microsoft.Extensions.DependencyInjection.Extensions` の `TryAddSingleton` を併用し、マルチスレッド環境での登録安全性も確保する。

### セキュリティ
Options の検証（DataAnnotation / `ValidateOnStart` 等）が「必ず一度は実行される」ことを保証することで、不正な構成によるシステム起動を確実に阻止する（Fail-Fast 原則の維持）。
