# VK.Blocks AI ガバナンス・エコシステム

このディレクトリには、人間と AI エージェントの自律的かつ確定的（Deterministic）な協調開発を実現するための **AI Governance Protocol（工業製品水準の設計思想・Industrial DNA）** が格納されています。

---

## 🌟 新思想：階層型AIガバナンス (Hierarchical AI Governance)

本プロジェクトでは、単一の長大な指示プロンプトによる「ルールの形骸化」や「推論のブレ（Rule Drift）」を根本から解決するため、**厳格な単方向継承（Unidirectional Layering）** と **多層化されたルール体系（L0〜L3）** を採用しています。

```mermaid
graph TD
    L0["<b>L0: VK Base & Mindset (l0-vk-base.md)</b><br/>第一原理思考・普遍的エンジニアリング哲学・契約優先・出力プロトコル"]
    L1["<b>L1: Architecture Principles (l1-vk-arch.md)</b><br/>システム境界・ポート＆アダプター・垂直スライス・内部隔離"]
    L2["<b>L2: Full Stack Baseline (l2-vk-fullstack.md)</b><br/>ワイヤ上の単一情報源（OpenAPI）・同型 Result&lt;T&gt;・RFC 7807・分散トレーシング"]
    L3["<b>L3: Backend Stack & BuildingBlocks (l3-vk-be.md)</b><br/>.NET / C# 12+・Tiered Strategy・Type A/B 禁令・動的ロード"]

    L0 -->|inherits| L1
    L1 -->|inherits| L2
    L2 -->|inherits| L3

    style L0 fill:#1e293b,stroke:#38bdf8,stroke-width:2px,color:#f8fafc
    style L1 fill:#1e293b,stroke:#818cf8,stroke-width:2px,color:#f8fafc
    style L2 fill:#1e293b,stroke:#c084fc,stroke-width:2px,color:#f8fafc
    style L3 fill:#1e293b,stroke:#f43f5e,stroke-width:2px,color:#f8fafc
```

### 1. 厳格な単方向継承モデル (`inherits`)
- **単方向依存**: 依存の向きは常に下位（基底層）に向かいます。上位レイヤーは下位レイヤーの制約を厳格化（Tighten）できますが、**緩和（Relax）することは例外なく禁止** されます。
- **Contract-First (契約優先)**: インターフェース、DTO、エラー定数、不変条件を実装コードに先立って定義します。
- **Zero Implicit Globals**: 静的変更可能状態や暗黙のシングルトンを排除し、時刻（`TimeProvider`）、ID生成（`IVKGuidGenerator`）、シリアライザ等はすべて抽象化を介して注入（DI）します。
- **クロススタック整合（フロントエンド連携）**: L0〜L2 レイヤーはフロントエンド（VK-Common-FE）と完全に対齊・共有されており、ワイヤ上の契約（OpenAPI、`VKResult<T>`、RFC 7807、W3C `traceparent`）を通じて両端が独立して同一の仕様を満たします。

---

## 🏛️ 三層協調アーキテクチャ (Tri-Layered Collaboration)

[ADR-001](/docs/04-ArchitectureDecisionRecords/DeveloperEcosystem/adr-001-establish-tri-layered-ai-agent-collaboration-architecture.md) に基づき、AI エコシステムは責任に応じて 3 つの層に分離されています：

| レイヤー | ディレクトリ | 役割 | 動作特性 |
| :--- | :--- | :--- | :--- |
| **1. Rules (規約・憲法)** | [/.agents/rules/](/.agents/rules/) | タスクの内容に関わらず遵守すべき **絶対的なアーキテクチャ制約** | 静的・不変のルール体系（L0〜L3） |
| **2. Skills & Blueprints (手順・型)** | [/.agents/skills/](/.agents/skills/) & [/docs/00-Blueprints/](/docs/00-Blueprints/) | RFC、Manifest、ADR、契約仕様、監査、バックログ等の **認知推論パイプラインと単一情報源テンプレート** | スラッシュコマンド（例: `/vk-draft-*`, `/vk-audit-*`）および文脈に応じた自動スキル起動 |
| **3. MCP Tools (メカニクス)** | [/src/Tools/McpServer/](/src/Tools/McpServer/) | AST解析、ディレクトリ走査、ルール抽出、連番採番等の **決定的・機械的処理** | 100% 確定的なツール実行（`vk-blocks-be-manager`） |

---

