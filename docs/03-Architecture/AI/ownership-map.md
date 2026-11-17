# AI Ownership Map

AI 系 BuildingBlocks における **能力 (Capability) → 唯一の所有モジュール** の対応表です。
各 `module-manifest.md` の `## 1. Ownership` は本表の行 ID を参照し、`vk_be_manifest_lint` が整合性を検証します。

## 規約

- **1 行 = 1 Owner**。共同所有は禁止。争点がある場合は行を分割する。
- **ID は不変**。lint 有効化後に廃止する場合は Capability 先頭に `[Retired]` を付けて行を残す（再採番禁止）。本版はフリーズ前のため ID の改廃を許容する。
- **Capability は原則で記述**し、型名・メソッド名を書かない（BASE-12）。
- **Owner の値**:
  - BuildingBlocks のディレクトリ名（例: `AI.Synapse`、`VectorSearch`）。
  - `Host` — ホストアプリケーション責務（lint は所有検証をスキップ）。
  - `TBD` — 未解決（lint エラー）。
- **Status の値**:
  - `Confirmed` — manifest 間・コード確認で矛盾なし。
  - `Provisional` — 暫定 Owner。境界の最終確認待ち。
  - `Disputed` — 複数モジュールが所有を主張している。解消まで lint 警告。
- **Notes の記法**（先頭に置く。複数ある場合は `;` 区切り）:
  - `Port:` — Host 行で、接続先ポートを定義するモジュール。
  - `Excludes:` — 所有してはならないモジュール（禁止側）。
  - `Ref:` — 出典（manifest 節・ADR）。
  - それ以外は境界・委譲先・未作成モジュールの注記。変更履歴は本表に書かない。

## Resilience Layering（障害対応の層別分担）

リトライ・タイムアウト・サーキットは機構（`Resilience` ライブラリ）を共有するが、**ポリシーの所有者は層ごとに 1 つ**とする。

| Layer | 責務 | Owner | 禁止事項 |
| :--- | :--- | :--- | :--- |
| 単一プロバイダ呼び出し | リトライ（指数バックオフ + ジッタ）、試行ごとのタイムアウト（`OWN-TRANSPORT-RESILIENCE`） | AI.MEAI | サーキット状態・クールダウンを持たない |
| 候補接続間フェイルオーバー実行 | 候補接続リストに沿ったフェイルオーバー試行ループ実行・出力コミット後の切替禁止（`OWN-FAILOVER-EXECUTION`） | AI.MEAI | ルーティング／フェイルオーバー方針決定や接続健康状態を持たない |
| 接続（方針・健康状態） | 接続単位のサーキット状態・クールダウン（`OWN-CONN-HEALTH`）、ルーティング／フェイルオーバー方針・アフィニティ（`OWN-ROUTE`） | AI.Synapse | 呼び出し内リトライやフェイルオーバー実行ループを持たない |
| ターン | ターン総デッドライン（キャンセル伝播） | AI.Cortex | ターン全体の再実行、独自サーキットを持たない |
| Host 側ストア（DB 等） | 各ストア固有の Resilience | Host | — |

**不変条件**

- リトライ回数 × 試行タイムアウト（バックオフ含む）は、ターンデッドライン以内に収める。
- 単一候補に対するトランスポートリトライ（`OWN-TRANSPORT-RESILIENCE`）が尽きた後、`AI.Synapse` が決定した候補リスト・切替方針（`OWN-ROUTE`）と接続健康状態（`OWN-CONN-HEALTH`）に基づき、`AI.MEAI` が候補接続間のフェイルオーバー実行ループ（`OWN-FAILOVER-EXECUTION`）を行う（出力コミット後の切替禁止）。
- ターン全体を再実行しない理由: ツール実行・履歴保存・記憶キュー投入などの副作用があり、再実行は二重課金と二重副作用を招く。
- Cortex に単一の「全局サーキット」を置かない理由: 1 プロバイダの障害でターン全体が遮断され、`AI.Synapse`（`OWN-ROUTE` / `OWN-CONN-HEALTH`）および `AI.MEAI`（`OWN-FAILOVER-EXECUTION`）による接続フェイルオーバが機能しなくなる。

