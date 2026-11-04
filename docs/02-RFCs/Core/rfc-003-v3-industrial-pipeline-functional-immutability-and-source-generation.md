# RFC-003: V3 Industrial Pipeline, Functional Immutability & Modern Source Generation

- **Status**: ✅ Approved (Implemented Evolution Wave 3)
- **Author**: Principal Architect & AI Pair
- **Date**: 2026-10-29
- **Target Module(s)**: `VK.Blocks.Core`, `VK.Tools.SourceGenerators`
- **Related ADRs**:
  - [ADR-014: Adoption of Hierarchical Configuration Pattern for Dynamic Call Behaviors](/docs/04-ArchitectureDecisionRecords/Core/adr-014-adoption-of-hierarchical-configuration-pattern-for-dynamic-call-behaviors.md)
  - [ADR-016: Functional Options Transformation for Immutable Building Blocks](/docs/04-ArchitectureDecisionRecords/Core/adr-016-functional-options-transformation-for-immutable-building-blocks.md)
  - [ADR-017: Options Registration Robustness and Double Binding Elimination](/docs/04-ArchitectureDecisionRecords/Core/adr-017-options-registration-robustness-and-double-binding-elimination.md)
  - [ADR-018: Standardized Entry Filtering and Verdict Engine](/docs/04-ArchitectureDecisionRecords/Core/adr-018-standardized-entry-filtering-and-verdict-engine.md)
  - [ADR-019: Standardized Pipeline Execution Framework](/docs/04-ArchitectureDecisionRecords/Core/adr-019-standardized-pipeline-execution-framework.md)
  - [ADR-020: Decoupling Args Generation from VKFeature to VKOptions](/docs/04-ArchitectureDecisionRecords/Core/adr-020-decoupling-args-generation-from-vkfeature-to-vkoptions.md)

---

## 1. 🎯 Motivation & Problem Statement (背景と課題定義)

- **第3波着手前の課題 (Wave 3 Pain Points)**:
  - **静的構成のみの硬直性と動的実行制御の不能 (`ADR-014`)**: 静的な `IOptions` のみに依存していたため、特定の単一リクエストだけタイムアウトを延長したり、AI の Temperature を動的調整するような呼び出しができなかった。全引数での手動指定は呼び出し元を激しく冗長化させていた。
  - **.NET 標準 `Action<T>` と不変レコード（`init`）の技術的衝突 (`ADR-016`)**: BB.05 では Options の不変性（`sealed record` + `init`）が義務付けられていたが、.NET 標準の `Action<T>` は原地変異（In-place Mutation）を前提としており、C# コンパイラエラー（CS8852）が発生していた。動的構成を可能にするため `set` に妥協するケースが生じ、スレッド競合や副作用のリスクを抱えていた。
  - **二重バインディングの冗長性と `TryAdd` の順序依存脆弱性 (`ADR-017`)**: `AddVKBlockOptions` 内で `.Bind()` が二重呼び出しされ不要な DI 記述子が蓄積していた。また `TryAddSingleton` を使用していたため、外部で先に `AddOptions` が呼ばれているとカスタムファクトリが静黙的にスキップされ、不変変換や検証が無視される脆弱性があった。
  - **高度な処理スライスにおけるフィルタ判定ロジックの重複 (`ADR-018`)**: `AI.Corpus` や `AI.Psyche` などのモジュールにおいて、コンテキストに基づく生存判定（Verdict）やフィルタリングが各ブロックで個別に二重実装され、統一的なインターフェースが存在しなかった。
  - **多段階ワークフローの散在と横断的関心事の統合不能 (`ADR-019`)**: ステージ実行、並行度制御、スケジューリング、ミドルウェア実行などのパイプラインロジックが各機能ブロックにバラバラに実装され、ロギングやメトリクス、回復性（Polly）を一貫適用できなかった。
  - **DI 登録属性によるコード生成制御という関心の分離（SoC）の破綻 (`ADR-020`)**: `Args` のコード生成設定が DI 登録フックである `[VKFeature]` に配置されており、データ契約である `Options` との結合が逆転していた。また、意図しないシンボル生成が起きる危険があった。

- **なぜ今やるのか (Why Now / Consumer — `BASE-06`)**:
  - AI コグニティブエンジン、多段ナレッジパイプライン、高並行マルチテナント処理など、極限の「不変性（Immutability）」と「オンデマンド動的制御」を両立する先進 BuildingBlock 群を確固たる土台で支えるため。

