# RFC-003: V3 Industrial Standardization, Sliced Registration & Multidimensional Entitlements

- **Status**: ✅ Approved (Implemented Evolution Wave 3)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Authorization`, `VK.Blocks.MultiTenancy.Authorization`, `VK.Tools.SourceGenerators`
- **Related ADRs**:
  - [ADR-011: Refactor Authorization Building Block Structure and Naming](/docs/04-ArchitectureDecisionRecords/Authorization/adr-011-refactor-authorization-building-block-structure-and-naming.md)
  - [ADR-012: Authorization Library Architecture Normalization](/docs/04-ArchitectureDecisionRecords/Authorization/adr-012-authorization-library-architecture-normalization.md)
  - [ADR-013: Feature-Sliced Modular Registration Pattern](/docs/04-ArchitectureDecisionRecords/Authorization/adr-013-feature-sliced-modular-registration-pattern.md)
  - [ADR-014: Automated Block Identity & Metadata via Source Generation](/docs/04-ArchitectureDecisionRecords/Authorization/adr-014-automated-block-identity--metadata-via-source-generation.md)
  - [ADR-015: Tenant-Level Capability Authorization (Entitlements)](/docs/04-ArchitectureDecisionRecords/Authorization/adr-015-tenant-level-capability-authorization-entitlements.md)
  - [ADR-016: Authorization Block Normalization and Core Protocol Standardization](/docs/04-ArchitectureDecisionRecords/Authorization/adr-016-authorization-block-normalization-and-core-protocol-standardization.md)
  - [ADR-017: Rename Feature Settings to Options and Align with Toggleable Contract](/docs/04-ArchitectureDecisionRecords/Authorization/adr-017-rename-feature-settings-to-options-and-align-with-toggleable-contract.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第3波着手前の課題 (Wave 3 Pain Points)**:
  - **名前空間の断片化と利用側 `using` の肥大化 (`ADR-011`)**: `Features/` 配下に深くネストされた名前空間により、下流アプリケーションが認可機能を利用する際に多数の冗長な `using` 宣言を強いられていた。
  - **全社規格 Blueprint (BB.01–BB.05) との不整合 (`ADR-012`)**: マーカーパターンの未実装、内部型のカプセル化深度の不足、およびプレフィックス規約の不徹底により、他の BuildingBlock（Core, Authentication）との統一的運用が阻害されていた。
  - **モノリシック登録による機能トグルの困難性 (`ADR-013`)**: 全機能の DI 登録が単一メソッドに集約されており、不要な認可機能（例: 役職ランクや業務時間制限）のオプトアウトや、特定機能のみの単体テストが困難であった。
  - **識別子・テレメトリの手動管理による不整合 (`ADR-014`)**: メトリクス名や ActivitySource 名、バージョン情報が手動文字列で保守され、タイポや分散トレースの集約分断が生じていた。
  - **テナント契約能力とユーザー権限の混同 (`ADR-015`)**: 「ユーザーが何をできるか（Permissions）」と「テナントの契約プランで何が有効か（Tenant Capabilities）」が混同され、コントローラー内に `if (tenant.HasFeature("Analytics"))` のようなアドホックな記述が散在していた。
  - **汎用エバリュエータによる DX の低下と設定マージの冗長性 (`ADR-016`)**: 汎用の `IVKEvaluator<T>.EvaluateAsync` のみでは各機能固有のセマンティクスが埋没し、また動的引数と静的設定の合成（AP.05）において `args?.Prop ?? options.Prop` の反復記述が発生していた。
  - **"Settings" と "Options" の用語乖離と多重継承冗長性 (`ADR-017`)**: インターフェース名（`IVKRoleSettings`）と具象クラス名（`VKRoleOptions`）の用語の食い違いが生じ、可変トグル機能において `IVKBlockOptions` と `IVKToggleableBlockOptions` の多重記述が発生していた。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - `VK.Blocks` が全社標準フレームワーク（Industrial DNA v2）へと昇格するにあたり、ゼロリフレクション、完璧な垂直切片、明快な多次元認可（ユーザー権限 × テナント能力）、および完全対称な Options-Overrides 契約モデルを完備した最終工業規格を確立するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **工業規格化と名前空間フラット化 (`ADR-011`, `ADR-012`, `AP.03`)**: 公開 API をすべてルート名前空間 `VK.Blocks.Authorization` に集約し、全公開型に `VK` プレフィックスを強制。内部実装は `Internal/` サブディレクトリへ隔離し `internal sealed` とする。
  - **`[VKBlockMarker]` によるメタデータの自動合成 (`ADR-012`, `ADR-014`, `BB.02`)**: `VKAuthorizationBlock` をマーカーとし、Source Generator が URN（`"VK.Blocks.Authorization"`）とアセンブリバージョン、テレメトリ識別子をコンパイル時に自動生成する。
  - **流暢なモジュラー登録ビルダー (`ADR-013`, `BB.03`, `BB.06`)**: `IVKAuthorizationBuilder` を導入し、各垂直スライスが独立して `Check-Self ➡️ Options ➡️ Mark-Self ➡️ Validate ➡️ Services` の 8 段階べき等登録シーケンスを完結させる。
  - **多次元アクセス制御としての Entitlements 認可 (`ADR-015`)**: テナントの契約プラン・機能有効化を属性ベースで制御する `[VKRequireTenantFeature]` を導入し、ユーザー権限（PBAC）とテナント能力の関心を完全分離する。
  - **Semantic-Priority パターンと `MergeWith` 拡張 (`ADR-016`, `AP.05`)**: ドメイン特有のメソッド（例: `IsWithinWorkingHoursAsync`）を公開しつつ、汎用インターフェースは明示的インターフェース実装で隠蔽。`MergeWith` 拡張メソッドにより設定マージを宣言的かつゼロオーバーヘッド化する。
  - **Options 語彙への完全統一と対称階層化 (`ADR-017`, `BB.05`)**: "Settings" を完全廃止し、静的 `IVKXxxOptions` と動的 `IVKXxxOverrides` の対称構造を確立。開閉可能機能は `IVKToggleableBlockOptions` を直接継承して二重定義を根絶する。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **課金決済・請求サブスクリプションエンジン**: テナントの契約プラン変更や決済処理自体はアプリケーションドメインの責務とし、本ブロックは契約状態の認可判定のみに専念する。
  - **動的プラグインの実行時アセンブリ動的ロード**: コンパイル時決定論（Compile-Time First, `BASE-03`）を堅持し、実行時コードローディングは採用しない。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **近代規格化プラントと多層認可グリッド (The Industrial Plant & Multidimensional Grid)**
>
> 従来のシステムは、職人が現場でパイプを切り貼りして建てた雑居ビルのような状態でした。配管の太さ（インターフェース）もまちまちで、「テナントの利用資格」と「個人の役職」が同じメモ帳に雑多に書かれていました。
> 第 3 波の近代工業化により、認可システムは**最新鋭の規格化プラント（Industrial Blueprint）**へと生まれ変わりました。
> すべての建材には標準規格印（`VK` プレフィックス）が刻まれ、中央配電盤（`VKAuthorizationBlock`）にはシリアル番号と規格書（URN・テレメトリ）が自動レーザー刻印されます。
> 各フロア（機能スライス）はモジュール式ユニット（`IVKAuthorizationBuilder`）として製造され、必要なユニットだけをカチッと接続できます。さらに、アクセス制御は**多次元認可グリッド（Multidimensional Grid）**へと進化し、「ビル全体のフロア契約が有効か（Entitlements: テナント機能）」という大枠の電源供給と、「個々の部屋を開ける鍵を持つか（PBAC: ユーザー権限）」という個別錠前が整然と二重連動する体制を確立しました。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **中央配電盤とレーザー刻印 (Central Panel & Stamped Identity)** | `[VKBlockMarker]`, `VKAuthorizationBlock` | Module Root | ブロック識別子、バージョン、診断メタデータを自動合成する起点（ADR-012, 014） |
| **モジュラー接続ユニット (Modular Plug Unit)** | `IVKAuthorizationBuilder`, `AddVKRoles()`, etc. | Common DI | スライスごとの独立したチェーン登録とべき等性保証（ADR-013） |
| **フロア電源ブレーカー (Floor Capability Breaker)** | `[VKRequireTenantFeature]`, `TenantFeatureAuthorizationHandler` | Entitlements Slice | テナント契約に基づくシステム機能の利用可否を属性ベースで一括制御（ADR-015） |
| **精密直感ダイヤル (Semantic-Priority Dial)** | `IVKWorkingHoursEvaluator : IVKEvaluator<T>` | Feature Protocols | ドメイン固有のメソッドを前面に出し、汎用規格を明示的実装で両立（ADR-016） |
| **完全対称配線規格 (Symmetric Options Wiring)** | `IVKXxxOptions : IVKToggleableBlockOptions`, `VKXxxArgs` | Options / Models | 静的設定と動的引数の語彙統一および二者択一階層契約（ADR-017） |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Fluent Builder & Entitlements Protocols (`Common/`, `Entitlements/`)

```csharp
namespace VK.Blocks.Authorization;