## Ownership Rows

### Contracts & Catalog

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-PORT-CONTRACT | プロバイダ非依存の AI ポート契約と NoOp/InMemory 既定実装 | AI | Confirmed | Ref: ADR-020 |
| OWN-PORT-ERRORS | AI 共通エラーコード定数と使用量・失敗の共通語彙 | AI | Confirmed | Ref: Synapse C4 |
| OWN-CATALOG | 静的モデル事実（制限・能力・ライフサイクル） | AI | Confirmed | Ref: Synapse B5 / MEAI §1 |
| OWN-TOOL-CONTRACT | ツールのフィルタ・承認・結果切詰めの契約 | AI | Confirmed | 契約のみで実行を持たない。実行点は OWN-TOOL-FILTER; Ref: AgentFramework §3 / MEAI §5 |
| OWN-TOKEN-COUNT | トークン計数の契約と既定の近似実装 | AI | Confirmed | Psyche・Afferent・Efferent 等の利用側は本契約を使い独自実装しない。実使用量は OWN-USAGE-ACTUAL |

### Execution

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-MODEL-CALL | 単一の論理モデル呼び出しとクライアント構築 | AI.MEAI | Confirmed | |
| OWN-PROVIDER-TRANSLATION | プロバイダ固有のスキーマ・ツール変換 | AI.MEAI | Confirmed | Excludes: AI.Eidos |
| OWN-TRANSPORT-RESILIENCE | 単一プロバイダ呼び出し内のリトライと試行ごとのタイムアウト | AI.MEAI | Confirmed | サーキットは OWN-CONN-HEALTH。上位層での再リトライ禁止（Resilience Layering 参照） |
| OWN-FAILOVER-EXECUTION | 候補接続間のフェイルオーバー実行（試行上限・出力コミット後の切替禁止） | AI.MEAI | Confirmed | 意思決定と健康状態は OWN-ROUTE / OWN-CONN-HEALTH（AI.Synapse）に委譲 |
| OWN-USAGE-ACTUAL | プロバイダ応答に基づく実使用量の確定（使用量の唯一の真実源） | AI.MEAI | Confirmed | 推定計数は OWN-TOKEN-COUNT。確定した実使用量は IVKAIInvocationObserver 経由で AI.Synapse に報告。Afferent/Efferent は推定のみ利用 |
| OWN-CALL-IDEMPOTENCY | 論理モデル呼び出し単位の冪等性 | AI.MEAI | Confirmed | ターン保存の冪等性は OWN-TURN-IDEMPOTENCY |
| OWN-TOOL-LOOP | 1 回のモデル呼び出し内における、モデル要求ツールの実行ループ | AI.MEAI | Provisional | MAF 側設定の最終確認待ち |
| OWN-TOOL-FILTER | ツール呼び出し前後のフィルタリングと監査の実行点 | AI.MEAI | Confirmed | 契約は OWN-TOOL-CONTRACT |
| OWN-OUTPUT-EFFECTS | 最終構造化出力に宣言された効果（ドメインイベント・状態変化）の適用 | AI.Efferent | Confirmed | |

