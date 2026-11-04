# RFC-002: V2 Modular Topology, Capability Alignment & Dependency Governance

- **Status**: ✅ Approved (Implemented Evolution Wave 2)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Core`, `VK.Tools.SourceGenerators`
- **Related ADRs**:
  - [ADR-003: Introducing Service Marker Pattern for BuildingBlock Modularization](/docs/04-ArchitectureDecisionRecords/Core/adr-003-introducing-service-marker-pattern-for-buildingblock-modularization.md)
  - [ADR-005: Use Static Abstract Interface for Configuration Section Resolution](/docs/04-ArchitectureDecisionRecords/Core/adr-005-use-static-abstract-interface-for-configuration-section-resolution.md)
  - [ADR-006: Establishing Granular Capability-Based Structure and Namespace Alignment in Core](/docs/04-ArchitectureDecisionRecords/Core/adr-006-restructuring-core-module-into-semantic-pillars.md)
  - [ADR-007: Unified BuildingBlock Identification and Source-Generated Zero-Reflection Validation](/docs/04-ArchitectureDecisionRecords/Core/adr-007-unified-buildingblock-identification-and-source-generated-zero-reflection-validation.md)
  - [ADR-008: Robust BuildingBlock Dependency Validation through Pre-order Traversal and Cycle Detection](/docs/04-ArchitectureDecisionRecords/Core/adr-008-robust-building-block-dependency-validation.md)
  - [ADR-009: Standardization of Building Block Naming and Structural Conventions](/docs/04-ArchitectureDecisionRecords/Core/adr-009-standardization-of-building-block-naming-and-structural-conventions.md)
  - [ADR-012: Functional Decoupling of Dependency Injection Extensions](/docs/04-ArchitectureDecisionRecords/Core/adr-012-functional-decoupling-of-dependency-injection-extensions.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第2波着手前の課題 (Wave 2 Pain Points)**:
  - **構成存在チェックによる不完全なモジュール検証 (`ADR-003`)**: 従来、モジュール間依存（例: `OpenIdConnect` が `Authentication` に依存）の検証に `services.Any(d => d.ServiceType == typeof(VKAuthenticationOptions))` を用いていたが、「設定が存在すること」と「実サービスが DI コンテナに登録され稼働可能であること」を混同しており、設定だけ読み込まれて機能が死んでいる不全状態を検知できなかった。
  - **リフレクションによるセクション名動的解決のオーバーヘッドと型安全性欠如 (`ADR-005`)**: `appsettings.json` のセクション名を特定するためにリフレクションで定数を読み出しており、スタートアップ時の微細な遅延と、コンパイル時にセクション名の存在を保証できない脆弱性を抱えていた。
  - **5つの大雑把な「柱」による責務混同とゴミ箱フォルダの再発 (`ADR-006`)**: コア機能を「5大ピラー」に無理に集約しようとした結果、`Common/` や `Utils/` への安易なコード退避が再発し、高凝集・疎結合なドメイン境界が曖昧化していた。
  - **マーカーボイラープレートの氾濫と識別子の二重性 (`ADR-007`)**: 各モジュールで `Identifier` と `BlockName` が重複定義され、シングルトンインスタンスや静的アクセサーの手動記述が開発者の認知負荷となっていた。
  - **事後走査（Post-order）の不正確なエラーと循環依存（A→B→A）によるクラッシュ (`ADR-008`)**: 依存関係検証において子の検証を親より先行させたため、根本原因ではない孫のエラーが表示された。また、相互参照時に無限再帰（`StackOverflowException`）が発生してプロセスが停止していた。
  - **公開 API と内部実装の命名混迷とカプセル化破綻 (`ADR-009`)**: 内部実装クラスにまで `VK` プレフィックスが付与されて名前空間が汚染され、外部利用者が非公開実装クラスへ直接依存してしまうリスクが存在した。
  - **DI 拡張の肥大化（God Class）と開発者体験（DX）の悪化 (`ADR-012`)**: 1 つの `VKBlockServiceExtensions.cs` に登録（Add）、問い合わせ（Is/Ensure）、ビルダー構築の全メソッドが集約され、コードの可読性と IntelliSense の視認性を著しく損なっていた。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - 数十に及ぶ BuildingBlock 群が相互に依存し合うエコシステムにおいて、起動時の設定不備を即座に特定する「Fail-Fast な依存関係グラフ統治」と「ゼロリフレクション・コンパイル時安全なモジュラー構造」を確立するため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **サービスマーカーによる登録状態の厳格証明 (`ADR-003`)**: `AddVKBlockMarker<T>` を導入し、「自前重複チェック（Idempotency）→ 前提依存チェック（Fail-Fast）→ サービス登録 → マーカーコミット」の 4 段階エントリプロトコルを確立する。
  - **C# 11 Static Abstract Interface によるセクション解決 (`ADR-005`)**: `IVKBlockOptions` に `static abstract string SectionName { get; }` を導入し、リフレクションを 100% 排除してコンパイル時型安全な設定バインドを実現する。
  - **12 の高解像度 Capability エリアと 1:1 名前空間アライメント (`ADR-006`)**: フォルダ構造と C# 名前空間を完全一致させ、`Context/`, `Tenancy/`, `Security/`, `Guids/`, `Errors/`, `Results/` などの独立した責務境界を確立する。
  - **Source Generator によるマーカー自動合成 (`ADR-007`)**: `VKBlockMarkerGenerator` を導入し、`IVKBlockMarker` を実装した partial クラスへ `Instance` シングルトンと `IVKBlockMarkerProvider<T>` を自動注入する。
  - **先行走査（Pre-order）と `HashSet<string>` による循環依存検知 (`ADR-008`)**: 直接の依存先を先に検証して的確なエラーを出力し、`visited` 追跡により循環参照を $O(V + E)$ で検知・遮断する。
  - **VK プレフィックス規約と `Internal/` フォルダ隔離 (`ADR-009`, `AP.03`, `ARCH-07`)**: 公開 API（Level 1）には `VK` プレフィックスを強制し、内部実装はプレフィックスを除去して `Internal/` に完全隠蔽する。
  - **DI 拡張機能の 3 責務分離 (`ADR-012`)**: `VKBlockRegistrationExtensions`（登録）、`VKBlockQueryExtensions`（問い合わせ/検証）、`VKBlockBuilderExtensions`（流式ビルダー）に分離し、保守性と DX を向上させる。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **動的リクエスト引数（Args）の自動生成**: リクエストスコープでのオーバーライド機構は第 3 波（Wave 3）へ委託する。
  - **関数型オプション流式変換（`Func<T, T>`）**: 不変レコードに対する関数型変換は第 3 波（Wave 3）へ委託する。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **近代都市グリッドと高精度インターロッキング機構 (Modern Urban Grid & High-Precision Interlocking Mechanism)**
>
> 雑然とした旧市街（初期コードベース）では、住居（データ型）と作業場（ロジック）とゴミ捨て場（Utils）が混在し、どの建物がどのインフラ（上下水道や電力）に接続されているか誰も把握していませんでした。工事の順番を間違えると街路全体が陥没（無限ループやクラッシュ）していました。
> 第 2 波の都市計画により、Core は厳格な「12 区画の近代都市グリッド（Capability Slices）」へと生まれ変わりました。
> すべての施設には規格化された住所プレート（`IVKBlockOptions.SectionName`）が掲げられ、建設にあたっては中央検査局（Source Generator）が発行する公認印章（`IVKBlockMarker`）の提示が義務付けられました。さらに、区画間のインフラ接続は「先行安全検査（Pre-order Traversal）」と「閉ループ検知器（Cycle Detector）」を備えたインターロッキング機構により統治され、依存欠落や循環参照を起動時に 0.1 秒で検知・遮断します。表通りには統一意匠（`VK` プレフィックス）が並び、裏方の保守回廊は地下道（`Internal/`）へ完全に隔離されています。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **都市グリッド区画 (Zoning Slices)** | 12 Capability Slices (`Context/`, `Tenancy/` 等) | Architecture Layout | フォルダと名前空間を 1:1 で一致させ、責務の漏洩を防止（ADR-006） |
| **公認建築印章 (Building Permit / Marker)** | `IVKBlockMarker`, `VKBlockRuntimeMarker` | `Common/DI/` | ブロックの一意な識別子、バージョン、依存関係を証明（ADR-003, ADR-007） |
| **規格表札 (Standardized Address Plate)** | `IVKBlockOptions.SectionName` | `Common/Config/` | 静的抽象メンバにより構成キーをリフレクションなしで型安全に供給（ADR-005） |
| **自動鋳造所 (Automated Mint / Foundry)** | `VKBlockMarkerGenerator` | Source Generator | マーカークラスへの `Instance` シングルトン注入とボイラープレート根絶（ADR-007） |
| **安全検査インターロック (Safety Interlock)** | `EnsureDependenciesRegistered` | `Common/DI/` | 先行走査と `HashSet` による循環参照防止および Fail-Fast 検証（ADR-008） |
| **統一表通りファサード (Public Facade)** | Level 1 Types (`VK*`) | Public Surface | 外部利用者に公開される安全で厳格なプロトコル群（ADR-009） |
| **地下保守回廊 (Underground Service Corridor)** | `Internal/` Folder & Classes | `Internal/` (Slice Private) | 外部からの直接参照を禁止し、内部実装の自律性を保護（ADR-009） |
| **行政窓口分離 (Triage Desks)** | `Registration` / `Query` / `Builder` Extensions | `Common/DI/` | DI 拡張の責務を分離し、IntelliSense と保守性を最適化（ADR-012） |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Block Marker & Identification Contract (`Common/DependencyInjection/`)

```csharp
namespace VK.Blocks.Core.Common.DependencyInjection;

