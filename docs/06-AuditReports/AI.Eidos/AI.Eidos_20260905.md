# 🏛️ アーキテクチャ監査レポート: AI.Eidos

> Active: [L1+L2:AI.Eidos] | Context: src/BuildingBlocks/AI.Eidos | Sync: [BB.01:ctx, AP.03:ctx, BB.02:ctx, BB.03:ctx, BB.04:L3, BB.05:ctx, AP.02:ctx, CS.02:L3, CS.01:ctx, AP.01:ctx, CS.03:ctx, CS.06:ctx, OR.01:L1, BB.06:ctx, BB.07:ctx, DL.01:ctx]
> **Audit Load**: `[BB.01:ctx, AP.03:ctx, BB.02:ctx, BB.03:ctx, BB.04:L3, BB.05:ctx, AP.02:ctx, CS.02:L3] | Status: Verified ✅`

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: 88/100
- **Fast Audit スコア**: 28/30 (93%)
- **対象レイヤー判定**: Domain / BuildingBlock Layer — Response Contract, Expression Negotiation & Output Parsing
- **総評 (Executive Summary)**: AI.Eidos は VK.Blocks の垂直スライスアーキテクチャに極めて忠実に構築された高品質モジュールである。4つの Feature Slice（Materialization / Negotiation / Schema / Streaming）が `[VKFeature]` Source Generator 統合により標準化された DI 登録パターンを採用し、Strategy パターンによる高い拡張性を実現している。`VKGuard` による徹底した境界防御（50+箇所）、`ConfigureAwait(false)` の一貫適用（4/4）、`VKResult<T>` パターンの全面的採用、`IVKJsonSerializer` / `IVKGuidGenerator` 経由の Core 抽象活用など、Industrial DNA の主要要件を高水準で満たしている。ただし、**`DefaultMaterializationBinder` における `JsonSerializer.Deserialize` 直呼び（CS.06 違反）**、**`throw new NotSupportedException` の使用（CS.01 違反）**、**`new VKError()` インラインエラー定数（CS.01 違反）**、および**複数箇所での bare `catch` ブロック**が改善点として残る。

---

# ⚡ Fast Audit (Phase 1): AI.Eidos

**Date**: 2026-09-05 | **Score**: 28/30 (93%)

## 📁 Structure (BB.01)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| S-01 | BB.01 | 🟡 | `DependencyInjection/` exists | ✅ Pass — SG-automated via `[VKBlockMarker]` |
| S-02 | BB.01 | 🟡 | `DependencyInjection/Internal/` exists | ✅ Pass — SG-automated |
| S-03 | BB.04 | 🟡 | `Diagnostics/` exists | ✅ Pass — `Common/Diagnostics/` |
| S-04 | BB.04 | 🟡 | `Diagnostics/Internal/` exists | ✅ Pass — `Common/Diagnostics/Internal/` |
| S-05 | BB.01 | 🔴 | Marker file at root | ✅ Pass — `VKAIEidosBlock.cs` |
| S-06 | BB.01 | 🟡 | Options co-location | ✅ Pass — 各 Feature Slice 内に配置 |

## 🏷️ Marker (BB.02)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| M-01 | BB.02 | 🔴 | `[VKBlockMarker]` attribute | ✅ Pass — `[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock)])]` |
| M-02 | BB.02 | 🟡 | Legacy `IVKBlockMarker` | ✅ Pass — 未使用 |
| M-03 | BB.02 | 🔴 | `sealed partial class` | ✅ Pass — `public sealed partial class VKAIEidosBlock` |
| M-04 | BB.02 | 🟡 | Dependencies declared | ✅ Pass — `Dependencies = [typeof(VKAIPsycheBlock)]` |

