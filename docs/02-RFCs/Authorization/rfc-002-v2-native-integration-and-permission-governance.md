# RFC-002: V2 Native Integration, Definition-First Governance & Hash Sync

- **Status**: ✅ Approved (Implemented Evolution Wave 2)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Authorization`, `VK.Tools.SourceGenerators`
- **Related ADRs**:
  - [ADR-006: Modernize Dynamic Policy Evaluation with IAuthorizationRequirementData](/docs/04-ArchitectureDecisionRecords/Authorization/adr-006-iauthorizationrequirementdata.md)
  - [ADR-007: Definition-First Permission Discovery Strategy](/docs/04-ArchitectureDecisionRecords/Authorization/adr-007-definition-first-permission-discovery-strategy.md)
  - [ADR-008: Hash-Based Metadata Synchronization for Startup Performance](/docs/04-ArchitectureDecisionRecords/Authorization/adr-008-hash-based-metadata-synchronization.md)
  - [ADR-009: Permission Naming and Isolation Strategy](/docs/04-ArchitectureDecisionRecords/Authorization/adr-009-permission-naming-and-isolation-strategy.md)
  - [ADR-010: Standardization of Authorization Vertical Slice Structure and Delegated Registration](/docs/04-ArchitectureDecisionRecords/Authorization/adr-010-standardization-of-authorization-vertical-slice-structure-and-delegated-registration.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第2波着手前の課題 (Wave 2 Pain Points)**:
  - **文字列ベースポリシー解析の脆弱性と観測性の欠如 (`ADR-006`)**: 第 1 波の `PermissionPolicyProvider` は文字列パース（`"Permission:Users.Read"`）に依存しており、ASP.NET Core の標準ポリシー機構との間に冗長な変換処理が存在した。また、文字列ポリシーは実行時評価がブラックボックス化し、「どの権限やロールが原因で認可が拒絶されたか」をタグ付けした詳細なテレメトリ・メトリクスを収集できなかった。
  - **受動的発見によるメタデータ欠落とコード・DB 乖離 (`ADR-007`)**: コントローラーに文字列を記述する受動的発見（Passive Discovery）では、権限の「表示名（DisplayName）」や「説明（Description）」といった管理用メタデータをコードから抽出できず、データベースに保存された権限マスタとコードの実態が容易に乖離していた。
  - **起動時 DB 同期の $O(N)$ 負荷と起動遅延 (`ADR-008`)**: コードと DB の同期を実現しようとした際、起動するたびに数百〜数千件の権限定義を全量比較・更新すると、DB ロックや I/O オーバーヘッドが発生し、特にコンテナ環境におけるスケールアウト時の起動速度（Spin-up Delay）を著しく悪化させていた。
  - **マルチモジュール統合時の権限名競合 (`ADR-009`)**: 複数チームが開発する大規模システムにおいて、`Identity` や `Finance` といった異なるモジュール間で「Read」「Update」のような汎用的な権限名が衝突し、意図しない認可ポリシーの混同やコンパイルエラーが発生していた。
  - **中央集権的 DI 登録の肥大化とスライスカプセル化の不全 (`ADR-010`)**: `AuthorizationBlockExtensions.cs` が全機能スライスの内部クラスを直接参照・登録しており、機能追加のたびに中央ファイルが肥大化。また、機能スライス内のフォルダ構造がバラバラで、Source Generator のトリガー配置が不明確であった。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - マルチモジュールが統合される大規模エンタープライズ基盤において、.NET 8+ の最新標準に適合した「ゼロ文字パース・高観測性」な認可パイプラインと、コードを唯一の真実（SSOT）とする「自律的権限治理（Governance）」を確立するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **.NET 8+ `IAuthorizationRequirementData` ネイティブ統合 (`ADR-006`)**: カスタム属性に `IAuthorizationRequirementData` を実装し、中間ポリシー文字列のパースを完全に排除。属性から直接 `IAuthorizationRequirement` を生成して評価パイプラインに直結させ、失敗時の詳細タグ付きメトリクスを可能にする。
  - **定義優先（Definition-First）スキャン戦略 (`ADR-007`)**: `[GeneratePermissions]` と `[Display]` を付与した定数クラスを定義の唯一の事実ソース（SSOT）とし、Source Generator が強型付け属性（例: `[RequireIdentityUserReadAttribute]`）と `PermissionsCatalog.All` リストを自動合成する。
  - **FNV-1a ハッシュによる $O(1)$ 起動時メタデータ同期 (`ADR-008`)**: 全権限メタデータの確定的な 64-bit 非暗号化ハッシュ（`MetadataHash`）をコンパイル時に算出し、DB 側の保存指紋と比較。不一致時のみ差分同期を実行することで、99% の起動ケースで同期コストを実質ゼロにする。
  - **権限ネーミング・アイソレーション戦略 (`ADR-009`)**: モジュールクラス名をプレフィックスとして強制付与し、未管理の直接文字列属性には `Misc` プレフィックスを割り当てて視覚的な治理シグナルを提供する。
  - **垂直スライス 4 層構造の標準化と委譲型登録 (`ADR-010`)**: 各機能フォルダを `[Root]`（公開API）、`Metadata/`（SGトリガー）、`Internal/`（内部実装）、`Persistence/`（永続化抽象）に統一し、中央の DI 登録を各機能スライスへ委譲する。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **UI 権限管理画面のフロントエンド実装**: 画面表示やロール割り当て UI はフロントエンド層の責務とし、本ブロックはメタデータ契約と API のみに専念する。
  - **外部動的認可エンジン（OPA/Cedar）の統合**: 外部リモートエンジンへのオフロードは必要とされるコンシューマが存在しないため含めない（YAGNI）。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **精密な印章鋳造所と高速税関指紋 (The Precision Mint & Fingerprinted Registry)**
>
> かつての体制では、各部署（モジュール）が「入室許可：経理部」といった紙のメモ（文字列）を手書きで発行し、受付（PolicyProvider）がいちいち文字を解読して別の通行証に書き直していました。文字の掠れやタイポ、部署名の重複により混乱が日常茶飯事でした。
> 第 2 波の改革では、中央の**精密印章鋳造所（Source Generator）**が稼働を開始しました。各モジュールが金型（`[GeneratePermissions]` 定数）を定義すると、鋳造所が寸分違わぬ規格化された「専用刻印（強タイプ属性）」をコンパイル時に一括プレスします。
> さらに、鋳造所は全刻印の成分から**不可変の指紋（FNV-1a MetadataHash）**を算出し、台帳に刻み込みます。国境ゲート（アプリケーション起動時）では、税関職員が過去の指紋と現在の指紋を一瞬で照合し、指紋が一致していれば膨大な台帳検査（DB 同期）を即座にスキップして通過させます。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **精密印章金型 (Definition Mint Mold)** | `[GeneratePermissions]`, `[Display]` | Domain Constants | 権限の一意名・表示名・説明をコードレベルで宣言する SSOT（ADR-007） |
| **自動鋳造刻印 (Minted Strong Attributes)** | `Require{Module}{Permission}Attribute` | SG Generated | `IAuthorizationRequirementData` を実装した直接要件生成属性（ADR-006, 007） |
| **高速税関指紋 (Fingerprint Hash)** | `PermissionsCatalog.MetadataHash` (FNV-1a) | SG Generated | メタデータ変更の有無を $O(1)$ で確定判定する 64-bit 決定論的ハッシュ（ADR-008） |
| **特例未管理識別 (Misc Quarantine)** | `RequireMisc{Action}Attribute` | SG Generated | 定数定義外の文字列リテラルを隔離し、リファクタリングを促す治理マーカー（ADR-009） |
| **標準化セル (Standardized Slice Cell)** | `Features/{Feature}/[Root/Metadata/Internal]` | Vertical Slice | 独立した 4 層構造による完全なカプセル化と委譲型 DI 登録（ADR-010） |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Native Requirement Attributes & Sync Protocols (`Features/Permissions/`)

```csharp
namespace VK.Blocks.Authorization.Features.Permissions;

