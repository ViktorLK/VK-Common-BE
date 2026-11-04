# ADR 004: Backlog Front-Matter v2 and Fibonacci Story Points Estimation Model

- **Date**: 2026-11-01
- **Status**: ✅ Accepted
- **Deciders**: Engineering Team
- **Technical Story**: src/Tools/McpServer, docs/07-Backlogs, .agents/skills

## 1. Context (背景)

ADR-003 により YAML front-matter を SSoT とする決定論的なバックログおよび Sprint ライフサイクルが導入された。
しかし、実際の運用（Sprint 1 回目）を経て以下の課題が浮き彫りとなった：
1. **見積もりの欠如**: バックログ 122 件中、工数・規模の入力が 0 件であったため、Sprint 計画（Planning）が勘に頼っており、ベロシティや容量（Capacity）の算出が不可能であった。
2. **DoR (Definition of Ready) の未整備**: 着手準備が不十分な課題がそのまま Sprint にコミットされ、期間中の手戻りや停滞の原因となっていた。
3. **スコープ変更の非追跡性**: Sprint 進行中の課題追加・削除の履歴が残らず、スコープクリープが不可視であった。
4. **フィードバック閉ループの不足**: 持ち越し（carry-over）回数の累積記録、Sprint 目標（Goal）の達成判定、および Retro（振り返り）の Try からのタスク起票が機械的に連携していなかった。

## 2. Problem Statement (問題定義)

- 工数（時間）見積もりは開発者の精神的負荷が高く、非同期・個人開発では形骸化しやすい。
- Front-matter のスキーマが静的かつ最小限であり、アジャイルの核となる「順位（rank）」「着手日（started）」「Ready 判定」「持ち越し回数」を表現できない。
- スプリント計画時に過去の実績ベロシティと照合した容量超過防止（Capacity Guard）が存在しない。

## 3. Decision (決定事項)

1. **フィボナッチ数列による Story Points 見積もりモデルの採用**:
   - 規模見積もり単位として **Story Points (1, 2, 3, 5, 8, 13)** を標準採用する。13 を超える規模は Sprint 計画前に必ず分割を要求する。
   - 既存の `estimate`（時間）フィールドは互換性のために残置しつつ、`points` を一次情報として扱う（FS-09 加算的拡張）。

2. **Front-Matter v2 スキーマの策定**:
   - **BacklogFrontMatter**:
     - `points`: `int?` (Story Points)
     - `rank`: `int?` (優先度内順位)
     - `started`: `string?` (初回着手日 yyyy-MM-dd)
     - `carry_over_count`: `int` (持ち越し回数、初期値 0)
     - `ready`: `bool` (DoR 適合フラグ)
   - **SprintFrontMatter**:
     - `committed_points`: `int?` (開始時確約ポイント)
     - `capacity_points`: `int?` (提示容量・過去平均速度)
     - `completed_points`: `int?` (完了ポイント)
     - `goal_met`: `bool?` (目標達成成否)
     - `scope_changes`: `List<ScopeChange>` (スコープ変更台帳: 日付、ID、アクション、理由)
     - `carried_over`: `List<string>` (持ち越しタスク ID 一覧)
     - `try_items`: `List<string>` (Retro の Try から起票された PBI ID)

3. **決定論的 DoR (Definition of Ready) ゲート**:
   - `ready: true` の合格基準：
     1. `points` 設定済み、かつ 13 以下。
     2. Description に `### 1. Problem / Context` と `### 2. Proposed Changes` が存在。
     3. DoD にタイトル行以外のチェックボックスが 1 つ以上存在。
     4. `category` が `feature` ではない（feature は story/task に分解必須）。
   - `sprint_start` は `ready: true` ではないタスクをコミットすることを原則拒否する。

4. **容量制限（Capacity Guard）と明示的オーバーライド (AP.06)**:
   - 直近 3 Sprint の平均完了ポイントを基準とし、計画ポイントが超過した場合は原則エラー（`Backlog.Sprint.CapacityExceeded`）とする。
   - 意図的な超過を行う場合は `allowOverCapacity: true` 引数の明示を要求する（暗黙の過負荷を禁止）。

5. **カテゴリ別 DoD テンプレートの導入**:
   - `task` / `debt` / `story`: 規約検証・0 warning・4 パス検証。
   - `spike`: 調査結論の記録、後続 PBI 起票、ADR/RFC 要否判定。
   - `feature`: 子 PBI 分解完了、RFC リンク。

## 4. Alternatives Considered (代替案の検討)

### Option 1: 実時間（Hours）による工数見積もりの強制
- **Pros**: 予実管理の直感性が高い。
- **Cons**: 見積もりバイアスが大きく、個人開発では見積もり作成自体がボトルネック化して形骸化する（実測で 122 件中 0 件の入力にとどまっていた最大の要因）。

### Option 2: 見積もりなし（NoEstimates / カンバン方式のみ）
- **Pros**: 入力負荷ゼロ。
- **Cons**: タイムボックス（1週間）あたりの容量管理ができず、バーンダウンやベロシティの算出が不可能となり、Sprint の意義が失われる。

## 5. Consequences (影響とトレードオフ)

- **Positive**:
  - 相対見積もり（Points）により迅速な見積もりが可能となり、容量計画が客観的データに基づいて行える。
  - スコープ変更台帳と DoR により、Sprint 中の割り込みや準備不足タスクの混入が可視化・抑止される。
  - 持ち越し回数が可視化され、2〜3 回以上停滞しているタスクの分割・破棄判断が容易になる。
- **Trade-off**:
  - 既存の PBI に対し、上位優先（P0/P1）から順次 `vk-backlog-refine` で points を付与していく運用ステップが必要となる。

