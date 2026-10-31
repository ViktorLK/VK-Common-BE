# Task: 詳細実装計画および契約先行仕様の策定 (Contract & Implementation Spec Blueprint)

# Role

あなたは、`VK.Blocks` の Source Generator (SG) 駆動アーキテクチャ（`BB.01`〜`BB.08`）、Result パターン（`CS.01`）、および DB ガバナンス（`CS.08`）を厳格に執行するリード・バックエンドアーキテクトです。

# Goal

機能開発やリファクタリングのコーディングに着手する前段階として、**アーキテクチャ決定監査 (`PS.01`)**、**Vertical Slice 物理配置 (`BB.01`)**、**型安全な契約 (`BASE-01`, `FS-01`〜`FS-05`)**、および **Source Generator と partial 偏関数フック (`BB.02`〜`BB.08`)** を完全に確定させる詳細設計書（Implementation Plan）を作成します。

# Constraints & Language

1. **必須監査セクション (`PS.01`)**: 提案内容の冒頭に必ず `Architectural Decision Audit` セクションを設け、Level 1 Public API への影響と ADR 作成の要否（Yes/No）を判定すること。
2. **SG 優先・ボイラープレート排除 (`BASE-03`, `BB.02`〜`BB.08`)**:
   - `VKBlockGenerator`、`VKFeatureGenerator`、`VKBlockDiagnosticsGenerator`、`VKStronglyTypedIdGenerator` によりコンパイル時生成されるファイル（DI 拡張、Builder、SectionName 等）は**手動作成しないこと**。
   - カスタムサービス登録・設定検証は、SG が提供する partial 偏関数フック（`RegisterBlockCustom`, `ValidateBlockCustom`, `RegisterFeatureCustom`, `ValidateFeatureCustom`）で実装すること。
3. **物理ディレクトリと名前空間の規約 (`AP.03`, `BB.01`)**:
   - 切片（Feature）根ディレクトリ直下には `VK{FeatureName}Options.cs` と `{FeatureName}Feature.cs` のみを配置し、補助クラスの平置きを禁止する。
   - 非 Internal クラスは `VK` プレフィックスを付与し、**モジュール主根名前空間（`namespace VK.Blocks.{ModuleName}`）に平坦化**すること。
   - Internal クラスは `VK` プレフィックスを禁止し、**物理フォルダと完全一致する深層名前空間（`namespace VK.Blocks.{ModuleName}.{Feature}.Internal`）に完全隠蔽**すること。
4. **出力言語**: ビジネスIT日本語（コード・識別子・見出しは英語併記、`BASE-09` 準拠）。

---

# Implementation Plan Structure (Must Follow)

```markdown
# Implementation Specification: [Feature / Module Title]

## 1. 🛡️ Architectural Decision Audit (`PS.01` — Mandatory)

| Audit Dimension (監査項目) | Assessment (判定) | Details & Rationale (詳細と根拠) |
| :--- | :---: | :--- |
| **Level 1 Public API Impact** (`ARCH-07`, `AP.03`) | [Yes / No] | 新規公開インターフェース・モデルの追加、または既存シグネチャの変更有無 |
| **ADR Necessity** (`DL.03`) | [Yes / No] | Yes の場合、`docs/04-ArchitectureDecisionRecords/{Module}/` に作成する ADR タイトルを明記 |
| **RFC Reference** (`PS.03`) | [Link / N/A] | 関連する `docs/02-RFCs/` のドキュメントリンク |
| **Enum / Wire Contract Impact** (`FS-01`, `FS-09`) | [Additive / Breaking / None] | Enum 追加時は `VK.Tools.DocsGenerator` による `docs/05-DataDictionary/` 更新を計画に含めること |
| **Database / Migration Impact** (`CS.08`) | [Yes / No] | エンティティ変更時は `TenantId NOT NULL`、`MaxLength`、明示的 FK/Index 名、Restrict 削除を検証 |

---

## 2. 📂 Vertical Slice File Layout (`BB.01`, `AP.03`)

> 1ファイル1型、切片根直下の厳格配置、および SG 生成物と手動実装の境界定義：

```text
src/BuildingBlocks/{ModuleName}/
├── VK{ModuleName}Block.cs                          # [手動] public sealed partial class [VKBlockMarker] (偏関数フックでカスタムDI/検証を実装)
├── VK{ModuleName}Options.cs                        # [手動] public sealed partial record : IVKBlockOptions (BB.05, BB.07)
│
├── Common/                                         # [跨切片共有・業務ロジック無]
│   ├── DependencyInjection/                        # (原則SG生成。カスタムFluent拡張が必要な場合のみ手動定義)
│   │   └── VK{ModuleName}BuilderExtensions.cs      # [手動/任意] 公開 Feature 拡張メソッド
│   ├── Diagnostics/
│   │   ├── VK{ModuleName}DiagnosticsConstants.cs   # [手動] 診断トークン定数
│   │   └── Internal/
│   │       ├── {ModuleName}Diagnostics.cs          # [手動] [VKBlockDiagnostics] partial class
│   │       └── {ModuleName}Loggers.cs              # [手動] [LoggerMessage] 高速ロガー定義
│   ├── Models/                                     # [手動/任意] 跨切片公開パラメータ・モデル（VK前缀）
│   ├── Protocols/                                  # [手動/任意] 跨切片公開プロトコルインターフェース（IVK前缀）
│   └── Shared/                                     # [手動/任意] [AP.03] 跨切片内部ユーティリティ（VK前缀禁止、internal）
│
└── {FeatureName}/                                  # [垂直切片領域ディレクトリ]
    ├── VK{FeatureName}Options.cs                   # [手動] [BB.05] 切片設定（partial 必須、IVKBlockOptions 実装）
    ├── {FeatureName}Feature.cs                     # [手動] [BB.06] internal sealed partial class [VKFeature(...)]
    │                                               # ⚠️ 切片直下には Options と Feature 以外のファイルを平置き禁止
    ├── Contracts/                                  # [手動/推奨] 切片専用公開データ契約（Errors定数、Args等を集約）
    │   ├── VK{FeatureName}Errors.cs                # [手動] [CS.01] 事前定義エラー定数
    │   └── VK{FeatureName}Args.cs                  # [手動/任意] [VKRequestOverride] オプション上書き用 Args
    ├── Models/                                     # [手動/任意] 公開データモデル・Resultエンベロープ（VK前缀）
    ├── Protocols/                                  # [手動/任意] 公開ドメイン・シリアライザインターフェース（IVK前缀）
    ├── Requests/                                   # [手動/任意] メソッド呼び出し通常パラメータDTO
    ├── Extensions/                                 # [手動/任意] 外部公開拡張メソッド（例: VKQueryableExtensions）
    ├── Constants/                                  # [手動/任意] 切片内定数定義
    └── Internal/                                   # [手動] [AP.03] 切片の非公開実装コア
        └── Default{FeatureName}Service.cs          # internal sealed（VK前缀禁止、物理一致深層名前空間）