// .NET 8+ Native Authorization Attribute (ADR-006, AP.01)
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class VKAuthorizeAttributeBase : Attribute, IAuthorizationRequirementData
{
    public abstract IEnumerable<IAuthorizationRequirement> GetRequirements();
}

// Definition-First Metadata Contract (ADR-007, ADR-008)
public sealed record VKPermissionMetadata
{
    public required string Name { get; init; }
    public required string Module { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
}

// Generated Catalog Contract (ADR-007, ADR-008)
public static partial class PermissionsCatalog
{
    /// <summary>
    /// Deterministic FNV-1a 64-bit hash of all declared permissions metadata.
    /// </summary>
    public const ulong MetadataHash = 0x84222325CBF29CE4UL;

    /// <summary>
    /// Read-only snapshot of all discovered permissions in the assembly.
    /// </summary>
    public static readonly IReadOnlyCollection<VKPermissionMetadata> All = [];
}
```

### 4.2 Error Contract (`VKAuthorizationErrors`)

動的評価および属性解析における構造化エラー定義案（`CS.01`, `FS-02`）。

```csharp
namespace VK.Blocks.Authorization;

public static partial class VKAuthorizationErrors
{
    public static readonly VKError InvalidOperator = new(
        "Authorization.InvalidOperator",
        "The specified operator for dynamic policy evaluation is not recognized.",
        VKErrorType.Validation);

