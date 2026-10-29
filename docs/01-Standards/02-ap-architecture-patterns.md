# 02. Architecture & Design Patterns (`AP.01`〜`AP.07`)

> **対応ルール**: [`.agents/rules/l3-vk-be-ap-architecture-patterns.md`](/.agents/rules/l3-vk-be-ap-architecture-patterns.md) (`AP.01`〜`AP.07`)

本ドキュメントは、C# 12/13 の言語セマンティクス、DI 登録ポリシー、セマンティック可視性・命名規則（`VK` プレフィックスと `Internal/` 境界）、ゼロリフレクション Options、Args パターン、および明示的オーバーライドパターンの詳細と実装例を定義します。

---

## `AP.01` — Modern C# Semantics & `VKGuard` 🔴
- **Sealed by Default**: 継承を意図した基底クラスを除き、すべてのクラスは `sealed class`（Source Generator 連携時は `sealed partial class`）として宣言します。
- **Immutable Records & `required`**: DTO、設定、認可要件はすべて不変の `sealed record` とし、状態変更には `with` 式を使用します。非 null プロパティには `required` を使用し、`default!` の使用は厳禁です。
- **Pattern Matching & Null Handling**: `== null` の代わりに `is null` / `is not null` および `??` / `?.` を使用し、コレクション初期化には C# 12 のコレクション式 `[]` を使用します。
- **Defensive Programming (`VKGuard`)**: すべてのコンストラクタおよびパブリックメソッドの入口境界では `VKGuard` を使用して引数を検証します（手動の `if (x == null) throw` は禁止）。

```csharp
public sealed class DefaultPermissionEvaluator(IVKPermissionStore store) : IVKPermissionEvaluator
{
    private readonly IVKPermissionStore _store = VKGuard.NotNull(store);

    public async Task<Result<bool>> HasPermissionAsync(
        string userId,
        string permissionCode,
        CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(userId);
        VKGuard.NotNullOrWhiteSpace(permissionCode);

        // ...
    }
}
```

---

## `AP.02` — Service Registration Policy
- **Idempotency & `TryAdd`**: BuildingBlock のサービス登録は完全に冪等である必要があります。個々のサービス登録には必ず `TryAddSingleton` / `TryAddScoped` / `TryAddTransient` を使用し、直接の `AddSingleton` 等は禁止します。
- **Infrastructure Override Exception**: 親モジュールのデフォルト実装（InMemory 等）を置き換えることだけを目的とするインフラ拡張モジュール（例: `*.EFCore`）に限り、登録順序に依存せず確実に上書きするため `AddScoped` / `AddSingleton` または `services.Replace()` の使用が許容されます。

---

## `AP.03` — Structural Organization, Visibility & Naming 🟡

### 1. Semantic Visibility & Namespace Rules
外部利用者の利便性（`using` の最小化）と内部実装のカプセル化を両立するため、配置場所によって可視性・名前空間・`VK` プレフィックスの有無を厳格に区別します。

| カテゴリ・配置場所 | 可視性 | 名前空間 (Namespace) | `VK` プレフィックス | 型名の例 |
| :--- | :---: | :--- | :---: | :--- |
| **Public API Surface**<br>(`Common/Models/`, `Common/Protocols/`, 各スライスの `Models/`, `Protocols/`, `Contracts/`, `Extensions/`) | **`public`** | **フラットなルート名前空間**<br>`namespace VK.Blocks.{Module};` | **必須 (`VK` / `IVK`)** | `VKAIUsage`<br>`IVKChatClient`<br>`VKChatErrors` |
| **Internal Shared Foundation**<br>(`Common/Shared/`) | **`internal`** | **フラットなルート名前空間**<br>`namespace VK.Blocks.{Module};` | **禁止 (付けない)** | `HashHelper`<br>`TokenParser` |
| **Internal Feature Scoping**<br>(任意の階層の `Internal/` フォルダ内) | **`internal`** | **物理フォルダ一致の深い名前空間**<br>`namespace VK.Blocks.{Module}.{Feature}.Internal;` | **禁止 (付けない)** | `DefaultChatClient`<br>`NoOpAuditProvider` |

### 2. Implementation Naming Taxonomy (`Internal/` 内の実装クラス)
`Internal/` 内に配置する具象クラスは、`VK` プレフィックスを付けず、以下の標準プレフィックスで役割を明示します：

