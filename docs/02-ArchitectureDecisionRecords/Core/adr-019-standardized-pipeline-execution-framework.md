# ADR 019: Standardized Pipeline Execution Framework

- **Date**: 2026-06-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/Core

## 1. Context (背景)

VK.Blocks 内の高度なビジネスドメイン（プロンプト構築 `AI.Psyche` やナレッジインジェクション `AI.Corpus` など）では、状態コンテキストを共有しながら、一連の処理ステップ（ステージ）をシーケンシャルに実行する複雑なワークフローを構築している。
従来、これらのパイプライン実行ロジックは各機能モジュールごとに個別に実装されていたため、以下の課題があった：
1. **コードの重複**: ステージのフィルタリング、並行度制御、例外ハンドリング、ステージのスケジューリング（実行順制御）などのランタイム処理コードが各モジュールで重複して実装されていた。
2. **横断的関心事の統合の難しさ**: ロギング、メトリクス収集、ガバナンス監査、リトライなどの処理（Middleware）をすべてのパイプラインへ一貫して適用する仕組みが不足していた。

## 2. Problem Statement (問題定义)

パイプラインの実行基盤におけるコードの重複を排除し、テスト容易性、高い拡張性（OCP）、および横断的関心事（可観測性・回復性）を一括適用可能な、VK.Blocks 共通の標準パイプライン実行エンジンが必要であった。

## 3. Decision (決定事項)

共通コアライブラリ `VK.Blocks.Core` に、**「汎用パイプライン実行フレームワーク（VKPipeline Framework）」**を導入する。

### 1. 抽象モデルの定義 (`Core/Pipelines/Protocols`)
- **`IVKPipeline<TContext>`**: パイプラインの定義。順序づけられたステージの集合を保持する。
- **`IVKPipelineExecutor<TContext>`**: コンテキストを処理するためのスレッドセーフなパイプライン実行エンジン。
- **`IVKMiddleware<TContext>`**: 各ステージの前後、または実行前後にフック可能なミドルウェア抽象。
- 前後フックとして、`IVKBeforePipelineStage<TContext>` および `IVKAfterPipelineStage<TContext>` を共通定義する。

### 2. パイプライン実行基底クラス (`VKPipelineExecutorBase<TContext>`)
- 共通の実行アルゴリズム（Template Method パターン）を提供し、各ドメイン（例：PsychePipeline）がこの基底クラスを継承するように設計する。
- 例外ハンドリング、ミドルウェアの適用順序、ステージ実行スケジュール（`VKPipelineStageSchedule`）の順序制御などを一元的に管理する。

### 3. デリゲートモデルによる柔軟性の担保 (`VKPipelineDelegate<TContext>`)
- パイプラインの各ノードの実行シグネチャをデリゲートとしてモデル化し、ステージ自体が任意の実行スコープを動的に呼び出せるようにする。

```
[Pipeline Executor]
   |
   +--> Invoke Middleware (Before)
   |       |
   |       +--> Execute "BeforeStages" (Sorted by Schedule)
   |       +--> Execute Main Pipeline Core Delegate
   |       +--> Execute "AfterStages" (Sorted by Schedule)
   |
   +--> Invoke Middleware (After)
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: サードパーティのライブラリ（MediatR Pipeline Behaviors 等）の流用
- **Approach**: MediatR の Behavior 機能を拡張してシーケンシャルなパイプラインを実現する。
- **Rejected Reason**: MediatR は主に 1 対 1 の CQRS メッセージ処理を前提としており、VK.Blocks が必要とする「1リクエスト中に定義された多数の順序制御された微細ステージを、同一のインメモリコンテキストを破壊せずに高速に流す」という用途にはオーバーヘッドが大きく、かつ機能要件（Stage スケジューラ等）と不整合するため却下した。

### Option 2: 特化型（非ジェネリック）パイプラインの各個維持
- **Approach**: 共通化を行わず、Psyche 専用、Corpus 専用のパイプラインランタイムをそれぞれのプロジェクトで別々にメンテし続ける。
- **Rejected Reason**: 横断的関心事（例えば、「パイプラインの全ステージの実行時間と成功/失敗をメトリクスに出力する」等）を新規追加する際に、全アセンブリに同様のコードを追加・修正する必要が生じ、長期的な保守コストが極めて高くなるため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **DRY 原則の徹底**: 各モジュールは「ステージの実行ロジック」のみに集中でき、実行インフラ（順序制御、Middleware 適用等）を自前で実装する必要がなくなった。
- **プラグイン可能な可観測性とガバナンス**: `IVKMiddleware` を 1 つ書くだけで、システム全体のすべてのパイプラインに対してセキュリティスキャン、ログ追跡、またはサーキットブレイクを適用できる。

### Negative
- **抽象度の向上による認知的負荷**: ジェネリックな型パラメータ（`TContext`）やデリゲートモデルが多用されるため、新規開発者が処理の流れを追う際のコードリーディングの難易度がやや上昇する。

### Mitigation
- パイプラインの基本構成とデバッグ手法を開発者向けガイドドキュメント（README 等）にビジュアルな図解付きで記述する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- コンテキスト状態は参照透過性を意識して処理され、マルチスレッド環境でのデータ競合を防ぐための安全なスレッドスコープが保証される。
- ミドルウェアでのエラー発生時は、全体の処理が安全に遮断され、`Result.Failure` が即座に返却される。

## 7. Status
✅ Accepted
