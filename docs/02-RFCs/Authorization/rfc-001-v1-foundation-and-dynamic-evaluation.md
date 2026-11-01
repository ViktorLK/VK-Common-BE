# RFC-001: V1 Foundation, Tenant Isolation & Dynamic Evaluation

- **Status**: ✅ Approved (Implemented Evolution Wave 1)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Authorization`
- **Related ADRs**:
  - [ADR-001: Centralize Tenant Isolation in Authorization Pipeline](/docs/04-ArchitectureDecisionRecords/Authorization/adr-001-centralize-tenant-isolation-in-authorization-pipeline.md)
  - [ADR-002: Establish Dynamic Policy Generation for Permission-Based Authorization](/docs/04-ArchitectureDecisionRecords/Authorization/adr-002-establish-dynamic-policy-generation-for-permission-based-authorization.md)
  - [ADR-003: Adopt Attribute Evaluator Pattern for Extensible Custom Requirements](/docs/04-ArchitectureDecisionRecords/Authorization/adr-003-adopt-attribute-evaluator-pattern-for-extensible-custom-requirements.md)
  - [ADR-004: Introduce Source Generators for Authorization Handlers and Permissions Catalog](/docs/04-ArchitectureDecisionRecords/Authorization/adr-004-introduce-source-generators-for-authorization-handlers-and-permissions-catalog.md)
  - [ADR-005: Refactor Authorization Evaluation to Specialized Permission Evaluator Pattern and Retain Automation](/docs/04-ArchitectureDecisionRecords/Authorization/adr-005-refactor-authorization-evaluation-to-specialized-permission-evaluator-pattern-and-retain-automation.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第1波着手前の課題 (Wave 1 Pain Points)**:
  - **テナント境界の欠如と横断データ漏洩リスク (`ADR-001`)**: マルチテナント SaaS 環境において、各コントローラーやビジネスロジックで個別に手動の `TenantId` 検証を行っていたため、開発者のバリデーション忘れが致命的なテナント間データ漏洩（Cross-Tenant Data Leakage）に直結していた。
  - **ロール爆発と起動時静的登録の限界 (`ADR-002`)**: RBAC（Role-Based Access Control）の硬直性を克服するため PBAC（Permission-Based Access Control）の導入が必要となったが、ASP.NET Core の標準手法では起動時に数千件もの静的ポリシー（`options.AddPolicy(...)`）の登録が強制され、Startup 時間の激化とメモリ浪費を招いていた。
  - **認可ロジックの散在と重複 (`ADR-003`)**: 「業務時間内か」「最低役職ランクを満たすか」といった多様な制約条件が各アクションメソッド内に if 文やカスタムフィルタとして散在し、DRY 原則とアーキテクチャの純粋性を著しく損なっていた。
  - **手動 DI 登録のミスとマジックストリング脆弱性 (`ADR-004`)**: 開発者が新しい認可ハンドラーを DI コンテナへ手動登録（`services.AddScoped<IAuthorizationHandler, ...>`）し忘れ、認可がすり抜けるセキュリティリスクが存在した。また、権限名のハードコードによるタイポが実行時まで検知できなかった。
  - **汎用インターフェースの責務曖昧さと Result Pattern 非準拠 (`ADR-005`)**: 汎用の `IVKAuthorizationHandler` は実質的に権限チェックに特化していたにもかかわらず役割が曖昧であり、戻り値が `bool` / `Task<bool>` であったため、認可失敗時の詳細なエラー理由を構造化データとして返却できなかった。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - `VK.Blocks` を採用するすべてのエンタープライズマルチテナントサービスにおいて、人為的ミスを原理的に排除する「ゼロトラストな認可安全網」と、高スループットかつ低摩擦（Low Friction）な「オンデマンド権限評価基盤」を確立するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **ゼロトラスト・テナント隔離防壁 (`ADR-001`)**: ASP.NET Core 認可パイプラインに `TenantAuthorizationHandler` を導入し、リクエストのクレーム内 `TenantId` と現行コンテキストの一致を一元検証して、不一致時は即座にリクエストを遮断する。
  - **動的ポリシーのオンデマンド生成 (`ADR-002`)**: `IAuthorizationPolicyProvider`（`PermissionPolicyProvider`）を拡張し、リクエストされた権限文字列から実行時にオンザフライで `AuthorizationPolicy` を動的生成・キャッシュすることで、起動時オーバーヘッドを実質ゼロ化する。
  - **属性評価器パターンによる宣言的認可 (`ADR-003`)**: 統一インターフェース `IAttributeEvaluator` とメタデータ駆動の `DynamicRequirement` を導入し、コントローラーから手続き型認可コードを完全排除する。
  - **コンパイル時コード生成による自動化 (`ADR-004`)**: Roslyn Incremental Source Generator（`AuthorizationHandlersGenerator`, `PermissionsCatalogGenerator`）を導入し、ハンドラーの DI 登録と権限カタログ定数をコンパイル時に自動生成する。
  - **Result Pattern の全面採用 (`ADR-005`, `CS.01`)**: 権限評価器を `IPermissionEvaluator` へ特化・昇格させ、戻り値を `Task<Result<bool>>` とすることで、失敗原因の構造化コンテキスト通知を実現する。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **身元証明・トークン検証**: ユーザーの署名検証や暗号解読は `VK.Blocks.Authentication` の排他的責務とし、本モジュールでは検証済み Claims のみを消費する。
  - **データベースへのメタデータ自動差分同期**: コード内権限定義と DB マスタの同期最適化は第 2 波（Wave 2）へ委託する。
  - **テナント契約能力（Entitlements）認可**: 契約プランや機能フラグに基づくテナントレベルの認可は第 3 波（Wave 3）へ委託する。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **要塞の多重防衛線と動的検問所 (The Multi-Layered Fortress & Dynamic Checkpoints)**
>
> 従来の城塞（システム）では、城内の各部屋（コントローラー）の前に立つ衛兵が、入室者ごとに個別の身元や所属領地を手作業で紙に書き出して照合していました。衛兵が居眠りしたり確認を怠ると、敵対領地のスパイが容易に機密保管庫へ侵入できました。
> 第 1 波の改革により、城門の直後に**領地境界の絶対防壁（Tenant Isolation Boundary）**を敷設し、所属領地が現在の城郭コンテキストと異なる者を城内へ一歩も入れないゼロトラスト体制を確立しました。
> また、すべての部屋の通行証（Policy）を朝一番にすべて印刷しておく非効率を全廃し、来訪者が通行証を提示したその瞬間に動的スタンプを押印・発行する**即応型検問窓口（Dynamic Policy Provider）**を設置しました。衛兵の配置作業や通行証の目録作成も、専任の軍師（Source Generator）が配備計画書（コンパイル時）に自動記述し、配置漏れを原理的にゼロにしました。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **領地境界の絶対防壁 (Border Safety Net)** | `TenantAuthorizationHandler` | `Internal/` | 全認可リクエストを先回りして検査し、テナント不一致を即座に拒絶（ADR-001） |
| **即応型検問窓口 (On-the-fly Checkpoint)** | `PermissionPolicyProvider` | `Internal/` | 要求された権限プレフィックスから動的に `AuthorizationPolicy` をオンデマンド生成（ADR-002） |
| **多目的検査官 (Attribute Inspector)** | `IAttributeEvaluator`, `DynamicRequirement` | `Internal/` | 業務時間や役職ランク等の宣言的制約を一元評価（ADR-003） |
| **精密配備軍師 (Compile-Time Quartermaster)** | `AuthorizationHandlersGenerator` | Source Generator | 実装された全ハンドラーを検知し `TryAddEnumerable` で自動 DI 登録（ADR-004） |
| **公式通行目録 (Official Catalog Registry)** | `PermissionsCatalogGenerator` | Source Generator | コード内の権限文字列を抽出・定数化しマジックストリングを根絶（ADR-004） |
| **特化型鑑定官 (Specialized Auditor)** | `IPermissionEvaluator` | Feature Protocol | Result Pattern に基づく構造化された権限評価（ADR-005） |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Public Protocols & Result Contracts (`Features/Permissions/`)

```csharp
namespace VK.Blocks.Authorization.Features.Permissions;