## 🛰️ 階層制ルール適用プロトコル (Tiered Strategy)

AI のコンテキストウィンドウ枯渇と注意力の散漫（Lost-in-the-Middle）を防止するため、**Tiered Strategy** による二段構えのルール適用を行います：

```text
[常時ロード層 (Always-On System Prompt)]
 ├── l0-vk-base.md       (L0: 基底思想)
 ├── l1-vk-arch.md       (L1: アーキテクチャ原則)
 ├── l2-vk-fullstack.md  (L2: フルスタック境界契約)
 └── l3-vk-be.md         (L3: バックエンド技術スタック)
      ├── Tier 1: Rule Index (全ルール 1 行サマリ / 認知維持)
      ├── Tier 2: Core Prohibitions (Type A/B 即時禁令)
      └── Tier 3: Dynamic Loading Protocol (動的ロード手順)

[オンデマンド動的ロード層 (Tier 3: trigger: manual)]
 ├── l3-vk-be-cs-core-standards.md
 ├── l3-vk-be-or-observability-resiliency.md
 ├── l3-vk-be-dl-development-lifecycle.md
 ├── l3-vk-be-ap-architecture-patterns.md
 ├── l3-vk-be-bb-block-blueprint.md
 └── l3-vk-be-ps-planning-standards.md
```

### 1. 常時ロード層 (Always-On)
- **Tier 1 (Rule Index)**: 全ルールの要約を常に AI の視界に置き、ルールの存在忘れをゼロにします。
- **Tier 2 (Core Prohibitions)**: 違反が即座に設計崩壊を招く制約を直接インライン化。
  - **Type A (🔴 Logic Bottom Line - 絶対禁令)**: 安定性と決定論を司る絶対規約（`CS.01`, `CS.03`, `CS.06`, `CS.07`, `CS.08`, `OR.01`, `DL.01`, `AP.01`, `AP.06`, `AP.07`, `PS.04`, `PS.05`）。Labs 環境を含め一切の例外を認めません。
  - **Type B (🟡 Industrial Habits - 工業標準)**: 命名・配置規約（`AP.03`, `BB.03`, `BB.07`, `BB.08`, `DL.02`〜`DL.05`）。原則厳守、`src/Labs` で明示的に許可された場合のみ免除可能です。

### 2. オンデマンド動的ロード層 (Tier 3: Dynamic Loading)
- 詳細仕様（`l3-vk-be-*.md`）は `trigger: manual` としてシステムプロンプト外に配置。
- **Hard-Lock 原則**: AI が記憶や要約から推測でコードを出力することを厳禁とし、シナリオに応じて MCP ツール（`vk_be_get_architectural_rule`）を呼び出し、最新の詳細仕様を同期（Sync）した上でコードを出力します。

---

## 🛡️ 出力プロトコルと監査 (Output Protocol & Traceability)

すべてのコード生成・ADR・マニフェスト出力において、以下の規約遵守が義務付けられています：

### 1. ハンドシェイク (The Handshake)
回答の **先頭1行目** に、アクティブなレイヤー、対象パス、および同期した Rule ID を明記します：
```text
Active: [L0-L3:Auth] | Context: src/BuildingBlocks/Auth | Sync: [CS.01, AP.01, BB.01]
```
> **Hard-Lock**: 該当シナリオに必要な Rule ID が `Sync: [...]` に含まれていない場合、コード生成は禁止されます。

### 2. 監査タグ (Audit Tag)
回答の末尾で、実際に実施した検証根拠を伴う監査ステータスを宣言します（自己チェックのみで完了扱いにしない）：
- 検証済み: `Audit: ✅ (verified: build)` または `Audit: ✅ (verified: build + isolated-review)`
- 未検証（自己確認のみ）: `Audit: ⚠️ (self-check only)`
- 違反・免除時: `Audit: 🚩 [RuleID] {Rationale}`（Type B 免除の理由を明記。即時自己修正を実施）

---

## 🧠 AI コンテキスト・エントロピー制御理論 (Context & Entropy Governance)

長時間のペアプログラミングにおいて、AI が初期規約を忘却したり自己解釈に陥る現象（ルールのドリフト / Rule Drift）を防ぐためのアプローチです：

1. **Lost-in-the-Middle 対策**:
   重要なアーキテクチャ規約は常時ロード層（L0〜L3）に凝縮し、動的ロードされた最新ルールをコンテキストの「最も新しい領域」に再注入します。
