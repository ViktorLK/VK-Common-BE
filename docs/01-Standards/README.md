# 🏛️ VK.Blocks Engineering Standards (`01-Standards`)

本ディレクトリ（`docs/01-Standards/`）は、`VK.Blocks` エコシステム全体の**開発標準・設計原則・具体的実装コード例（Human & Agent Reference Handbook）**を収めています。

検索性とトレーサビリティを最大化するため、本ディレクトリのファイル構成（`00`〜`06`）は **[`.agents/rules/`](/.agents/rules) のルール体系（`L0`〜`L2` および `L3` の6大カテゴリ `CS`, `AP`, `BB`, `OR`, `DL`, `PS`）と完全に 1:1 でアライメント（対照）** されています。

---

## 🔄 `.agents/rules` との役割分担と 1:1 対応表

- **[`.agents/rules/`](/.agents/rules)**: AI エージェントが MCP ツール経由で動的ロードする**高密度な制約条文（Rule ID 辞書）**。
- **ここ（`docs/01-Standards/`）**: 各 Rule ID の**背景思想（Why）、詳細な設計ガイド、および具体的な C# コード例（Usage Examples）**。人間が学習・レビューで参照するほか、AI エージェントが具体的な実装パターン例を確認する際にも参照します。
- **[`docs/00-Blueprints/`](/docs/00-Blueprints/README.md)**: 開発ライフサイクルの各フェーズ（RFC・Architecture・ADR・Spec・Audit・Backlog・Release）で成果物を起票する際の**実践テンプレート集**。

| 番号 | 標準ドキュメント (`docs/01-Standards/`) | 1:1 対応する AI ルール (`.agents/rules/`) | 収録される Rule ID と主要トピック |
| :---: | :--- | :--- | :--- |
| **00** | **[00-foundation-principles.md](./00-foundation-principles.md)** | [`l0-vk-base.md`](/.agents/rules/l0-vk-base.md)<br>[`l1-vk-arch.md`](/.agents/rules/l1-vk-arch.md)<br>[`l2-vk-fullstack.md`](/.agents/rules/l2-vk-fullstack.md) | **`BASE-01`〜`12` / `ARCH-01`〜`13` / `FS-01`〜`13`**<br>契約先行、ゼロ暗黙グローバル、単一方向レイヤー、Core 純粋性、RFC 7807 `ProblemDetails`、強型付け ID (`[VKStronglyTypedId]`)、`VKPagedResult<T>` |
| **01** | **[01-cs-core-standards.md](./01-cs-core-standards.md)** | [`l3-vk-be-cs-core-standards.md`](/.agents/rules/l3-vk-be-cs-core-standards.md) | **`CS.01`〜`CS.08` (Core Standards)**<br>`Result<T>` パターンとエラー定数階層、レイヤー依存、Async / `ConfigureAwait(false)`、`AsNoTracking` & `ArrayPool`、監査・論理削除インターセプタ、`TimeProvider` / `IVKGuidGenerator`、`GetRequiredService`、DB 永続化・マルチテナント規約 |
| **02** | **[02-ap-architecture-patterns.md](./02-ap-architecture-patterns.md)** | [`l3-vk-be-ap-architecture-patterns.md`](/.agents/rules/l3-vk-be-ap-architecture-patterns.md) | **`AP.01`〜`AP.07` (Architecture Patterns)**<br>C# 12/13（`sealed`, `required`, `VKGuard`）、`TryAdd` 登録ポリシー、可視性境界（`Internal/`）と `VK` プレフィックス、実装クラス分類（`Default`/`Basic`/`NoOp`）、Zero-Reflection Options、Args パターン、明示的 Override |
| **03** | **[03-bb-block-blueprint.md](./03-bb-block-blueprint.md)** | [`l3-vk-be-bb-block-blueprint.md`](/.agents/rules/l3-vk-be-bb-block-blueprint.md) | **`BB.01`〜`BB.08` (Block Blueprint)**<br>垂直スライス物理ディレクトリ構造、`[VKBlockMarker]`、SG 自動生成 DI シーケンスと `RegisterBlockCustom` フック、`[VKBlockDiagnostics]`、Options 1ファイル分離、`[VKFeature]` と親ブロック自動引き上げ |
| **04** | **[04-or-observability-resiliency.md](./04-or-observability-resiliency.md)** | [`l3-vk-be-or-observability-resiliency.md`](/.agents/rules/l3-vk-be-or-observability-resiliency.md) | **`OR.01`〜`OR.03` (Observability & Resiliency)**<br>`[LoggerMessage]` 構造化ログ集約、OpenTelemetry `ActivitySource` / `Meter`、EF Global Filter テナント分離 & PII マスキング、Polly レジリエンス（Retry + CircuitBreaker + Timeout） |
| **05** | **[05-dl-development-lifecycle.md](./05-dl-development-lifecycle.md)** | [`l3-vk-be-dl-development-lifecycle.md`](/.agents/rules/l3-vk-be-dl-development-lifecycle.md) | **`DL.01`〜`DL.05` (Development Lifecycle)**<br>単体テスト4経路プロトコル（`VKUnitTestBase`, `VKTestDataBuilder`）、No TODO / 即時コンパイル原則、ADR 起票トリガー、Backlog 同期トリガー、Source Generator タグ（`[SG Marker]`, `[SG Hook]` 等） |
| **06** | **[06-ps-planning-standards.md](./06-ps-planning-standards.md)** | [`l3-vk-be-ps-planning-standards.md`](/.agents/rules/l3-vk-be-ps-planning-standards.md) | **`PS.01`〜`PS.05` (Planning Standards)**<br>実装計画の ADR 監査、Walkthrough の閉ループ検証、RFC-First ポリシー（Metaphor → Mapping → Blueprint）、モジュールコンテキスト優先順位と Type A/B 免除ルール |

---

## 🛠️ クイック・チェックリスト (Base Style Summary)

- **Language & Compiler**: C# 12/13、File-scoped namespace、全プロジェクトで `<Nullable>enable</Nullable>`、警告ゼロ。
- **Types & Visibility (`AP.01`, `AP.03`)**: クラスはデフォルトで `sealed`、DTO/Options は `sealed record`（`required` / `init`）。`Internal/` 外はフラットなルート名前空間＋`VK` プレフィックス、`Internal/` 内はフルパス名前空間＋プレフィックスなし。
- **Error Handling (`CS.01`)**: 業務・運用エラーはすべて `Result<T>` と定義済み `Error` 定数で表現し、`null` 返却や例外によるフロー制御を行わない。
- **Async & I/O (`CS.03`)**: すべての I/O に `CancellationToken` を伝搬。ライブラリコードは `.ConfigureAwait(false)` 必須（テストコードでは禁止）。
- **Boundary & Determinism (`AP.01`, `CS.06`)**: 境界検証には `VKGuard`、時刻には `TimeProvider`、GUID には `IVKGuidGenerator` を使用する。
