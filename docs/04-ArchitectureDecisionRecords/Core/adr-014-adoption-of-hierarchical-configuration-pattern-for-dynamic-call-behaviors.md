# ADR 014: Adoption of Hierarchical Configuration Pattern for Dynamic Call Behaviors

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core / Overall Architecture Standard

## 2. Context (背景)

VK.Blocks の各ビルドブロック（AI, Caching, Storage, Messaging 等）の実装において、特定のパラメータ（Timeout, RetryCount, Temperature, TTL 等）に対して、アプリケーション全体での統一的な既定値（Global Defaults）と、リクエスト単位での動的な上書き（Local Overrides）を両立させる必要性が高まった。

## 3. Problem Statement (問題定義)

静的な構成（`IOptions`）のみに依存すると、実行時の柔軟な変更（例：特定のリクエストだけキャッシュ期間を短くする、AIの温度を上げる等）ができなくなる。一方で、すべての呼び出しに対してパラメータの指定を強制すると、呼び出し側のコードが極めて冗長になり、開発者の生産性が低下する。
また、パラメータの優先順位（どちらが優先されるか）が各モジュールで異なると、システム全体の挙動の予測可能性が損なわれる。

## 4. Decision (決定事項)

VK.Blocks 全体の共通設計原則として **「階層的構成パターン（Hierarchical Configuration Pattern）」** を採用する。

### 設計の三原則

1.  **Global Defaults (Options)**:
    - 各モジュールの `IVKBlockOptions` を通じて供給される静的な既定値。
    - アプリケーションとしての「標準的な挙動」を定義する。

2.  **Local Overrides (Args)**:
    - 各機能ごとに `XxxArgs` または `XxxRequest` record を定義する。
    - メソッドの引数として `null` 許容（オプション）で受け取る。

3.  **Merging Priority (優先順位)**:
    - 実装レイヤーでは常に以下の優先順位で値を解決することを義務付ける。
    - **`args?.Property ?? Options.Property`**

### コード例 (Implementation Draft)

```csharp
// 1. Options (Global)
public sealed record VKFeatureOptions : IVKBlockOptions {
    public int TimeoutSeconds { get; init; } = 30;
}

// 2. Args (Local Override)
public sealed record VKFeatureArgs {
    public int? TimeoutSeconds { get; init; }
}

// 3. Implementation (Merge)
public async Task DoWorkAsync(..., VKFeatureArgs? args = null) {
    // グローバル既定値をローカル指定（引数）で上書きする
    int timeout = args?.TimeoutSeconds ?? _options.TimeoutSeconds;
    // ... logic ...
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: すべての引数をメソッドの第一引数（必須）にする**
    - **Rejected Reason**: シンプルなユースケースでも冗長な指定が必要になり、ライブラリとしての使い勝手が低下する。
- **Option 2: `IOptionsSnapshot` をリクエストスコープで書き換える**
    - **Rejected Reason**: 設定ファイルの書き換えを伴うためオーバーヘッドが大きく、またスレッドセーフティや副作用の管理が困難。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- **ガバナンスと柔軟性の両立**: 構成ファイルによる一律管理と、コードによる局所的制御が明確に分離される。
- **クリーンな API**: 既定値で十分な場合は引数を省略でき、特殊な場合のみ詳細な指定を行う洗練されたインターフェースを提供できる。
- **一貫性**: すべてのビルドブロックで同じ「解決ルール」が適用されるため、学習コストが低い。

### Negative
- **実装の定型化**: マージロジック（`??`）を記述する手間。

### Mitigation
- **設計ガイドライン**: 本ADRにより設計指針を明確化し、コードレビュー時のチェック項目とする。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **不変性**: `ExecutionSettings` はすべて `sealed record` とし、実行中の設定変更による副作用を防止する。
- **制約**: セキュリティやコストに直結するパラメータについては、Options 側で「ハード上限（Hard Limit）」を定義し、ExecutionSettings でそれを超える値が指定されても無視するガードロジックの導入を推奨する。
