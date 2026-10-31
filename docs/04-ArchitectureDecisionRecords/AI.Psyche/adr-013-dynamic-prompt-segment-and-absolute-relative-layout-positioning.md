# ADR 013: Dynamic Prompt Segment and Absolute Relative Layout Positioning

- **Date**: 2026-06-13
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psyche において、当初プロンプトの構成順序は固定された物理权重（定数定義された `PromptLayout`）を用いて一元ソートする設計（ADR-001）を採用していた。しかし、パイプラインの進化や `AI.Corpus` などの外部モジュールの介入に伴い、「システムプロンプトの直後に動的に差し込む」「特定の履歴メッセージの直前に注入する」といった相対的な挿入（Relative Positioning）や、「指定の絶対インデックス位置に差し込む」といった絶対座標配置（Absolute Positioning）の要求が生まれ、定数ベースの単純な一次元ソートでは表現しきれなくなった。

## 2. Problem Statement (問題定義)

静的な物理ウェイトソート（ADR-001）のみの設計には、以下の課題が存在する：
1. **動的レイアウト合成の制限**: ステージ間で順序の依存関係がある場合（例: `Knowledge` ステージが `Persona` 記述の内部の特定の段落の後に補足を差し込みたいなど）、相対的な挿入位置を解決する記述方法が存在せず、レイアウトが崩れてしまう。
2. **チャットMessage構造への強制フィッティング**: Message 配列の特定インデックス位置（例: システムプロンプトの直後、またはユーザーの発言の直前など）にピンポイントでフラグメントを差し込む高度な差し込みができず、LLM のシステム指示効果が薄れる。
3. **データモデルの表現不足**: 元の `VKPromptFragment` はコンテンツと階層タイプ（TierType）のみを保持していたため、レイアウトの座標情報や優先順位を表現するセマンティクスが不足していた。

## 3. Decision (決定事項)

多様な独立ステージによる高度なプロンプト合成と座標解決を可能にするため、**「Dynamic Layout Positioning with Absolute/Relative coordinates (絶対・相対座標による動的レイアウトポジショニング)」**を採用する。

1. **`VKPromptSegment` へのデータ構造刷新**:
   - 元の `VKPromptFragment` を `VKPromptSegment` に刷新し、コンテンツに加えて配置情報プロパティを追加する。
   - `AbsoluteDepth`（絶対座標深度: 指定のMessage配列インデックス）
   - `RelativeDepth`（相対座標深度: `VKPromptRelativeDepth` 定義に基づく前後関係）
   - `DepthPriority`（0〜999 の数値: 同一座標内での競合時のソート優先順位）
2. **`DefaultCoordinateResolveTask` の導入**:
   - Weaving タスクチェーン内に、座標解決を担当する `DefaultCoordinateResolveTask` を新設する。
   - すべての assembled `VKPromptSegment` を走査し、相対的なアンカーを絶対座標に変換した上で、`DepthPriority` に基づく安定的ソートを実行して最終的なレンダリング物理順序インデックスを動的算出する。

### 核心的なプロンプトセグメントと座標定義

```csharp
namespace VK.Blocks.AI.Psyche;

public sealed record VKPromptSegment
{
    public bool IsEnabled { get; init; } = true;
    public string Content { get; init; } = string.Empty;
    public string? Name { get; init; }
    public VKChatRole Role { get; init; } = VKChatRole.System;

    // 絶対配置用
    public int? AbsoluteDepth { get; init; }

    // 相対配置用
    public VKPromptRelativeDepth? RelativeDepth { get; init; }

    // 同一深度での競合解決優先順位 (0 ~ 999 範囲制限)
    private readonly int _depthPriority = 0;
    public int DepthPriority
    {
        get => _depthPriority;
        init => _depthPriority = VKGuard.InRange(value, 0, 999, nameof(DepthPriority));
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Template Placeholder Replacement
- **Approach**: プロンプト全体を1つの大きなテキストテンプレートとし、その中にプレースホルダー（例: `{{persona_section}}`）を配置し、各ステージがそのプレースホルダーをテキストで置換する。
- **Rejected Reason**: チャットの Message 構造（Role/Content の配列）を動的に組み立てる際に適さず、文字列の置換漏れや変数の衝突リスクが高いため。

### Option 2: Pre-defined Segment Slots in Context
- **Approach**: Context 内に `SystemPromptSlot`、`PersonaSlot`、`EchoSlot` のような固定スロットを用意して各ステージが登録する。
- **Rejected Reason**: スロットの種類が固定化されてしまい、将来的に全く新しい種類（例: コグニティブな短期記憶スロットなど）を追加する際に Context 自体の定義変更（破壊的変更）が必要になるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **無限のプロンプト合成表現**: 各ステージが互いの実装を知らなくても、`RelativeDepth` を使って「別のセグメントの直後にこれを差し込む」といった宣言的な配置指定が可能になり、安全に協調動作できる。
- **堅牢なソート優先度**: 同一の絶対深度に配置された複数のセグメントも、`DepthPriority` の数値によって確実かつ予測可能に整列させることができる。

### Negative
- **レイアウトの視認性低下**: プロンプトの物理的な出力順序が静的コード上から一目でわかりづらくなり、タスクランナーが計算した結果として決定されるため、レイアウトバグのデバッグ難易度が上がる。

### Mitigation
- Weaving プロセスの診断ログ（`WeavingDiagnostics`）において、座標解決された最終的なセグメントの順序と深度情報、コンテンツのプレビューをトレース出力できるようにし、デバッグの透明性を確保する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Priority Range Protection**: `DepthPriority` には `VKGuard.InRange(value, 0, 999)` によるガード条件を適用し、意図しない範囲外の優先度によってソート処理が崩壊することを防ぐ。
- **Fallback to Stable Order**: 座標と優先度が完全に同一のセグメントが存在する場合、Weaving Context に追加された時系列順（安定ソート）を守るようにフォールバックし、プロンプトの順序の決定論性（Determinism）を確実に担保する。

## 7. Status
✅ Accepted
