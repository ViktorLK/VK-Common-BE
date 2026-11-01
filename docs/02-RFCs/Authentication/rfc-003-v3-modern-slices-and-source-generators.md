# RFC-003: V3 Modern Vertical Slices, Zero-Reflection & Compiler Automation

- **Status**: ✅ Approved (Implemented Evolution Wave 3)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Authentication`, `VK.Blocks.Authentication.OpenIdConnect`, `VK.Tools.SourceGenerators`
- **Related ADRs**:
  - [ADR-015: Eliminate Reflection for OAuth Mappers via Source Generators](/docs/04-ArchitectureDecisionRecords/Authentication/adr-015-eliminate-reflection-for-oauth-mappers-via-source-generators.md)
  - [ADR-016: Adopt High-Performance Logging Pattern with Feature-Local Source Generators](/docs/04-ArchitectureDecisionRecords/Authentication/adr-016-adopt-high-performance-logging-pattern-with-feature-local-source-generators.md)
  - [ADR-021: Zero-Reflection Auto-Discovery of Authentication Extensions via Source Generators](/docs/04-ArchitectureDecisionRecords/Authentication/adr-021-zero-reflection-auto-discovery-of-authentication-extensions-via-source-generators.md)
  - [ADR-022: Standardization of Building Block API Surface via VK-Prefixing](/docs/04-ArchitectureDecisionRecords/Authentication/adr-022-standardization-of-building-block-api-surface-via-vk-prefixing.md)
  - [ADR-023: Modular Feature-Sliced Registration Pattern for Authentication Sub-Blocks](/docs/04-ArchitectureDecisionRecords/Authentication/adr-023-modular-feature-sliced-registration-pattern-for-authentication-sub-blocks.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第3波着手前の課題 (Wave 3 Pain Points)**:
  - **実行時リフレクションによる Native AOT 阻害 (`ADR-015`, `ADR-021`)**: 認証プロバイダーや OAuth マッパーの動的解決にアセンブリ走査（`Assembly.GetTypes()`）とリフレクションを用いており、コールドスタートの遅延や Native AOT コンパイル時のトリミング破損リスクを抱えていた。
  - **ロギングによる文字列補間とヒープ確保 (`ADR-016`)**: 認証失敗やトークン検証のログ出力において標準の `ILogger.LogInformation("Token {0}", token)` が使われ、ボクシングアロケーションと PII 誤混入のリスクが生じていた。
  - **水平層構造による保守性の低下 (`ADR-023`)**: 当初のディレクトリ構成が技術駆動型（Handlers/, Models/, Interfaces/）であり、JWT や ApiKey の修正時にプロジェクト全体を行き来する必要があった（低凝集・高結合）。
  - **パブリック API 命名の不統一 (`ADR-022`)**: 外部公開されるインターフェイスやモデルに統一されたプレフィックスがなく、ASP.NET Core 標準型や下流アプリケーションの型との名前空間衝突が発生していた。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - クラウドネイティブ環境（コンテナ即時起動、サーバーレス、低メモリ消費）および Native AOT 配備を見据え、ゼロリフレクション・ゼロアロケーション・高凝集な垂直スライスアーキテクチャへの最終昇格を果たすため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **ゼロリフレクション・コンパイル時自動検出 (`ADR-015`, `ADR-021`)**: Roslyn Source Generator により、認証拡張や OAuth マッピングコードをビルド時に自動生成し、実行時リフレクションを 100% 排除する。
  - **Source-Generated High-Performance Logging (`ADR-016`, `OR.01`)**: `[LoggerMessage]` 属性によるゼロアロケーション構造化ログを全垂直スライスに導入し、PII を完全に排除したテレメトリを確立する。
  - **垂直スライス切片化アーキテクチャ (`ADR-023`, `BB.01`)**: `ApiKeys/`, `Jwt/`, `OAuth/`, `Cookies/`, `Tracking/` の機能単位で垂直切片化し、`Internal/` ディレクトリにより実装詳細を完全カプセル化する。
  - **ビルダーパターンによる宣言的オプトイン (`ADR-023`, `BB.06`)**: `builder.Services.AddVKAuthenticationBlock().AddVKJwt().AddVKApiKeys()` による、Source Generator 連動の型安全かつチェーン可能な DI API を提供する。
  - **`VK` プレフィックスの標準化 (`ADR-022`, `AP.03`)**: パブリック API 層の全シンボルに `VK` 接頭辞を付与し、ライブラリのブランド一貫性と衝突防止を徹底する。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **動的プラグインの実行時アセンブリロード**: 実行時のアンマネージドアセンブリ動的ロード等は対象外とし、コンパイル時決定論（Compile-Time First, `BASE-03`）を徹底する。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **近代モジュラー建築と事前ファブリケーション工場 (The Pre-Fabricated Modular Factory)**
>
> かつての建築現場（従来のランタイム）では、現場監督（リフレクション）が壁の中を手探りで探しながら、現場合わせで配管や電気コードを繋いでいました。これは施工ミス（AOTトリミング破損）や工期遅延（起動時間悪化）の温床でした。
> 第 3 波の近代化により、すべての壁面ユニット、配管ジョイント、配線（DI・マッピング）は、出荷前の精密工場（Roslyn Source Generator）においてミリ単位で**プレハブ製造（Compile-Time Generation）**されるようになりました。
> 建築現場（アプリケーション起動時）では、完成したスライスユニット（`Jwt` 切片、`ApiKey` 切片）をカチッとはめ込むだけで即座に通電・機能します。各切片は完全に独立した耐火隔壁（`Internal/` カプセル化）で守られ、共通ブランド（`VK` プレフィックス）が刻印されています。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **Factory Mold (工場金型)** | `[VKBlockMarker]`, `[VKFeature]` | SG Markers | コンパイル時 DI 生成をトリガーする宣言的マーカー属性 |
| **Pre-Fab Cable Assembly (事前配線)** | `VKAuthenticationBlockExtensions.g.cs` | SG Generated | リフレクションをゼロにするコンパイル時生成の DI 拡張メソッド群 |
| **Fireproof Modular Slice (耐火スライス)** | `ApiKeys/`, `Jwt/`, `OAuth/` | Vertical Slices | 契約・実装・エラー・診断が独立完結した高凝集機能ディレクトリ |
| **Zero-Heat Sensor (熱ゼロセンサー)** | `[LoggerMessage]` (e.g. `ApiKeyLog.cs`) | Slice Diagnostics | 文字列連結とボクシングを一切伴わない超高速・PII 排除ログ機構 |
| **Standard Brand Trademark (刻印商標)** | `VK` Prefix (`IVKJwtAuthService`, `VKJwtOptions`) | Public API Baseline | 識別性向上と下流アプリケーション名前空間との衝突回避 |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Feature-Sliced Modular Builder API (`Common/DependencyInjection/`)

```csharp
namespace VK.Blocks.Authentication;

