# 03. Building Block Blueprint (`BB.01`〜`BB.08`)

> **対応ルール**: [`.agents/rules/l3-vk-be-bb-block-blueprint.md`](/.agents/rules/l3-vk-be-bb-block-blueprint.md) (`BB.01`〜`BB.08`)

本ドキュメントは、`VK.Blocks` モジュールを新規構築・拡張する際の具体的な物理ディレクトリ構造、マーカーパターン（`[VKBlockMarker]` / `[VKFeature]`）、Source Generator による DI 自動生成シーケンス、診断定数、および Options 配置ルールを定義します。

---

## `BB.01` — Standard Folder Structure (Vertical Slice Priority)
すべての BuildingBlock は、システム共通基盤を収める **`Common/`** と、ドメイン機能ごとの垂直スライス **`{FeatureName}/`** で構成します。

```text
VK.Blocks.{ModuleName}/
├── VK{ModuleName}Block.cs          # [BB.02] モジュール唯一 Marker (public sealed partial class)
├── VK{ModuleName}Options.cs        # [BB.05] モジュール全体 Options (public sealed partial record)
│
├── Common/                         # [BB.01] スライス横断のシステム基盤（具体ビジネスロジックは置かない）
│   ├── DependencyInjection/        # 公開 Builder 拡張 (VK{Module}BuilderExtensions.cs) や Protocols/
│   ├── Constants/                  # (Optional) モジュール横断の公開定数
│   ├── Diagnostics/                # モジュール共通の診断定数 (VK{Module}DiagnosticsConstants.cs) と Internal/
│   ├── Models/                     # (Optional) スライス横断の公開データモデル (VK プレフィックス)
│   ├── Protocols/                  # (Optional) スライス横断の公開インターフェース (IVK プレフィックス)
│   └── Shared/                     # (Optional) 2つ以上のスライスで共有する内部ユーティリティ (internal / プレフィックスなし)
│
└── {FeatureName}/                  # 垂直スライス（例: Pagination/, ApiKeys/, Permissions/）
    ├── VK{FeatureName}Options.cs   # [BB.05/07] スライス専用 Options (スライス直下に独立配置)
    ├── {FeatureName}Feature.cs     # [BB.06] Feature Marker & サブ登録フック (internal sealed partial class)
    ├── Contracts/                  # 公開エラー定数 (VK{Feature}Errors.cs) や Args 等のデータ契約
    ├── Models/                     # 公開モデル・DTO (VK プレフィックス)
    ├── Protocols/                  # 公開インターフェース (IVK プレフィックス)
    ├── Extensions/                 # 外部公開用の拡張メソッド (VK...Extensions)
    ├── Diagnostics/                # スライス専用のメトリクス定数と Internal/ ロガー
    └── Internal/                   # [AP.03] スライス内部の非公開実装（外部からは完全に隠蔽）
        └── Default{Service}.cs     # internal sealed class（VK プレフィックスなし）
```

---

## `BB.02` — The Marker Pattern (`[VKBlockMarker]`)
各モジュールのルートディレクトリには、`[VKBlockMarker]` 属性を付与した `public sealed partial class VK{ModuleName}Block` を配置します。

- `IVKBlockMarker` インターフェースや `BlockName` プロパティ等を手動で実装してはなりません（Source Generator が自動生成します）。
- 前提となる依存ブロックがある場合は `Dependencies` プロパティで明示します。

```csharp
namespace VK.Blocks.Authentication;

// [SG Marker] - This attribute triggers the Source Generator to generate module metadata and base implementation.
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKAuthenticationBlock;
```

---

## `BB.03` — Idempotent DI Registration (SG-Automated) 🟡
DI 登録フロー全体は**完全な同期処理**でなければなりません（DI 拡張メソッド内での DB 通信や `.Wait()` / `.GetAwaiter().GetResult()` は起動時デッドロックを引き起こすため厳禁です）。

### 1. Source Generator が自動生成する7ステップの順序
`VKBlockGenerator` は `[VKBlockMarker]` を検知し、以下の厳密な順序で動作する登録パイプライン（`AddVK{ModuleName}Block`）を自動生成します（開発者がボイラープレートを手書きする必要はありません）：