    public static readonly VKError AttributeNotFound = new(
        "Authorization.AttributeNotFound",
        "The requested dynamic attribute was not found on the user principal.",
        VKErrorType.NotFound);

    public static readonly VKError DynamicPoliciesFailed = new(
        "Authorization.DynamicPoliciesFailed",
        "The dynamic policies requirement was not met.",
        VKErrorType.Forbidden);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance & Zero Overhead (`ADR-006`, `ADR-008`)**:
  - `IAuthorizationRequirementData` の採用により、リクエスト毎の文字列結合やパース処理、不要な `PolicyProvider` インスタンス化のオーバーヘッドを完全撤廃。
  - FNV-1a ハッシュによる差分判定により、起動時の DB I/O を 99% 削減。コールドスタートおよび水平スケール時のレイテンシを最小化。
- **Hash Collision Risk & Mitigation (`ADR-008`)**:
  - 64-bit FNV-1a は衝突耐性が極めて高いものの、理論的確率はゼロではない。
  - **緩和策**: 開発・テスト環境において強制同期を行える `ForceSync` オプションを `IPermissionSyncOptions` に備え、指紋一致時でも明示的な再同期を可能とする。
- **Build Time vs Runtime Safety (`ADR-007`, `ADR-009`)**:
  - Incremental Generator パイプラインを厳格に最適化し、フルコンパイル依存を回避することでビルド時間への影響を極小化。コンパイル時のタイポ検出による開発生産性向上（DX）がビルドコストを大幅に上回る。
- **Alternatives Considered**:
  - *代替案 A: 起動時のリフレクション走査による DB 同期 (`ADR-007`, `ADR-008`)*
    - **却下理由**: 起動時間が数百ミリ秒〜数秒悪化し、クラウドネイティブ環境の高速起動要件に反するため。
  - *代替案 B: 外部 YAML/JSON ファイルによる権限定義 (`ADR-007`)*
    - **却下理由**: コード内の属性利用と設定ファイルの乖離が防げず、C# の型システムやインテリセンスの恩恵を受けられないため。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (IAuthorizationRequirementData Modernization)**:
  - `DynamicAuthorizeAttribute`, `AuthorizePermissionAttribute` への `IAuthorizationRequirementData` 実装と中間文字列ポリシー解析の廃止（`ADR-006`）。
- [x] **Step 2 (Definition-First SG Pipeline)**:
  - `[GeneratePermissions]` 定義パーサーの実装と強型付け属性の自動合成機能（`ADR-007`）。
- [x] **Step 3 (FNV-1a Fingerprint Sync Engine)**:
  - `MetadataHash` 定数生成と起動時の $O(1)$ 指紋差分同期アルゴリズムの実装（`ADR-008`）。
- [x] **Step 4 (Permission Governance & Misc Strategy)**:
  - モジュールプレフィックス強制ルールと未管理直接文字列に対する `Misc` 治理タグの付与（`ADR-009`）。
- [x] **Step 5 (Vertical Slice Normalization & Delegated DI)**:
  - 全機能スライスの 4 層構造標準化と中央登録クラスの分割委譲（`ADR-010`）。

