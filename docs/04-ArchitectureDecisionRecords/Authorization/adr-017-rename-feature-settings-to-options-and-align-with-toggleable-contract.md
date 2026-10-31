# ADR 017: Rename Feature Settings to Options and Align with Toggleable Contract

- **Date**: 2026-05-18
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks Authorization Block Contract-First Architecture

---

## 1. Context (背景)

VK.Blocks アーキテクチャでは、コンパイル時コード生成（Source Generator）を用いた「スキーマ優先（Contract-First）」の引数生成モデル（Automated Args Generation）を採用しています。

しかし、既存の静的設定（Static Settings）と実行時引数（Dynamic Overrides/Args）の定義モデルにおいて、以下の命名上の不整合（Naming Inconsistency）と冗長な定義パターン（Redundant Boilerplate）が発生していました。

1. **命名の一致性の欠如（Naming Mismatch）**：
   基盤となる抽象フレームワークのインターフェースが `IVKBlockOptions` および `IVKToggleableBlockOptions`（"Options"）であるのに対し、機能固有のインターフェース契約は `IVKRoleSettings`（"Settings"）と命名され、その具体的な実装クラスは `VKRoleOptions`（"Options"）と命名されていました。
   これにより、開発時には **Options ➡️ Settings ➡️ Options** という用語の行き来（Cognitive Flip-flop）が生じ、認知負荷と設計の一貫性を損なっていました。

2. **多重実装の冗長性（Redundant Base Interfaces）**：
   可変トグル（Enabled 開閉機能）を持つ各 Feature の Options 実装において、`IVKBlockOptions` と `IVKToggleableBlockOptions` の両方を多重に列挙する冗長コードが存在していました。`IVKToggleableBlockOptions` は `IVKBlockOptions` を継承しているため、本質的には二者択一であるべきです。

---

## 2. Problem Statement (問題定義)

1. **言語的一貫性の違反**：
   API 設計における同一概念（Configuration Segment）に対して、`Options` と `Settings` という異なる名詞が混在しており、フレームワーク全体の整合性が阻害されています。
2. **手動定義のボイラープレート**：
   すべての Options クラスにおいて、`: IVKBlockOptions, IVKToggleableBlockOptions, IVKRoleSettings` のような長いインターフェース宣言が必要となっており、記述エラーの温床となっていました。
3. **バリデーター保護のバイパス漏れ**：
   `IVKToggleableBlockOptions` の指定が漏れた Options（例: `VKTenantIsolationOptions`, `VKMinimumRankOptions`）において、Source Generator が `Enabled = false` 時のバリデーション穿透（Validation Bypass）コードを自動生成できず、実行時のバリデーション挙動に不整合が発生していました。

---

## 3. Decision (決定事項)

我々は、**「Settings 語彙の完全廃止」**と**「Options 契約の一元化と階層継承」**を決定し、静的・動的のアクセス仕様を以下のように対称的（Symmetric）に再定義します。

### 3.1. 命名の対称性と一本化（Symmetric Options-Overrides Vocabulary）

"Settings" という語彙をフレームワークから完全に廃止し、**"Options"** と **"Overrides"** の二大対称語彙に整理します。

| ライフサイクル          | 契約インターフェース | 具現化クラス   | 役割                                             |
| :---------------------- | :------------------- | :------------- | :----------------------------------------------- |
| **静的配置（Static）**  | `IVKXxxOptions`      | `VKXxxOptions` | 起動時に読み込まれる静的なシステム設定契約。     |
| **動的評価（Dynamic）** | `IVKXxxOverrides`    | `VKXxxArgs`    | リクエストスコープで動的に上書きされる評価引数。 |

### 3.2. 契約レベルでの「二者択一」階層化（Unified Contract Inheritance）

各 Feature の Options 契約（インターフェース）自体が、基盤の `IVKBlockOptions` または `IVKToggleableBlockOptions` を直接継承します。これにより、具現化 Options クラス側のインターフェース宣言を極限までシンプル化します。

#### A. 開閉機能（Toggleable）を持つ Feature

Feature 固有の Options 契約は `IVKToggleableBlockOptions` を継承します：