---

## 2. 🧭 Goals & Non-Goals (目標と非目標)

- **Goals (達成すべき目標)**:
  - **階層的構成パターン（Hierarchical Configuration Pattern）の標準化 (`ADR-014`, `AP.05`)**: 静的既定値（Options）と動的指定（Args）を `args?.Property ?? Options.Property` の統一優先順位で安全にマージする契約を確立する。
  - **関数型オプション流式変換（Functional Options Transformation）(`ADR-016`, `BB.05`)**: `Action<TOptions>` を `Func<TOptions, TOptions>` へ刷新し、C# の `with` 式を用いた不変レコードの非破壊的変更（Non-destructive Mutation）を DI 登録時に実現する。
  - **二重バインド排除と `services.Replace` による順序非依存 DI 登録 (`ADR-017`, `AP.02`)**: 冗長な `.Bind()` を完全撤廃し、`services.Replace` を用いて登録順序に関わらず確実に `VKBlockOptionsFactory` を優先適用する。
  - **統一エントリー判定エンジン `IVKEntryFilter` と不変判定レコード `VKFilterVerdict` (`ADR-018`)**: 任意のアイテムとコンテキストを受け取り、生存・排除を非同期判定する汎用プロトコルを Core に一元化する。
  - **標準パイプライン実行基底 `IVKPipeline` / `VKPipelineExecutorBase` (`ADR-019`)**: ステージ順序制御、ミドルウェア（前後フック）、例外処理をカプセル化した堅牢な多段階ワークフロー実行基盤を提供する。
  - **`[VKOptions]` への Args 生成責務集約と安全なオプトイン (`ADR-020`)**: `[VKFeature]` からの依存を剥奪し、`[VKOptions(ArgsMode = ..., GenerateMerge = ...)]` によりモデル凝集度を最大化し、デフォルトを `None` として不要なコード生成を完全排除する。

- **Non-Goals (スコープ外 — `BASE-06` 準拠)**:
  - **特定ドメイン（AI や検索）固有のステージ・フィルタ実装**: プロンプト生成ステージやベクター距離計算フィルタは具象モジュールの排他的責務とし、Core は純粋な実行エンジン・抽象のみを提供する。
  - **宣言的クエリ式木仕様（Specification Pattern）の混同**: `IVKEntryFilter` はインメモリの単一アイテム判定に特化し、DB トランスレート用 Specification とは明確に分離する。

---

## 3. 🧬 Metaphor & Mapping (メンタルモデルと構造マッピング — `PS.03`)

> **Metaphor (概念メタファー)**: **完全自動化アセンブリラインと不変規格コンポーネント (Fully Automated Assembly Line & Immutable Standard Components)**
>
> かつての工場（旧システム）では、コンベアの上で作業員がその場しのぎに部品を削って形を変えたり（原地変異）、個別の工程ごとにバラバラな治具や検査手順を自作していました。また、製品の基本図面（Options）と現場での即時微調整（Args）の指示系統が混乱し、品質が不安定でした。
> 第 3 波の工業規格化により、工場全体が「完全自動化アセンブリライン」へと昇華しました。
> 運ばれる部材はすべて寸分の狂いもない「不変シャーシ（`sealed record`）」であり、変更が必要な場合は古いものを壊すのではなく、最新の金型で新仕様のシャーシを打ち直します（`Func<T, T>` + `with` 式）。
> 生産ライン（`IVKPipeline`）は多段ステージで構成され、各工程の前後には自動検査センサー（`IVKMiddleware`）が配置されています。異物や規格外の部材は高精度選別ゲート（`IVKEntryFilter`）が瞬時に検知・排除（`VKFilterVerdict`）し、不良品の混入をゼロにします。さらに、部品の追加仕様（Args）は部品自身の規格刻印（`[VKOptions]`）に集約され、ロボット（Source Generator）が必要な時だけ寸分違わぬ拡張ユニットを全自動で組み立てます。