## 🔌 DI Registration (BB.03, AP.02/04)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| D-01 | BB.03 | 🔴 | Idempotency check | ✅ Pass — SG-automated via `[VKBlockMarker]` |
| D-02 | BB.03 | 🔴 | Self-marker registration | ✅ Pass — SG-automated |
| D-03 | AP.04 | 🟡 | Options standard helper | ✅ Pass — SG-automated via `[VKFeature]` |
| D-04 | AP.02 | 🔴 | `TryAdd` pattern | ✅ Pass — 全 15 件 `TryAddSingleton` / `TryAddEnumerable` |
| D-05 | AP.02 | 🔴 | No direct `Add` | ✅ Pass — 検出なし |
| D-06 | BB.03 | 🟡 | Wrapper → Internal delegation | ✅ Pass — SG-automated |
| D-07 | BB.03 | 🔴 | Wrapper naming pattern | ✅ Pass — SG-automated |

## ⚙️ Options (BB.05, AP.04)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| O-01 | BB.05 | 🟡 | `sealed partial record` + `IVKBlockOptions` | ✅ Pass — 全 4 Features |
| O-02 | BB.05 | 🟡 | `VK` prefix | ✅ Pass — `VKMaterializationOptions` 等 |
| O-03 | AP.04 | 🟡 | `SectionName` defined | ✅ Pass — SG-generated via `partial` |
| O-04 | BB.05 | 🔴 | NOT `sealed class` (legacy) | ✅ Pass — 検出なし |