```csharp
namespace VK.Blocks.Authorization;

// 1. 契約レベルで Toggleable と明記
public interface IVKRoleOptions : IVKToggleableBlockOptions
{
    // Enabled は IVKToggleableBlockOptions から自動的に継承
    string? RoleClaimType { get; init; }
}

// 2. 実装クラスはビジネス契約を 1 つ実装するだけ
public sealed partial record VKRoleOptions : IVKRoleOptions
{
    public bool Enabled { get; init; } = true;
    public string? RoleClaimType { get; init; }
}
```

#### B. 常に有効（Always-On）な Feature

Feature 固有の Options 契約は `IVKBlockOptions` を継承します：

```csharp
namespace VK.Blocks.Authorization;

// 1. 契約レベルで Always-On と明記
public interface IVKAlwaysOnOptions : IVKBlockOptions
{
    string SomeRequiredConfig { get; init; }
}

// 2. 実装クラス
public sealed partial record VKAlwaysOnOptions : IVKAlwaysOnOptions
{
    public string SomeRequiredConfig { get; init; }
}
```

### 3.3. Source Generator の一元化（Clean Source Generator Mapping）

移行の複雑さと不要な技術負債（Technical Debt）を排除するため、Source Generator (`VKFeatureGenerator.cs`) からは `Settings` 語彙の解析コードを一切排除し、**`Options` 語彙に「一挙に（一步到位）」統一します**。他の Building Block モジュールに関しても順次 `Options` 形式に追従させ、ジェネレーター内の解析ルールを完全に一本化します。

---

## 4. Alternatives Considered (代替案の検討)

### Option 1: 従来の多重インターフェース宣言を維持する

- **概要**: Options クラス側に手動で `IVKBlockOptions` と `IVKToggleableBlockOptions` を明示し続ける。
- **却下理由**: 冗長コードの排除にならず、開発者が Toggleable マーカーの指定を忘れるバグを根本治療できません。

### Option 2: 段階的移行のために旧 Settings の互換解析を SG に残す

- **概要**: 既存モジュール（AI 等）の追従が終わるまで、SG 内で `Settings` と `Options` の両方のマッピングコードを維持する。
- **却下理由**: ジェネレーターに不要な分岐ロジックとレガシーの汚染が残り、アーキテクチャの純粋性が損なわれます。「一步到位（一挙にクリーンカット）」で SG を完全刷新し、他モジュールは後から Options 規格へアラインさせることで、ジェネレーターの極限のシンプルさを担保します。

---

## 5. Consequences & Mitigation (結果と緩和策)

### Positive

- **完全な一貫性**: Options 契約が `IVKRoleOptions` ➡️ `VKRoleOptions` となり、標準的な C# 設計（Interface/Implementation）に完全に合致。
- **ジェネレーターの純粋性**: Source Generator にレガシーの `Settings` 解析コードが一切残らず、メンテナンス性が飛躍的に向上。
- **ボイラープレートの極小化**: 実装 Options レコード側は、自身のビジネスインターフェースを 1 つ宣言するだけで済み、コード量が削減。
- **コンパイル時の防御強化**: 契約レベルで `IVKToggleableBlockOptions` を强制継承させるため、トグル可能機能でバリデーター保護が漏れる危険性を完全に排除。

### Negative

- **他モジュールの一時的ビルド警告/エラー**: 他のモジュール（AI 等）が `Options` 規格に追従するまで、その Feature は一時的に SG の恩恵を受けられなくなります。
- **緩和策**: 本決定後に他の全 Building Block（AI, Caching, Logging 等）を順次 `Options` にアラインさせ、全リポジトリの完全クリーン状態を早期に達成します。

---

## 6. Implementation & Verification Plan (実装・検証計画)

### 6.1. 実装タスク

1. `VKFeatureGenerator.cs` をクリーンアップし、レガシー `Settings` マッピングコードを完全に除去して `IVK...Options` 形式に一本化。
2. 認可ブロック（Authorization Block）の全 7 つの Feature 契約ファイル `IVK...Settings.cs` を削除し、`IVK...Options.cs` を新規作成。
3. `VK...Options.cs` の宣言を `IVK...Options` への単一実装に整理。

### 6.2. 検証計画

- `dotnet build` を実行し、Source Generator が生成するコードに問題がないことを確認。
- `dotnet test` を実行し、既存の全 145 件の認可ユニットテストが正常にビルド・パスすることを確認。
