# ADR 010: Provider Agnostic Multi Agent Collaboration

- **Date**: 2026-06-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.SemanticKernel

## 1. Context (背景)

複雑な業務シナリオにおいて、単一の AI プロンプトでは解決できない課題を、役割分担された複数の AI エージェント（例: ユーザー要件を分析する「アナリスト」、C# コードを生成する「プログラマー」、コードの品質と規約を検査する「レビュアー」）の協調対話によって解決する「マルチエージェント（Multi-Agent）ワークフロー」の需要が高まっている。これらを業界共通の BuildingBlock 規格で実装するにあたり、特定のプラットフォーム（Microsoft や OpenAI 固有の SDK）に依存せず、かつ高度な協調ルーティングを行える設計が必要であった。

## 2. Problem Statement (問題定義)

マルチエージェント機能をプロバイダー固有の API に直接依存させて構築する手法には、以下の問題がある：
1. **ベンダーロックインの激化**: 協調対話（Chat Group）の履歴管理や発言者の遷移制御（Selection Strategy）は複雑であり、これを Microsoft Semantic Kernel や OpenAI Assistants API に密結合させると、他方のプラットフォームへ移行する際にアプリケーション層の再設計が必要になる。
2. **ルーティング（発言者選択）の柔軟性欠如**: 固定のシーケンシャルな順番（順繰り）でしかエージェントが発言できない場合、前のエージェントの出力にエラーがあった際に「修正担当の別エージェントに差し戻す」といった動的なフロー制御を行えない。
3. **無限ループとクォータ消費**: エージェント同士が同じ内容を繰り返し発言し合う無限対話（Infinite Loop）に陥った際、自動で検知して安全にプロセスを中断する防線（Termination Strategy）が不在の場合、クラウド API 使用量が激増する。

## 3. Decision (決定事項)

エージェント協調の抽象定義と、Semantic Kernel の Agentic 機能を利用した高性能な具現を接続するため、**「Provider-Agnostic Agentic Collaboration (ベンダーに依存しないエージェント協調)」**パターンを採用する。

1. **核心的な抽象インターフェースをコア（AI Block）に導入**:
   - 役割を定義する `IVKAgent`、それらを束ねて実行する `IVKAgentGroup`、および構築を担う `IVKAgentFactory` を共通の `VK.Blocks.AI` 名前空間に定義する。
2. **`AI.SemanticKernel` による Agent 実行の実装**:
   - `AISKAgent`（`ChatCompletionAgent` を内包）および `AISKAgentGroupRunner` を実装する。
3. **LLM を用いた動的ルーティングとキーワード終端（Termination）の実装**:
   - 誰が次に発言すべきかを決定する selection 処理に `KernelFunctionSelectionStrategy` を採用し、大モデル自身に文脈から最適なエージェント名を選択させる（LLM-Based Selection）。
   - 特定の終了キーワード（例: `[DONE]`、`[APPROVED]`）を検知して安全に対話を終了する `KeywordTerminationStrategy` を独自に実装する。

### 核心的なエージェントグループ実行設計

```csharp
namespace VK.Blocks.AI.SemanticKernel.Agents.Internal;

internal sealed class AISKAgentGroupRunner : IVKAgentGroup
{
    private readonly Microsoft.SemanticKernel.Kernel _kernel;

    public async Task<VKResult<VKAgentGroupResult>> ExecuteAsync(
        string input,
        IReadOnlyList<IVKAgent> agents,
        VKAgentGroupOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var opt = options ?? new VKAgentGroupOptions();

        // 1. 各 IVKAgent を AISKAgent にキャストし、内部の ChatCompletionAgent を取得
        var skAgents = agents.Cast<AISKAgent>().Select(a => a.InnerAgent).ToList();

        // 2. LLMベースの動的ルーティング関数の生成
        var selectionStrategy = opt.SelectionMode switch
        {
            VKAgentSelectionMode.LLMBased => new KernelFunctionSelectionStrategy(CreateSelectionFunction(skAgents), _kernel)
            {
                ResultParser = (result) => result.GetValue<string>() ?? skAgents[0].Name!
            },
            _ => new SequentialSelectionStrategy()
        };

        // 3. 終了キーワード戦略の構成
        var terminationStrategy = new KeywordTerminationStrategy(opt.TerminationKeywords)
        {
            MaximumIterations = opt.MaxRounds
        };

        // 4. SK AgentGroupChat の起動と実行
        var chat = new AgentGroupChat([.. skAgents])
        {
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = selectionStrategy,
                TerminationStrategy = terminationStrategy
            }
        };

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

        // 5. ストリーム実行と結果の集計
        await foreach (var message in chat.InvokeAsync(cancellationToken).ConfigureAwait(false))
        {
            // 会話履歴の組み立てと結果の格納
        }
        
        return VKResult.Success(new VKAgentGroupResult { ... });
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Custom Message Loop Implementation
- **Approach**: 自社で `while` ループを回し、LLM に「次は誰が発言すべきか」を毎回問い合わせながら `IVKChatEngine` を手動で交互に呼び出す。
- **Rejected Reason**: 会話履歴のメモリ管理、マルチタスク並行処理、および SK 特有のメタデータ引き継ぎなどを自前で再実装する必要があり、実装コストとバグ発生リスクが非常に高いため。

### Option 2: AutoGen Integration
- **Approach**: マイクロソフトの AutoGen SDK を別途 BuildingBlock として採用し、統合する。
- **Rejected Reason**: AutoGen は Python 主体で進化しており、C# 用の SDK はプレビュー段階で破壊的変更が多いうえ、Semantic Kernel のプラグインエコシステムと DI 統合が難しいため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **業界標準への準拠**: 下游アプリケーションは SK のクラスを直接見ることなく、`IVKAgentGroup` に対して指示を送るだけでエージェント協調を簡単に実行できる。
- **インテリジェント・ルーティング**: 単なる順序実行ではなく、タスクの進捗状況に応じて LLM が賢く次の担当エージェントを選択するため、複雑な差し戻し処理が自然に発生する。

### Negative
- **プロバイダーの一貫性制約**: 全てのエージェントが同一の `AI.SemanticKernel` 由来（`AISKAgent`）である必要があり、別モジュールのエージェント（例: 完全に独立した別エンジンのエージェント）を単一グループ内に混ぜて実行できない。

### Mitigation
- 異なる種類の Agent をグループに混在させようとした場合は、DI 登録時または実行時に `UnsupportedAgentType` エラーを即時返却し、未定義の動作を完全に遮断する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Loop Iteration Guards**: 大モデル間の意図しない無限ループによるリソース消費を物理的に防ぐため、`VKAgentGroupOptions` の `MaxRounds` には省略不可のハード上限（デフォルト 10 往復）を設定する。
- **Trace Transparency**: グループ内のどのエージェントが発言したかを明確にするため、出力される `VKAgentGroupResult.Messages` には各発言の `AuthorName` を確実にマッピングし、監査証跡としてのトレーサビリティを保証する。

## 7. Status
✅ Accepted
