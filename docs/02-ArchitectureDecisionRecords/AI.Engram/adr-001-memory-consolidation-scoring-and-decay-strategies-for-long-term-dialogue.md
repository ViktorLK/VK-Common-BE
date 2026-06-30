# ADR 001: Memory Consolidation, Scoring, and Decay Strategies for Long-Term Dialogue

- **Date**: 2026-06-15
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Engram

## 1. Context (背景)

長期的な対話が可能な AI エージェント（ロールプレイや継続対話システム）を構築する際、エージェントは過去の会話履歴や事実情報（記憶トレース = Engram / エングラム）を保持し、適切に思い出す必要がある。しかし、過去の対話や事実を無制限にプロンプトのコンテキストに詰め込み続けると、すぐに LLM のトークン制限を超過し、かつ無関係な過去情報によって現在の応答品質が劣化（コンテキスト汚染）する。これに対処するには、記憶の「重要度スコアリング」「時間経過に伴う減衰（忘れやすさ）」「古い記憶の要約・統合」「不要な記憶の排除」といった人間の脳に似た記憶整理のメカニズムが必要である。

## 2. Problem Statement (問題定義)

単純な履歴保持やベクトルストア検索のみによる記憶管理には、以下の課題がある：
1. **重要度と鮮度のトレードオフ管理の欠如**: 直近の些細な雑談と、3日前の極めて重要な約束事が、ベクトル類似度や時間順だけで一様に扱われ、本当に優先すべき記憶が埋もれてしまう。
2. **アロケーションおよびトークン超過**: 記憶のライフサイクル（整理・圧縮）を司る共通のエンジンがないため、アプリケーションごとにその場限りの文字列圧縮や切り詰めロジックが実装され、再利用性がない。
3. **記憶の「忘却」プロセスの欠如**: 古くなった情報や低価値な情報を安全に「減衰・破棄」してコンテキストから排除する統一的なインターフェースが定義されていない。

## 3. Decision (決定事項)

エージェントの長期的記憶とコンテキスト制御を体系化するため、新設の **`AI.Engram` モジュール**において**「Engram Core Memory Strategies (エングラム・コア記憶戦略)」**を採用する。

1. **`AI.Engram` ビルディングブロックの新設**:
   - 記憶の整理・評価・忘却をカプセル化し、ピュアな記憶ライフサイクルエンジンとして実装する。
2. **戦略パターンの導入による関心の分離**:
   - 記憶の整理アルゴリズムを以下の4つの独立した Strategy インターフェースとして定義し、ポリモーフィックに差し替え可能とする：
     - `IVKScoringStrategy`: 各エングラム（記憶）の関連性・感情値・基本重要度を評価するスコアリング戦略。
     - `IVKDecayStrategy`: 時間経過（ターン数や時間）に伴う記憶の減衰度（忘却率）を算出する減衰戦略。
     - `IVKConsolidationStrategy`: 古くなった記憶エングラム群を統合し、要約・要約事実（Consolidated Engram）に圧縮する統合戦略。
     - `IVKPruningStrategy`: 容量制限（Token Budget）や関連しきい値に基づき、不要になった低価値記憶を完全に排除（Prune）する刈り込み戦略。
3. **Psyche パイプラインとの疎結合連携**:
   - `AI.Engram` 内部のロジックは、プロンプトのレンダリングとは独立して機能し、Engram 側のストアから要約・選別された記憶エントリが、Psyche や Corpus 側の Gathering 段階に流れるデータソースとして疎結合に統合される。

### 核心的な記憶制御インターフェース設計

```csharp
namespace VK.Blocks.AI.Engram;

// 1. 重要度・関連性評価
public interface IVKScoringStrategy
{
    Task<double> ScoreAsync(VKEngram engram, VKScoringContext context, CancellationToken ct = default);
}

// 2. 時間減衰 (忘却曲線)
public interface IVKDecayStrategy
{
    double CalculateDecay(VKEngram engram, int currentTurn);
}

// 3. 記憶圧縮・統合
public interface IVKConsolidationStrategy
{
    Task<VKResult<IReadOnlyList<VKEngram>>> ConsolidateAsync(IReadOnlyList<VKEngram> engrams, CancellationToken ct = default);
}

// 4. 刈り込み
public interface IVKPruningStrategy
{
    IReadOnlyList<VKEngram> Prune(IReadOnlyList<VKEngram> engrams, VKPruningOptions options);
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Handle Memory Compacting in Vector Store layer directly
- **Approach**: Vector Store 自体の更新トリガー（例: トリガー発火時に古い類似ベクトルを削除・マージする）で解決する。
- **Rejected Reason**: データベースエンジン側に高度な言語理解（要約や感情関連性の再スコアリング）を求めることになり、インフラの移植性を損なうため。

### Option 2: Monolithic AI.Psyche Memory Stage
- **Approach**: `AI.Psyche` 内にすべての記憶圧縮・減衰ロジックを統合する。
- **Rejected Reason**: 記憶の「意味的統合（Consolidation）」にはそれ自体が単独で LLM を呼び出して要約を生成するなどの重い非同期プロセスが伴い、Psyche のシンプルなプロンプト組装エンジンとしてのライフサイクルと合致しないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **認知的記憶モデルの実現**: 重要な事実（約束、設定など）はスコアが高く維持され、些細な会話は速やかに減衰・要約されていくため、人間に近い極めて自然で持続性のある対話が可能になる。
- **プラグイン可能な戦略**: アプリケーション（例: 感情重視の会話ボット、タスク完了重視の業務アシスタント）の特性に合わせて、スコアリングや減衰アルゴリズムを自在に差し替え可能。

### Negative
- **記憶の要約時の追加コスト**: 記憶統合（Consolidation）を実行する際、要約のための LLM 呼び出しがバックグラウンドで走るため、API コストが追加で発生する。

### Mitigation
- 記憶統合処理を対話フロー（リクエストのホットパス）の同期実行から完全に切り離し、セッション終了時や、バックグラウンドの低優先度非同期ジョブ（Cron 実行等）にてバッチ実行することで、ユーザーのレスポンス遅延とコストのスパイクを最小化する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **PII Protection on Consolidation**: 記憶を要約・統合（Consolidation）してデータベースに再格納する際、元のエングラムに含まれていた個人情報や機密データ（PII）が要約結果に不用意に固定化されて残らないよう、要約用プロンプトに厳格なプライバシー保護指示を埋め込む。

## 7. Status
✅ Accepted