| Step | アクション | 生成される処理内容 |
| :---: | :--- | :--- |
| **1** | **Check-Self** | `if (services.IsVKBlockRegistered<VK{Module}Block>()) return builder;` |
| **2** | **Options Registration** | `services.AddVKBlockOptions<VK{Module}Options>(configuration, transform);` |
| **3** | **Mark-Self** | `services.AddVKBlockMarker<VK{Module}Block>();` |
| **4** | **Validate Options** | `services.TryAddEnumerableSingleton<IValidateOptions<VK{Module}Options>, ...>();` |
| **4b** | **Options Provider** | `services.TryAddSingleton<IVK{Module}OptionsProvider, Default{Module}OptionsProvider>();` |
| **5** | **Feature Toggle** | `if (!options.Enabled) return builder;` *(※`IVKToggleableBlockOptions` 実装時のみ)* |
| **6** | **Custom Hook** | `RegisterBlockCustom(builder)` の呼び出し |

### 2. 手動カスタマイズが必要な場合のフック実装 (`RegisterBlockCustom` / `ValidateBlockCustom`)
モジュール固有のサービス登録や複雑なクロスプロパティ検証が必要な場合のみ、`partial` メソッドを実装してフックします：

```csharp
namespace VK.Blocks.Authentication;

[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKAuthenticationBlock
{
    // [SG Hook] - This method is called by the Source Generator to inject manual DI registration logic.
    static partial void RegisterBlockCustom(IVKAuthenticationBuilder builder)
    {
        builder.Services.TryAddScoped<IVKTokenValidator, DefaultTokenValidator>();
    }

    // [SG Hook] - This method is called by the Source Generator to inject manual options validation logic.
    static partial void ValidateBlockCustom(
        VKAuthenticationOptions options,
        ref ValidateOptionsResult? result)
    {
        if (options.DefaultScheme is { Length: 0 })
        {
            result = ValidateOptionsResult.Fail("DefaultScheme cannot be empty.");
        }
    }
}
```

---

## `BB.04` — Diagnostics Blueprint
- **Diagnostics Class**: `[VKBlockDiagnostics(typeof(VK{ModuleName}Block))]` 属性を付与した `internal static partial class` を定義し、モジュール専用の `ActivitySource` と `Meter` を自動生成させます。
- **Diagnostics Constants**: メトリクス名やタグ名を表すセマンティック定数は `public static class VK{ModuleName}DiagnosticsConstants`（または機能別の `VK{FeatureName}DiagnosticsConstants`）として定義します。

---

## `BB.05` & `BB.07` — Options Architecture & Isolation 🟡
- **Structure (`BB.05`)**: すべての Options は `IVKBlockOptions`（または `IVKToggleableBlockOptions`）を実装する `public sealed partial record` とし、プロパティは `init` で宣言します（`partial` にすることで SG が `SectionName` を自動生成します）。
- **Functional Transformation (`BB.05`)**: コードによる設定変更には、不変レコードの `with` 式と親和性の高い `Func<TOptions, TOptions> transform` パターンを使用します。
- **One Class, One File (`BB.07`)**: Options レコードは必ずモジュールまたは機能スライスのルートにある専用の `.cs` ファイル（`VK{FeatureName}Options.cs`）に独立して配置します。インターフェースファイルやハンドラークラスの中にネストさせることは厳禁です。

```csharp
namespace VK.Blocks.Authentication;

public sealed partial record VKApiKeyOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public string HeaderName { get; init; } = "X-Api-Key";
    public TimeSpan CacheDuration { get; init; } = TimeSpan.FromMinutes(5);
}
```

---

## `BB.06` & `BB.08` — Modular Feature Pattern & Implicit Pull-Up 🟡
複数の独立した機能スライスを持つ BuildingBlock では、各スライスのルートに `{FeatureName}Feature.cs` を配置し、`[VKFeature]` 属性を付与します。

- **Chained Builder (`BB.06`)**: `IVK{ModuleName}Builder` 上の拡張メソッド（例: `builder.AddApiKeys()`）で機能をチェーン登録でき、階層的な設定セクション（`VKBlocks:{Parent}:{Feature}`）に自動バインドされます。
- **Implicit Parent Pull-Up (`BB.08`)**: サブ機能が登録される際、親ブロック（`ParentBlock`）がまだ登録されていなければ Source Generator が自動的に親ブロックの登録処理を先に呼び出します。

```csharp
namespace VK.Blocks.Authentication;

// [SG Marker] - This attribute triggers the Source Generator to generate feature registration and options binding.
[VKFeature(typeof(VKAuthenticationBlock), OptionsType = typeof(VKApiKeyOptions))]
internal sealed partial class ApiKeyFeature
{
    // [SG Hook] - This method is called by the Source Generator to inject manual feature service bindings.
    static partial void RegisterFeatureCustom(IVKAuthenticationBuilder builder)
    {
        builder.Services.TryAddScoped<IVKApiKeyValidator, DefaultApiKeyValidator>();
    }
}
```

