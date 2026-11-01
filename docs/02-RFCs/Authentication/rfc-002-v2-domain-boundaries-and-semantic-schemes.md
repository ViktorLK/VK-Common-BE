# RFC-002: V2 Domain Boundaries, Claims SSOT & Semantic Schemes

- **Status**: ✅ Approved (Implemented Evolution Wave 2)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Authentication`, `VK.Blocks.Authentication.OpenIdConnect`
- **Related ADRs**:
  - [ADR-008: Centralized Identity Claims Resolution as SSOT](/docs/04-ArchitectureDecisionRecords/Authentication/adr-008-centralized-identity-claims-resolution-as-ssot.md)
  - [ADR-009: Strict Eager Validation for Authentication Configuration](/docs/04-ArchitectureDecisionRecords/Authentication/adr-009-strict-eager-validation-for-authentication-configuration.md)
  - [ADR-010: Dynamic Dictionary-Driven OAuth Configuration](/docs/04-ArchitectureDecisionRecords/Authentication/adr-010-dynamic-dictionary-driven-oauth-configuration.md)
  - [ADR-011: Dual Mode JWT Validation Strategy](/docs/04-ArchitectureDecisionRecords/Authentication/adr-011-dual-mode-jwt-validation-strategy.md)
  - [ADR-014: Strategic Roadmap for Semantic Auth Attributes and Enterprise Features](/docs/04-ArchitectureDecisionRecords/Authentication/adr-014-semantic-auth-attributes-roadmap.md)
  - [ADR-018: Standardization of Authentication Opt-in Model and Idempotent Validation Patterns](/docs/04-ArchitectureDecisionRecords/Authentication/adr-018-standardization-of-authentication-opt-in-model-and-idempotent-validation-patterns.md)
  - [ADR-019: Decoupling Authentication and Authorization via Dependency Inversion](/docs/04-ArchitectureDecisionRecords/Authentication/adr-019-decoupling-authentication-and-authorization-via-dependency-inversion.md)
  - [ADR-020: Adoption of Semantic Authentication Schemes for Group-Based Authorization](/docs/04-ArchitectureDecisionRecords/Authentication/adr-020-adoption-of-semantic-authentication-schemes-for-group-based-authorization.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第2波着手前の課題 (Wave 2 Pain Points)**:
  - **技術スキームとの硬直した結合 (`ADR-019`, `ADR-020`)**: エンドポイントや認可ポリシーが `[Authorize(AuthenticationSchemes = "Bearer")]` のように物理スキーム名をハードコードしており、同一エンドポイントを「ユーザー（JWT）」と「サービス間連携（API Key / mTLS）」の両方で呼び出す際にコード改修が必要だった。
  - **認可ロジックとの境界侵食 (`ADR-019`)**: 認証ハンドラ内部でロールやポリシーの評価を行ってしまい、`VK.Blocks.Authorization` との境界が曖昧になっていた。
  - **Claims 解析の分散と不整合 (`ADR-008`)**: 各コントローラが独自に `User.FindFirst("sub")` や `User.FindFirst(ClaimTypes.NameIdentifier)` を呼び出しており、IdP 差異によって `UserId` や `TenantId` の取得漏れが発生していた。
  - **設定の遅延障害 (`ADR-009`, `ADR-018`)**: 不正な設定（秘密鍵の欠落や不正な有効期限）が存在してもアプリケーション起動が成功してしまい、最初のリクエスト受信時に初めて 500 エラーとして爆発していた。
  - **外部プロバイダーのハードコード (`ADR-010`)**: OAuth 構成がクラスプロパティとして固定されており、新規ソーシャルログイン（Google, GitHub 等）を追加するたびにライブラリ自体のコード変更が必要だった。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - マルチテナント SaaS、BFF、マイクロサービス間通信において、技術スキームに縛られない「セマンティックな認証ポリシー」と「厳格な Claims SSOT（Single Source of Truth）」を全社統一で提供するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **セマンティック認証スキーム (`ADR-020`)**: 物理スキーム（Bearer, ApiKey 等）を「`User`」「`Service`」「`Internal`」という業務的セマンティクスグループに抽象化し、エンドポイント側は `[VKAuthGroup("User")]` で宣言可能とする。
  - **認証・認可の完全分離 (`ADR-019`)**: 認証モジュールから認可モジュールへの直接参照を断ち、`ISemanticSchemeProvider` を介した依存性の逆転（DIP）を適用する。
  - **Claims 解析の SSOT 化 (`ADR-008`)**: `ClaimsPrincipalExtensions` に属性抽出（`GetUserId()`, `GetTenantId()` 等）を完全集約し、`VKClaimTypes` を標準とする。
  - **Fail-Fast 起動時検証 (`ADR-009`, `ADR-018`)**: 全オプションクラスに `IValidateOptions<T>` を実装し、`.ValidateOnStart()` により構成不備をホスト起動時に検知・即時中断させる。
  - **辞書駆動型 OAuth (`ADR-010`)**: `Dictionary<string, OAuthProviderOptions>` により、設定ファイルのみで任意の OAuth/OIDC プロバイダーを動的追加可能とする。
  - **双模 JWT 検証 (`ADR-011`)**: 対称鍵（Symmetric）によるローカル高速検証と、IdP の OIDC ディスカバリー（Asymmetric JWKS）を透過的にサポートする。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **アクセス権限・RBAC/PBAC ポリシーの評価**: ロール・パーミッションの検証は `VK.Blocks.Authorization` に完全に委ねる。
  - **ユーザープロファイルの保存**: ユーザーメタデータやパスワードハッシュの永続化は `VK.Blocks.Identity` の責務とし、関与しない。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **外交儀典とセマンティック身分証プロトコル (The Diplomatic Protocol & Semantic Passports)**
>
> 以前のシステムは、「赤いパスポート（JWT）を持つ者のみ通過可」という物理的な紙の形式で入場制限をしていました。これでは、公用車（API Key）で急遽訪れた政府使節を同じ通用口から通すことができませんでした。
> 第 2 波の改革では、「一般旅行者（User グループ）」、「公用特使（Service グループ）」、「施設内スタッフ（Internal グループ）」という**セマンティックな身分（Semantic Scheme）**で入場条件を定義するようにしました。
> 各ゲート（エンドポイント）は「一般旅行者であれば、紙のパスポートでもスマホの電子証明書でもどちらでもよい」と宣言するだけで、税関翻訳官（`ISemanticSchemeProvider`）がその身分を満たす物理スキームを自動的に調停・検証します。また、各国の異なる身分証表記は、入国審査官（`ClaimsPrincipalExtensions`）が共通の外交規格（SSOT）へ標準化します。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **Semantic Identity Group (身分分類)** | `VKAuthGroupAttribute`, `VKAuthenticationConstants.Groups` | Public (`Common/`) | 物理スキームから独立した業務レベルの認証グループ定義 (`User`, `Service`, `Internal`) |
| **Protocol Diplomat (外交調停官)** | `ISemanticSchemeProvider`, `ApiKeySemanticSchemeProvider` | `Internal/` | セマンティックグループを基に、要求されたリクエストで実行すべき具象認証ハンドラを動的解決 |
| **Official Interpreter (外交標準翻訳官)** | `VKClaimsPrincipalExtensions`, `VKClaimTypes` | Public Extensions | 外部プロバイダーの雑多な Claim 形式をシステム内単一の真実（SSOT）へ抽出・変換 |
| **Border Inspector Checklist (入国査問票)** | `IValidateOptions<T>`, `ValidateOnStart()` | `Common/Internal/` | 起動時に全オプションの整合性を厳格検証し、不備があればホスト起動を拒否（Fail-Fast） |
| **Dynamic Passport Registry (開かれた旅券台帳)** | `Dictionary<string, VKOAuthProviderOptions>` | Public Options | 設定ファイルの追記のみで未知の OAuth プロバイダーを動的登録可能にする設計（OCP） |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Semantic Scheme Protocols & Extensions (`Common/Protocols/`)

```csharp
namespace VK.Blocks.Authentication;

