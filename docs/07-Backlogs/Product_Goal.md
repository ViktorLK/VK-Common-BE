# 🎯 Product Goal

> 手動管理ドキュメント（GENERATED ではない）。四半期ごと、または目標達成時に見直す。

## Goal (2026-Q4 ~ 2027-Q1)

**VK.Blocks の主要 BuildingBlock を "Industrial DNA" 準拠に引き上げ、Scorecard 総合スコア 60 / 100 以上で安定リリース可能な状態にする。**

## Key Metrics

計測元: [Health_Scorecard.md](/docs/07-Backlogs/Health_Scorecard.md)（`vk_be_generate_dashboard` で更新）

| # | Metric | Baseline (2026-11-01) | Target | Rule |
| :-- | :--- | :---: | :---: | :---: |
| M1 | Overall Score | 34.2 | ≥ 60 | - |
| M2 | `[VKBlockMarker]` 導入率 | 23% (21/90) | ≥ 60% | `BB.02` |
| M3 | `Diagnostics/` 導入率 | 20% (18/90) | ≥ 50% | `OR.01, BB.04` |
| M4 | Ambient Clock / Guid 違反数 | 34 | 0 | `CS.06` |
| M5 | Unit Test プロジェクト導入率 | 29% (26/90) | ≥ 50% | `DL.01` |

## Sprint Goal との接続ルール

- 各 Sprint Goal は上記 Metric のうち **最低 1 つ (M1〜M5)** を前進させることを明記する（例: `[M2,M4] Authorization の DI を Marker 方式に統一`）。
- 1 Sprint = 1 テーマ（1 モジュールまたは 1 Metric）を原則とする。コンテキストスイッチを避けるため。
- Metric に寄与しないタスクは、P0 またはバグ修正の場合のみ Sprint に入れる。

## Out of Scope (今期)

- 新規 BuildingBlock の追加（RFC 段階まで）
- AI 系 icebox アイテムの実装着手