2. **Differential Loading (差分ロード)**:
   モジュール作業時はまず `vk_be_get_module_context` でモジュール固有の設計方針（Manifesto / Prompts）を取得し、不足している共通ルールのみを `vk_be_get_architectural_rule` でピンポイントに補充します。
3. **決定論的アライメント**:
   AI に記憶から「思い出す」のではなく「MCP を介して確定的な外部ソースから再取得させる」ことで、ハルシネーションの余地を排除します。

---

## 🛠️ クイックリファレンス

### 開発者（人間）向け
- **規約の確認**: [/.agents/rules/](/.agents/rules/) 配下のドキュメントが、リポジトリ全体の設計・コーディング標準の Single Source of Truth（唯一の基準）です。
- **エンタープライズ開発ライフサイクル標準フロー (`00-Blueprints` 連動)**:
  新機能やモジュールの開発は、単一情報源テンプレート群（`00-Blueprints`）に基づき、以下のパイプラインに沿って確定的な順序で推進します：

```mermaid
flowchart LR
    S1["<b>01. 立項</b><br/><code>/vk-draft-rfc</code><br/>RFC提案策定"] -->
    S2["<b>02. 境界</b><br/><code>/vk-draft-architecture</code><br/>アーキテクチャ・境界設計"] -->
    S3["<b>03. 決定</b><br/><code>/vk-draft-adr</code><br/>設計決定記録 (ADR)"] -->
    S4["<b>04. 契約</b><br/><code>/vk-draft-impl-plan</code><br/>実装仕様・データ辞書"] -->
    S5["<b>05. 監査</b><br/><code>/vk-audit-*</code><br/>アーキテクチャ・DNA監査"] -->
    S6["<b>06. 課題</b><br/><code>/vk-sprint-add-task</code><br/>タスク起票・Sprint連携"]

    style S1 fill:#1e293b,stroke:#38bdf8,stroke-width:1.5px,color:#f8fafc
    style S2 fill:#1e293b,stroke:#818cf8,stroke-width:1.5px,color:#f8fafc
    style S3 fill:#1e293b,stroke:#a855f7,stroke-width:1.5px,color:#f8fafc
    style S4 fill:#1e293b,stroke:#ec4899,stroke-width:1.5px,color:#f8fafc
    style S5 fill:#1e293b,stroke:#f59e0b,stroke-width:1.5px,color:#f8fafc
    style S6 fill:#1e293b,stroke:#10b981,stroke-width:1.5px,color:#f8fafc
```

#### 🧩 提供スキル一覧 (Available Skills)

