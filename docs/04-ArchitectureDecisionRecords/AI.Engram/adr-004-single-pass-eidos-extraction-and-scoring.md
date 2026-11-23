# ADR 004: Single-Pass LLM Extraction, Scoring, and Contradiction Arbitration with Eidos

- **Date**: 2026-11-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Engram/Consolidation

## 1. Context (背景)

従来の記憶統合（Consolidation）では、1 ターンの記憶処理に対して複数回の LLM 呼び出しが行われていた：
1. `FactExtractionConsolidationStrategy` で生の箇条書きテキストを抽出
2. `LlmHeuristicScoringTask` で各記憶の重要度スコア（0.0〜1.0）を別個に推論
3. 矛盾検出・調停時に再度 LLM を呼び出して過去記憶との整合性を判定

これにより、ターンごとのレイテンシ増大、API 料金の浪費、および中間文字列のパース失敗リスクが増大していた。

## 2. Decision (決定事項)

`VK.Blocks.AI.Eidos` による構造化スキーマバインディングを導入し、**「単一パスでの事実抽出・分類・採点・矛盾調停」**に統合する。

1. **`VKMemoryExtractionEnvelope` 契約**:
   - 1 回の LLM 呼び出しで、候補事実（`FactContent` / `SummaryAbstract`）、カテゴリ、重要度スコア、感情シグナル、および既存記憶に対する操作指示（`ADD` / `UPDATE` / `INVALIDATE` / `NOOP`）を一括出力する。
2. **単独 LLM 採点タスクの廃止**:
   - `LlmHeuristicScoringTask` を撤廃し、高速な決定論的ルール処理（`RuleBasedScoringTask`）と感情重み付け（`EmotionalImpactScoringTask`）のみを前処理・後処理パイプラインに残す。
3. **L0 エピソード再構成の支援**:
   - L0 `VKEpisode` のシーケンスを渡すことで、モデル更新時に L1 派生記憶群を決定論的に再抽出可能にする。

## 3. Status
✅ Accepted