public interface IVKBlockMarker
{
    string Identifier { get; }
    string Version { get; }
    IReadOnlyList<IVKBlockMarker> Dependencies { get; }
    string ActivitySourceName { get; }
    string MeterName { get; }

    /// <summary>
    /// Performs pre-order recursive dependency validation with cycle detection (ADR-008).
    /// </summary>
    void EnsureDependenciesRegistered(IServiceCollection services, string dependentId)
    {
        EnsureCore(this, services, dependentId, []);
    }

    private static void EnsureCore(
        IVKBlockMarker marker, 
        IServiceCollection services, 
        string dependentId, 
        HashSet<string> visited)
    {
        if (!visited.Add(marker.Identifier)) return;

        foreach (var dependency in marker.Dependencies)
        {
            if (!services.IsVKBlockRegistered(dependency.Identifier))
            {
                throw new InvalidOperationException(
                    $"BuildingBlock '{dependentId}' depends on '{dependency.Identifier}', " +
                    $"but '{dependency.Identifier}' has not been registered in the DI container.");
            }

            EnsureCore(dependency, services, marker.Identifier, visited);
        }
    }
}

public interface IVKBlockMarkerProvider<TSelf> 
    where TSelf : IVKBlockMarkerProvider<TSelf>
{
    static abstract IVKBlockMarker Instance { get; }
}
```

### 4.2 Static Abstract Options Contract (`Common/Configuration/`)

```csharp
namespace VK.Blocks.Core.Common.Configuration;