| スラッシュコマンド | 分類 | 概要 | 連携先 / 主な成果物 |
| :--- | :--- | :--- | :--- |
| [`/vk-draft-rfc`](/.agents/skills/vk-draft-rfc/SKILL.md) | ライフサイクル (02) | RFC 立項（新機能・新モジュール提案の文書化） | `/docs/02-RFCs/`<br/>([02-RfcBlueprint.md](/docs/00-Blueprints/02-RfcBlueprint.md)) |
| [`/vk-draft-architecture`](/.agents/skills/vk-draft-architecture/SKILL.md) | ライフサイクル (03) | システム全景・アーキテクチャ図解の生成 | `/docs/03-Architecture/`<br/>([03-ArchitectureBlueprint.md](/docs/00-Blueprints/03-ArchitectureBlueprint.md)) |
| [`/vk-draft-adr`](/.agents/skills/vk-draft-adr/SKILL.md) | ライフサイクル (04) | アーキテクチャ決定レコード (ADR) の策定・採番記録 | `/docs/04-ArchitectureDecisionRecords/`<br/>([04-AdrBlueprint.md](/docs/00-Blueprints/04-AdrBlueprint.md)) |
| [`/vk-draft-impl-plan`](/.agents/skills/vk-draft-impl-plan/SKILL.md) | 実装計画 (Plan) | 実装契約・エラー一覧・詳細設計の策定 | Implementation Plan<br/>([Plan-ContractAndImplementationBlueprint.md](/docs/00-Blueprints/Plan-ContractAndImplementationBlueprint.md)) |
| [`/vk-audit-architecture`](/.agents/skills/vk-audit-architecture/SKILL.md) | 監査・検証 (05) | 隔離読取専用サブエージェントによる深層アーキテクチャ・DNA・状態遷移監査 | `/docs/06-AuditReports/`<br/>([06-ArchitectureAuditBlueprint.md](/docs/00-Blueprints/06-ArchitectureAuditBlueprint.md)) |
| [`/vk-audit-fast`](/.agents/skills/vk-audit-fast/SKILL.md) | 監査・検証 (05) | コンパイル硬壁・Manifest Lint・最新 BB 構造の高速インライン診断 | チャット（インライン出力） |
| [`/vk-review-diff`](/.agents/skills/vk-review-diff/SKILL.md) | 監査・検証 | コンテキスト隔離された読取専用サブエージェントによる Diff 独立審査（アーキテクチャ変更時は自動昇格） | チャット（`PASS`/`FAIL` + 行番号根拠） |
| [`/vk-sprint-add-task`](/.agents/skills/vk-sprint-add-task/SKILL.md) | ライフサイクル (06) | バックログ起票および Active Sprint への自動同期 | `/docs/07-Backlogs/`<br/>([07-BacklogItemBlueprint.md](/docs/00-Blueprints/07-BacklogItemBlueprint.md)) |
| [`/vk-sprint-refine`](/.agents/skills/vk-sprint-refine/SKILL.md) | スプリント管理 | バックログの Story Points 見積・順位付け・DoR 確認 | `/docs/07-Backlogs/` |
| [`/vk-sprint-plan`](/.agents/skills/vk-sprint-plan/SKILL.md) | スプリント管理 | 1週間 Sprint の計画・目標設定・Ready タスク確約 | `/docs/07-Backlogs/Active_Sprint.md` |
| [`/vk-sprint-task-start`](/.agents/skills/vk-sprint-task-start/SKILL.md) | スプリント管理 | タスクの `doing` 遷移・開始日記録・Working Notes 確認 | `/docs/07-Backlogs/Active_Sprint.md` |
| [`/vk-sprint-daily`](/.agents/skills/vk-sprint-daily/SKILL.md) | スプリント管理 | 日次進捗（Yesterday/Today/Blockers）の記録 | `/docs/07-Backlogs/Active_Sprint.md` |
| [`/vk-sprint-task-done`](/.agents/skills/vk-sprint-task-done/SKILL.md) | スプリント管理 | DoD 検証・コミット自動紐付け・ビルド確認・完了遷移 | `/docs/07-Backlogs/` |
| [`/vk-sprint-review`](/.agents/skills/vk-sprint-review/SKILL.md) | スプリント管理 | Sprint 終了・KPT レトロスペクティブ・スコアカード更新 | `/docs/07-Backlogs/Health_Scorecard.md` |
| [`/vk-init-new-block`](/.agents/skills/vk-init-new-block/SKILL.md) | 生成・構築 | 新規 BuildingBlock の初期化・垂直スライス配置 | `/src/BuildingBlocks/` |
| [`/vk-generate-endpoint`](/.agents/skills/vk-generate-endpoint/SKILL.md) | 生成・構築 | CQRS エンドポイント垂直スライスの生成 | 各機能ディレクトリ |
| [`/vk-generate-sample`](/.agents/skills/vk-generate-sample/SKILL.md) | 生成・構築 | 実装サンプル・デモコードの生成 | `/src/Samples/` |
| [`/vk-generate-unit-tests`](/.agents/skills/vk-generate-unit-tests/SKILL.md) | 生成・構築 | 単体テスト雛形の生成 | テストプロジェクト |
| [`/vk-devops-pipeline`](/.agents/skills/vk-devops-pipeline/SKILL.md) | 生成・構築 | Azure DevOps CI/CD パイプライン生成 | `azure-pipelines.yml` |
| [`/vk-code-normalization`](/.agents/skills/vk-code-normalization/SKILL.md) | 品質・正規化 | VK.Blocks 標準に準拠した AI コード正規化 | 対象ファイル / ディレクトリ |
| [`/vk-normalize-mechanical`](/.agents/skills/vk-normalize-mechanical/SKILL.md) | 品質・正規化 | 物理構造・dotnet format による機械的正規化 | 対象ファイル / ディレクトリ |
| [`/vk-refactor-exceptions`](/.agents/skills/vk-refactor-exceptions/SKILL.md) | 品質・正規化 | 例外スローを Result.Failure パターンへ改修 | 対象ファイル |
| [`/vk-coverage-optimization`](/.agents/skills/vk-coverage-optimization/SKILL.md) | 品質・正規化 | テストカバレッジの段階的最適化 | カバレッジ対象 |
| [`/vk-dotnet-test-report`](/.agents/skills/vk-dotnet-test-report/SKILL.md) | 品質・正規化 | テスト実行と HTML カバレッジレポート生成 | `TestResults/` |
| [`/vk-generate-readme`](/.agents/skills/vk-generate-readme/SKILL.md) | ドキュメント | モジュール単位の README.md 自動生成 | 各モジュールルート |
| [`/vk-generate-root-readme`](/.agents/skills/vk-generate-root-readme/SKILL.md) | ドキュメント | リポジトリルートの README.md 自動生成 | ルート `README.md` |
| [`/vk-generate-pr-summary`](/.agents/skills/vk-generate-pr-summary/SKILL.md) | ドキュメント | Git Diff を解析した PR サマリの生成 | チャット（PR 本文用） |
| [`/vk-docs-normalize-links`](/.agents/skills/vk-docs-normalize-links/SKILL.md) | 支援・ツール | ドキュメント内部リンクのルート相対パス正規化 | `/docs/` 配下 |
| [`/vk-copy-file`](/.agents/skills/vk-copy-file/SKILL.md) | 支援・ツール | 指定先への安全なファイルコピー | 指定ファイル |

