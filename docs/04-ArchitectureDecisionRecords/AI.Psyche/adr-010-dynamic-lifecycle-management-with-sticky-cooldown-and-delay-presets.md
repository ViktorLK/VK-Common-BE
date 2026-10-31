# ADR 010: Dynamic Lifecycle Management with Sticky Cooldown and Delay Presets

- **Date**: 2026-06-05
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psyche では、ユーザー入力のキーワードをフックして知識ベース（Knowledge）を Prompt に注入する。しかし、フックされた知識が次のターンで直ちに消滅したり、あるいは逆に永久に残り続けたりすると、プロンプトのコンテキストウィンドウが不安定になり、LLM の回答の文脈維持性能が低下する。そのため、知識が一定ターン残留する「StickyTurns」、再発火を防ぐ「CooldownTurns」、発火を遅らせる「DelayTurns」といった高度なライフサイクル制御が必要となる。しかし、これらを任意の整数で指定させると、プロンプト執筆者が一貫性のない値を指定しやすく、対話のテンポ管理が難しくなる。

## 2. Problem Statement (問題定義)

数値の直接指定によるライフサイクル制御には、以下の問題がある：
1. **設定値のばらつきと混乱**: 開発者やプロンプトデザイナーが「短期的な記憶」「標準的な話題」などを表現する際、ある箇所では `3` ターン、別の箇所では `5` ターンと指定してしまい、システム全体での会話テンポや記憶の持続時間に一貫性がなくなる。
2. **マジックナンバーの蔓延**: 設定ファイル（YAML/JSON）やデータストア内に `StickyTurns = 15` などの意図が不明な数値が散乱し、後からのチューニングや保守が困難になる。

```csharp
// 悪い例: マジックナンバーを直接初期値にし、意図が分かりづらい
public int StickyTurns { get; init; } = 0;
public int CooldownTurns { get; init; } = 0;
```

## 3. Decision (決定事項)

プロンプト構築のテンポと状態維持ライフサイクルを一貫して管理するため、**「Lifecycle Constants with T-Shirt Sizing Presets (Tシャツサイズ型の定数プリセット)」**を採用する。

1. **`VKKnowledgeLifecycles` 静的クラスの導入**:
   - `Sticky`（残留期間）、`Cooldown`（冷却期間）、`Delay`（遅延期間）のそれぞれに対し、人間の認知モデルや対話リズムに適した Tシャツサイズ型の定数を宣言する。
   - `Sticky`: `Flash` (0: 単発), `Short` (2: 短期記憶), `Topic` (5: 話題維持), `Scene` (15: シーン持続), `Anchor` (-1: 永続固着)
   - `Cooldown`: `None` (0), `Short` (3), `Rhythm` (5), `Long` (10), `Once` (-1: 1回限り)
   - `Delay`: `Immediate` (0), `NextTurn` (1), `Slow` (3)
2. **`VKKnowledgeEntry` でのプリセット初期化**:
   - `VKKnowledgeEntry` 内の各ライフサイクルプロパティのデフォルト値を、これらの定数プリセットを用いて宣言し、マジックナンバーをコードベースから完全に追放する。

### 核心的なライフサイクル定数の定義

```csharp
namespace VK.Blocks.AI.Psyche;

public static class VKKnowledgeLifecycles
{
    public static class Sticky
    {
        public const int Flash = 0;
        public const int Short = 2;
        public const int Topic = 5;
        public const int Scene = 15;
        public const int Anchor = -1; // 永久固着
    }

    public static class Cooldown
    {
        public const int None = 0;
        public const int Short = 3;
        public const int Rhythm = 5;
        public const int Long = 10;
        public const int Once = -1; // セッション中1回のみ
    }

    public static class Delay
    {
        public const int Immediate = 0;
        public const int NextTurn = 1;
        public const int Slow = 3;
    }
}

public sealed record VKKnowledgeEntry : IVKFragmentMetadata
{
    public required VKKnowledgeId Id { get; init; }
    
    // プリセットによる規定値初期化
    public int StickyTurns { get; init; } = VKKnowledgeLifecycles.Sticky.Flash;
    public int CooldownTurns { get; init; } = VKKnowledgeLifecycles.Cooldown.None;
    public int DelayTurns { get; init; } = VKKnowledgeLifecycles.Delay.Immediate;
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Strong-typed Enum for Lifecycles
- **Approach**: `StickyTurns` などの型を `int` ではなく `enum VKStickyType { Flash, Short }` とする。
- **Rejected Reason**: 特殊なケース（例: どうしても `7` ターン残留させたい特殊ルール）において数値の直接指定ができなくなり、応用が利かなくなるため。定数プリセット（`const int`）であれば、デフォルトを推奨値で縛りつつ任意の数値指定も許容できる。

### Option 2: Database-only Presets Table
- **Approach**: プリセット定義をデータベーステーブルに保存し、実行時に ID で解決する。
- **Rejected Reason**: ストア呼び出しの I/O オーバーヘッドが発生し、高頻度で実行されるプロンプト構築のホットパスを阻害するため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **一貫性のある対話デザイン**: 「Topic (話題)」や「Scene (戦闘や会話シーン)」といった抽象的な時間概念に基づいてプロンプトの残留時間を統一できるため、キャラクターやアシスタントの文脈維持にブレがなくなる。
- **ドキュメンテーションの自己完結**: コードや設定ファイルを一目見るだけで、その知識がどの程度の持続時間（短期か、一発限りか）を意図しているかが直感的に理解できる。

### Negative
- **静的な定義の限界**: ターン経過の評価ロジックがエンジン側に必要であり、単に値を用意するだけでは機能しない。

### Mitigation
- `DefaultKnowledgeStage` の実行エンジンにおいて、トリガーされた会話履歴のインデックスとの差分から経過ターン数（Elapsed Turns）を算出する評価ロジックを堅牢に実装し、この定数定義と完全に連動させる。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Negative Value Validation**: 永続固着を意味する `-1` (Anchor/Once) 以外の負の整数は、意図しない無限ループやバグを引き起こすため、プロパティ代入時またはバリデーターにおいて禁止（`-1` 以外の負数は例外スローまたはバリデーションエラーとする）する。

## 7. Status
✅ Accepted
