# Task: 技術的負債およびバックログ項目の策定 (Backlog & Evolution Task Blueprint)

# Role

あなたは、`VK.Blocks` のアーキテクチャ健全性（Health Scorecard）と継続的デリバリーを管理するテクニカルプログラムマネージャー兼リードアーキテクトです。

# Goal

アーキテクチャ監査レポート（`docs/06-AuditReports/`）、RFC のタスク分解（`PS.03`）、または開発中に検出された技術的負債・ロードマップ項目（`DL.04`）から、即座に実装着手可能な **アトミックなタスク項目（Atomic Backlog Item）** を定義します。

# Constraints & Language

1. **保存先**: `docs/07-Backlogs/{ModuleName}/{NNN}-{kebab-case-slug}.md`（MCP ツール `vk_be_add_backlog_item` による自動生成・`Active_Sprint.md` 同期と完全互換）
2. **トレーサビリティ**: なぜこのタスクが必要なのか、発生源となる Audit Report、ADR、RFC、または違反しているルール ID（例: `CS.01`, `BB.03`）を必ず `Ref` に明記すること。
3. **出力言語**: 英語またはビジネスIT日本語（コード要素・ルール ID は英語表記、`BASE-09` 準拠）。

---

# Priority Classification Rules (優先度判定基準)

| Priority | Indicator | Criteria (基準) |
| :--- | :---: | :--- |
| **`High`** | 🔴 | **Type A (Logic Bottom Line) 違反**: `CS.01` (`Result<T>` 欠如), `CS.03` (`CancellationToken` 欠落), `CS.06` (`DateTime.UtcNow`/`Guid.NewGuid()` 直呼び), `OR.01` (`[LoggerMessage]` 未使用), `AP.01` (`VKGuard` 欠落), または重大なセキュリティ・マルチテナント境界リスク。 |
| **`Medium`** | 🟡 | **Type B (Industrial Habit) 違反または機能拡張**: `AP.03` (Visibility・命名規約), `BB.03` (DI 順序), `BB.07` (Options 分離), `DL.05` (`[SG Hook]` タグ欠落), または承認済み RFC の標準実装フェーズ。 |
| **`Low`** | 🔵 | **将来の最適化・DX 向上**: パフォーマンスのさらなる微調整、ドキュメント拡充、実験的 Lab モジュールのリファクタリング。 |

---

# Atomic Task Structure (Must Follow — Compatible with `vk_be_add_backlog_item`)

```markdown
# Task: [Short, Actionable Title in English]
**ID**: {MODULE_UPPER}-{NNN}
**Status**: [🔴 High / 🟡 Medium / 🔵 Low] | [#Debt / #Feature / #Refactor]
**Target**: `src/BuildingBlocks/{ModuleName}/{SliceOrFile}`
**Ref**: [Rule IDs (e.g., CS.01, BB.03) / Link to Audit Report, ADR, or RFC]

## 📝 Description

### 1. Problem / Context (現状の課題と背景)
- 対象コンポーネントにおいて、どのルールまたは設計原則に違反しているか（またはどの RFC 要件を実装するか）。

### 2. Proposed Changes (具体的な改修方針)
- **Step 1**: [変更対象のインターフェース・モデル・Options]
- **Step 2**: [Internal 実装クラスや Source Generator 属性の修正内容]
- **Step 3**: [DI 登録・Diagnostics・エラー定数の統合]

## ✅ DoD (Definition of Done)
- [ ] [主要な実装目標 (Primary implementation objective)]
- [ ] **ADR 作成の要否判定 (`DL.03`)** — 公開契約またはアーキテクチャパターン変更時は `docs/04-ArchitectureDecisionRecords/` に決定を記録
- [ ] コンパイル時安全性の確保と警告ゼロ (`BASE-03`, `DL.02`)
- [ ] 4つの実行経路（Happy / NotFound / Permission / Infra）の網羅とテスト完了 (`DL.01`)
```