| Metaphor Component (概念要素) | Technical Artifact (技術要素) | Layer / Scope | Responsibility (責務) |
| :--- | :--- | :--- | :--- |
| **規格設計図・刻印 (Blueprint Specification)** | `[VKOptions(ArgsMode = ...)]` | `Attributes/` | リクエスト上書きコード生成を Options 自身に集約し自己完結化（ADR-020） |
| **不変シャーシ (Immutable Chassis)** | `sealed record` with `init` | Public Models | 登録後の副作用を原理的に排除する不変データモデル（ADR-016, BB.05） |
| **高精度打ち直し機 (Precision Refoundry)** | `Func<TOptions, TOptions>` with `with` | `Common/DI/` | 不変性を 100% 保持したまま流式にオプションを変換（ADR-016） |
| **二重押印防止器 (Double Stamp Eliminator)** | `services.Replace` | `Common/DI/` | DI 登録時序に関わらず確実にカスタムファクトリを適用（ADR-017） |
| **自動品質選別ゲート (Quality Sorter Gate)** | `IVKEntryFilter<TItem, TContext>` | `Filtering/` | コンテキストに基づきアイテムの合否を非同期判定（ADR-018） |
| **選別判定書 (Standardized Verdict)** | `VKFilterVerdict` | `Filtering/` | アイテムの生存・除外理由と適用スコアを保持する不変レコード（ADR-018） |
| **多段コンベアベルト (Multi-Stage Track)** | `IVKPipeline`, `VKPipelineExecutorBase` | `Pipeline/` | 順序付けられたステージ群とコンテキスト共有ワークフローを実行（ADR-019） |
| **前後センサーフック (Safety Interceptor)** | `IVKMiddleware<TContext>` | `Pipeline/` | パイプライン実行前後にロギングや回復性を統一フック（ADR-019） |

---

## 4. 🏗️ Contract & Code Blueprint (契約と主要インターフェース設計 — `BASE-01`)

### 4.1 Immutable Options & Robust Registration (`Common/DependencyInjection/`)

```csharp
namespace VK.Blocks.Core.Common.DependencyInjection;

public static class VKBlockOptionsRegistrationExtensions
{
    /// <summary>
    /// Registers strongly-typed options using functional transformation with full immutability (ADR-016, ADR-017).
    /// </summary>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<TOptions, TOptions>? transform = null)
        where TOptions : class, IVKBlockOptions, new()
    {
        var options = new TOptions();
        configuration.GetSection(TOptions.SectionName).Bind(options);

        if (transform is not null)
        {
            options = transform(options);
        }

        // Robust replacement: guarantees our factory takes precedence regardless of timing (ADR-017)
        services.Replace(ServiceDescriptor.Singleton<IOptionsFactory<TOptions>>(
            new VKBlockOptionsFactory<TOptions>(options)));

        services.AddOptions<TOptions>(); // Clean registration without duplicate .Bind()
        return options;
    }
}
```

### 4.2 Entry Filtering & Verdict Engine (`Filtering/`)

```csharp
namespace VK.Blocks.Core.Filtering;

/// <summary>
/// A standardized filter contract for evaluating an item against a context (ADR-018).
/// </summary>
public interface IVKEntryFilter<in TItem, in TContext>
{
    int FilterOrder { get; }

    ValueTask<VKFilterVerdict> EvaluateAsync(
        TItem item, 
        TContext context, 
        CancellationToken cancellationToken = default);
}

public sealed record VKFilterVerdict
{
    public required bool IsAccepted { get; init; }
    public string? Reason { get; init; }
    public double? Score { get; init; }

    public static VKFilterVerdict Accept(double? score = null) => 
        new() { IsAccepted = true, Score = score };

    public static VKFilterVerdict Reject(string reason) => 
        new() { IsAccepted = false, Reason = reason };
}
```

### 4.3 Standardized Pipeline Execution Framework (`Pipeline/`)

```csharp
namespace VK.Blocks.Core.Pipeline;

public interface IVKPipeline<TContext>
{
    IReadOnlyList<IVKPipelineStage<TContext>> Stages { get; }
}

public interface IVKPipelineExecutor<TContext>
{
    Task<VKResult> ExecuteAsync(
        IVKPipeline<TContext> pipeline, 
        TContext context, 
        CancellationToken cancellationToken = default);
}

public abstract class VKPipelineExecutorBase<TContext> : IVKPipelineExecutor<TContext>
{
    private readonly IEnumerable<IVKMiddleware<TContext>> _middlewares;

    protected VKPipelineExecutorBase(IEnumerable<IVKMiddleware<TContext>> middlewares)
    {
        _middlewares = VKGuard.NotNull(middlewares);
    }

    public virtual async Task<VKResult> ExecuteAsync(
        IVKPipeline<TContext> pipeline, 
        TContext context, 
        CancellationToken cancellationToken = default)
    {
        // 1. Invoke Before Middlewares
        // 2. Execute Sorted Stages sequentially
        // 3. Invoke After Middlewares
        return VKResult.Success();
    }
}
```

