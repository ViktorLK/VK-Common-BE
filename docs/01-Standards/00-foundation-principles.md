# 00. Foundation Principles (L0-L2 基盤憲章・アーキテクチャ・フルスタック原則)

> **対応ルール**: [`l0-vk-base.md`](/.agents/rules/l0-vk-base.md) (`BASE-*`) / [`l1-vk-arch.md`](/.agents/rules/l1-vk-arch.md) (`ARCH-*`) / [`l2-vk-fullstack.md`](/.agents/rules/l2-vk-fullstack.md) (`FS-*`)

本ドキュメントは、特定の言語やモジュールを超える **最上位のエンジニアリング原則（L0: Base、L1: Architecture、L2: Full-Stack Wire Contracts）** の解説と実装例を定義します。下位レイヤー（L3 Backend の `CS`, `AP`, `BB`, `OR`, `DL`, `PS`）はすべて本原則を継承・具体化します。

---

## 1. L0: VK IT Base & Engineering Mindset (`BASE-01`〜`BASE-12`)

- **`BASE-01` Contract-First (契約先行)**: 実装コードを書き始める前に、必ずインターフェース（`IVK...`）、DTO、ドメインモデル、エラーコード（`Errors` 定数）、および不変条件を定義します。
- **`BASE-02` Zero Implicit Globals (暗黙的グローバル状態の禁止)**: 静的な可変状態（`static` フィールドへの書き込み）、ブラックボックスなシングルトン、Composition Root 以外での環境変数の直接読み取りは禁止です。時刻（`TimeProvider`）、ID 生成（`IVKGuidGenerator`）、ロギング、実行コンテキストはすべて抽象を通じて注入します。
- **`BASE-03` Compile-Time First (コンパイル時強制)**: 型システム、Roslyn アナライザー、Source Generator で強制できる制約を実行時（リフレクション等）に先送りしてはなりません。
- **`BASE-04` Anti-Over-Friendliness (過度な忖度の禁止)**: フレームワークがビジネス意図を勝手に推測して「賢い」暗黙的動作をすることを禁じます。中立で文書化された Fallback（`NoOp` 等）のみ許容されます。
- **`BASE-05` Explicit Failure Model (明示的な失敗モデル)**: 予見可能な業務・運用エラーはすべて型付き `Result<T>` で返し、`null` 返却や例外による制御フローを禁止します。
- **`BASE-06` Radical YAGNI**: 確定した利用者が存在しない抽象化、オプション、空のアダプター、将来のためのパッケージ分割を禁止します。
- **`BASE-07` Production-Grade Delivery**: 擬似コード、プレースホルダースタブ、裸の `// TODO` の混入を禁止します。
- **`BASE-08` Honesty and Conflict Surfacing (誠実性と対立の表面化)**: API、パッケージ、バージョン、動作の捏造を厳禁とします。指示がルールと衝突した場合は、ルール ID、矛盾点、および合規な代替案を提示し、暗黙の逸脱を行いません（逸脱には明示的な OVERRIDE が必要）。
- **`BASE-09` Language Convention (言語規約)**: 識別子、コード、コメント、コミットメッセージは英語で記述します。解説、ドキュメント、ADR は日本語で記述します。
- **`BASE-10` Surgical Change Discipline (最小変更規律)**: 仕様を満たす最小限の差分（Diff）のみを提供し、無関係なコードのリファクタリングを行いません。曖昧さがある場合は明示的な前提を表明して進め、検証残しを明記します。
- **`BASE-11` Documentation & Path Hygiene (ドキュメント内部リンク規約)**: リポジトリ内の全 Markdown ドキュメントにおける内部リンクは、一貫性と可搬性を担保するためリポジトリルート相対パス（`/docs/...`、`/src/...`、`/.agents/...`）で統一します。同一ディレクトリおよびその子階層への参照（`./`）を除き、親階層への相対パス参照（`../`）は厳禁とします。
- **`BASE-12` Principles Over Signatures**: アーキテクチャマニフェストや境界文書には、変わりやすいメソッドシグネチャではなく不変の原則と契約を記述します。

---

## 2. L1: VK System Architecture Principles (`ARCH-01`〜`ARCH-13`)

