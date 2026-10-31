# ADR 003: Evolve AI Agent Governance into Multi-Layered Inheritance and Tiered Dynamic Loading Architecture

- **Date**: 2026-10-05
- **Status**: ✅ Accepted (Updates and Extends [ADR-001](./adr-001-establish-tri-layered-ai-agent-collaboration-architecture.md))
- **Deciders**: Architecture Team
- **Technical Story**: DeveloperEcosystem / AI Governance Protocol

---

## 1. Context (背景)

[ADR-001](./adr-001-establish-tri-layered-ai-agent-collaboration-architecture.md) において、AI 協調開発の関心事の分離（Rules / Workflows / MCP Tools の三層構造）を確立し、コーディング規約を単一のマスターチェックリスト（`.agents/rules/vk-blocks-checklist.md`）に集約しました。

しかし、プロジェクトの成熟とエコシステムの拡大（特にフロントエンド `VK-Common-FE` との連携）に伴い、以下の新たな課題が生じました：

1. **ルールの関心事の混在**:
   普遍的なエンジニアリング思考（第一原理、契約優先）、言語非依存のシステムアーキテクチャ原則（境界付けられたコンテキスト、Port-and-Adapter）、フルスタック境界契約（OpenAPI、RFC 7807 ProblemDetails）、および .NET / C# 12+ 固有の具象ルールが単一のファイルに混在し、責任境界が曖昧になっていました。
2. **LLM の「中間での紛失（Lost-in-the-Middle）」とコンテキスト消費**:
   すべての詳細ルールを単一ファイルとして常時プロンプトに注入すると、コンテキストウィンドウのトークンを大量に消費するだけでなく、LLM の注意力が希薄化し、ルールの自己解釈や忘却（Rule Drift）が発生しやすくなっていました。
3. **フロントエンド・バックエンド間の規約乖離**:
   ワイヤ上の契約（Error レスポンス形式、トレーシング伝播、タイムスタンプ形式等）が各リポジトリのルールファイルに個別に記述され、ドリフトのリスクを抱えていました。

---

## 2. Problem Statement (問題定義)

- **単一法典の限界**: すべてのルールを 1 つのファイルで管理することは、関心事の分離（Separation of Concerns）およびトークン経済性の観点からスケールしない。
- **クロススタック規約の欠落**: フロントエンドとバックエンドで共通して遵守すべきワイヤ契約（L2）が明確にレイヤー化されておらず、クライアント・サーバー間の不整合が発生しやすい。
- **動的検証の仕組みの不在**: ルールが静的に注入されるだけで、AI が作業シナリオに応じた詳細仕様を能動的にロードして検証するプロトコルが不十分であった。

---

## 3. Decision (決定事項)

AI ガバナンス法典を **「4層単方向継承モデル（L0〜L3）」** および **「Tiered Strategy（常時ロード＋MCP オンデマンド動的ロード）」** へ進化させます。

### 3.1. 4層の単方向継承モデル (`inherits`)

ルール体系を関心事の抽象度に応じて 4 つのレイヤーに分離し、下位（親）に向かう厳格な単方向継承を適用します：

```text
L0: vk-base (基底思想・第一原理)
 └── L1: vk-arch (システム設計原則)
      └── L2: vk-fullstack (ワイヤ境界契約)
           └── L3: vk-be (バックエンド技術スタック)
```

- **L0: VK IT Base (`l0-vk-base.md`)**: 第一原理思考、契約優先（Contract-First）、ゼロ暗黙状態、言語非依存の基底規約および出力プロトコル。
- **L1: System Architecture (`l1-vk-arch.md`)**: 境界づけ、Port-and-Adapter、垂直スライス、コアの純粋性、最小公開原則。
- **L2: Full Stack Baseline (`l2-vk-fullstack.md`)**: ワイヤ上の単一情報源（OpenAPI）、同型 `Result<T>` エラーモデル（RFC 7807）、強く型付けされた ID、W3C 分散トレーシング。**※ VK-Common-FE と完全に対齊・共有。**
- **L3: Technology Stack (`l3-vk-be.md`)**: .NET 8/10+、C# 12+、ASP.NET Core、EF Core、Source Generators 固有の具象規約。
- **Precedence 則**: 下位レイヤーは上位レイヤーの制約を厳格化（Tighten）できますが、**緩和（Relax）することは例外なく禁止** されます。

### 3.2. 階層制ルール適用プロトコル (Tiered Strategy)

コンテキスト節約と注意力の最大化を両立するため、二段構えのロード戦略を採用します：