| プレフィックス | 宣言 | 意図と用途 |
| :--- | :--- | :--- |
| **`Default...`** | `internal sealed` | **公式推奨の本番実装**。高パフォーマンス・スレッドセーフ（例: `DefaultPermissionEvaluator`）。 |
| **`Basic...`** | `internal sealed` | **軽量・単一ノード実装**。インメモリ等、分散スケールを前提としない基本実装。 |
| **`NoOp...`** | `internal sealed` | **機能無効化時のフォールバック**。ゼロアロケーションで即座に完了または規定の `Result.Failure` を返す。 |
| **`Composite...`** | `internal sealed` | **複数プロバイダーの統合・調停**。複数の内部実装を束ねるアグリゲーター。 |
| **`{Vendor}...`** | `internal sealed` | **外部 SDK アダプター**（例: `Ef...`, `Sk...`）。インターフェースの背後で交換可能な腐敗防止層。 |

### 3. Interface Versioning, Constants & Type Segregation
- **Interface Versioning**: 公開済み Level 1 インターフェース（`IVK...`）への破壊的変更は避け、機能追加時は C# Default Interface Methods (DIM) を優先します。破壊的変更が不可避な場合は ADR (`DL.03`) が必須です。
- **Constants Visibility**: マジックストリングを排除し、単一ファイル内は `private const`、同一スライス内共有は `internal static class {Feature}Constants`、モジュール横断は `public static class VK{Module}Constants` に配置します（単なる `Constants.cs` は禁止）。
- **One File, One Type**: 1つの `.cs` ファイルには1つの主要型のみを宣言します（クラス内部専用の `private sealed record` 等のネスト型のみ例外として許容）。

---

## `AP.04` — Configuration Policy (Zero-Reflection)
- すべての BuildingBlock の Options クラスは `IVKBlockOptions`（または `IVKToggleableBlockOptions`）を実装し、リフレクションに依存しない構成バインディングを実現します。
- 初期化後は完全に不変（Immutable）とし、フレームワークは `IOptions<T>` とシングルトンプロバイダーの**冪等二重登録（Idempotent Dual-Registration）**を維持します。

---

## `AP.05` — Request-Scoped Args Pattern (SG-Automated)
モジュール全体のデフォルト設定（`Options`）に対し、リクエスト／メソッド呼び出し単位で一部の設定のみを上書きしたい場合は、Source Generator 駆動の **Args パターン** を使用します。

- Options レコード上の `[VKOptions(ArgsMode = ...)]` とプロパティ上の `[VKRequestOverride]` / `[VKNoRequestOverride]` 属性により、Nullable プロパティを持つ `VK{Feature}Args` レコード（`IVKArgs<TArgs>` 実装、`Empty` 静的インスタンス付き）が自動生成されます。

```csharp
// Options 定義側
[VKOptions(ArgsMode = VKArgsGenerationMode.Explicit)]
public sealed partial record VKChatOptions : IVKBlockOptions
{
    public string DefaultModel { get; init; } = "gpt-4o";

    [VKRequestOverride]
    public double Temperature { get; init; } = 0.7;

    [VKRequestOverride]
    public int MaxTokens { get; init; } = 2048;
}

// 実行時のマージ利用例 (Generated VKChatArgs を使用)
double effectiveTemp = args?.Temperature ?? _options.Temperature;
```

---

## `AP.06` — Explicit API & Override Pattern (Anti-Overprotection) 🔴
- **No Silent Fallbacks**: 開発者の設定漏れを隠蔽するために暗黙的な `NoOp` を自動フォールバック登録することは禁止です。`NoOp` は明示的に設定で機能が無効化（`Enabled = false` 等）された場合のみ登録します。必須設定の欠落は起動時に Fail-Fast させます。
- **Explicit Builder Overrides**: デフォルト実装をカスタム実装で差し替える拡張ポイントは、Builder 上の明示的メソッド（例: `builder.OverrideAuditProvider<T>()`）として公開し、内部では `services.Replace()` を用いて登録順序への依存を完全に排除します。

---

## `AP.07` — Non-Intrusive Capability Boundary 🔴
- **No Implicit Side-Effects**: Core や基盤 BuildingBlock は、リクエストコンテキストに明示的な識別子がない場合に勝手にドメインエンティティ（`SessionThread` 等）を暗黙生成・永続化してはなりません。ビジネスエンティティのライフサイクル所有権は常に Application 層にあります。