### Policy & State

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-ROUTE | 接続選択・フェイルオーバ・アフィニティ固定 | AI.Synapse | Confirmed | |
| OWN-CONN-HEALTH | 接続単位のヘルス（サーキット状態を含む）と失敗時の処置方針 | AI.Synapse | Confirmed | MEAI は失敗を報告するのみ |
| OWN-QUOTA | 呼び出し側（テナント・ユーザー）の予算クォータ | AI.Synapse | Confirmed | プロバイダ容量は OWN-PROVIDER-CAPACITY |
| OWN-PROVIDER-CAPACITY | プロバイダ側の容量制限（RPM/TPM 等）の追跡と回避 | AI.Synapse | Confirmed | OWN-QUOTA から分離 |
| OWN-CUMULATIVE-QUOTA | 累計クォータ（トークン／コスト）の追跡と上限判定 | AI.Synapse | Confirmed | コスト換算は OWN-COST に依存 |
| OWN-COST | 価格版管理とコスト換算・集計 | AI.Synapse | Confirmed | |
| OWN-CREDENTIAL-REF | 資格情報参照の解決とエンドポイント検証 | AI.Synapse | Confirmed | 秘密の保管は OWN-SECRET-STORE |
| OWN-SECRET-STORE | 秘密情報の保管とローテーション | Host | Confirmed | Port: AI.Synapse; Ref: Synapse S4 |

### Agents

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-AGENT-RUN | 単一エージェント実行 | AI.AgentFramework | Confirmed | 上限は OWN-AGENT-RUN-BUDGET |
| OWN-AGENT-GROUP | マルチエージェント協調 | AI.AgentFramework | Confirmed | 上限は OWN-AGENT-GROUP-BUDGET |
| OWN-AGENT-WORKFLOW | チェックポイント付き決定論的グラフ実行（エージェント用・プロセス内） | AI.AgentFramework | Provisional | VK.Blocks.Workflow（ライブラリ横断・AI 外）との境界は未確定 |
| OWN-AGENT-TOOL-POLICY | ツールホワイトリストと人間による承認（HITL） | AI.AgentFramework | Confirmed | |

### Ingress / Egress

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-INGRESS-NORMALIZE | 外部信号（テキスト・音声・知覚・イベント）の正規化 | AI.Afferent | Confirmed | |
| OWN-INPUT-GUARD | ユーザー生入力のガードレール（プロンプト組み立て前） | AI.Afferent | Confirmed | 検出器契約は AI で共有 |
| OWN-OUTBOUND-GUARD | 組み立て済み送信リクエスト（RAG・記憶・ツール結果含む）のガードレール | AI.MEAI | Confirmed | 全入力が揃う最後の単一地点であるため MEAI。検出器契約は AI で共有 |
| OWN-EGRESS-GUARD | モデル出力のガードレール（モデレーション・漏洩防止） | AI.Efferent | Confirmed | |
| OWN-EGRESS-STREAM | 出力ストリームの整形・チャンク化・割り込み | AI.Efferent | Confirmed | |
| OWN-EXPRESSION | マルチモーダル表現（音声合成・韻律・ビゼーム） | AI.Efferent | Confirmed | |
| OWN-DELIVERY-WATERMARK | 実配信位置の追跡と報告 | AI.Efferent | Confirmed | |
| OWN-COMPLIANCE-MARK | AI 透かしと規制上の開示文 | AI.Efferent | Confirmed | |

### Limits & Budgets

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-INPUT-SIZE | ユーザー入力の最大長制限 | AI.Afferent | Confirmed | |
| OWN-PROMPT-BUDGET | 組み立て済みプロンプトのコンテキストウィンドウ収容（裁断） | AI.Psyche | Confirmed | 知識候補側は OWN-KNOWLEDGE-BUDGET。旧: Weaving と Corpus の共同記述を分割 |
| OWN-KNOWLEDGE-BUDGET | 知識候補のトークン予算フィルタ | AI.Corpus | Confirmed | OWN-PROMPT-BUDGET から分離 |
| OWN-REQUEST-CAP | 単一モデルリクエストのハード上限（超過時は型付きエラー） | AI.MEAI | Confirmed | サイレント切詰め禁止 |
| OWN-AGENT-RUN-BUDGET | 単一エージェント実行のステップ数・トークン・時間の上限 | AI.AgentFramework | Confirmed | |
| OWN-AGENT-GROUP-BUDGET | エージェントグループ単位の上限 | AI.AgentFramework | Confirmed | OWN-AGENT-GROUP から分離 |
| OWN-TURN-TIME | 1 ターンの総所要時間予算（デッドライン） | AI.Cortex | Confirmed | Ref: ADR-CTX-02。リトライ・サーキットは持たない |

