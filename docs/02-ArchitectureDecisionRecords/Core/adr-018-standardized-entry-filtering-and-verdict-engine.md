# ADR 018: Standardized Entry Filtering and Verdict Engine

- **Date**: 2026-06-15
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Core

## 1. Context (背景)

VK.Blocks の複数の BuildingBlock において、特定のコンテキスト（TenantId、ターン数、予算、その他のビジネス制約）に基づいてオブジェクトやデータエントリーのコレクション（ナレッジライフサイクルエントリー、プロンプトセグメント、ルール定義など）をフィルタリングし、その生存判定を行う必要性が生じている。これらのフィルタリングロジックや生存判定レコード（Verdict）が、各モジュール（例: `AI.Corpus` や `AI.Psyche` 等）で個別に定義・実装されると、モジュール間でのフィルターの再利用が困難になり、データモデルの不一致が発生する。

## 2. Problem Statement (問題定義)

モジュール固有の局所的なフィルター抽象化には、以下の課題が存在する：
1. **コードの重複と規格不統一**: フィルタを実行した結果を表現するデータモデル（例: `VKFilterVerdict`（生存・排除・維持など））がモジュールごとにほぼ同じ構造で二重実装され、コア共通ライブラリとしての統一感が損なわれる。
2. **横断的フィルターの結合不足**: セグメントに対する一般的なフィルタリング（例: テナント隔離や無効化フラグのチェックなど）は、本来すべてのモジュールで共通して適用できるべきだが、インターフェースが共通化されていないため、各ブロックで都度ボイラープレートコードが書かれる。
3. **拡張性とテスト容易性の低下**: 入力（`TItem`）と判定用コンテキスト（`TContext`）を受け取って非同期に判定を返す汎用的な設計になっていないため、拡張性の高いフィルタエンジンが組めない。

## 3. Decision (決定事項)

フィルター設計の再利用性と統一性を保証するため、**「Standardized Entry Filtering and Verdict Engine in Core (Coreにおける標準化エントリーフィルターおよび判定エンジン)」**を決定する。

1. **`VK.Blocks.Core` へのフィルター抽象の一元化**:
   - `Core/Filtering` 名前空間に、汎用的なフィルターインターフェース `IVKEntryFilter<TItem, TContext>` を定義する。
   - フィルタの実行結果（生存状態、除外理由、適用スコアなど）を表現する共通レコード `VKFilterVerdict` を定義する。
2. **拡張メソッドによるユーティリティの提供**:
   - フィルターの評価結果を安全に集計し、排除されたものをパージする共通のロジックを `VKFilterExtensions` として定義し、各ブロックでのソートとフィルタ適用時のコード量を削減する。
3. **具象ブロックでの適用**:
   - `AI.Corpus` の `IVKKnowledgeLifecycleFilter` などは、この Core の `IVKEntryFilter` 抽象をベースに継承・展開する構造へとリファクタリングを施す。

### Core での標準フィルター定義

```csharp
namespace VK.Blocks.Core.Filtering;

/// <summary>
/// A standardized filter contract for evaluating an item against a given context.
/// Complies with CS.01 / CS.03.
/// </summary>
/// <typeparam name="TItem">The type of the item to be evaluated.</typeparam>
/// <typeparam name="TContext">The context type providing execution state.</typeparam>
public interface IVKEntryFilter<in TItem, in TContext>
{
    /// <summary>
    /// Gets the execution priority of the filter.
    /// </summary>
    int FilterOrder { get; }

    /// <summary>
    /// Evaluates the item and returns a filter verdict.
    /// </summary>
    Task<VKFilterVerdict> EvaluateAsync(TItem item, TContext context, CancellationToken ct = default);
}

/// <summary>
/// Represents the result of a single filter evaluation.
/// </summary>
public sealed record VKFilterVerdict
{
    public required bool IsPassed { get; init; }
    public string? Reason { get; init; }
    public double ScoreModifier { get; init; } = 1.0;
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Keep Filters block-specific
- **Approach**: 各モジュールがそれぞれ独自の `IVKFilter` や `Verdict` クラスを内部で保ち続ける。
- **Rejected Reason**: システム全体が成熟するにつれ、フィルターの共通化（例: すべてのAIブロックで共通して機能する "TokenLimitFilter" や "TenantGuardFilter" など）の要求が高まるが、インターフェースが共通化されていないとそれらを別ブロックに共有できないため。

### Option 2: Generic Filter delegate
- **Approach**: クラスインターフェースではなく、`Func<TItem, TContext, Task<VKFilterVerdict>>` のような単純なデリゲートのみを定義する。
- **Rejected Reason**: 各フィルターが DI コンテナから独自のサービス（キャッシュストアや DB コンテキストなど）をインジェクションしてステートフルに動作するため、クラスベースの DI 登録とライフサイクル管理ができるインターフェースモデルの方が適しているため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **抜群の再利用性**: Core レベルでフィルターモデルが定義されたため、任意のモジュール（例: AI.Psyche, AI.Afferent, AI.Corpus）で全く同じ `IVKEntryFilter` 基盤をベースに再利用およびモックテストが可能。
- **統一されたロギングと監視**: どのフィルターがどの理由（`Reason`）でエントリーを弾いたのかを、共通の `VKFilterVerdict` を介してシステム全体で統一的に構造化ログ・トレース出力できる。

### Negative
- **Core の役割拡大**: Core モジュールにビジネスロジックに近い「フィルタリング概念」が追加されるため、Core の依存度が僅かに高くなる。

### Mitigation
- `IVKEntryFilter` はインターフェースおよびデータホルダー（Verdict）としてのピュアな定義のみを Core に置き、具体的なフィルタロジックの実装自体は一切 Core に含めず、各具象モジュール側で閉じて実装するポリシーを堅持する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Failure Isolation**: 複数のフィルターを順次実行する際、いずれかのフィルター内部で例外が発生した場合のハンドリングポリシーを定義し、個別のフィルターのバグが呼び出し元全体の処理停止を引き起こさないよう、デフォルトのフォールバック動作（例外時は安全側に倒して Passed = false にする等）を適用する。

## 7. Status
✅ Accepted