// Public Protocol: 認証グループ（セマンティック）と物理スキームの解決契約 (ADR-020)
public interface IVKSemanticSchemeProvider
{
    /// <summary>
    /// Resolves technical authentication schemes associated with a semantic group (e.g. "User", "Service").
    /// </summary>
    ValueTask<IReadOnlyCollection<string>> GetSchemesForGroupAsync(
        string semanticGroupName, 
        CancellationToken cancellationToken = default);
}

// Public Attribute: エンドポイントにおけるセマンティック認証グループ指定 (ADR-014, ADR-020)
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class VKAuthGroupAttribute : Attribute
{
    public string GroupName { get; }
    public VKAuthGroupAttribute(string groupName) => GroupName = groupName;
}

// Public Extensions: Claims SSOT 抽出プロトコル (ADR-008)
public static class VKClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirst(VKClaimTypes.UserId)?.Value 
        ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetTenantId(this ClaimsPrincipal principal) =>
        principal.FindFirst(VKClaimTypes.TenantId)?.Value;
}
```

### 4.2 Error Contract (`VKAuthenticationErrors`)

```csharp
namespace VK.Blocks.Authentication;

public static class VKAuthenticationErrors
{
    public static readonly VKError SemanticGroupNotFound = new(
        "Auth.SemanticGroupNotFound", 
        "The requested semantic authentication group is not registered.", 
        VKErrorType.Failure);