### AI エージェント向け
1. **Bootstrap**: モジュール初動時は `vk_be_get_module_context(path)` でローカル指示を取得せよ。
2. **Dynamic Sync**: 変更内容に応じた Rule ID を `vk_be_get_architectural_rule(ruleIds)` で取得し、Handshake 行を宣言せよ。
3. **Compile-Time First**: `dotnet build -warnaserror` によるコンパイル時検証を第一とし、以下のハードウォールを遵守せよ。
4. **Governance Self-Lint (`vk_be_agents_lint`)**: `.agents/` 変更後は `vk_be_agents_lint`（または `dotnet run --project src/Tools/McpServer/VK.Tools.McpServer.csproj -- --agents-lint`）でリンク・ツール名・Skill 索引・L3 Rule ID の整合性を検証せよ。
5. **Loop Exits & External Pitfall Memory (`BASE-13`, `BASE-14`)**: 2回連続で同じ失敗・指摘を受けたら即停止し `EXPECTED / SAW / ALREADY TRIED` を報告せよ。失敗アプローチや回避策は PBI を使わない通常会話でも `/src/.prompts/pitfalls.md`（リポジトリ共通）または `/src/BuildingBlocks/{Module}/.prompts/pitfalls.md`（モジュール別）へ 1〜2 行で追記し、`vk_be_get_module_context` 経由で自動継承せよ（PBI の `Working Notes` はタスク単位の補助として併用可）。
6. **Audit**: 回答末尾で検証根拠付きの `Audit` タグ（`verified: build` / `verified: build + isolated-review` / `self-check only`）または違反フラグを出力せよ。

#### 🧱 コンパイル時ハードウォール（Compile-Time Hard Walls）

| Diagnostic ID | 対応ルール | 検証内容 | 実装場所 |
| :--- | :--- | :--- | :--- |
| **`VK0001`**, `CA1852` | `AP.01` | 非抽象・非継承設計クラスの `sealed` 強制 | `VK.Tools.Analyzers` / `.editorconfig` |
| **`VK0002`** | `CS.06` | `DateTime.Now` / `UtcNow` 等を禁止（`TimeProvider` 強制） | `VK.Tools.Analyzers` |
| **`VK0003`** | `CS.06` | `Guid.NewGuid()` を禁止（`IVKGuidGenerator` 強制） | `VK.Tools.Analyzers` |
| **`VK0004`** | `CS.06` | `JsonSerializer` 直呼び出し禁止（`IVKJsonSerializer` 強制） | `VK.Tools.Analyzers` |
| **`VK0005`**, `CA2007` | `CS.03` | `.Result`/`.Wait()`/`GetResult()` 禁止・`.ConfigureAwait(false)` 強制 | `VK.Tools.Analyzers` / `.editorconfig` |
| **`VK0006`** | `AP.01` | プロパティ初期化子での `default!` 禁止 | `VK.Tools.Analyzers` |
| `CA1848`, `VK1010` | `OR.01` | `[LoggerMessage]` 強制・`EventId` 重複禁止 | `.editorconfig` / `SourceGenerators` |
| `IDE0130` | `AP.03` | `Internal/` 配下の物理パスと名前空間の一致強制 | `.editorconfig` |