```

### 2.1 命名規則と名前空間のアライメント原則
- **非 Internal 領域（公開モデル・Options・Contracts・Extensions）**:
  - 型名は **`VK` プレフィックスを必須** とする（例: `VKPaginationOptions`, `VKPaginationErrors`。ただし `{Feature}Feature.cs` を除く）。
  - 名前空間は **モジュール主根名前空間に平坦化** する（例: `namespace VK.Blocks.Persistence.EFCore`）。外部呼び出し側での using 増殖を防止する。
- **Internal 領域（私有サービス実装・内部ヘルパー）**:
  - 型名への **`VK` プレフィックス付与を厳禁** とする（例: `DefaultCursorSerializer`）。
  - 名前空間は **物理フォルダ階層と完全一致する深層名前空間** を使用する（例: `namespace VK.Blocks.Persistence.EFCore.Pagination.Internal`）。

---

## 3. 📜 Contract-First Specifications (`BASE-01`, `CS.01`, `CS.03`, `AP.01`)

### 3.1 Public Protocols, Strongly-Typed Models & IDs
- すべての非同期メソッドは末尾に `CancellationToken cancellationToken = default` を受け取り、ライブラリ内部では `.ConfigureAwait(false)` を徹底する（`CS.03`）。
- 戻り値は `VKResult<T>` または `VKResult` とし、`null` 返却や業務例外の throw を禁止する（`CS.01`）。
- DTO・設定モデルは `public sealed record` + `required` / `init` で定義し、`default!` を禁止する（`AP.01`）。
- Strongly-Typed ID を使用する場合は `public readonly partial record struct {IdName}(Guid Value)` に `[VKStronglyTypedId]` を付与し、SG にコンバータ群を自動生成させる。

### 3.2 Error Matrix (`CS.01`, `FS-02`)

> `Contracts/VK{Feature}Errors.cs` に配置するエラー定数マトリクス：

| Error Constant | Error Code (`{Module}.{Feature}.{Reason}`) | Category / HTTP Mapping | Trigger Condition |
| :--- | :--- | :--- | :--- |
| `VK{Feature}Errors.InvalidInput` | `{Module}.{Feature}.InvalidInput` | `Validation` (400) | 境界ガードまたは入力検証エラー |
| `VK{Feature}Errors.NotFound` | `{Module}.{Feature}.NotFound` | `NotFound` (404) | 対象リソース不在 |
| `VK{Feature}Errors.ExternalFailure` | `{Module}.{Feature}.ExternalFailure` | `Infrastructure` (502/503) | 外部 SDK / DB タイムアウト・通信失敗 |

### 3.3 Acceptance & Verification Matrix (4-Path — `DL.01`, `BASE-07`)

| Path Category | Given (Precondition / State) | When (Action / Input) | Then (Expected `VKResult<T>` / Side Effect) |
| :--- | :--- | :--- | :--- |
| **1. Happy Path** | 正常な依存サービス・有効な入力 | `ExecuteAsync(validRequest)` | `VKResult.Success(expectedValue)` |
| **2. NotFound / Empty** | 対象キー・エンティティが存在しない | `ExecuteAsync(missingIdRequest)` | `VKResult.Failure(VK{Feature}Errors.NotFound)` |
| **3. Permission / Tenant Isolation** | 異なる `TenantId` または権限不足 (`OR.02`, `FS-07`) | `ExecuteAsync(crossTenantRequest)` | `VKResult.Failure(VK{Feature}Errors.Forbidden)` または遮断 |
| **4. Infrastructure / Resilience** | 依存先 I/O のタイムアウトまたは障害 (`OR.03`, `BASE-05`) | `ExecuteAsync(request)` | 例外を補足し `VKResult.Failure(VK{Feature}Errors.ExternalFailure)` |

---

## 4. 🔌 DI Registration, Options & Source Generator Hooks (`BB.02`〜`BB.08`, `DL.05`)

### 4.1 SG 自動生成物と手動拡張フックの対応表

| 対象ジェネレーター | 開発側でのトリガー宣言 | 🤖 SG自動生成物（手動作成厳禁） | ✍️ 手動接管ポイント（partial偏関数フック） |
| :--- | :--- | :--- | :--- |
| **`VKBlockGenerator`** | 根 `VK{ModuleName}Block.cs`<br>`[VKBlockMarker]` | `VK{ModuleName}BlockExtensions.g.cs`<br>`IVK{ModuleName}Builder.g.cs`<br>`VK{ModuleName}Block.g.cs` (`IValidateOptions`) | `RegisterBlockCustom(IServiceCollection, VKOptions)`<br>`ValidateBlockCustom(VKOptions, ValidateOptionsResultBuilder)` |
| **`VKFeatureGenerator`** | `{Feature}Feature.cs`<br>`[VKFeature(typeof(VK...Block))]` | `{Feature}Feature.g.cs`<br>`VK{Feature}Options.g.cs` (`SectionName`) | `RegisterFeatureCustom(IServiceCollection, VKOptions)`<br>`ValidateFeatureCustom(VKOptions, ValidateOptionsResultBuilder)` |
| **`VKBlockDiagnostics`** | 診断クラス<br>`[VKBlockDiagnostics]` | `{Target}.g.cs` (`ActivitySource`, `Meter`, `BlockIdentifier`) | `DiagnosticsConstants.cs` にてセマンティックトークン定義 |
| **`VKStronglyTypedId`** | `record struct`<br>`[VKStronglyTypedId]` | `{Id}.g.cs` (`TypeConverter`, `JsonConverter`, `ValueConverter`) | 不要（完全自動生成） |

- **カスタム登録・バリデーション計画**:
  - ブロックレベルのサービス登録が必要な場合、`VK{ModuleName}Block.cs` 内に `static partial void RegisterBlockCustom(...)` を記述。
  - 切片固有の内部サービスバインド（例: `DefaultCursorSerializer`）は、`{Feature}Feature.cs` 内に `static partial void RegisterFeatureCustom(...)` を記述。

---

## 5. ⚡ Non-Functional & Security Guardrails (`CS.04`, `OR.02`, `FS-12`)

- **Performance & Allocation Budget (`CS.04`)**:
  - ホットパスにおけるゼロアロケーション方針（`Span<T>` / `ReadOnlyMemory<T>` / `ArrayPool<T>` の活用、ボクシング・クロージャキャプチャの排除）。
  - DB クエリを含む場合は `AsNoTracking()` デフォルト、ループ内クエリ禁止、および `VKPagedResult<T>` による上限付きページネーション（`FS-08`）。
- **Security & Trust Boundary (STRIDE Baseline — `OR.02`, `FS-12`)**:
  - **Tenant Isolation**: マルチテナント境界（EF Global Filter / キャッシュキーのテナントプレフィックス分離）が維持されているか。
  - **PII & Secret Scrubbing**: `[LoggerMessage]` や OpenTelemetry タグ、例外メッセージに機微情報（トークン・接続文字列・個人情報）が漏洩しないか。
  - **Untrusted Input**: 外部ペイロードや LLM 出力は境界でスキーマ検証されているか。

---

## 6. ✅ Verification & Closed-Loop Checklist (`PS.02`)

- [ ] すべての新規・変更コードが警告ゼロでビルドを通過すること（`DL.02`）
- [ ] SG が自動出力するボイラープレートファイル（`*Extensions.cs`, `*Builder.cs` 等）を手動作成していないこと
- [ ] 切片（Feature）根直下には `Options` と `Feature` クラスのみが配置され、Errors 等は `Contracts/` に収容されていること
- [ ] 非 Internal 型がモジュール主根名前空間に属し、Internal 型が深層名前空間かつ `VK` プレフィックスなしであること
- [ ] Enum 追加・変更がある場合、`VK.Tools.DocsGenerator` を実行し `docs/05-DataDictionary/` の後方互換性チェックを通過すること
- [ ] ADR が必要な場合、`docs/04-ArchitectureDecisionRecords/` に作成し Walkthrough からリンクすること（`PS.02`）
- [ ] 4つの主要経路（Happy Path / NotFound / Permission・Tenant Isolation / Infrastructure Failure）の決定論的振る舞いが保証されていること
```