1. **常時ロード層 (Always-On System Prompts)**:
   - `l0-vk-base.md`、`l1-vk-arch.md`、`l2-vk-fullstack.md`、`l3-vk-be.md`
   - **Tier 1 (Rule Index)**: 全ルールの 1 行要約による認知維持。
   - **Tier 2 (Core Prohibitions)**:
     - **Type A (🔴 絶対禁令)**: 安全・決定論のコア規約（`CS.01`, `CS.03`, `CS.06`, `CS.07`, `CS.08`, `OR.01`, `DL.01`, `AP.01`, `AP.06`, `AP.07`, `PS.04`, `PS.05`）。Labs 環境を含め例外なし。
     - **Type B (🟡 工業標準)**: 命名・配置規約。`src/Labs` で明示的に免除されない限り厳守。
2. **オンデマンド動的ロード層 (Tier 3: trigger: manual)**:
   - 詳細仕様ファイル（`l3-vk-be-cs-core-standards.md` 等）はプロンプト外に配置。
   - **Hard-Lock 原則**: 作業シナリオに応じ、MCP ツール（`vk_be_get_architectural_rule`）で該当ルールの最新仕様を明示的に取得（Sync）してからコード生成を行う。当て推量によるコード出力を禁止。

### 3.3. 出力プロトコルと監査 (The Handshake & Audit Tag)

- **Handshake**: 回答の先頭行にアクティブレイヤーと同期済み Rule ID を明記：
  `Active: [L0-L3:{Module}] | Context: {Path} | Sync: [{RuleID},...]`
- **Audit Tag**: 回答の末尾で自己監査ステータスを宣言：
  `Audit: ✅` または `Audit: 🚩 [RuleID] {Rationale}`（Type B 免除の理由を明記）

---

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 単一ファイル（ADR-001 の方式）の維持・拡大 (Rejected)**
  - _理由_: ルール追加に伴いプロンプトが数万トークンに達し、コンテキスト消費が深刻化する。またフロントエンドとの共通部分（L0〜L2）を分離して共有することができない。
- **Option 2: 全ルールをシステムプロンプトから除外し、完全 MCP 動的ロードにする (Rejected)**
  - _理由_: AI が「どのようなルールが存在するか（Awareness）」を完全に失い、どのシナリオでどのルールを取得すべきかの判断自体が不安定になる。
- **Option 3: 階層継承（L0〜L3）＋ 常時インデックス ＋ オンデマンド動的ロードのハイブリッド (Accepted)**
  - _採用理由_: 常にルールの存在を意識させつつ（Tier 1/2）、詳細仕様は必要な時だけ最新コンテキスト領域に引き戻す（Tier 3）ため、トークン効率・認知精度・保守性のすべてにおいて最適。

---

## 5. Consequences & Mitigation (結果と緩和策)

### Positive (メリット)
- **トークン効率の飛躍的向上**: 詳細な実装コード例や規約本文が常時プロンプトから除外され、会話の有効コンテキストウィンドウが大幅に拡大。
- **ルールドリフトの根絶**: MCP を介して外部ソースから決定論的に詳細仕様を取得するため、AI の記憶違いや自己解釈による違反が排除される。
- **フルスタック開発の整合性**: フロントエンドと同一の L0〜L2 レイヤー規約を共有することで、API 境界での手戻りがゼロ化。

### Negative (懸念点)
- ファイル数が増加し、初見の開発者に対する初期理解コストが上がる。
- MCP ツール（`vk_be_get_architectural_rule`）が正常稼働していることが前提となる。

### Mitigation (緩和策)
- [/.agents/README.md](/.agents/README.md) にアーキテクチャ図とクイックリファレンスを完備。
- `McpTools.Rules.cs` の走査ロジックを堅牢化し、レイヤー定義ファイルと詳細ルールファイルを自動識別して取得可能に改修済み。

---

## 6. Implementation & Compatibility (実装詳細と互換性)

- **ファイル移行**:
  - `vk-blocks-checklist.md` → `l3-vk-be.md`
  - `01-core-standards.md` 〜 `06-planning-standards.md` → `l3-vk-be-*.md`
- **MCP Server 改修**:
  - `McpTools.Rules.cs` のファイル取得処理を `0*.md` 固定から `*.md`（レイヤーインデックス除外）の動的フィルタリングへ更新し、ビルド検証完了。
- **ADR-001 との関係**:
  - 本 ADR は ADR-001 の「三層協調アーキテクチャ（Rules / Workflows / MCP Tools）」の基本理念を継承しつつ、その「Rules」の内部構造を単一ファイルから多層階層モデル（L0〜L3）へと発展的解消（Supersede & Extend）するものです。
