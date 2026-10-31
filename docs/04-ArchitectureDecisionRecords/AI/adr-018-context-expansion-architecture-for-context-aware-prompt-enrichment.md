# ADR 018: Context Expansion Architecture for Context-Aware Prompt Enrichment

- **Date**: 2026-06-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI

## 1. Context (背景)

LLM（大規模言語モデル）を用いた高度な自律型エージェントやオーケストレーションシステムにおいて、システムプロンプトやユーザー指示（Raw Prompt）をそのまま送信するだけでは、コンテキスト（背景知識、参照ドキュメント、ユーザーの現在状態など）が不足し、不正確な生成（ハルシネーション）を招きやすい。
これまでの実装では、プロンプト構築前に参照ナレッジなどを注入する処理が各機能モジュール（`AI.Psyche` やアプリケーションサービス）ごとに独自にスクリプト化、またはハードコードされており、以下の問題が生じていた：
1. **拡張戦略の柔軟性欠如**: プロンプトにどのような文脈情報を追加するか（静的データのロード、LLM による動的要約、No-Op パススルーなど）を、実行時に切り替える統一的な仕組みがなかった。
2. **トークンバジェット管理の難しさ**: 文脈を拡張した結果、LLM の最大トークン数を超過するのを防ぐバジェットチェックが統合されておらず、パイプラインの各所で個別に例外スローを考慮する必要があった。

## 2. Problem Statement (問題定義)

拡張対象となるプロンプト定義に対して、多様な拡張戦略（Strategy）を統一的に適用し、トークン上限の制約を守りつつ動的に文脈情報（References）を安全にエンリッチ可能な、共通のコンテキスト拡張（Context Expansion）基盤が必要であった。

## 3. Decision (決定事項)

VK.Blocks.AI の下部に、**「戦略パターンに基づく汎用コンテキスト拡張（Context Expansion）サブシステム」**を導入する。

### 1. 抽象ストラテジの定義 (`IVKContextExpansionStrategy`)
- 拡張アルゴリズムを抽象化し、実行時に切り替え可能とする。
  - `DefaultContextExpansionStrategy`: 標準の LLM を用いたコンテキストフィルタ・要約による拡張。
  - `NoOpContextExpansionStrategy`: 拡張を行わずそのままパススルーする。

### 2. 統一ステージ (`DefaultContextExpansionStage`) の導入
- プロンプト生成ライフサイクル（Pipeline）にバインド可能な標準ステージとして `DefaultContextExpansionStage` を定義し、パイプライン実行時に選択されたストラテジを呼び出す。

### 3. オプションの分離 (`VKContextExpansionOptions`)
- 拡張をトグルする `Enabled` フラグ、最大拡張トークン閾値、および使用するストラテジタイプ等を設定可能にする。

```
[Prompt Execution Pipeline]
       |
       +--> Enter: DefaultContextExpansionStage
                   |
                   +--> Inspect strategy: IVKContextExpansionStrategy
                   +--> Expand reference texts -> Inject into Context
                   +--> Check Token limits
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: AI.Psyche の Weaving ステージ（Persona / Echo）のいずれかに内包する
- **Approach**: 独立したステージとせず、`DefaultPersonaStage` 等の内部ロジックの一部として拡張を行う。
- **Rejected Reason**: コンテキスト拡張は Persona（システム定義）や Echo（会話履歴）とは根本的に異なる「外部参照知識の動的合成」という独立した責務であり、同じステージに埋め込むとコードが肥大化し単一責任の原則（SRP）を損なうため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **高い独立性**: プロンプトの組み立てロジック（Psyche）やデータベース検索（Recall/VectorStore）から独立して、「取得したテキストをプロンプトに合わせてどう要約・整形するか」というルールのみに集中できる。
- **プラグイン構造**: 新しい拡張手法（他社 API 連携、チャンク切り出しアルゴリズム等）を `IVKContextExpansionStrategy` を介して容易に追加可能。

### Negative
- **追加の LLM コスト**: 拡張戦略として `DefaultContextExpansionStrategy`（LLM 要約等）を選択した場合、メインの推論呼び出しの前に前処理としての LLM コストとレイテンシが発生する。

### Mitigation
- 軽量で高速なローカルモデルを拡張戦略専用に割り当てられるようにオプション構成を分離し、また低遅延が要求されるシナリオでは `NoOpContextExpansionStrategy` へ動的にフォールバック可能にする。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 拡張されたテキストがプロンプトインジェクションの脆弱性を突くペイロードを含まないよう、インジェクション対策（エスケープ、XML等でのカプセル化）を前処理として施す。

## 7. Status
✅ Accepted
