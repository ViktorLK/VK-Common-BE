# ADR 001: Structured Tapestry and Layout Indexing

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psycheにおけるプロンプト構築（Prompt Weaving）は、システム指示、ペルソナ情報、会話履歴、知识ベースといった多様なフラグメント（Fragment）を動的に統合するプロセスである。これらを最終的にLLM APIへ送信する際、単純な文字列結合ではチャット形式（User/System/AssistantのMessageオブジェクト配列）の構造化データへの適合が難しく、また組み立てられたフラグメントの物理的な順序関係が各モジュールのロジック内に直接ハードコードされてしまうと、プロンプトのレイアウトを変更する際の保守性が極めて低くなる。

## 2. Problem Statement (問題定义)

単純な文字列結合やレイアウト順序のハードコードには、以下の課題が存在する：

1. **セマンティクス情報の喪失**: LLM APIが要求するチャットロール（System/User/Assistant）ごとのメッセージ構造を保持できなくなる。
2. **物理レイアウトの結合度**: 新たなプロンプトコンポーネントを追加する際、どの位置（例：システムの直後、履歴の直前など）に挿入すべきかを記述するロジックが各コードベースに分散し、レイアウト調整のたびに広範囲なコード修正が発生する。
3. **テスト・デバッグの困難さ**: 最終的なプロンプトのレイアウト構造をプログラムから予測・アサートすることが難しく、意図しないメッセージ順序による大モデルの出力品質低下を招く。

```csharp
// 悪い例：順序がロジックに密結合している
var prompt = systemDirective + "\n" + personaInstructions + "\n" + chatHistory;
```

## 3. Decision (決定事項)

プロンプトのセマンティクス表現と物理的な配置順序（レイアウト）を完全に分離するため、**「Structured Tapestry (構造化織物)」パターン**および**「Absolute Layout Indexing (絶対レイアウトインデックス)」**を採用する。

1. **`VKPromptTapestry` の導入**:
   - 生成された全ての提示文や会話履歴を `VKPromptFragment`（メッセージのロール、コンテンツ、および階層タイプを示す `VKPromptTierType` を内包）として管理し、その集合である `VKPromptTapestry` を最終生成物とする。
2. **`PromptLayout` による物理配置順序の静的定義**:
   - `VKPromptTierType` ごとに絶対的な描画順序インデックス（物理权重）を定義する。
   - 初期定義では以下のように決定し、各Weaving段階は自らの出力にTierTypeをタグ付けするだけで、最終的な物理並び替えはエンジンが一元管理する。

| VKPromptTierType | Render Order (物理インデックス) |
| :--- | :--- |
| **Directive** | 0 |
| **Persona** | 1000 |
| **Knowledge** | 2000 |
| **Echo** | 3000 |

### 核心的なデータ構造設計

```csharp
namespace VK.Blocks.AI.Psyche;

public enum VKPromptTierType
{
    Directive = 0,
    Persona = 1000,
    Knowledge = 2000,
    Echo = 3000
}

public sealed record VKPromptFragment
{
    public required VKPromptTierType Tier { get; init; }
    public required string Role { get; init; } // e.g. "system", "user"
    public required string Content { get; init; }
}

public sealed record VKPromptTapestry
{
    public required IReadOnlyList<VKPromptFragment> Fragments { get; init; }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Fluent String Template
- **Approach**: アプリケーション側でプロンプトのテンプレート文字列を用意し、トークン置換（例：`{{history}}`）を行う。
- **Rejected Reason**: チャット構造（Message配列）への自動変換が難しく、置換漏れやプレースホルダー記述のミスのチェックが実行時まで困難であるため。

### Option 2: Relative Ordering (LinkedList Style)
- **Approach**: 各フラグメントに「〇〇の後に挿入」という相対的な関係を持たせる。
- **Rejected Reason**: フラグメントの数が増えた場合、巡回参照や順序解決の計算ロジックが極めて複雑になり、デバッグが困難になるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **レイアウトの宣言的制御**: レイアウト順序が `PromptLayout` で一元管理されるため、プロンプトの順序変更（例：ペルソナ情報を知識より後ろに持っていく等）がコードのロジック修正なしで定数の変更のみで完結する。
- **構造化APIへの完全適合**: 生成物がロール情報を持つため、OpenAI等のChat Completion APIへマッピングする処理が安全かつ容易になる。

### Negative
- **抽象化レイヤーの増加**: 単なる文字列結合と比べてデータモデルのインスタンス生成およびソート処理のオーバーヘッドが発生する。

### Mitigation
- 物理ソート処理は極めて軽量な `IReadOnlyList` の配列インデックスソートとして実装し、不要なオブジェクトアロケーションを抑える。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Layout Stability**: レンダリング順序のソートには、元の入力順序を維持する安定的ソート（Stable Sort）アルゴリズムが採用されるように配慮する。
- **Injection Protection**: 各フラグメントはロールとコンテンツが明確に分離されているため、ユーザーの入力（`Echo`）がシステムプロンプト（`Directive`）のロールとして誤判定されて実行されるリスクを最小限に防ぐ（System Prompt Injectionの防止）。

## 7. Status
✅ Accepted