// Public Protocol: 権限評価に特化した契約 (ADR-005, CS.01, AP.01)
public interface IPermissionEvaluator
{
    /// <summary>
    /// Evaluates if the authenticated user possesses the specified permission.
    /// Returns typed Result<bool> carrying detailed failure context.
    /// </summary>
    Task<Result<bool>> HasPermissionAsync(
        ClaimsPrincipal user, 
        string permission, 
        CancellationToken cancellationToken = default);
}

// Declarative Attribute: 宣言的権限属性 (ADR-002, ADR-004)
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public string Permission { get; }

    public AuthorizePermissionAttribute(string permission)
        : base(policy: $"{PolicyPrefix}{permission}")
    {
        Permission = permission;
    }
}
```

### 4.2 Error Contract (`VKAuthorizationErrors`)

RFC 7807 準拠の構造化エラーを定義（`CS.01`, `FS-02`）。

```csharp
namespace VK.Blocks.Authorization;

public static class VKAuthorizationErrors
{
    public static readonly VKError TenantMismatch = new(
        "Authorization.TenantMismatch",
        "The authenticated user does not belong to the required tenant for this resource.",
        VKErrorType.Forbidden);

    public static readonly VKError PermissionDenied = new(
        "Authorization.PermissionDenied",
        "The user does not have the required permission to perform this action.",
        VKErrorType.Forbidden);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance & Startup Efficiency (`ADR-002`)**:
  - 静的な全ポリシー登録を廃止したことで、数千件の権限を持つ大規模エンタープライズにおいても起動時間とメモリ消費量が $O(1)$ に圧縮。
  - オンザフライ生成時の遅延は、ASP.NET Core の `DefaultAuthorizationPolicyProvider` が内包するキャッシュ層により初回の 1 回のみに抑えられ、後続リクエストはゼロオーバーヘッド。
- **Fail-Closed Security & Tenant Safety (`ADR-001`)**:
  - `TenantAuthorizationHandler` はフェイルクローズ（Fail-Closed）を厳格遵守。クレーム内に `TenantId` が存在しない場合、または現行コンテキストと合致しない場合は直ちに評価失敗とし、下流へのバイパスを遮断。
- **Alternatives Considered**:
  - *代替案 A: EF Core の Global Query Filter のみに依存 (`ADR-001`)*
    - **却下理由**: データベースを直接操作しないエンドポイント（キャッシュアクセス、外部メッセージ送信、外部 API 連携）へのアクセスが防御できず、認可境界として不完全であるため。
  - *代替案 B: 起動時のリフレクションによるアセンブリスキャン (`ADR-004`)*
    - **却下理由**: コールドスタート遅延が著しく、Native AOT コンパイルを阻害するため。Roslyn Incremental Source Generator によるコンパイル時事前生成を採用。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Tenant Isolation Pipeline)**:
  - `TenantAuthorizationHandler` の実装および全認可リクエストへの強制インターセプト機構の確立（`ADR-001`）。
- [x] **Step 2 (Dynamic Policy Provider)**:
  - `PermissionPolicyProvider` の作成とプレフィックス駆動のオンデマンドポリシー生成の実装（`ADR-002`）。
- [x] **Step 3 (Attribute Evaluator Engine)**:
  - `IAttributeEvaluator` および `DynamicRequirement` の導入によるメタデータ駆動認可の実装（`ADR-003`）。
- [x] **Step 4 (Source Generators Integration)**:
  - `AuthorizationHandlersGenerator` による DI 自動登録および `PermissionsCatalogGenerator` による定数カタログ生成（`ADR-004`）。
- [x] **Step 5 (Result Pattern Refactoring)**:
  - `IVKAuthorizationHandler` から `IPermissionEvaluator` への移行と `Result<bool>` によるエラーコンテキストの標準化（`ADR-005`）。