// Public Builder API: 宣言的チェーン登録契約 (ADR-013, BB.06)
public interface IVKAuthorizationBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}

// Entitlements Attribute: テナント能力認可属性 (ADR-015, FS-07)
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class VKRequireTenantFeatureAttribute : Attribute, IAuthorizationRequirementData
{
    public string FeatureId { get; }

    public VKRequireTenantFeatureAttribute(string featureId)
    {
        FeatureId = VKGuard.NotNullOrWhiteSpace(featureId);
    }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new VKTenantFeatureRequirement(FeatureId);
    }
}

// Semantic-Priority Protocol: ドメイン特化と汎用の両立契約 (ADR-016, CS.01)
public interface IVKWorkingHoursEvaluator : IVKEvaluator<VKWorkingHoursArgs>
{
    /// <summary>
    /// Domain-specific explicit evaluation method.
    /// </summary>
    ValueTask<VKResult<bool>> IsWithinWorkingHoursAsync(
        ClaimsPrincipal user, 
        VKWorkingHoursArgs? args = null, 
        CancellationToken cancellationToken = default);

    // Generic fallback via Explicit Interface Implementation
    ValueTask<VKResult<bool>> IVKEvaluator<VKWorkingHoursArgs>.EvaluateAsync(
        ClaimsPrincipal user, 
        VKWorkingHoursArgs? args, 
        CancellationToken cancellationToken)
        => IsWithinWorkingHoursAsync(user, args, cancellationToken);
}

