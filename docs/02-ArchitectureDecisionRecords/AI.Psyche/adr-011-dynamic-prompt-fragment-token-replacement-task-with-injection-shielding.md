# ADR 011: Dynamic Prompt Fragment Token Replacement Task with Injection Shielding

- **Date**: 2026-06-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psyche では、プロンプトの各フラグメントテンプレート（`VKPromptFragment`）内に定義されたプレースホルダー変数（例: `{{UserName}}`、`{{DateTime}}` など）を実行時に動的に置換する仕組みが必要である。これらはアプリケーション側から引き渡される辞書（`Variables`）に基づいて評価される。しかし、すべてのフラグメントに対して無差別に置換処理を実行すると、ユーザーが入力したテキスト（会話履歴）の中に悪意ある変数タグ（例: `{{ApiKey}}` や `{{SystemPrompt}}` など）が含まれていた場合にそれがシステム側の値で自動置換され、機密情報の漏洩やプロンプト指示の改ざん（Prompt Variable Injection）を許してしまう重大なセキュリティリスクがある。

## 2. Problem Statement (問題定義)

一律の変数置換処理や安全ガードのないテンプレート評価には、以下のリスクが存在する：
1. **変数インジェクション攻撃 (Prompt Variable Injection)**: ユーザーの入力テキスト（`Echo` 履歴に含まれるもの）に `{{SystemSecret}}` などの変数が含まれていると、システムがそれを実際の秘密キーや機密データに自動置換して LLM に送信してしまい、モデルの応答を介して間接的に漏洩する。
2. **パフォーマンスの劣化**: 既に静的に確定しているはずの会話履歴（Echo）や、変更の余地がない固定テキストに対してリクエストごとに毎回正規表現やテンプレートエンジンのレンダリングを回し続けると、不要なメモリ割り当てと遅延が発生する。

```csharp
// 悪い例: ユーザーのチャット履歴である Echo も含めて、すべてのフラグメントを無差別に置換してしまう
foreach (var fragment in context.Fragments)
{
    var rendered = await _templateEngine.RenderAsync(fragment.Content, variables);
    newFragments.Add(fragment with { Content = rendered });
}
```

## 3. Decision (決定事項)

プロンプトテンプレートの動的な表現力とインジェクション耐性の防御を両立させるため、**「Dynamic Prompt Fragment Token Replacement Task with Injection Shielding (インジェクション遮断付きフラグメント変数置換タスク)」**を採用する。

1. **`DefaultFragmentReplacementTask` の導入**:
   - Weaving タスクチェーン内に `DefaultFragmentReplacementTask` を追加し、`IVKPromptTemplateEngine` を用いて各フラグメントのレンダリング（変数置換）を行う。
2. **履歴（`Echo`）ティアの厳格な除外（Injection Shielding）**:
   - 置換処理のループにおいて、対象フラグメントの階層タイプが `VKPromptTierType.Echo`（会話履歴）である場合は、**テンプレートエンジンによる評価を完全にスキップ**し、元のテキストのままパイプラインへ引き渡す。
3. **リクエストレベルとグローバルレベルの変数マージ**:
   - `context.Args?.Variables ?? _options.Variables` を用いて、リクエスト単位の上書き変数とグローバル変数を安全にマージして評価に使用する。

### 核心的な置換タスクと安全ガードの実装

```csharp
namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultFragmentReplacementTask : IVKWeavingTask
{
    private readonly IVKPromptTemplateEngine _templateEngine;
    private readonly VKWeavingOptions _options;

    public async Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var variables = context.Args?.Variables ?? _options.Variables;
        if (variables == null || variables.Count == 0)
        {
            return VKResult.Success();
        }

        var newFragments = new List<VKPromptFragment>(context.Fragments.Count);
        foreach (var fragment in context.Fragments)
        {
            // 安全ガード: 空文字または会話履歴 (Echo) ティアはインジェクション防止のため置換処理をスキップ
            if (string.IsNullOrWhiteSpace(fragment.Content) || fragment.TierType == VKPromptTierType.Echo)
            {
                newFragments.Add(fragment);
                continue;
            }

            // テンプレートエンジンによる安全なレンダリング
            var msgResult = await _templateEngine.RenderAsync(fragment.Content, variables, cancellationToken).ConfigureAwait(false);
            if (msgResult.IsSuccess)
            {
                newFragments.Add(fragment with { Content = msgResult.Value });
            }
            else
            {
                newFragments.Add(fragment); // 失敗時は元のテンプレートをそのまま残す
            }
        }

        context.SetFragments(newFragments);
        return VKResult.Success();
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: HTML/Markdown-style Encoding for Echo
- **Approach**: ユーザー入力内の波括弧 `{` や `}` を HTML エンティティ（例: `&#123;`）やエスケープ文字に一律置換し、その上で全体にテンプレートエンジンを通す。
- **Rejected Reason**: エスケープ処理によって大モデルへのプロンプトセマンティクスが一部狂う場合があり、かつテンプレートエンジン自体のパース挙動に依存するため、エスケープ漏れによる脆弱性を 100% 防御できないため。

### Option 2: Pre-evaluating Inputs at Application Layer
- **Approach**: アプリケーションレイヤーで事前にすべての変数を置換してから Psyche に渡し、Psyche 内部では一切変数置換を行わない。
- **Rejected Reason**: どのフラグメントにどの変数を当てるかのロジックがアプリケーション側に漏れ出し、プロンプトの構成変更のたびに呼び出し元のコード修正が発生するため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **強力なインジェクション耐性**: ユーザーから入力された悪意あるプレースホルダー文字列がシステム側で評価される経路が完全に遮断され、高いセキュリティ堅牢性を実現。
- **パフォーマンスの向上**: 最もデータサイズが大きくなる傾向にある会話履歴（Echo）の文字列置換処理をスキップするため、実行時オーバーヘッドとメモリ割り当てが大幅に削減される。

### Negative
- **会話履歴内での変数評価の制限**: ユーザーのチャットメッセージ内に意図的にシステム変数を埋め込んで対話させる（例: ユーザーが「今のシステム時刻を教えて」とメッセージに `{{DateTime}}` を含めるなどの高度なユースケース）ことはできなくなる。

### Mitigation
- そのような高度な動的解決は、変数置換ではなく、LLM に Tool Call（関数呼び出し）を実行させて解決させるアプローチへ誘導し、セキュアな設計を維持する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Strict Task Order**: 変数置換は他のすべてのフラグメント（Persona, Directive）が集約された後、最終的なソートとレンダリングが行われる直前に実行されなければならないため、`TaskOrder` は `VKWeavingTaskOrder.Replacement` に厳密に固定する。

## 7. Status
✅ Accepted
