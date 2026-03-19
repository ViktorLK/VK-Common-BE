# アーキテクチャ監査レポート — VK.Blocks.ExceptionHandling

**監査日**: 2026-03-16  
**対象モジュール**: `VK.Blocks.ExceptionHandling`  
**対象ファイル数**: 14 ファイル (.cs)  
**監査者**: VK.Blocks Lead Architect (AI)

---

## 📊 監査サマリー (Audit Summary)

- **総合スコア**: **72 / 100**
- **対象レイヤー判定**: Cross-Cutting Concern / Infrastructure Middleware
- **総評 (Executive Summary)**:

本モジュールは、ASP.NET Core ミドルウェアパイプラインにおける例外ハンドリングを一元化するための基盤モジュールである。Chain of Responsibility パターンによるハンドラパイプライン、RFC 7807 準拠の `ProblemDetails` 拡張、および `TraceId` の自動伝播など、VK.Blocks の設計哲学に沿った堅実な基盤が構築されている。

しかし、以下の点において改善の余地がある：

1. **`sealed` キーワードの欠如** — 大半のクラスが `sealed` 宣言されておらず、Rule 15 (Modern C# Semantics) に違反している。
2. **`ExceptionContext` が `class` で定義** — コンテキスト情報はイミュータブルな `record` であるべきところ、ミュータブルな `class` として実装されている。
3. **ハンドラ間のコード重複** — `BaseException` の Extensions をコピーするロジックが複数ハンドラに散在している。
4. **マジックストリング** — エラーコード (`"NotFound"`, `"Unauthorized"`, `"ValidationErrors"`, `"InternalServerError"`, `"stackTrace"`) がリテラルとしてハードコードされている。
5. **DI 登録でのオプション二重構築** — `AddExceptionHandling` メソッド内でオプションオブジェクトを手動で `new` & `Invoke` しており、Options パターンの意図を逸脱している。

---

## 🚨 重大なアーキテクチャの懸念事項 (Critical Architectural Smells)

### ❌ CS-01: `sealed` 宣言の欠如 (Rule 15 違反)

**該当箇所**: `ExceptionContext.cs`, `ProblemDetailsFactory.cs`, `BaseExceptionHandler.cs`, `NotFoundExceptionHandler.cs`, `ValidationExceptionHandler.cs`, `DefaultExceptionHandler.cs`, `ExceptionHandlerPipeline.cs`, `ExceptionHandlingMiddleware.cs`, `ExceptionHandlingOptions.cs`, `VKProblemDetails.cs`

VK.Blocks コーディング規約 Rule 15 では、明示的なポリモーフィズムが不要なすべての Application/Infrastructure クラスに `sealed` を付与することが義務付けられている。現状、**`UnauthorizedExceptionHandler` のみ**が `sealed` 宣言されており、他の 9 クラスは未対応である。

**影響**: 継承による意図しない振る舞い変更のリスク、JIT 最適化（仮想呼び出しの脱仮想化）の阻害。

**推奨**: 全クラスに `sealed` を付与すること。

---

### ❌ CS-02: DI 登録における Options パターンの逸脱

**該当箇所**: [ExceptionHandlingExtensions.cs](/src/BuildingBlocks/ExceptionHandling/DependencyInjection/ExceptionHandlingExtensions.cs#L33-L39)

```csharp
var optionsValue = new ExceptionHandlingOptions();
configure?.Invoke(optionsValue);

foreach (var handlerType in optionsValue.Handlers)
{
    services.AddScoped(typeof(IExceptionHandler), handlerType);
}
```

`ExceptionHandlingOptions` を `new` で直接インスタンス化し、`configure` デリゲートを手動で呼び出している。このアプローチには以下の問題がある：

1. **二重構築**: `services.Configure(configure)` (L26-27) で既に Options パイプラインに登録しているにもかかわらず、登録フェーズで再度構築している。Options の `PostConfigure` や他の `Configure` チェインが反映されない。
2. **型安全性の欠如**: `Handlers` プロパティが `IList<Type>` であり、`IExceptionHandler` を実装しない型でも追加可能。コンパイル時に検出できない。

**推奨**: 型安全な登録 API（ジェネリックメソッド `AddHandler<T>()` 等）への移行、または `IConfigureOptions<T>` パターンの採用を検討すること。

---

### ❌ CS-03: マジックストリングの散在 (Rule 13 違反)

**該当箇所**: 複数ファイル

| マジックストリング                      | ファイル                          | 行     |
| --------------------------------------- | --------------------------------- | ------ |
| `"NotFound"`                            | `NotFoundExceptionHandler.cs`     | L26    |
| `"Unauthorized"`                        | `UnauthorizedExceptionHandler.cs` | L26    |
| `"ValidationErrors"`                    | `ValidationExceptionHandler.cs`   | L25    |
| `"InternalServerError"`                 | `DefaultExceptionHandler.cs`      | L31    |
| `"stackTrace"`                          | `ProblemDetailsFactory.cs`        | L32    |
| `"errors"`                              | `ValidationExceptionHandler.cs`   | L27    |
| `"Bad Request"`, `"Unauthorized"`, etc. | `ProblemDetailsFactory.cs`        | L40-46 |

**影響**: エラーコードの一貫性が保証されず、リファクタリング時の変更漏れリスクが高い。

**推奨**: `ExceptionHandlingConstants.cs` を作成し、全エラーコードおよび ProblemDetails のタイトル文字列を定数として一元管理すること。

---

## 🛡️ 非機能要件とセキュリティ (Non-Functional Requirements & Security)

### 🔒 NF-01: `NotFoundExceptionHandler.CanHandle` における型名文字列マッチング

**該当箇所**: [NotFoundExceptionHandler.cs](/src/BuildingBlocks/ExceptionHandling/Handlers/NotFoundExceptionHandler.cs#L20)

```csharp
_ => context.Exception.GetType().Name.Contains("NotFoundException")
```

**同様の問題**: [UnauthorizedExceptionHandler.cs](/src/BuildingBlocks/ExceptionHandling/Handlers/UnauthorizedExceptionHandler.cs#L20)

```csharp
_ => context.Exception.GetType().Name.Contains("UnauthorizedException")
```

型名の文字列マッチングはリフレクションベースのフォールバックであり、以下のリスクがある：

1. **脆弱性**: 型名が変更された場合にサイレントに失敗する。
2. **パフォーマンス**: 文字列アロケーションと比較処理が毎リクエスト発生する。
3. **予期しない一致**: `CustomNotFoundExceptionWrapper` のような型にも意図せずマッチする。

**推奨**: 明示的な型マッチングのみに限定するか、属性ベースのマーカー（例: `[HttpStatusCode(404)]`）を導入すること。

---

### 🔒 NF-02: セキュリティ上のリスク — `ExposeStackTrace` のデフォルト値

**該当箇所**: [ExceptionHandlingOptions.cs](/src/BuildingBlocks/ExceptionHandling/Options/ExceptionHandlingOptions.cs#L12)

`ExposeStackTrace` のデフォルト値は `false` であり、これは正しい。ただし、`DefaultExceptionHandler` では明示的に `exception.Message` を ProblemDetails の `Detail` に設定した後、`ExposeStackTrace` が `false` の場合にのみ上書きしている。

```csharp
// ProblemDetailsFactory.Create → problemDetails.Detail = exception.Message;
// DefaultExceptionHandler → if (!_options.ExposeStackTrace) { problemDetails.Detail = "An unexpected error occurred..."; }
```

`ProblemDetailsFactory.Create` メソッド自体が常に `exception.Message` を `Detail` に設定するため、**`BaseExceptionHandler` 等ではそのままユーザーに返却されている**。内部例外メッセージが機密情報を含む可能性がある場合、これはセキュリティリスクとなる。

**推奨**: `ProblemDetailsFactory` レベルで機密メッセージ制御を集約するか、ハンドラ固有のメッセージマスキングポリシーを導入すること。

---

## 🧪 テスト容易性と疎結合性 (Testability & Decoupling)

### ⚙️ TC-01: `ProblemDetailsFactory` がインターフェースを持たない

**該当箇所**: [ProblemDetailsFactory.cs](/src/BuildingBlocks/ExceptionHandling/Factories/ProblemDetailsFactory.cs)

`ProblemDetailsFactory` は具象クラスとして直接各ハンドラにコンストラクタインジェクションされている。インターフェース (`IProblemDetailsFactory`) が存在しないため、以下の問題がある：

1. **単体テストの困難性**: ハンドラのテスト時に `ProblemDetailsFactory` のモック化が不可能。テスト時にも `IOptions<ExceptionHandlingOptions>` を含む完全な依存ツリーが必要。
2. **DIP 違反**: 上位モジュール（ハンドラ）が下位モジュール（ファクトリ）の具象型に直接依存している。

**推奨**: `IProblemDetailsFactory` インターフェースを `Abstractions` に導入し、ハンドラはインターフェース経由で依存すること。

---

### ⚙️ TC-02: ハンドラ間のレスポンス書き込みロジックの重複

**該当箇所**: 全ハンドラ (`BaseExceptionHandler`, `NotFoundExceptionHandler`, `UnauthorizedExceptionHandler`, `ValidationExceptionHandler`, `DefaultExceptionHandler`)

各ハンドラが以下の共通パターンを重複して実装している：

```csharp
context.HttpContext.Response.StatusCode = statusCode;
await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, ct);
```

さらに、`BaseException.Extensions` のコピーロジックも `BaseExceptionHandler`, `NotFoundExceptionHandler`, `UnauthorizedExceptionHandler` の 3 箇所に重複している。

**影響**: DRY 原則違反。レスポンスフォーマットの変更時に全ハンドラの修正が必要。

**推奨**: レスポンス書き込みを `ProblemDetailsFactory` または共通のベースクラス / ヘルパーメソッドに集約すること。Extensions のコピーロジックも同様に抽出すること。

---

## 🔭 可観測性の準拠度 (Observability Readiness)

### 📡 OB-01: ハンドラレベルのログ出力不足

**該当箇所**: `BaseExceptionHandler.cs`, `NotFoundExceptionHandler.cs`, `UnauthorizedExceptionHandler.cs`, `ValidationExceptionHandler.cs`

`DefaultExceptionHandler` のみが `ILogger` を注入し、例外をログ出力している。その他のハンドラでは例外が発生してもログに記録されず、可観測性が損なわれている。

**推奨**: 少なくとも `LogWarning` レベルで、ハンドルされた例外の種類、`TraceId`、主要コンテキストをログに記録すること。または、ミドルウェアレベルでパイプライン実行前後のログを統一的に出力する設計に変更すること。

---

### 📡 OB-02: ミドルウェアのログテンプレートにプレースホルダ不足

**該当箇所**: [ExceptionHandlingMiddleware.cs](/src/BuildingBlocks/ExceptionHandling/Pipeline/ExceptionHandlingMiddleware.cs#L25)

```csharp
logger.LogWarning("The response has already started, the exception handling middleware will not be executed.");
```

ログテンプレートに `{TraceId}` や `{Path}` などの構造化プレースホルダが含まれていない。Rule 6 (Observability) では構造化ログテンプレートの使用が義務付けられている。

**推奨**:

```csharp
logger.LogWarning("The response has already started, the exception handling middleware will not be executed. TraceId: {TraceId}, Path: {Path}", context.TraceIdentifier, context.Request.Path);
```

---

## ⚠️ コード品質とコーディング規約のリスク (Code Quality & Standard Risks)

### ⚠️ CQ-01: `ExceptionContext` は `record` であるべき (Rule 15)

**該当箇所**: [ExceptionContext.cs](/src/BuildingBlocks/ExceptionHandling/Abstractions/Contracts/ExceptionContext.cs)

`ExceptionContext` はリクエスト処理中の例外コンテキストを表すデータ転送オブジェクトであるが、ミュータブルな `class` として実装されている。`Handled` プロパティと `TraceId` プロパティが setter を持つため完全なイミュータブル化は困難だが、`ExceptionContext` のコア部分（`HttpContext`, `Exception`）は不変である。

**推奨**: 現在のミュータブル設計が意図的であれば（パイプライン内で `Handled` フラグを更新するため）、その設計判断を XML コメントで明文化すること。もしイミュータブル化が可能であれば `sealed record` への移行を検討すること。

---

### ⚠️ CQ-02: `VKProblemDetails` は `sealed` であるべき

**該当箇所**: [VKProblemDetails.cs](/src/BuildingBlocks/ExceptionHandling/Abstractions/Contracts/VKProblemDetails.cs)

`ProblemDetails` を継承する `VKProblemDetails` 自体はさらなる派生を意図していないため、`sealed` を付与すべきである。

---

### ⚠️ CQ-03: `ExceptionHandlingOptions.Handlers` の型安全性

**該当箇所**: [ExceptionHandlingOptions.cs](/src/BuildingBlocks/ExceptionHandling/Options/ExceptionHandlingOptions.cs#L17)

```csharp
public IList<Type> Handlers { get; } = new List<Type>();
```

`Type` を `IList<Type>` で管理しているため、`IExceptionHandler` を実装しない型も追加可能である。実行時エラーの原因となる。

**推奨**:

```csharp
public sealed record ExceptionHandlingOptions
{
    public bool ExposeStackTrace { get; init; } = false;
    internal List<Type> Handlers { get; } = [];

    public ExceptionHandlingOptions AddHandler<T>() where T : class, IExceptionHandler
    {
        Handlers.Add(typeof(T));
        return this;
    }
}
```

---

## ✅ 評価ポイント (Highlights / Good Practices)

1. **RFC 7807 準拠**: `VKProblemDetails` が `ProblemDetails` を拡張し、`ErrorCode`, `TraceId`, `Timestamp` を含む標準化されたエラーレスポンスを構築している。
2. **Chain of Responsibility パターン**: `IExceptionHandler` + `ExceptionHandlerPipeline` による責務連鎖パターンの適用が適切。ハンドラの追加・削除が容易。
3. **`CancellationToken` の伝播**: `IExceptionHandler.HandleAsync` および `ExceptionHandlerPipeline` が `CancellationToken` を正しく受け渡している。ミドルウェアでは `context.RequestAborted` を利用している。
4. **防御的プログラミング**: `ExceptionContext` コンストラクタでの `ArgumentNullException` チェック、ミドルウェアでの `HasStarted` チェックが実装されている。
5. **関心の分離**: 例外判定 (`CanHandle`) と処理 (`HandleAsync`) の分離が明確であり、SRP に準拠している。
6. **拡張性**: `ExceptionHandlingOptions.Handlers` によるカスタムハンドラの登録がサポートされている。
7. **File-Scoped Namespace**: 全ファイルで file-scoped namespace が使用されている。
8. **XML ドキュメント**: インターフェースおよび主要クラスに XML コメントが適切に付与されている。
9. **内部詳細の秘匿**: `DefaultExceptionHandler` が本番環境で例外メッセージをマスクする設計を採用している。

---

## 💡 改善ロードマップ (Evolutionary Roadmap)

### 1. 最優先対応 (Immediate Action)

| #   | 対応項目                                                           | 重要度 | 対象ファイル                                      |
| --- | ------------------------------------------------------------------ | ------ | ------------------------------------------------- |
| 1   | 全クラスに `sealed` を付与 (CS-01)                                 | 🔴 高  | 全 `.cs` ファイル                                 |
| 2   | マジックストリングの定数化 (CS-03)                                 | 🔴 高  | 新規 `ExceptionHandlingConstants.cs` + 各ハンドラ |
| 3   | ミドルウェアのログテンプレートに構造化プレースホルダを追加 (OB-02) | 🔴 高  | `ExceptionHandlingMiddleware.cs`                  |

### 2. リファクタリング提案 (Refactoring)

| #   | 対応項目                                                                  | 重要度 | 対象ファイル                                                     |
| --- | ------------------------------------------------------------------------- | ------ | ---------------------------------------------------------------- |
| 4   | `IProblemDetailsFactory` インターフェースの導入 (TC-01)                   | 🟡 中  | 新規 `IProblemDetailsFactory.cs` + 全ハンドラ                    |
| 5   | レスポンス書き込みロジックの集約 (TC-02)                                  | 🟡 中  | 全ハンドラ + `ProblemDetailsFactory`                             |
| 6   | DI 登録の Options 二重構築を解消 (CS-02)                                  | 🟡 中  | `ExceptionHandlingExtensions.cs`                                 |
| 7   | `ExceptionHandlingOptions.Handlers` の型安全な API 化 (CQ-03)             | 🟡 中  | `ExceptionHandlingOptions.cs`                                    |
| 8   | 型名文字列マッチングの排除 (NF-01)                                        | 🟡 中  | `NotFoundExceptionHandler.cs`, `UnauthorizedExceptionHandler.cs` |
| 9   | `ProblemDetailsFactory.Create` のメッセージマスキングポリシー統合 (NF-02) | 🟡 中  | `ProblemDetailsFactory.cs`                                       |
| 10  | 各ハンドラへの構造化ログ出力追加 (OB-01)                                  | 🟢 低  | 全ハンドラ                                                       |

### 3. 推奨される学習トピック (Learning Suggestions)

1. **`IConfigureOptions<T>` / `IPostConfigureOptions<T>` パターン** — DI 登録時のオプション構築を Options パイプラインに統合する手法を習得すること。
2. **Decorator パターンによるクロスカッティングロギング** — ハンドラパイプラインにデコレータで統一的なログ出力を追加する設計を検討すること。
3. **`sealed` による JIT 最適化** — .NET ランタイムにおける `sealed` キーワードのパフォーマンス効果（脱仮想化、メソッドインライン化）を理解すること。
4. **Result パターンとの連携** — Application Layer の `Result<T>.Failure` から例外を経由せずに ProblemDetails を生成する統合パターンを検討すること。

---

## 📋 VK.Blocks 規約チェックリスト

- ✅ Result<T> → 本モジュールは Infrastructure レイヤーの例外ハンドリング基盤であり、`Result<T>` の直接返却は対象外。例外 → ProblemDetails への変換が責務。
- ✅ Async/CT → `CancellationToken` が `HandleAsync` 経由で全ハンドラに正しく伝播されている。
- ✅ TenantId → 本モジュールはテナント分離の責務を持たないため対象外。
- ❌ LogTemplate → `ExceptionHandlingMiddleware.cs` L25 の `LogWarning` にプレースホルダが不足 (OB-02)。
- ✅ No Null → null を返却する箇所は存在しない。
- ❌ Error Constant → エラーコード文字列がリテラルとしてハードコードされている (CS-03)。
- ✅ Polly → 本モジュールは外部呼び出しを行わないため対象外。
- ✅ NoTracking → DB アクセスを行わないため対象外。