- **`ARCH-01` Strict Unidirectional Layering**: 依存関係は常に上位から下位（Infrastructure → Application → Domain/Core）への単一方向のみとし、相互依存や無申告の横方向（Peer 間）参照を禁止します。
- **`ARCH-02` Port-and-Adapter Decoupling**: ドメインおよびアプリケーションロジックはポート（抽象インターフェース）にのみ依存し、具体的なインフラアダプター（EF Core, Redis, Cloud SDK）は Composition Root（Host 起動時）でのみ解決します。
- **`ARCH-03` Core Purity (Core の純粋性)**: Core モジュールには「すべての I/O から切り離しても単体で意味を持つ機能」のみを配置します。Web フレームワークや外部 I/O への依存は持ち込みません。
- **`ARCH-04` Packaging Integrity**: 別個のサードパーティ依存関係または物理的なデプロイ境界が生じる場合のみ別パッケージ（`.csproj`）に分割し、それ以外は同一パッケージ内の機能スライスとして構成します。
- **`ARCH-05` Co-Located Defaults, Deterministic Overrides**: 中立な Fallback 実装（`NoOp`, `Basic` InMemory）は契約と同じパッケージに同梱して `TryAdd` で冪等登録し、インフラ実装への置き換えは登録順序に依存しない明示的な Builder API で行います。
- **`ARCH-06` Vertical Slices**: 技術分類（`Helpers/`, `Utils/`, `Managers/`）ではなく、機能スライス単位でコードをグループ化します（例外はプライベート実装用の `Internal/` のみ）。
- **`ARCH-07` Minimal Public Exposure**: デフォルトは `internal` とし、公開 API・エラーコード・構成キーの変更はすべて破壊的変更として厳格に管理します。
- **`ARCH-08` Composition Root Isolation**: DI のワイヤリングは Composition Root に限定し、モジュール登録は自己完結かつ冪等に行います。
- **`ARCH-09` Boundary-Established Context**: テナント・ユーザー・トレース等の実行コンテキストは HTTP ミドルウェアやキューコンシューマーの入口境界で確立し、内部へは引数または注入されたコンテキスト抽象経由で明示的に渡します。
- **`ARCH-10` Orchestration Purity**: コーディネーターやパイプラインはオーケストレーションに専念し、ドメインアルゴリズムや状態遷移ルールを直接実装しません。
- **`ARCH-11` Extension Taxonomy**: 多様性の切り替えには `Strategy + Resolver`、順次ワークフローには `Composite`、前後ラップ処理には Onion 型 `Middleware` を使い分けます。
- **`ARCH-12` Perimeter Cross-Cutting**: リトライ、タイムアウト、サーキットブレーカー、キャッシュ、トレーシングはポートや境界にデコレーター／ミドルウェアとしてアタッチし、ドメインロジック内に混在させません。
- **`ARCH-13` Typed Configuration**: すべてのモジュールは起動時に検証される強型付け `Options` と、呼び出し単位で上書き可能な Nullable `Args` モデルを公開します。

---

## 3. L2: VK Full-Stack Wire Contracts (`FS-01`〜`FS-13`)

バックエンドとフロントエンドが共有するワイヤー（HTTP / Realtime）上の不変契約です。

- **`FS-01` Single Source of Truth on the Wire**: ワイヤー契約はサーバー生成の OpenAPI 仕様を唯一の真実の源泉（SSOT）とします。
- **`FS-02` Isomorphic Error Model (RFC 7807 `ProblemDetails`)**:
  ワイヤー上のエラー応答は RFC 7807 `ProblemDetails` 形式に統一し、不変のエラーコード（`{Module}.{Category}.{Reason}`）、`traceId`、および構造化されたフィールドエラーを含めます。
- **`FS-03` DTO and Domain Separation**: DTO はワイヤー境界のペイロード専用とし、インバウンド境界で検証した後にドメインモデルへマッピングします。永続化エンティティをそのまま外部 API に露出しません。
- **`FS-04` Strongly Typed Identifiers (強型付け識別子)**:
  エンティティ ID に生の `string` や裸の `Guid` を使わず、`[VKStronglyTypedId]` 属性を用いた `readonly record struct` でラップします（ワイヤー上は GUID 文字列としてシリアライズ）。

```csharp
// [FS-04] Strongly Typed Identifier definition (SG-backed)
[VKStronglyTypedId]
public readonly partial record struct TenantId(Guid Value);
```

- **`FS-05` Time**: ワイヤー上のタイムスタンプは明示的オフセット付き ISO 8601 とし、DB 永続化はすべて UTC（`DateTimeOffset`）で行います。時刻取得は必ず注入された `TimeProvider` を通じて行います。
- **`FS-06` Cancellation, Timeout & Idempotency**: `CancellationToken` をエンドツーエンドで伝搬させます。自動リトライは冪等な読み取り、または決定論的な冪等キーを伴う書き込みにのみ適用します。HTTP 429 応答時は `Retry-After` ヘッダーを付与します。
- **`FS-07` Identity and Tenant Context**: テナントとユーザー識別子は検証済みトークンクレームから境界で解決します。リクエスト DTO 毎に `TenantId` フィールドを手渡ししたり、未検証のクライアントヘッダーを信頼したりすることは厳禁です。
- **`FS-08` Pagination and Query Contracts**: リスト取得はすべて上限付きの統一ページネーション封筒（`VKPagedResult<T>`）を使用し、無制限のリスト返却エンドポイントを禁止します。
- **`FS-09` & `FS-13` Serialization & Evolution**: JSON キーは `camelCase`、Enum は文字列としてシリアライズします。公開ライブラリの型には `VK` プレフィックスを付与します。
- **`FS-10` Realtime Envelope**: WebSocket / SSE / SignalR メッセージはバージョン付きの共通エンベロープ（`type`, `version`, `correlationId`, `payload`, `timestamp`）を共有します。
- **`FS-11` & `FS-12` Observability & Zero-Trust Data**: W3C `traceparent` を伝搬させ、外部ペイロードや構造化 LLM 出力は使用前にスキーマ検証を必須とします。