/// <summary>
/// Defines a contract for strongly-typed building block options with compile-time section resolution.
/// </summary>
public interface IVKBlockOptions
{
    /// <summary>
    /// Gets the configuration section name for this options type in appsettings.json.
    /// Resolved at compile time without reflection (ADR-005).
    /// </summary>
    static abstract string SectionName { get; }
}
```

### 4.3 Decoupled DI Extensions Triad (`Common/DependencyInjection/`)

```csharp
namespace VK.Blocks.Core.Common.DependencyInjection;

// 1. Registration Responsibility (ADR-012)
public static class VKBlockRegistrationExtensions
{
    public static IServiceCollection AddVKBlockMarker<TMarker>(this IServiceCollection services)
        where TMarker : class, IVKBlockMarker, IVKBlockMarkerProvider<TMarker>
    {
        var marker = TMarker.Instance;
        services.TryAddEnumerable(ServiceDescriptor.Singleton(new VKBlockRuntimeMarker(marker.Identifier)));
        return services;
    }
}

// 2. Query & Verification Responsibility (ADR-012)
public static class VKBlockQueryExtensions
{
    public static bool IsVKBlockRegistered(this IServiceCollection services, string identifier) =>
        services.Any(d => d.ImplementationInstance is VKBlockRuntimeMarker m && m.Identifier == identifier);

