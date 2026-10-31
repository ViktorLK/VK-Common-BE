# Task: モジュール横断データ辞書・契約カタログの策定 (Data Dictionary & Contract Catalog Blueprint)

# Role

あなたは、`VK.Blocks` のワイヤー契約（`FS-01`〜`FS-05`）、エラーモデル（`CS.01`, `FS-02`）、オブザーバビリティ語彙（`BB.04`, `OR.01`）、および DB スキーマガバナンス（`CS.08`）を統括するデータ＆契約アーキテクトです。

# Goal

`docs/05-DataDictionary/` において、自動生成される `Enums.md`（`VK.Tools.DocsGenerator` 出力）を補完し、フロントエンド・下流マイクロサービス・運用基盤が参照する **Single Source of Truth（単一の真実の源）** として、以下の4つの公開契約カタログを定義・維持します。

1. **Error Code Catalog (`CS.01`, `FS-02`)**: 不変のエラーコード定数と RFC 7807 / HTTP ステータスマッピング
2. **Enum & Wire Value Rules (`FS-01`, `FS-09`, `FS-13`)**: 列挙型の互換性ルールと自動生成連携
3. **Diagnostics & Telemetry Semantic Dictionary (`BB.04`, `OR.01`, `FS-11`)**: ActivitySource / Meter 名および OpenTelemetry タグキー
4. **Persistence Schema & Entity Governance (`CS.08`, `OR.02`)**: 永続化を伴う BuildingBlock のテーブル・カラム・インデックス規約

# Constraints & Language

1. **出力先**: `docs/05-DataDictionary/{Module}-Contracts.md`（または全体横断カタログ）
2. **不変性 (`ARCH-07`, `FS-02`)**: 一度公開されたエラーコード文字列、Telemetry タグ名、Enum 値のリネーム・削除は Breaking Change として扱い、必ず ADR（`DL.03`）を伴うこと。
3. **出力言語**: ビジネスIT日本語（識別子・コード・見出しは英語併記、`BASE-09` 準拠）。

---

# Data Dictionary Structure (Must Follow)

```markdown
# 📖 Data Dictionary & Contract Catalog: `VK.Blocks.{ModuleName}`

> **Governance**: 本ドキュメントに記載されたエラーコード、テレメトリタグ、およびスキーマ定義は Level 1 公開契約（`ARCH-07`）であり、後方互換性のない変更には新バージョンの発行と ADR が必須です。

---

## 1. 🚨 Error Code Catalog (`CS.01`, `FS-02`)

> 命名規則: `{Module}.{Feature}.{Reason}`（Backend `VK{Feature}Errors` 定数と Frontend `as const` マッピングで完全一致させること）

| Error Constant (C#) | Wire Error Code (`string`) | `VKErrorType` / Category | HTTP Status | Description & Remediation (発生条件と対処) |
| :--- | :--- | :--- | :---: | :--- |
| `VK{Feature}Errors.InvalidInput` | `{Module}.{Feature}.InvalidInput` | `Validation` | `400` | 入力パラメータ不備。フィールド単位のエラー詳細を確認 |
| `VK{Feature}Errors.Unauthorized` | `{Module}.{Feature}.Unauthorized` | `Unauthorized` | `401` | 認証トークン欠落または期限切れ |
| `VK{Feature}Errors.Forbidden` | `{Module}.{Feature}.Forbidden` | `Forbidden` | `403` | テナント不一致または権限不足 (`OR.02`, `FS-07`) |
| `VK{Feature}Errors.NotFound` | `{Module}.{Feature}.NotFound` | `NotFound` | `404` | 指定された型安全な ID（Strongly-typed ID）のリソースが存在しない |
| `VK{Feature}Errors.Conflict` | `{Module}.{Feature}.Conflict` | `Conflict` | `409` | 楽観的排他制御（ConcurrencyStamp）競合または一意制約違反 |
| `VK{Feature}Errors.ExternalFailure` | `{Module}.{Feature}.ExternalFailure` | `Infrastructure` | `502` / `503` | 依存する外部／下流サービスのタイムアウトまたはサーキットブレーカー開放 |

---

## 2. 🔢 Enum & Wire Serialization Rules (`FS-01`, `FS-09`, `FS-13`)

> ⚠️ **注意**: 個別の Enum メンバー定義は `VK.Tools.DocsGenerator` によって [`Enums.md`](./Enums.md) および [`enums.manifest.json`](./enums.manifest.json) に自動抽出されます。手動で重複テーブルを作成しないでください。

- **Wire Format**: JSON シリアライズ時は文字列（camelCase / PascalCase 規約準拠）として伝送し、クライアント側は未知の Enum 値を受信してもクラッシュしないフォールバック処理を持つこと（`FS-09`）。
- **Evolution Rule**: 既存メンバーの整数値変更・名前変更は禁止（Breaking Change）。新規メンバーの末尾追加（Additive）のみ同一メジャーバージョン内で許容。

---

## 3. 📡 Diagnostics & Telemetry Dictionary (`BB.04`, `ARCH-12`, `FS-11`)

> `VK{ModuleName}DiagnosticsConstants` で公開される OpenTelemetry の計装契約：

### 3.1 Instrumentation Identity
- **ActivitySource / Meter Name**: `VK.Blocks.{ModuleName}`（`[VKBlockDiagnostics]` により自動バインド）

### 3.2 Semantic Tags & Metrics

| Constant Name | Wire Tag / Metric Key | Type | PII Scrubbed | Description |
| :--- | :--- | :---: | :---: | :--- |
| `TagTenantId` | `vk.tenant.id` | `string` | ✅ Safe | 実行コンテキストのテナント識別子 |
| `TagOperation` | `vk.operation.name` | `string` | ✅ Safe | 実行中のスライス操作名 |
| `TagErrorCode` | `vk.error.code` | `string` | ✅ Safe | 失敗時の `VKError.Code` (`CS.01`) |
| `MetricDuration` | `vk.{module}.duration.ms` | `Histogram<double>` | ✅ Safe | 操作の所要時間（ミリ秒） |

---

## 4. 🗄️ Persistence Schema Dictionary (`CS.08`, `OR.02` — 該当モジュールのみ)

> データベースエンティティを持つ BuildingBlock（Outbox, Idempotency, Persistence, Auditing 等）の物理スキーマ契約：

### Table: `{TableName}`
- **Multi-Tenancy**: `TenantId` (`NOT NULL`, EF Core Global Query Filter 適用必須)
- **Soft Delete / Audit**: `IsDeleted` (`ISoftDelete`), `CreatedAtUtc` / `UpdatedAtUtc` (`IAuditable`, `TimeProvider` 注入)

| Column Name | DB Type | Nullable | MaxLength / Precision | Constraint / Index (`IX_` / `FK_` / `PK_`) |
| :--- | :--- | :---: | :---: | :--- |
| `Id` | `uuid` / `uniqueidentifier` | `NOT NULL` | - | `PK_{TableName}` (Strongly-Typed ID `FS-04`) |
| `TenantId` | `varchar(64)` | `NOT NULL` | `64` | `IX_{TableName}_TenantId` |
| `ConcurrencyStamp` | `varchar(64)` | `NOT NULL` | `64` | Optimistic Concurrency Token |
```