// Public Builder API: スライス単位の宣言的チェーン構成 (ADR-023, BB.06)
public interface IVKAuthenticationBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}

public static class VKAuthenticationBuilderExtensions
{
    public static IVKAuthenticationBuilder AddVKJwt(
        this IVKAuthenticationBuilder builder, 
        Action<VKJwtOptions>? configure = null);

    public static IVKAuthenticationBuilder AddVKApiKeys(
        this IVKAuthenticationBuilder builder, 
        Action<VKApiKeyOptions>? configure = null);

    public static IVKAuthenticationBuilder AddVKOAuth(
        this IVKAuthenticationBuilder builder, 
        Action<VKOAuthOptions>? configure = null);
}
```

### 4.2 Source-Generated Zero-Allocation Diagnostics (`Jwt/Diagnostics/Internal/`)

```csharp
namespace VK.Blocks.Authentication.Jwt.Diagnostics.Internal;

// Internal SG-driven Logger (ADR-016, OR.01)
internal static partial class JwtLog
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "JWT validation failed for issuer '{Issuer}'. Reason: {FailureReason}")]
    public static partial void TokenValidationFailed(
        ILogger logger, 
        string issuer, 
        string failureReason);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "JWT successfully authenticated. UserId: '{UserId}', TenantId: '{TenantId}'")]
    public static partial void TokenValidated(
        ILogger logger, 
        string userId, 
        string tenantId);
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Build Time vs Runtime Performance (`ADR-015`, `ADR-021`)**:
  - Source Generator の導入によりビルド時間はコンマ数秒増加するものの、実行時のアセンブリ走査・JIT リフレクションコストを完全にゼロ化。Native AOT のフル互換性とミリ秒レベルの高速コンテナ起動を実現。
- **Encapsulation vs Convenience (`ADR-023`)**:
  - `Internal/` による厳格なカプセル化（`AP.03`）により、同一ブロック内であっても他スライスの Internal 実装クラスへの横断参照を禁止。これによりスライス間の結合度を極小化し、機能単位の安全な改修・廃止を保証。
- **Alternatives Considered**:
  - *代替案 A: Scrutor による動的アセンブリ走査*
    - **却下理由 (`ADR-021`)**: Scrutor は柔軟であるものの、実行時リフレクションに依存し、トリミング時に型情報が破棄される Native AOT 環境でランタイムクラッシュを引き起こすため。
  - *代替案 B: プレフィックスなしの標準 C# 命名（`JwtAuthService`, `JwtOptions`）*
    - **却下理由 (`ADR-022`)**: Microsoft の公式パッケージや下流開発者のドメインモデルと名前空間で容易に衝突し、IDE の補完混乱を招くため。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Source Generator for OAuth Mappers)**:
  - リフレクション排除とコンパイル時マッパーコード生成の導入（`ADR-015`）。
- [x] **Step 2 (Source-Generated Logging)**:
  - 全垂直スライスのロギングを `[LoggerMessage]` 部分クラスへ刷新（`ADR-016`）。
- [x] **Step 3 (Zero-Reflection Provider Auto-Discovery)**:
  - Source Generator による拡張プロバイダーのコンパイル時登録の自動化（`ADR-021`）。
- [x] **Step 4 (Public API Standardization & VK Prefixing)**:
  - パブリックインターフェイス、モデル、Options への `VK` 接頭辞の厳格付与（`ADR-022`）。
- [x] **Step 5 (Vertical Slice Architecture Migration)**:
  - `ApiKeys/`, `Jwt/`, `OAuth/`, `Cookies/`, `Tracking/` の独立垂直スライスへの物理移行完了（`ADR-023`）。