    public static readonly VKError ConfigurationInvalid = new(
        "Auth.ConfigurationInvalid", 
        "Authentication configuration failed eager validation at startup.", 
        VKErrorType.Failure);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Performance vs Flexibility (`ADR-020`)**:
  - セマンティック解決（グループ ➔ 物理スキーム群）をリクエストごとに行うと辞書ルックアップが発生するため、スキーム解決結果は起動時に `FrozenDictionary<string, string[]>` でプリコンパイルし、実行時オーバーヘッドを $O(1)$・ゼロアロケーションに抑止。
- **Fail-Fast Defense (`ADR-009`)**:
  - `.ValidateOnStart()` の強制により、無効なキー設定や未設定の Issuer がある場合はサービスが起動すらできなくなる。これにより「起動したのに認証できない」本番障害を 100% コンパイル/起動時に遮断。
- **Alternatives Considered**:
  - *代替案 A: ASP.NET Core の PolicyScheme の素直な利用*
    - **却下理由 (`ADR-020`)**: PolicyScheme は「1 つのデフォルトフォールバック」しか選べず、複数の認証スキーム（例: Bearer と Cookie の併用）をグループとして束ねて順次試行することが困難だったため。
  - *代替案 B: 各 Controller で `ClaimTypes.NameIdentifier` を直接参照*
    - **却下理由 (`ADR-008`)**: 各 IdP（Auth0 では `sub`、Azure AD では `oid`、独自JWTでは `user_id`）によって Claim 名がバラバラであり、ビジネスコードへの IdP 依存流出を防ぐため。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Claims SSOT Consolidation)**:
  - `VKClaimTypes` 定義と `ClaimsPrincipalExtensions` への属性抽出ロジック集約（`ADR-008`）。
- [x] **Step 2 (Fail-Fast Configuration Validation)**:
  - 全 Options への `IValidateOptions<T>` 実装と `.ValidateOnStart()` 登録の標準化（`ADR-009`, `ADR-018`）。
- [x] **Step 3 (Dictionary-Driven Dynamic OAuth)**:
  - ハードコードされたプロパティを辞書型に改修し、動的プロバイダー登録を解禁（`ADR-010`）。
- [x] **Step 4 (Dual-Mode JWT Strategy)**:
  - 対称キーと OIDC JWKS 非対称キーのハイブリッド検証パイプラインを構築（`ADR-011`）。
- [x] **Step 5 (Decoupled Semantic Schemes)**:
  - `ISemanticSchemeProvider` と `[VKAuthGroup]` 属性を導入し、認可依存を完全切断（`ADR-019`, `ADR-020`）。