### Prompt & Conversation

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-PROMPT-ASSEMBLY | プロンプト組み立てとステージ実行 | AI.Psyche | Confirmed | Ref: MEAI §8 |
| OWN-PROMPT-HISTORY | 今回のプロンプトに含める会話履歴の選定 | AI.Psyche | Confirmed | 履歴縮退の 3 行の境界: 本行=1 プロンプト単位 / OWN-RUN-CONTEXT-REDUCTION=1 エージェント実行内 / OWN-MEMORY-COMPRESSION=ターンをまたぐ永続圧縮 |
| OWN-RUN-CONTEXT-REDUCTION | 単一エージェント実行中の肥大化した会話履歴の縮退 | AI.AgentFramework | Confirmed | MEAI の履歴縮退機構を再利用 |
| OWN-HISTORY-MODEL | 会話履歴の型定義と読み書きポート（Echo） | AI.Psyche | Confirmed | |
| OWN-HISTORY-PERSIST | 会話履歴の永続化実装 | Host | Confirmed | Port: AI.Psyche |
| OWN-PERSONA-DIRECTIVE | ペルソナ・ディレクティブ・パターンの型定義と読み出しポート | AI.Psyche | Provisional | 解決・権限・既定値は OWN-CONTEXT-RESOLUTION（Host）。書き込み経路も Host |

### Output Contract

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-OUTPUT-CONTRACT | 出力契約の定義と版管理 | AI.Eidos | Confirmed | |
| OWN-OUTPUT-NEGOTIATION | 出力表現方式の事前交渉とフォールバック | AI.Eidos | Confirmed | |
| OWN-OUTPUT-PARSE | 出力の抽出・検証・修復・DTO バインド | AI.Eidos | Confirmed | |

### Knowledge & Memory

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-STATIC-KNOWLEDGE | 静的権威知識の取込・承認・版管理・注入 | AI.Corpus | Confirmed | |
| OWN-EPISODE | 不変・追記専用のエピソードログ | AI.Engram | Confirmed | 不変性の唯一の例外は OWN-MEMORY-ERASURE |
| OWN-DERIVED-MEMORY | 双時間軸の派生記憶 | AI.Engram | Confirmed | |
| OWN-MEMORY-COMPRESSION | 派生記憶の L1→L2→L3 圧縮と保持 | AI.Engram | Confirmed | |
| OWN-SESSION-SUMMARY | セッション要約の生成 | AI.Engram | Confirmed | |
| OWN-RECALL | 動的記憶の想起と提示 | AI.Engram | Disputed | 検索実行は OWN-VECTOR-SEARCH に委譲。Corpus の「static + dynamic recall の結合」との境界要確認 |
| OWN-MEMORY-ERASURE | 記憶（エピソード・派生記憶）の法令準拠の物理削除（ベクトル索引へのカスケード含む） | AI.Engram | Confirmed | 履歴・監査ログを含む横断削除は OWN-CROSS-STORE-ERASURE |
| OWN-AUDIT-PERSISTENCE | 監査ログの永続化 | Host | Confirmed | Port: 未確定（MEAI / Eidos が参照）; Ref: MEAI §1 |

### Retrieval Infrastructure

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-VECTOR-SCHEMA | ベクトルストアのスキーマ・レジストリ・ライフサイクル | VectorStore | Provisional | manifest 作成時に確定 |
| OWN-VECTOR-INGEST | 埋め込み・量子化・重複排除・書き込み | VectorIngest | Provisional | |
| OWN-VECTOR-SEARCH | 類似検索・フィルタ・ハイブリッド検索 | VectorSearch | Provisional | OWN-RECALL はこれに委譲 |

