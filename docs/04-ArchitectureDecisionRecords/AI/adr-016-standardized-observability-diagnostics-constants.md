# ADR 016: Standardized Observability Diagnostics Constants

- **Date**: 2026-06-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI

## 1. Context (背景)

VK.Blocks 内の AI モジュールおよびその関連サブモジュール（AI.Corpus, AI.Psyche, AI.Engram 等）は、複雑な推論パイプラインや記憶制御サイクルを実行する。これらの分散的かつマルチステップな処理において、問題の迅速な特定やパフォーマンスのモニタリングを実現するためには、高度な可観測性（Observability）が欠かせない。
しかし、これまでは各モジュール・機能ごとに診断用トークンやログ定義が個別にハードコードされており、以下の問題が生じていた：
1. **診断キーの重複と衝突**: メトリクス名やトレースのイベント名が分散的に定義され、重複やスペルミスのリスクが高かった。
2. **イベント ID 体系の崩壊**: ソースジェネレーターによる構造化ログ（`[LoggerMessage]`）のイベント ID が重複し、ログ集約ツール（Elasticsearch, OpenSearch 等）でのパースエラーを引き起こしていた。
3. **ダッシュボード構築の困難さ**: 一貫したトークン構造がないため、Prometheus/Grafana でモジュール横断的なグラフを描画するクエリが極めて複雑になっていた。

## 2. Problem Statement (問題定義)

システム全体の診断メタデータ（Meters、ActivitySources、LoggerEventIds）をモジュール・機能横断で衝突なく一意に管理し、高いメンテナンス性と検索性を両立する、統一的な可観測性診断常数の設計パターンが必要であった。

## 3. Decision (決定事项)

AI 関連モジュールにおいて、**「オフセットベースの一意イベント ID 管理と標準化診断定数クラス（Standardized Diagnostics Constants）の導入」**を決定する。

### 1. オフセットベースのイベント ID 管理 (`VKDiagnosticOffsets`)
- 全体の ID 空間を整理し、各機能ブロックに対してユニークな数値オフセットを割り当てる。
- 例：
  - 共通 AI 基礎: `VKDiagnosticOffsets.AIBase` (3000)
  - AI.Psyche ビヘイビア: `VKDiagnosticOffsets.Behaviors` (3100)
  - AI.Psyche 指示文: `VKDiagnosticOffsets.Directive` (3200)
  - AI.Psyche エコー: `VKDiagnosticOffsets.Echo` (3300)
  - AI.Psyche 知識インジェクション: `VKDiagnosticOffsets.Knowledge` (3400)
  - AI.Psyche ペルソナ: `VKDiagnosticOffsets.Persona` (3500)
  - AI.Psyche 織り込み（Weaving）: `VKDiagnosticOffsets.Weaving` (3600)

### 2. 診断定数の局所化と Feature-first 配置 (AP.03 の適用)
- 単一の巨大な `Constants.cs` を作るのを禁止し、各機能フォルダーの `Diagnostics/` 配下に `VK{FeatureName}DiagnosticsConstants.cs` を作成する。
- 例: `VKBehaviorsDiagnosticsConstants` 内で、ビヘイビア制御固有の Activity 名、Meter 名、および `VKDiagnosticOffsets.Behaviors` から算出した一意のイベント ID を定義する。

### 3. ソースジェネレーターとの統合
- `[LoggerMessage]` 属性を持つログ定義（例：`BehaviorsDiagnostics`）は、対応する `VKBehaviorsDiagnosticsConstants` の一意 ID を参照し、コンパイル時点で一意性が保証された部分クラスとして実装される。

```csharp
// 例：指示文機能の診断定数定義
internal static class VKDirectiveDiagnosticsConstants
{
    public const string FeatureName = "AI.Psyche.Directive";
    
    // イベント ID の一意性をオフセットで保証
    private const int Offset = VKDiagnosticOffsets.Directive; // 3200
    
    public const int LoadStoreFailed = Offset + 1;
    public const int CompilationError = Offset + 2;
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: 動的な ID 生成 (Runtime Allocation)
- **Approach**: 起動時にプログラムで自動的に ID をインクリメントしながらマッピングする。
- **Rejected Reason**: ログメッセージのスキーマは静的（コンパイル時）に確定している必要があり、ソースジェネレーターの `[LoggerMessage]` はコンパイル定数しか受け取れないため却下。

### Option 2: 一元管理用の超巨大 Constants クラス
- **Approach**: ソリューション全体のイベント ID や定数を 1 つのファイル `VKDiagnosticsGlobalConstants.cs` に集約する。
- **Rejected Reason**: 複数人開発でコンフリクトが頻発する上、各 BuildingBlock のカプセル化（依存関係の分離）を破壊するため却下した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **ID 競合の完全排除**: 静的なオフセット設定により、新機能追加時も既存の ID 体系を破壊することなく安全にスケールアウトできる。
- **ダッシュボード定義の簡素化**: `AI.Psyche.*` などの階層的かつ規則的な命名体系により、Grafana 側でのワイルドカード検索や動的フィルタリングが容易になる。
- **カプセル化の維持**: Feature フォルダ内に定数が完結するため、AP.03 の「一ファイル一タイプ」および「不要な VK プレフィックスの制限」に整合する。

### Negative
- **オフセット管理簿の維持管理**: `VKDiagnosticOffsets` という中央管理用の enum/class をメンテナンスする必要がある。

### Mitigation
- 新たに機能（Building Block 又は Feature）を追加する際のみオフセット定義を増やすルールとし、開発者ガイドに「オフセットの確保手順」を明文化する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- 診断定数にシステム設定情報や機密データ（API キー等）を含めないこと。
- メトリクス名や Activity 名は OpenTelemetry 標準仕様（Semantic Conventions）に準拠させる。

## 7. Status
✅ Accepted
