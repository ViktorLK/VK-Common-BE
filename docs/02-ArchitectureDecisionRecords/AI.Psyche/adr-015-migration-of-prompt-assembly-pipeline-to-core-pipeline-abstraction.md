# ADR 015: Migration of Prompt Assembly Pipeline to Core Pipeline Abstraction

- **Date**: 2026-06-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

以前の決定（ADR 006）により、AI.Psyche 内で並行グループと実行順序を制御するカスタムWeavingパイプラインランタイム（`VKWeavingStepRunner` 等）が導入された。また、プロンプト組み立てプロセスの前後処理として複数のステージ（Before/After Stage）が定義され、動作していた。
しかし、VK.Blocks 内で別のシーケンシャルワークフロー（ナレッジインジェクションのステージ制御など）の需要が高まるにつれ、独自のパイプライン基盤を個別アセンブリで維持することは、保守性の低下と可観測性（Logging/Tracing）の実装の乖離を招く原因となった。
これを受け、共通コア `VK.Blocks.Core` にて汎用的なパイプライン実行エンジン（`VKPipelineExecutorBase` 等）が標準化（Core/ADR 019）された。

## 2. Problem Statement (問題定義)

AI.Psyche が持つ高度なWeaving順序制御・並行チャンク制御を維持しつつ、システム共通のパイプライン実行基盤に統合することで、コードの重複を排除し、プラットフォーム共通の可観測性ミドルウェアの恩恵を受けられるように移行設計を行う必要があった。

## 3. Decision (決定事項)

AI.Psyche のプロンプト構築ランタイムを、**「Core 共通パイプラインフレームワーク（`VKPipelineExecutorBase`）へ移行・統合する」**。

### 1. 基底エグゼキューターの継承
- `DefaultPsychePipelineExecutor` は、`VKPipelineExecutorBase<VKPsycheContext>` を継承する。
- 状態コンテキストには、既存の型キー拡張に対応した `VKPsycheContext` をそのまま型パラメータとして引き渡す。

### 2. インターフェースの整合
- 既存の Psyche 独自定義ステージインターフェース（`IVKPsycheBeforePipelineStage` 等）を、Core 側の標準ステージインターフェース（`IVKBeforePipelineStage<VKPsycheContext>` 等）を継承する形に定義し直す。
- 実行スケジュールスケジュール情報（`VKStageSchedule`）を、Core 共通の `VKPipelineStageSchedule` へ置換する。

### 3. ミドルウェア層の共通化
- ログ記録やイベント処理など、Psyche 固有の診断インターセプションを `IVKPsycheMiddleware` (Core の `IVKMiddleware<VKPsycheContext>` を継承) として再定義し、共通のランタイムパイプラインパイプラインにバインドする。

```
[DefaultPsychePipelineExecutor]
   : Inherits VKPipelineExecutorBase<VKPsycheContext>
   |
   +--> Runs standard pipeline engine in Core
   +--> Dispatches to target Psyche stages (Persona, Echo, Weaving etc.)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: AI.Psyche のパイプラインを現状維持
- **Approach**: Core パイプラインは他の簡易的なワークフローにのみ適用し、Psyche は Weaving などの並行処理が特殊なためカスタムランタイムを使い続ける。
- **Rejected Reason**: 複雑なモジュールこそ、一貫したロギングやトレース（OpenTelemetry 統合）などの共通機能が必要であり、インフラが分離していると監視漏れや動作バグを誘発しやすいため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **インフラコードの削減**: 順序並べ替えやミドルウェアの適用ループなど、低レベルなパイプライン処理コードが AI.Psyche から排除され、メンテ対象コードが大幅に削減された。
- **一貫した可観測性**: Core のパイプラインランタイムを通じて自動的に構造化診断ログが統一フォーマットで出力される。

### Negative
- **既存ステージインターフェースの破壊的変更**: `IVKPsycheBeforePipelineStage` 等の定義が変更されるため、カスタムステージを追加していたプロジェクトへのコード修正が必要となる。

### Mitigation
- 本リリース前に移行用リリースノートを明記し、DI 登録周りのシグネチャ変更を最小限に抑えるよう、Psyche 側のビルダ拡張メソッド (`VKAIPsycheBuilderExtensions`) 内で変更差分を吸収するラッパーを提供する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- パイプラインの実行順序およびステージのトグル（`IsActive`）は、移行後も `VKPipelinesOptions` の定義に基づいて検証される。
- 各ステージで発生した処理エラーは、Core 側のエラー収集ハンドラによって自動的に補足され、安全な `VKPipelineErrors` にマッピングして呼び出し元に返される。

## 7. Status
✅ Accepted