### Cognition

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-INTENT-CLASSIFY | 意図分類 | AI.Cognitive | Provisional | AI.Cognitive の manifest は空; Ref: Synapse §11 |

### Orchestration & Host Boundary

| ID | Capability | Owner | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| OWN-TURN-ORCHESTRATION | 1 ターンのシナリオ合成と、ライブラリ横断のライフサイクル調整 | AI.Cortex | Confirmed | ドメインロジックは持たない; Ref: ADR-CTX-01 / ADR-CTX-02 |
| OWN-SESSION-LIFECYCLE | セッションの有効期限・終了時アクション | AI.Cortex | Confirmed | Ref: ADR-CTX-03 |
| OWN-TURN-IDEMPOTENCY | ターン単位の保存冪等性 | AI.Cortex | Confirmed | 呼び出し単位は OWN-CALL-IDEMPOTENCY |
| OWN-CORRELATION | TraceId・SessionId の相関伝播 | AI.Cortex | Confirmed | Ref: ADR-CTX-04 |
| OWN-CROSS-STORE-ERASURE | 記憶・ベクトル・履歴・監査にまたがる削除の編排 | AI.Cortex | Provisional | 未実装。Host 保有ストアは Host が実行 |
| OWN-TENANT-ISOLATION | テナント分離（実行コンテキスト確立点での強制） | Host | Confirmed | ライブラリは TenantId を検証しない |
| OWN-CONTEXT-RESOLUTION | Persona・Directive・Pattern・Profile の解決・権限確認・既定値・上書き | Host | Confirmed | Port: AI.Psyche（Request が唯一の業務コンテキスト入力） |

## Invariant Prefix Registry

manifest の `## 5. Invariants` で使用する ID 接頭辞。L3 ルール接頭辞（CS/AP/BB/OR/DL/PS）および `OWN` との衝突を禁止します。

| Prefix | Module            |
| :----- | :---------------- |
| AIB    | AI                |
| AFF    | AI.Afferent       |
| AGT    | AI.AgentFramework |
| COG    | AI.Cognitive      |
| CRP    | AI.Corpus         |
| CTX    | AI.Cortex         |
| EFF    | AI.Efferent       |
| EID    | AI.Eidos          |
| ENG    | AI.Engram         |
| MEA    | AI.MEAI           |
| PSY    | AI.Psyche         |
| SYN    | AI.Synapse        |

`VectorStore` / `VectorIngest` / `VectorSearch` の接頭辞は各 manifest 作成時に追加します。

## 対象外・未登録モジュール

- **対象外（保留）**: `AI.Somatic`、`AI.Adapt`、`AI.Praxis`。所有行は登録しない（旧 `OWN-TOOL-DECIDE` は削除）。方針確定後に行を追加する。`PRX` 接頭辞のみ既存 manifest 互換のため残す。
- **未登録**: 既存 manifest が参照しているが本表の対象外または未作成のモジュール: `AI.Probe`、`Echo/Session`。所有範囲が確定次第、行を追加してください。
- **AI 外の共有ライブラリ**（所有行なし）: `Resilience`（リトライ・サーキット等の機構。ポリシー所有は Resilience Layering 参照）、`VK.Blocks.Workflow`（ライブラリ横断の長時間トランザクション。`OWN-AGENT-WORKFLOW` との境界は未確定）。

## 未解決事項

1. `OWN-RECALL`（Engram）と Corpus の static + dynamic recall 結合の境界。
2. `OWN-TOOL-LOOP` の MAF 側設定の最終確認。
3. `OWN-AGENT-WORKFLOW` と `VK.Blocks.Workflow` の境界。
4. `OWN-AUDIT-PERSISTENCE` の Port 定義モジュール。
5. `AI.Cognitive` の manifest 作成（`OWN-INTENT-CLASSIFY`）。