## 🔍 Implementation Patterns (CS.01/03/06, OR.01, AP.01, BB.04)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| I-01 | AP.01 | 🔴 | Sealed usage | ✅ Pass — 25 sealed / 0 non-sealed = 100% |
| I-02 | AP.01 | 🔴 | VKGuard usage | ✅ Pass — 50+ 箇所で徹底使用 |
| I-03 | CS.03 | 🔴 | ConfigureAwait compliance | ✅ Pass — 4/4 = 100% |
| I-04 | OR.01 | 🟡 | `[LoggerMessage]` source gen | ⚠️ N/A — ログ出力なし（Diagnostics 定数のみ定義） |
| I-05 | OR.01 | 🔴 | No direct logger calls | ✅ Pass — 検出なし |
| I-06 | BB.04 | 🟡 | `[VKBlockDiagnostics]` | ✅ Pass — `[VKBlockDiagnostics<VKAIEidosBlock>]` |
| I-07 | CS.01 | 🔴 | Result pattern usage | ✅ Pass — `VKResult<T>` 全面採用 |
| I-08 | CS.06 | 🔴 | No `DateTime.UtcNow` | ✅ Pass — 検出なし |
| I-09 | CS.06 | 🔴 | No `Guid.NewGuid()` | ✅ Pass — `IVKGuidGenerator` 使用 |
| I-10 | CS.06 | 🔴 | No `JsonSerializer` direct | ❌ Fail — [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L135 で `JsonSerializer.Deserialize` 直呼び |
| I-11 | CS.02 | 🟡 | No dependency pollution | ✅ Pass — EF Core / Redis 依存なし |

## 📛 Naming & Visibility (AP.03)

| ID | Rule | Tier | Check | Result |
|:---|:-----|:-----|:------|:-------|
| N-01 | AP.03 | 🟡 | Public types use `VK` / `IVK` prefix | ✅ Pass — 検出なし |
| N-02 | AP.03 | 🟡 | Internal/ types are `internal` | ✅ Pass — 全 Internal/ 内で public 型なし |
| N-03 | AP.03 | 🟡 | Namespace alignment | ✅ Pass |

## 📊 Summary Table

| Category | Tier | ✅ | ❌ | ⚠️ |
|:---|:---|:---|:---|:---|
| Structure (6) | 🟡 | 6 | 0 | 0 |
| Marker (4) | 🔴 | 4 | 0 | 0 |
| DI Registration (7) | 🔴 | 7 | 0 | 0 |
| Options (4) | 🟡 | 4 | 0 | 0 |
| Implementation (11) | 🔴 | 9 | 1 | 1(N/A) |
| Naming (3) | 🟡 | 3 | 0 | 0 |

---

# Phase 2: Registration Audit (DI層精査)

## BB.03 実行順序

`[VKBlockMarker]` Source Generator により自動生成されるため、8ステップの実行順序はコンパイル時に保証される。手書きの `RegisterBlockCustom` フックは `VKAIEidosBlock.cs` に実装され、`IVKSchemaFactory` の `TryAddSingleton` のみを含む。正しい位置（Step 6: Custom Hook）で実行される。

| Check | Result | Evidence |
|:------|:-------|:---------|
| Check-Self → Options → Mark-Self → Validator → Toggle → Custom Hook | ✅ Pass | SG-automated, custom hook at [VKAIEidosBlock.cs](/src/BuildingBlocks/AI.Eidos/VKAIEidosBlock.cs):L15-18 |
| Feature Slices use `[VKFeature]` SG | ✅ Pass | 4/4 Features: [MaterializationFeature.cs](/src/BuildingBlocks/AI.Eidos/Materialization/MaterializationFeature.cs), [NegotiationFeature.cs](/src/BuildingBlocks/AI.Eidos/Negotiation/NegotiationFeature.cs), [SchemaFeature.cs](/src/BuildingBlocks/AI.Eidos/Schema/SchemaFeature.cs), [StreamingFeature.cs](/src/BuildingBlocks/AI.Eidos/Streaming/StreamingFeature.cs) |

## BB.03 Func Transform

ブロックレベル Options の変換パラメータは SG が `Func<T,T>` パターンで生成するため、手書きコード不要。Feature Options も `[VKFeature]` SG により同パターンを採用。

| Check | Result |
|:------|:-------|
| `Func<T,T>` transform (ADR-016) | ✅ Pass — SG-automated |

## BB.03 Enabled Policy Position

`IVKBlockOptions` を使用（`IVKToggleableBlockOptions` ではない）ため、`if (!options.Enabled)` チェックは N/A。

## BB.03 Builder Pattern

SG-generated `IVKAIEidosBuilder` がビルダーパターンを実装。全 Feature 登録で `TryAdd` を使用。

| Check | Result |
|:------|:-------|
| Builder returns `IVKBlockBuilder<T>` | ✅ Pass — SG-automated |
| Uses `TryAdd` extensions | ✅ Pass — 15 件全て `TryAdd` |

## BB.05 OptionsValidator Quality

Feature Options は `[VKFeature]` SG により `IValidateOptions<T>` の基本実装が自動生成される。複雑なクロスプロパティ検証（例: `MaxRepairAttempts` vs `MaxTotalAttempts` の整合性チェック）は未実装だが、現時点では致命的ではない。

| Check | Result |
|:------|:-------|
| `IValidateOptions<T>` implementation | ⚠️ Warn — SG 生成の基本バリデーションのみ。`MaxRepairAttempts < MaxTotalAttempts` 等のクロスプロパティ検証なし |

---

# Phase 3: Implementation Audit (深層分析)

## 設計原則 (Design Principles)

### SOLID

- **SRP**: ✅ 各 Feature Slice は明確に単一責任を持つ。Schema = スキーマ解決・進化分析、Negotiation = 能力検出・フォーマット交渉・スキーマ投影、Materialization = 検証・バインディング・修復・リトライ、Streaming = ストリーミング解析。
- **OCP**: ✅ 全サービスがインターフェース経由で DI 登録されており、`TryAdd` パターンにより拡張者が独自実装で差し替え可能。
- **LSP**: ✅ `IVKContractSpec` の階層 (`VKExplicitContractSpec`, `VKNamedContractSpec`, `VKTypeContractSpec`) は switch 式でパターンマッチングにより正しく処理されている。
- **ISP**: ✅ インターフェースは適切に分離されている（`IVKMaterializationValidator`, `IVKMaterializationBinder`, `IVKMaterializationRepairService`, `IVKMaterializationRetryPolicy`）。
- **DIP**: ✅ 全実装クラスは抽象インターフェースにのみ依存し、具象クラスへの直接参照はない。

### KISS / DRY / YAGNI

- ✅ `SchemaFingerprint` と `SchemaComposer` は内部ユーティリティとして適切に切り出されている。
- ✅ `VKMaterializationEnvelopeFactory` は `static` ファクトリとして DRY 原則に従い、エンベロープ構築を一元化。
- ⚠️ `DefaultMaterializationBinder.ExtractJsonBlock` と `DefaultStreamingParser.LocateActiveJsonCandidate` に JSON 抽出ロジックの部分的重複がある（ThinkTag 除去 + code block 抽出パターン）。

## 設計パターン (Design Patterns)

| Pattern | Location | Assessment |
|:--------|:---------|:-----------|
| **Strategy** | `IVKMaterializationValidator`, `IVKMaterializationBinder`, `IVKContractNegotiator`, `IVKContractProjector`, `IVKProviderCapabilityDetector`, `IVKSchemaResolver`, `IVKStreamingParser` | ✅ 優秀 — 全コア処理が Strategy パターンで拡張可能 |
| **Factory** | `IVKSchemaFactory` (DefaultSchemaFactory), `VKContractSpecFactory`, `VKMaterializationEnvelopeFactory`, `VKDynamicSchemaBuilder` | ✅ 優秀 — スキーマ・コントラクト生成を一元化 |
| **Pipeline / Chain of Responsibility** | `IVKPsychePipelineStage` (Schema → Negotiation), `IVKPsycheMiddleware` (Materialization) | ✅ 優秀 — Psyche パイプラインの Stage/Middleware 分離が明確 |
| **Builder** | `VKDynamicSchemaBuilder` (Fluent API) | ✅ 優秀 — ネスト対応のフルーエントビルダー |
| **Cache-Aside** | `DefaultSchemaFactory._schemaCache`, `DefaultContractProjector._promptInstructionCache` / `_projectedSchemaCache` / `_tailPatternCache` | ✅ `ConcurrentDictionary` によるスレッドセーフなキャッシング |

## 架構原則 (Architectural Principles)

- **関注点分離**: ✅ Schema / Negotiation / Materialization / Streaming が完全に独立した垂直スライスとして分離。`Common/` は共有モデル・プロトコルのみ。
- **カプセル化**: ✅ 全 `Internal/` フォルダ内の型は `internal sealed` で宣言。外部からのアクセスは公開インターフェース経由のみ。
- **凝集度**: ✅ 各 Feature の `Internal/` には当該 Feature の実装のみが配置。
- **結合度**: ✅ Feature 間の直接参照は最小限。`DefaultSchemaEvolutionAnalyzer` が `IVKMaterializationValidator` を参照する点は適切な依存（進化分析にはバリデーションが必要）。

## 架構風格 (Architectural Styles)

- **Clean Architecture / Vertical Slice**: ✅ BB.01 に完全準拠。4 Feature Slices + Common 基盤の構造が明確。
- **レイヤー依存**: ✅ `csproj` は `Core` → `AI` → `AI.Psyche` のみを参照。インフラストラクチャ依存なし（CS.02 完全準拠）。

## 架構パターン (Architectural Patterns)

- **Pipeline Architecture**: ✅ Psyche パイプラインの `IVKPsychePipelineStage`（Before Phase: Schema → Negotiation）と `IVKPsycheMiddleware`（Onion Phase: Materialization）に正しく統合。
- **Contract-First Design**: ✅ `VKAIEidosResponseContract` → `VKAIEidosSchema` → JSON Schema という階層的なコントラクト定義が一貫。

## 企業級パターン (Enterprise Patterns)

- **冪等性**: ✅ 全 DI 登録が `TryAdd` パターン。`[VKBlockMarker]` による二重登録防止。
- **キャッシュ**: ✅ `DefaultSchemaFactory` のスキーマキャッシュ、`DefaultContractProjector` のプロジェクションキャッシュが `ConcurrentDictionary` + `Lazy<T>` で実装。
- **自己修復 / リトライ**: ✅ `DefaultMaterializationMiddleware` の 3 段階リトライポリシー（AutoRepair → AcceptPartial → Abort）+ 最大試行数による Circuit Breaker パターン。
- **可観測性**: ✅ `VKAIEidosDiagnosticsConstants` にセマンティックメトリクス・トレーシングトークンが定義。`[VKBlockDiagnostics<VKAIEidosBlock>]` 適用済み。

## VK.Blocks 固有の準拠度 (Deep)

### ✅ 評価ポイント

- **VKGuard 境界防御**: 50+ 箇所で `VKGuard.NotNull` / `VKGuard.NotNullOrWhiteSpace` を徹底使用。Primary constructor + fluent assignment パターンが一貫（例: `_resolver = VKGuard.NotNull(resolver)`）。
- **CancellationToken 伝播**: async メソッドチェーン全体で途切れなく渡されている。`DefaultSchemaStage.ExecuteAsync` → `_resolver.ResolveFromArgsAsync` → `cancellationToken` が正しく伝播。
- **IVKGuidGenerator**: [DefaultContractProjector.cs](/src/BuildingBlocks/AI.Eidos/Negotiation/Internal/DefaultContractProjector.cs):L19 で `IVKGuidGenerator` を注入し、`VKPatternId.New(_guidGenerator)` で使用。`Guid.NewGuid()` の直呼びなし。
- **IVKJsonSerializer**: [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L10 および [DefaultMaterializationMiddleware.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationMiddleware.cs):L27 で注入・使用。
- **Result<T> パターン**: 全公開インターフェースが `VKResult<T>` を返却。Infrastructure 境界の `JsonException` は `catch` で捕捉後 `VKResult.Failure<T>` にマッピング。

### 🚩 違反箇所

#### 🔴 CS.06 — `JsonSerializer.Deserialize` 直呼び

- **箇所**: [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L135
- **内容**: `Bind(string rawJson, Type targetType, ...)` メソッド内で `JsonSerializer.Deserialize(cleanedJson, targetType)` を直接呼び出している。同クラスの `Bind<T>` メソッドでは `_jsonSerializer.Deserialize<T>` を正しく使用しているが、非ジェネリック版では `IVKJsonSerializer` を経由していない。
- **影響**: `IVKJsonSerializer` で設定されたカスタム `JsonSerializerOptions`（命名規則、コンバータ等）が適用されず、デシリアライズ結果に不整合が発生する可能性がある。

#### 🟡 CS.01 — `throw new NotSupportedException` の使用

- **箇所**: [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L94, L128
- **内容**: `ToleranceMode != Strict` の場合に `throw new NotSupportedException` を使用。CS.01 では Infrastructure 層以外での `throw` を避け、`Result.Failure<T>` を返すべきとしている。
- **影響**: 呼び出し元が `Result<T>` を期待している中で未処理例外が発生し、パイプライン全体がクラッシュする可能性がある。

#### 🟡 CS.01 — インラインエラー文字列

- **箇所**: [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L104-107, L114-117, L138-141, L148-151、[DefaultMaterializationValidator.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationValidator.cs):L72-75
- **内容**: `new VKError("AI.Eidos.Binding.NullResult", ...)` のようにエラーコード・メッセージがインラインで記述されている。CS.01 では専用 `Errors` クラスの `static readonly` 定数を使用すべきとしている。
- **推奨**: `VKAIEidosErrors.cs` を `Common/` に作成し、エラー定数を一元管理する。

#### 🟡 例外ハンドリング — bare `catch` ブロック

- **箇所**: [DefaultContractProjector.cs](/src/BuildingBlocks/AI.Eidos/Negotiation/Internal/DefaultContractProjector.cs):L143, L241、[DefaultContractNegotiator.cs](/src/BuildingBlocks/AI.Eidos/Negotiation/Internal/DefaultContractNegotiator.cs):L98、[SchemaFingerprint.cs](/src/BuildingBlocks/AI.Eidos/Schema/Internal/SchemaFingerprint.cs):L28
- **内容**: `catch` のみで例外型を指定せず、ログ出力やトレーシングもなく、静かにフォールバックしている。
- **影響**: デバッグ時に根本原因の特定が困難。予期しない例外（`OutOfMemoryException` 等）も飲み込まれる。

## 深度ロジック＆状態演進審査 (Deep Logic & State Evolution Audit)

### 脳内実行: 成功パス

1. `DefaultSchemaStage.ExecuteAsync` → `context.State<VKAIEidosResponseContract>()` or `_resolver.ResolveFromArgsAsync` → `context.SetState(contract)` ✅
2. `DefaultNegotiationStage.ExecuteAsync` → `_capabilityDetector.DetectCapabilities` → `_negotiator.Negotiate` → `context.SetState(negotiationResult)` → `_projector.ApplyProjection` → `context.SetArgs(projectedChatArgs)` → PromptJson の場合はフラグメント注入 ✅
3. `DefaultMaterializationMiddleware.InvokeAsync` → `next()` で LLM コール → `ExtractRawJson` → `_validator.Validate` → 成功 → `_binder.Bind` → `SetEnvelope` → `context.ResponseBuilder.ModelResult` に格納 ✅

**状態の伝播**: `VKPsycheContext` の `SetState` / `SetArgs` / `AddFragment` / `ResponseBuilder` を介して一貫して伝播。最終結果は `ModelResult` と `Metadata["VKMaterializationEnvelope"]` に正しく格納される。

### 脳内実行: 失敗パス

1. スキーマ解決失敗 → `ResolveFromArgsAsync` が `null` を返却 → `DefaultSchemaStage` は `contract is not null` チェック後、`VKResult.Success()` を返す → **契約なしでパイプライン続行（設計通り）** ✅
2. LLM 応答が JSON なし → `ExtractRawJson` が `null` → `SetEnvelope(model: null, rawContent: null, issues: ["No JSON content..."])` → `VKResult.Success()` ✅ — エンベロープに問題が記録される
3. バリデーション失敗 → リトライポリシー → 最大試行超過 → Circuit Breaker → `SetEnvelope(model: null)` + `VKResult.Success()` ✅

### 🔍 発見されたロジック懸念

#### ⚠️ `DefaultMaterializationMiddleware` — `validationRes.Value` の null アクセスリスク

- **箇所**: [DefaultMaterializationMiddleware.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationMiddleware.cs):L89
- **内容**: `validationRes.IsSuccess && validationRes.Value.IsValid` チェックが `false` の場合、L89 で `validationRes.Value` が `null` の可能性がある（`validationRes.IsFailure` の場合）。`?? new VKMaterializationValidationResult { ... }` でフォールバックしているが、`VKResult` が Failure の場合に `Value` プロパティへのアクセスが安全かどうかは `VKResult<T>` の実装に依存する。
- **影響**: `VKResult<T>.Value` が Failure 時に例外をスローする実装の場合、ランタイムクラッシュの可能性がある。

#### ⚠️ `DefaultContractProjector.SynthesizeExampleJson` — `JsonSerializerOptions` 直使用

- **箇所**: [DefaultContractProjector.cs](/src/BuildingBlocks/AI.Eidos/Negotiation/Internal/DefaultContractProjector.cs):L140
- **内容**: `exampleObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true })` で `new JsonSerializerOptions` を直接インスタンス化。`IVKJsonSerializer` 未経由だが、これはデバッグ/表示目的の整形出力であり、データバインディングではないため影響は軽微。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

- ❌ **[CS.06 — Core 抽象違反]**: [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L135 — `JsonSerializer.Deserialize(cleanedJson, targetType)` が `IVKJsonSerializer` を経由せず直接呼び出されている。同クラスの `Bind<T>` メソッド（L101）では `_jsonSerializer.Deserialize<T>` を正しく使用しており、**非ジェネリック版のみが不整合**。カスタム `JsonSerializerOptions` が適用されないため、命名規則・コンバータの不一致によりデシリアライズ失敗のリスクがある。

- ❌ **[CS.01 — throw 禁止違反]**: [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L94, L128 — `throw new NotSupportedException` は BuildingBlock 層で禁止されている。`Bind` メソッドの戻り値は `VKResult<T>` であり、`VKResult.Failure<T>(...)` を返すべき。呼び出し元の `DefaultMaterializationMiddleware` は `VKResult` を前提としたエラーハンドリングを行っているため、未処理例外によるパイプラインクラッシュのリスクがある。

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

- 🔒 **[Regex DoS リスク]**: `DefaultMaterializationBinder.ThinkTagRegex` および `DefaultStreamingParser.ThinkTagRegex` で `[\s\S]*?` を使用。悪意のある LLM 応答（巨大な `<think>` ブロック）によるバックトラッキングの可能性がある。`RegexOptions.NonBacktracking` (NET 7+) の適用を検討すべき。
- 🔒 **[メモリ使用]**: `DefaultMaterializationMiddleware` の `while` ループ内で `accumulatedIssues` / `accumulatedErrors` が無制限に蓄積される。`maxAttempts` で上限があるが（最大 10）、各試行で大量のバリデーションエラーが発生する場合は GC 圧力が増大する。
- 🔒 **[bare catch ブロック]**: `SchemaFingerprint.cs:L28`, `DefaultContractProjector.cs:L143,L241`, `DefaultContractNegotiator.cs:L98` — 例外型を指定せず静かに飲み込んでいる。`OutOfMemoryException` や `StackOverflowException` のような致命的例外も捕捉される。

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

- ⚙️ **[テスト容易性 — 優秀]**: 全サービスがインターフェース経由で DI 登録されており、`NSubstitute` / `Moq` 等でのモック差し替えが容易。`VKGuard` による境界チェックにより、テスト時のエッジケース検出も明確。
- ⚙️ **[static クラスのテスト]**: `VKMaterializationEnvelopeFactory`、`SchemaComposer`、`SchemaFingerprint` は `static` だが、純粋関数的な実装であるため単体テストは容易。
- ⚙️ **[唯一の懸念]**: `DefaultSchemaFactory` が `System.Reflection` と `NullabilityInfoContext` に依存しているが、これはリフレクションベースのスキーマ生成という性質上不可避。

## 🔭 可観測性の準拠度 (Observability Readiness)

- 📡 **[DiagnosticsConstants]**: [VKAIEidosDiagnosticsConstants.cs](/src/BuildingBlocks/AI.Eidos/Common/Diagnostics/VKAIEidosDiagnosticsConstants.cs) にて Activity Tracing Names（6 種）、Metric Instrument Names（5 種）、Semantic Tags（9 種）が包括的に定義されている。BB.04 準拠。
- 📡 **[VKBlockDiagnostics]**: `[VKBlockDiagnostics<VKAIEidosBlock>]` が `AIEidosDiagnostics.cs` に適用済み。
- 📡 **[未実装の DiagnosticsConstants 活用]**: `VKAIEidosDiagnosticsConstants` は定義されているが、実装コード内で直接参照・計装されている箇所がない。Diagnostics 定数が「定義のみ」の状態。将来の計装フェーズで活用される前提と推察される。

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

- ⚠️ **[JSON 抽出ロジックの重複]**: `DefaultMaterializationBinder.ExtractJsonBlock` と `DefaultStreamingParser.LocateActiveJsonCandidate` で ThinkTag 除去 + JSON 候補抽出パターンが重複。共通ユーティリティ（`Common/Shared/` 内）への抽出を推奨。
- ⚠️ **[SchemaComposer.MergeSchemas の例外飲み込み]**: [SchemaComposer.cs](/src/BuildingBlocks/AI.Eidos/Schema/Internal/SchemaComposer.cs):L75 で `catch (Exception)` により全例外を飲み込み、フォールバックとして元のスキーマを返却。ログ出力なしで根本原因の特定が困難。
- ⚠️ **[VKMaterializationEnvelopeFactory の公開範囲]**: `public static class` だが、主に `DefaultMaterializationMiddleware`（internal）からのみ使用される。本当に public API として公開する必要があるか再検討の余地がある。

## ✅ 評価ポイント (Highlights / Good Practices)

1. **完璧な垂直スライス構造**: 4 Feature Slices（Materialization / Negotiation / Schema / Streaming）が `[VKFeature]` SG により標準化された DI 登録パターンを採用。BB.01 に完全準拠。
2. **VKGuard の徹底使用**: 50+ 箇所で一貫した境界防御。Primary constructor + fluent assignment パターンが全クラスで統一。
3. **100% sealed 率**: 全 25 型（class + record）が `sealed` 宣言。AP.01 完全準拠。
4. **Result<T> パターンの全面採用**: 公開インターフェースの全メソッドが `VKResult<T>` を返却。Infrastructure 境界の `JsonException` は適切に `Result.Failure` にマッピング。
5. **3段階リトライポリシー**: `DefaultMaterializationMiddleware` の AutoRepair → AcceptPartial → Abort + Circuit Breaker パターンは、LLM 応答の不確実性に対する堅牢な防御戦略。
6. **スキーマ進化分析**: `DefaultSchemaEvolutionAnalyzer` による互換性チェック（Breaking / Compatible / Identical）と歴史的ペイロード検証は、スキーマバージョニングにおけるエンタープライズレベルの設計。
7. **`IVKGuidGenerator` の正しい使用**: `DefaultContractProjector` で `VKPatternId.New(_guidGenerator)` を使用。CS.06 準拠。
8. **ConcurrentDictionary + Lazy<T>**: `DefaultContractProjector` のキャッシュ設計はスレッドセーフかつメモリ効率的。

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| # | 対象ファイル | 改善内容 | Rule |
|:--|:------------|:---------|:-----|
| 1 | [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L135 | `JsonSerializer.Deserialize` → `_jsonSerializer.Deserialize` に置換。`IVKJsonSerializer` に非ジェネリック `Deserialize(string, Type)` メソッドがない場合は、ジェネリック版を `typeof(T)` 対応に拡張するか、リフレクション呼び出しを `IVKJsonSerializer` 経由にラップ | CS.06 |
| 2 | [DefaultMaterializationBinder.cs](/src/BuildingBlocks/AI.Eidos/Materialization/Internal/DefaultMaterializationBinder.cs):L94, L128 | `throw new NotSupportedException` → `return VKResult.Failure<T>(VKAIEidosErrors.Binding.UnsupportedToleranceMode)` に置換 | CS.01 |
| 3 | `Common/` に `VKAIEidosErrors.cs` を新規作成 | インライン `new VKError(...)` を全て `static readonly` 定数に移行 | CS.01 |

### 2. リファクタリング提案 (Refactoring)

| # | 対象 | 改善内容 | Rule |
|:--|:-----|:---------|:-----|
| 4 | bare `catch` ブロック | `catch (JsonException)` 等に限定し、必要に応じてログ出力を追加 | — |
| 5 | JSON 抽出ロジック | `DefaultMaterializationBinder.ExtractJsonBlock` と `DefaultStreamingParser.LocateActiveJsonCandidate` の共通部分を `Common/Shared/` にユーティリティとして抽出 | DRY |
| 6 | `VKMaterializationOptions` | `MaxRepairAttempts < MaxTotalAttempts` の整合性検証を `ValidateBlockCustom` に追加 | BB.05 |
| 7 | Regex | `RegexOptions.NonBacktracking` の適用を検討（.NET 7+ 必須） | Performance |

### 3. 推奨される学習トピック (Learning Suggestions)

- **`IVKJsonSerializer` 拡張**: 非ジェネリック `Deserialize(string, Type)` のサポートにより、リフレクションベースのバインディングシナリオでも Core 抽象を維持する方法。
- **OpenTelemetry 計装**: `VKAIEidosDiagnosticsConstants` に定義済みのメトリクス・トレーシングトークンを実装コードに計装し、可観測性を完成させる方法。

---

Audit: 🚩 [CS.06] `DefaultMaterializationBinder.Bind(string, Type)` で `JsonSerializer.Deserialize` 直呼び | 🚩 [CS.01] `throw new NotSupportedException` 使用 | 🚩 [CS.01] インラインエラー文字列（`new VKError(...)` 定数未使用）

Phase 1: 28/30 (93%) | Phase 2: PASS (SG-automated) | Phase 3 Score: 88/100