// Toggleable Options Contract: 二者択一階層契約 (ADR-017, BB.05)
public interface IVKRoleOptions : IVKToggleableBlockOptions
{
    string? RoleClaimType { get; init; }
}

[VKOptions]
public sealed partial record VKRoleOptions : IVKRoleOptions
{
    public bool Enabled { get; init; } = true;
    public string? RoleClaimType { get; init; }
}
```

### 4.2 Error Contract (`VKAuthorizationErrors`)

Entitlements および最新の標準化に対応するエラーコード体系（`CS.01`, `FS-02`）。

```csharp
namespace VK.Blocks.Authorization;

public static partial class VKAuthorizationErrors
{
    public static readonly VKError FeatureDenied = new(
        $"{VKAuthorizationBlock.BlockName}.FeatureDenied",
        "The required tenant feature is not enabled for the authenticated user's tenant.",
        VKErrorType.Forbidden);

    public static readonly VKError OutOfWorkingHours = new(
        $"{VKAuthorizationBlock.BlockName}.OutOfWorkingHours",
        "Access is restricted to specific working hours defined in the policy.",
        VKErrorType.Forbidden);

    public static readonly VKError InsufficientRank = new(
        $"{VKAuthorizationBlock.BlockName}.InsufficientRank",
        "The user's rank is lower than the minimum required rank.",
        VKErrorType.Forbidden);

    public static readonly VKError RoleDenied = new(
        $"{VKAuthorizationBlock.BlockName}.RoleDenied",
        "The user does not belong to any of the required roles for this action.",
        VKErrorType.Forbidden);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance & Zero Allocation (`ADR-016`, `CS.04`)**:
  - `MergeWith` パターンにより、ローカル引数（Args）とグローバル設定（Options）の合成をインライン展開可能な構造体操作で実現。ヒープ割り当て（Heap Allocation）をゼロに抑制。
  - エバリュエータのシグネチャに `ValueTask<VKResult<bool>>` を採用し、キャッシュヒット時の同期完了パスでの Task オブジェクト割り当てを完全排除。
- **Multi-Tenant Dependency Decoupling (`ADR-015`)**:
  - `Entitlements` 機能は `ITenantProvider` に依存するが、インターフェースを介した抽象化を維持し、`VK.Blocks.MultiTenancy` が未導入の環境では該当機能をオプトインしない構成を可能とした。
- **Cognitive Consistency & Migration (`ADR-017`)**:
  - "Settings" 語彙を完全に破棄し "Options" に統一したことで、当初は既存コードへの破壊的変更（Breaking Change）を伴うが、コンパイラエラーとして即時検知され、長期的には設計の対称性と開発効率が最大化される。
- **Alternatives Considered**:
  - *代替案 A: テナント機能チェックをビジネスロジック内で命令的に実行 (`ADR-015`)*
    - **却下理由**: 各コントローラーに `if` 文が散在して漏れが発生し、宣言的セキュリティの監査性が損なわれるため。
  - *代替案 B: 汎用 `IVKEvaluator<T>` のみを提供 (`ADR-016`)*
    - **却下理由**: 直感的なドメインメソッドが失われ、開発者が何を評価しているのかがコードから把握しづらくなるため。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Structure & Namespace Flattening)**:
  - ルート名前空間への集約、公開型の `VK` プレフィックス化、および `Internal/` フォルダへの深層カプセル化（`ADR-011`, `ADR-012`）。
- [x] **Step 2 (Block Marker & SG Diagnostics)**:
  - `[VKBlockMarker]` 部分クラスの定義と Source Generator による URN・バージョン定数の自動合成（`ADR-012`, `ADR-014`）。
- [x] **Step 3 (Modular Sliced Registration Pipeline)**:
  - `IVKAuthorizationBuilder` の導入と各機能スライスにおける 8 段階べき等登録シーケンスの実装（`ADR-013`）。
- [x] **Step 4 (Tenant Capability Entitlements)**:
  - `[VKRequireTenantFeature]`, `VKTenantFeatureRequirement`, および `TenantFeatureAuthorizationHandler` の実装（`ADR-015`）。
- [x] **Step 5 (Semantic-Priority & MergeWith Normalization)**:
  - エバリュエータへの明示的インターフェース実装の適用、および `MergeWith` 拡張メソッドによる AP.05 設定合成の実装（`ADR-016`）。
- [x] **Step 6 (Unified Options-Overrides Contract)**:
  - "Settings" 語彙の完全撤廃、`IVKToggleableBlockOptions` 契約階層化、および Source Generator の解析ルール一本化（`ADR-017`）。