### 4.4 Declarative Options & Args Generation (`Attributes/`)

```csharp
namespace VK.Blocks.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class VKOptionsAttribute : Attribute
{
    public VKArgsGenerationMode ArgsMode { get; init; } = VKArgsGenerationMode.None;
    public bool GenerateMerge { get; init; } = false;
    public Type? ArgsBaseType { get; init; }
}

public enum VKArgsGenerationMode
{
    None = 0,
    Implicit = 1,
    Explicit = 2
}
```

---

## 5. ⚖️ Trade-offs & Risk Analysis (トレードオフと非機能リスク)

- **Functional Immutability vs Memory Allocation (`ADR-016`)**:
  - `Func<TOptions, TOptions>` と `with` 式による新しいインスタンス生成はスタートアップの DI 登録時（1 回のみ）に発生するため、ランタイムホットパスでのメモリ割り当ては一切増加せず、不変性によるスレッド安全性と堅牢性を最大化。
- **Registration Idempotency & Order-Independence (`ADR-017`)**:
  - `services.Replace` を採用したことで、ライブラリの登録順序（例: コンシューマが先に `AddOptions` を呼んでいたケース）に左右されず、確実にカスタム `VKBlockOptionsFactory` がバインド済みレコードを供給する。
- **Clean Separation of Concerns in Code Generation (`ADR-020`)**:
  - `[VKFeature]` から `Args` 生成プロパティを完全剥奪し `[VKOptions]` へ移管したことで、DI 構成とデータモデル契約の依存関係が逆転する悪臭を解消。
- **Alternatives Considered**:
  - *代替案 A: 可変（Mutable）クラスに戻して標準 `Action<T>` を利用 (`ADR-016`)*
    - **却下理由**: 登録後のインスタンス書き換えリスク（Race Condition）が生じ、フレームワーク全体の `init` 不変設計（BB.05）を破壊するため。
  - *代替案 B: 外部 MediatR Pipeline Behaviors に依存 (`ADR-019`)*
    - **却下理由**: Core のゼロ外部依存原則（ARCH-03, Manifest Rule 1）に違反し、サードパーティのライフサイクルにフレームワークの基本パイプラインが縛られるため。
  - *代替案 C: `[VKFeature]` に Args 生成設定を維持 (`ADR-020`)*
    - **却下理由**: DI 登録を行わないスタンドアロンな Options に対して不要な Feature クラスの作成が強制され、直感性に反するため。

---

## 6. 📋 Decomposition & Implementation Plan (段階的実装とバックログ分解計画)

本波で完了した実装タスク群（トレーサビリティ担保）：

- [x] **Step 1 (Hierarchical Configuration Pattern)**:
  - 静的 Options と動的 Args のマージ優先順位（`args?.Property ?? Options.Property`）の策定（`ADR-014`）。
- [x] **Step 2 (Functional Options Transformation)**:
  - `Func<TOptions, TOptions>` と `with` 式による不変オプション登録への改修（`ADR-016`）。
- [x] **Step 3 (Double Binding Elimination & Replace)**:
  - 冗長な `.Bind()` の削除および `services.Replace` による順序非依存性の確立（`ADR-017`）。
- [x] **Step 4 (Standardized Entry Filtering)**:
  - `IVKEntryFilter<TItem, TContext>` および `VKFilterVerdict` の Core 実装（`ADR-018`）。
- [x] **Step 5 (Standardized Pipeline Framework)**:
  - `IVKPipeline`、`VKPipelineExecutorBase`、およびミドルウェアフック機構の提供（`ADR-019`）。
- [x] **Step 6 (Args Generation Decoupling)**:
  - `[VKFeature]` から `[VKOptions]` への Args 生成プロパティの移管とジェネレータの刷新（`ADR-020`）。