    public static void EnsureVKBlockRegistered<TRequired, TDependent>(this IServiceCollection services)
        where TRequired : class, IVKBlockMarker, IVKBlockMarkerProvider<TRequired>
        where TDependent : class, IVKBlockMarker, IVKBlockMarkerProvider<TDependent>
    {
        TRequired.Instance.EnsureDependenciesRegistered(services, TDependent.Instance.Identifier);
    }
}

// 3. Builder Responsibility (ADR-012)
public static class VKBlockBuilderExtensions
{
    // Houses fluent chaining methods and pipeline builders
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Zero-Reflection vs Compile-Time Generation (`ADR-007`)**:
  - `static abstract` のみをインターフェースに定義した場合、ジェネリック型を介した動的な再帰走査が複雑化する課題があった。
  - インスタンスプロパティ（`IVKBlockMarker`）と静的アクセサー（`IVKBlockMarkerProvider<T>`）を Source Generator でブリッジさせる構造を採用したことで、リフレクションを一切使わずにクリーンな DAG（有向非巡回グラフ）走査と $O(1)$ 解決を実現。
- **Fail-Fast Error Clarity & Stack Safety (`ADR-008`)**:
  - 先行走査（Pre-order）によって、「A が B を必要としているが B がない」という根本原因が直ちに例外メッセージとして出力される。
  - `HashSet<string>` による訪問管理により、万一モジュール間で循環依存が記述された場合でも、無限ループを起こさずに安全に検知・脱出する。
- **Alternatives Considered**:
  - *代替案 A: Options 型のみによる登録チェック (`ADR-003`)*
    - **却下理由**: オプションがバインドされていることと、実際のインターセプターやハンドラーが DI コンテナに存在することの乖離を検出できないため。
  - *代替案 B: 属性（Attribute）によるセクション名指定 (`ADR-005`)*
    - **却下理由**: 実行時リフレクションが不可避であり、Native AOT コンパイル時のトリミングリスクやコールドスタート遅延を排除できないため。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Service Marker Pattern)**:
  - `AddVKBlockMarker` および `IsVKBlockRegistered` の導入（`ADR-003`）。
- [x] **Step 2 (Static Abstract Options Resolution)**:
  - `IVKBlockOptions` への `static abstract SectionName` 導入とリフレクション排除（`ADR-005`）。
- [x] **Step 3 (Granular Capability Slices)**:
  - 12 の Capability スライスへの分離と名前空間 1:1 アライメント（`ADR-006`）。
- [x] **Step 4 (Source Generator Marker Injection)**:
  - `VKBlockMarkerGenerator` による `Instance` シングルトン自動生成と識別子統合（`ADR-007`）。
- [x] **Step 5 (Pre-Order Traversal & Cycle Detection)**:
  - `EnsureDependenciesRegistered` の先行走査と `HashSet` 循環参照防止ロジックの実装（`ADR-008`）。
- [x] **Step 6 (VK Prefix & Structural Encapsulation)**:
  - 公開 API への `VK` プレフィックス付与と内部クラスの `Internal/` 隔離（`ADR-009`）。
- [x] **Step 7 (DI Extensions Functional Decoupling)**:
  - DI 拡張を Registration, Query, Builder の 3 クラスへ分離（`ADR-012`）。
