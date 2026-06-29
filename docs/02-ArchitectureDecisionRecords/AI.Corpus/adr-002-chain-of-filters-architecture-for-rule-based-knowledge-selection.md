# ADR 002: Chain of Filters Architecture for Rule Based Knowledge Selection

- **Date**: 2026-06-13
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Corpus

## 1. Context (背景)

AI.Corpus において、収集された大量のナレッジ候補（Candidates）は、多角的なルール（残存、クールダウン、前提条件のチェック、排除リスト、トークン予算、ペルソナ制限など）によって評価・選別されなければならない。これらの多種多様なルール評価ロジックを単一の巨大なサービスの中に記述すると、特定の条件判定コードの追加や変更時にシステム全体が影響を受け、開閉原則（OCP）に著しく反する設計になってしまう。

## 2. Problem Statement (問題定義)

モノリシックなルール評価エンジンには、以下の深刻な課題がある：
1. **スパゲッティコードの発生**: `if (cooldown) { ... } else if (sticky) { ... }` のような複雑な分岐処理が何重にもネストし、一部のルールを変更した際の副作用の予測が困難になる。
2. **機能のカスタマイズ制限**: テナントやアプリケーションごとに「今回は Cooldown は使いたいが、Probability（確率選択）は無効にしたい」といったフィルタの On/Off を柔軟に制御するコンフィギュレーションが行えない。
3. **テストの複雑化**: すべてのルールが結合した状態でテストを記述する必要があるため、テストケースの組み合わせが指数関数的に増大し、テストコードの維持コストが破綻する。

## 3. Decision (決定事項)

ルールの直交性と拡張性を最大化するため、**「Chain-of-Filters (フィルターチェーン)」**アーキテクチャを採用する。

1. **`IVKKnowledgeLifecycleFilter` 抽象インターフェースの定義**:
   - すべてのフィルタリングロジックが実装すべき統一的なインターフェースを宣言する。
   - 各フィルタは、`FilterAsync(context, candidates)` メソッドを実装し、判定結果（生存、除外、維持など）のメタデータを格納した `VKFilterVerdict` を返す。
2. **17種の単一責任フィルターの実装**:
   - 以下の機能をそれぞれ独立したクラス（例: `CooldownFilter`、`StickinessFilter` 等）に完全に分離する：
     - `StickinessFilter`（強制維持）、`CooldownFilter`（クールダウン期間制御）、`DelayFilter`（遅延注入）、`DependencyFilter`（依存関係解決）、`ExclusiveGroupFilter`（排他グループ）、`TokenBudgetFilter`（容量制限）など。
3. **優先度グループ付き実行エンジン（`CorpusFilteringStage`）**:
   - `CorpusFilteringStage` が DI コンテナから解決されたフィルター群を読み込み、事前定義された優先順位グループ（例: 0:強制維持 -> 1:メタデータ制限 -> 2:行動ゲート -> 3:相互排他 -> 4:バジェット）に沿って順番に実行し、候補リストを削ぎ落としていく。

### 核心的なフィルターインターフェースとチェイン実行の設計

```csharp
namespace VK.Blocks.AI.Corpus;

public interface IVKKnowledgeLifecycleFilter
{
    // 各フィルターの実行優先順位
    int FilterOrder { get; }
    
    Task<IReadOnlyList<VKFilterVerdict>> FilterAsync(
        VKCorpusContext context,
        IReadOnlyList<VKKnowledgeLifecycleEntry> candidates,
        CancellationToken ct = default);
}

// フィルタリングステージでの評価ループ
internal sealed class CorpusFilteringStage : IVKBeforePipelineStage<VKPsycheContext>
{
    private readonly IEnumerable<IVKKnowledgeLifecycleFilter> _filters;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        var activeCandidates = context.GetCandidates();
        
        // 優先度順にソートされたフィルターチェーンを順次実行
        foreach (var filter in _filters.OrderBy(f => f.FilterOrder))
        {
            var verdicts = await filter.FilterAsync(corpusContext, activeCandidates, ct).ConfigureAwait(false);
            
            // 判定結果に基づいて activeCandidates を更新・フィルタリング
            activeCandidates = ApplyVerdicts(activeCandidates, verdicts);
        }
        
        context.SetSelectedKnowledge(activeCandidates);
        return VKResult.Success();
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Rule Engine library (e.g., RulesEngine or NRules)
- **Approach**: 外部の汎用ルールエンジンライブラリ（C# 用の NRules など）を導入し、DSL や JSON で記述されたルールを実行時に評価する。
- **Rejected Reason**: ナレッジライフサイクル特有の「前後のターン数との差分計算」や「TokenBudget の累積」のようなステートフルな処理を汎用ルールエンジンで表現するのが極めて難しく、かつ過剰な外部ライブラリ依存が生じるため。

### Option 2: Strategy Pattern inside a single Service
- **Approach**: 単一の選別サービス内で、ループを回しながら判定メソッドのデリゲートを配列で呼び出す。
- **Rejected Reason**: 各フィルタが独自の依存関係（例: `CooldownFilter` は永続化ストアが必要だが、`ProbabilityFilter` は乱数生成のみで良いなど）を持つため、それらを単一サービスでインジェクションするとコンストラクタが肥大化（Constructor Bloat）するため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **抜群の保守性と単一責任の追求**: 新しいフィルタ（例: テナント独自ルール）を開発する際は、既存のコードを 1 行も変更することなく、`IVKKnowledgeLifecycleFilter` を新規実装して DI に登録するだけで完結する（完全な OCP の達成）。
- **個別の単体テスト**: 各フィルタの判定条件を完全に独立してモックテストできるため、テストカバレッジが 100% に保ちやすい。

### Negative
- **実行順序の依存性**: フィルタ間の実行順序（例: Cooldown 判定は、Stickiness 判定より後に動かなければならない等）の管理が `FilterOrder` の静的数値に依存するため、順序の設計ミスにより意図しないフィルタ挙動になる場合がある。

### Mitigation
- フィルタを「0.強制維持」「1.静的メタデータ」「2.動的行動ゲート」「3.排他と相互排除」「4.物理制限（予算）」の 5 レベルの優先度グループに分類し、ドキュメントと定数定義で優先順位設計のポリシーを厳格化する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Filter Bypass Prevention**: 安全性や権限監査に関連するフィルタ（例: `UserSegmentFilter` や `PersonaFilter` などのアイデンティティ境界チェック）は、設定トグルによる無効化を禁止するか、あるいはチェーンの最前列で実行を保証するように保護する。

## 7. Status
✅ Accepted
