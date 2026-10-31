# ADR 007: Compiled Expression Trees Matcher

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psycheの `Knowledge` 段階では、ユーザーのチャット入力テキストに対して、大量の知識エントリ（`VKKnowledgeEntry`）に設定されたキーワードや正規表現ルールを照合し、合致したナレッジをプロンプトに注入する。ルール数が数百から数千にスケールした際、毎リクエストでルール評価の動的解釈を実行していると、重大なCPUオーバーヘッドとGC（ガベージコレクション）圧力が発生する。また、ユーザーや管理者が定義した正規表現の品質によっては、ReDoS（正規表現の脆弱性によるサービス拒否攻撃）が発生するリスクもある。

## 2. Problem Statement (問題定義)

動的リフレクションやナイーブな正規表現評価には、以下の深刻な問題が存在する：
1. **CPU・アロケーションオーバーヘッド**: テキスト解析時に毎回文字列検査オブジェクトを新規作成し、評価ロジックをインタープリタ的にループ実行すると、LOH（大型オブジェクトヒープ）やGCの頻発に繋がる。
2. **ReDoS 脆弱性**: マッチングに使用される正規表現に `(a+)+` のようなバックトラックが指数関数的に増大するパターンが含まれていた場合、悪意あるユーザー入力によりスレッドが無限ループに陥り、CPUが100%に張り付いてサーバー全体がハングアップする。
3. **拡張性と保守性のトレードオフ**: AND・OR・NOT などの論理結合演算の条件判定を動的に評価するネストされた if/else ロジックは、コードが複雑化しやすくバグが生まれやすい。

## 3. Decision (決定事項)

高スループットな処理性能とReDoSに対する防御策を両立させるため、**「Compiled Expression Trees Matcher (コンパイル済みの式木マッチングエンジン)」**を採用する。

1. **`LINQ Expression Trees` による動的コンパイル**:
   - `VKKnowledgeEntry` がロードされた時点で、そのマッチングルール（`Keys`）と論理結合子（`FilterLogic`）を解析し、プログラムコードと同等のネイティブILに動的コンパイル（`Expression.Lambda.Compile()`）する。
2. **スレッドセーフなキャッシュ管理**:
   - 変換された実行可能デリゲート（`Func<string, bool>`）は、エントリーのIDをキーとして `ConcurrentDictionary` に永続キャッシュし、照合時はキャッシュされたデリゲートを直接呼び出す（Zero Reflection）。
   - 知識エントリの更新（Upsert/Delete）時には、対応するキャッシュエントリを明示的にクリア（Invalidate）する。
3. **厳格な実行タイムアウト制限**:
   - すべての正規表現マッチング（Regex / WholeWord）に対して、`TimeSpan.FromMilliseconds(100)` のタイムアウト制限を強制適用し、マッチング処理が100msを超えた場合は自動的に処理を切り離してエラーとする。

### 核心的な動的式木コンパイル設計

```csharp
namespace VK.Blocks.AI.Psyche;

public static class VKKnowledgeMatcher
{
    private static readonly ConcurrentDictionary<string, Func<string, bool>> _compiledMatchers = new();

    public static Func<string, bool> GetMatcher(VKKnowledgeEntry entry)
    {
        VKGuard.NotNull(entry);
        return _compiledMatchers.GetOrAdd(entry.Id, _ => CompileMatcher(entry));
    }

    private static Func<string, bool> CompileMatcher(VKKnowledgeEntry entry)
    {
        // 簡易フォールバックを実装しつつ、式木を組み立ててコンパイルする
        ParameterExpression contextParam = Expression.Parameter(typeof(string), "context");
        Expression primaryExpression = BuildCombinationExpression(contextParam, entry.Keys.ToList(), entry.FilterLogic);
        var lambda = Expression.Lambda<Func<string, bool>>(primaryExpression, contextParam);
        return lambda.Compile();
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Interpreter Pattern with Reflection
- **Approach**: 各ルールオブジェクトが `IsMatch(string text)` メソッドを持ち、内部で `switch` 文やリフレクションを用いて判定する。
- **Rejected Reason**: ルール数がスケールした際にメソッド呼び出しスタックとオブジェクトアロケーションが膨大になり、ミリ秒単位の性能要求を満たせないため。

### Option 2: Pre-compiled Source Generated Matchers
- **Approach**: ビルド時にすべての照合ルールをソースコードとして出力しておく。
- **Rejected Reason**: ナレッジベースやキーワードルールは運用中に管理画面から動的に追加・変更される性質のものであり、ビルド時静的コンパイルでは動的追加に対応できないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **ネイティブコード級の高速実行**: 実行時は単一のデリゲート呼び出し（CPUのインライン最適化が効く）となり、アロケーションが完全にゼロになるため、極めて高速なフィルタリングが可能。
- **堅牢なセキュリティ**: タイムアウトの設定により、不適切な正規表現によるCPUスレッド枯渇（ReDoS）が物理的に発生しなくなる。

### Negative
- **初回コンパイルオーバーヘッド**: 新しいルールが最初に評価される際、JITコンパイルが走るため、ミリ秒以下の遅延（Warm-upレイテンシ）が生じる。

### Mitigation
- アプリケーション起動時、または管理画面でルールが保存された時点でバックグラウンドタスクを走らせ、事前コンパイル（Pre-compilation）を完了させておくことで、本番リクエスト中のレイテンシへの影響を完全に排除する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Regex Compatibility Protection**: 正規表現のコンパイル中に構文エラーなどが発生した場合は、処理をクラッシュさせずに `Expression.Constant(false)`（常にマッチしない定数）を返す安全なフォールバックを実装する。
- **TimeSpan Constraint**: 正規表現エンジンに対してタイムアウト引数を持たないコンストラクタの使用をコードレビューおよび静的解析で禁止する。

## 7. Status
✅ Accepted
