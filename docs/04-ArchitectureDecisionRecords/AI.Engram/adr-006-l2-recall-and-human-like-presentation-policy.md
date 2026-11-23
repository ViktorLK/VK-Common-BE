# ADR 006: L2 Recall Engine, Side-Effect-Free Decay, and Human-Like Presentation Policy

- **Date**: 2026-11-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Engram/Recall

## 1. Context (背景)

従来はバックグラウンドの忘却サイクル（`DefaultMemoryReclamationService`）がデータベース内の記憶重要度（`Importance`）を直接減衰・上書き更新し、低スコアの記憶を物理削除していた。
これにより以下の問題が発生していた：
1. 一度減衰した重要度が恒久的に破壊され、ロールバックや別コンテキストでの再評価が不可能になる。
2. ロボットのように過去の些細な日時を精密暗唱してしまう（不気味の谷現象）。
3. 忘却したのか、確信度が低いのかを自然言語で人間らしく表現する術がなかった。

## 2. Decision (決定事項)

記憶の減衰と想起提示を **「L2 Recall スライスへの移設、副作用のない減衰評価、および人間らしい提示ポリシー（HumanLikePresentationPolicy）」**へと再設計する。

1. **L2 想起時計算への減衰移設**:
   - `IVKRecallDecayStrategy` は想起実行時にインメモリで保持率（`RetentionFactor`）を計算し、L1 データベースの値を一切変更しない。
   - 想起された記憶は間隔反復（Spaced Repetition）アクセス強化を受ける。
2. **`VKCharacterMemoryProfile` によるペルソナ別パラメータ統一**:
   - 忘却半減期、感情感度、詳細保持率、ヘッジ閾値、想起失敗閾値をペルソナごとに設定可能にする。
3. **`HumanLikePresentationPolicy` の決定論的実行**:
   - 追加の LLM 呼び出しを一切行わずに、正規表現とスコア閾値によって「確信度の低い記憶に対するヘッジ属性（たしか〜だったかも）」や「過去日時の曖昧化」をプロンプトセグメントへ付与する。
   - 想起失敗閾値を下回った記憶は、自然な「思い出せない」表現とするかコンテキストから除外する。

## 3. Status
✅ Accepted

