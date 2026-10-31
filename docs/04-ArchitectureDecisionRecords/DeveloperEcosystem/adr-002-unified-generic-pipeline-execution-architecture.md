# ADR 002: Unified Generic Pipeline Execution Architecture

- **Date**: 2026-06-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Core

## 1. Context (背景)

プロンプト構築（Weaving）や入力遮断（Afferent Guardrails）といった AI 処理のパイプラインでは、処理の実行フローにおいて共通の実行モデルが必要となる。これには、事前ステージの実行（Before Stages）、オニオン構造のミドルウェアチェーン（Middleware Onion Chain）、ターミナルエンジン（LLM 呼び出しや最終アサーション）、および事後ステージの実行（After Stages）が含まれる。これらのパイプライン制御ロジックをモジュールごとに個別に実装すると、コードの重複が発生し、実行順序の制御（Scheduling）やキャンセル伝随（Cancellation Propagation）の一貫性を損なうリスクがある。

## 2. Problem Statement (問題定義)

モジュール個別のパイプライン実行コードには、以下の課題がある：
1. **車輪の再発明とコードの重複**: 順序（`StageOrder`）や並行グループ（`ParallelGroup`）に基づく実行チャンク化とタスク並行化（`Task.WhenAll`）のロジックが、`AI.Psyche` などの複数のモジュールで二重に実装され、実装バグの温床となる。
2. **ミドルウェアの規格不統一**: 処理に割り込んで処理前・処理後にログ出力やリトライを差し挟む Onion Middleware パターンのインターフェース（`InvokeAsync(context, next)`）のシグネチャがモジュールごとにばらつき、共通のインターセプターを再利用できない。
3. **拡張時の破壊的変更**: 新しい実行パイプラインを持つモジュール（例: 新規の `AI.Afferent` 等）を追加するたびに、一から同様のランナーやスケジューラを書き起こす必要があり、生産性が低下する。

```csharp
// 悪い例: モジュールごとに個別のパイプラインランナーが独自にタスク実行を制御している
public class PsychePipelineRunner { ... }
public class AfferentPipelineRunner { ... }
```

## 3. Decision (決定事項)

パイプライン実行アルゴリズムの再利用性と堅牢な一元化を両立するため、**「Core-Level Generic Pipeline Execution Abstractions (Core レベルの汎用パイプライン実行抽象)」**を採用する。

1. **`VK.Blocks.Core` への実行基盤の定義**:
   - 共通のパイプライン実行基盤クラス `VKPipelineExecutorBase<TContext, TResponse>` を定義する。
   - `IVKBeforePipelineStage<T>`、`IVKAfterPipelineStage<T>`、`IVKMiddleware<TContext, TResponse>` の共通インターフェースおよびデリゲート `VKPipelineDelegate<TResponse>` を宣言する。
2. **実行フェーズの標準化**:
   - `VKPipelineExecutorBase` において、`BeforeStages (Chunked/Parallel Execution) -> Middleware Onion Wrapping -> Terminal Invocation -> AfterStages (Chunked/Parallel Execution)` の一連の実行アルゴリズムをテンプレートメソッドパターンで確定する。
3. **各モジュールでの再利用継承**:
   - `DefaultPsychePipelineExecutor` 等は、この基盤クラスを継承し、ターミナルアクション（例: `IVKChatEngine` の呼び出し）のみをオーバーライド（`InvokeTerminalAsync`）する形に整理する。

### 核心的な共通パイプライン実行アルゴリズム (Core)

```csharp
namespace VK.Blocks.Core;

public abstract class VKPipelineExecutorBase<TContext, TResponse> : IVKPipelineExecutor<TContext, TResponse>
    where TContext : class
{
    private readonly List<List<IVKBeforePipelineStage<TContext>>> _beforeChunks;
    private readonly List<List<IVKAfterPipelineStage<TContext>>> _afterChunks;
    private readonly List<IVKMiddleware<TContext, TResponse>> _middlewares;

    protected abstract Task<VKResult<TResponse>> InvokeTerminalAsync(TContext context, CancellationToken cancellationToken);
    protected abstract bool CheckAborted(TContext context);
    protected abstract VKResult GetAbortResult(TContext context);

    public virtual async Task<VKResult<TResponse>> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
    {
        // 1. BEFORE ステージの実行 (順序ソート、Task.WhenAll 並行チャンク実行)
        var beforeResult = await VKPipelineRunner.ExecuteChunksAsync(...).ConfigureAwait(false);
        if (beforeResult.IsFailure) return VKResult.Failure<TResponse>(beforeResult.Errors);

        // 2. ミドルウェアオニオンチェーンの構築
        VKPipelineDelegate<TResponse> chain = () => InvokeTerminalAsync(context, cancellationToken);
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var currentNext = chain;
            chain = () => middleware.InvokeAsync(context, currentNext, cancellationToken);
        }

        // 3. ミドルウェア & ターミナル実行
        var response = await chain().ConfigureAwait(false);

        // 4. AFTER ステージの実行
        // ...
        return response;
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Composition over Inheritance via Pipeline Builder
- **Approach**: 継承（`VKPipelineExecutorBase` の継承）を使わず、`PipelineBuilder` クラスを用いて実行メソッドのデリゲートを組み立てるアプローチ。
- **Rejected Reason**: デリゲートの組み立てに動的アロケーションが多く発生し、ホットパスでのパフォーマンス要求を満たしにくいこと、またステージ一覧をDIから自動収集して段階的に解決するクラス型モデルの方が、DI統合やカスタマイズがしやすいため。

### Option 2: MediatR Pipeline Behaviors
- **Approach**: 業界標準の `MediatR` の Pipeline Behavior パターンを全面的に借用する。
- **Rejected Reason**: MediatR は非同期の並行タスクグループ（Chunked/Parallel Grouping）を標準でサポートしておらず、順序定義と並行化を実装するために結局独自のスケジューラが必要になるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **DRY (Don't Repeat Yourself) の徹底**: スケジューリング、並行化実行（`Task.WhenAll`）、ミドルウェアのネスト、およびキャンセルチェックの共通コードが一元化され、バグ改修が全モジュールに自動波及する。
- **極めて高い拡張性**: 新しいパイプラインを必要とするモジュールは、コンテキストとステージの型を定義してDI登録するだけで、標準的な並行化・ミドルウェアサポートを即座に享受できる。

### Negative
- **抽象的な型階層**: ジェネリクス `<TContext, TResponse>` が深く入れ子になるため、初めてコードを読む開発者にとってパイプラインの追跡がやや抽象的で難解に感じられる。

### Mitigation
- 各モジュールごとに具象インターフェース（例: `IVKPsycheBeforePipelineStage : IVKBeforePipelineStage<VKPsycheContext>`）を定義して隠蔽し、ジェネリクスを直接意識せずに記述できるボイラープレート保護を施す。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Abortion Checks**: ミドルウェアの実行前後、Before/After ステージの実行契機ごとに、コンテキストの `IsAborted` フラグを走査する防御処理を強制し、先行タスクでエラーが起きた場合に後続の並行タスクが不必要にリクエストを外部へ投げ続けるのを防止する。

## 7. Status
✅ Accepted
