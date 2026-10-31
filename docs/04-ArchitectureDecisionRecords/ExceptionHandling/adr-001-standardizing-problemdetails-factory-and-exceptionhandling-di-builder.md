# ADR 001: Standardizing ProblemDetails Factory and ExceptionHandling DI Builder

- **Date**: 2026-03-17
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.ExceptionHandling

## 2. Context (背景)

VK.Blocks のアーキテクチャ監査レポート (2026-03-16) において、`ExceptionHandling` モジュール内の依存関係解決およびテスト容易性に関する複数の課題が明らかになった。特に、例外発生時のフォーマットを一元管理する `ProblemDetailsFactory` が具象クラスとして注入されており、例外ハンドラの単体テスト時にフレームワークの深部機能（`HttpContext` や `IOptions`）の完全なモック化を余儀なくされていた。

## 3. Problem Statement (問題定義)

現在の実装には以下の重大なアーキテクチャ上の問題 (Critical Architectural Smells) が存在した：

1. **テスト容易性の欠如 (DIP違反)**: 
   全例外ハンドラが `ProblemDetailsFactory` (具象クラス) に直接依存していた。これにより、ハンドラのテスト時にファクトリのみをモック化することが不可能であった。
2. **Options パターンの誤用と型安全性の欠如**: 
   DI 登録時 (`AddExceptionHandling`) に `ExceptionHandlingOptions` を手動で `new` して `Handlers` (型: `IList<Type>`) に追加していた。これにより、
   - `IExceptionHandler` を実装していない不正な型が登録される実行時リスクがあった。
   - `IPostConfigureOptions` 等の標準的な構成パイプラインをバイパスしてしまっていた。

## 4. Decision (決定事項)

上記の問題を解決するため、以下の判断を下した。

1. **`IProblemDetailsFactory` インターフェースの抽出と導入**:
   具象クラスをインターフェースに抽象化し、例外ハンドラ側の依存を `IProblemDetailsFactory` へ切り替える。
2. **`IExceptionHandlingBuilder` パターンの導入**:
   `IList<Type>` による危険なハンドラ登録を廃止し、ASP.NET Core の標準的な Fluent Builder スタイルを採用する。ジェネリック型制約 `where T : class, IExceptionHandler` を付与することで、コンパイル時に型安全性を保証する。

```csharp
// 改善後のDI登録インターフェース
public interface IExceptionHandlingBuilder
{
    IServiceCollection Services { get; }
    IExceptionHandlingBuilder AddHandler<T>() where T : class, IExceptionHandler;
}
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 現状維持のままテスト用DIコンテナを構築する**
  - **Approach**: インターフェースを抽出せず、テストプロジェクト側で WebApplicationFactory 等を用いて完全な DI トリーを構築する。
  - **Rejected Reason**: ハンドラのロジック検証という単体テストの目的に対して大掛かりすぎ、テスト実行速度が低下する。また、DI登録の型安全性問題 (Optionsの誤用) が解決しないため却下。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - **テスト容易性の向上**: 各 `IExceptionHandler` を純粋な単体テストで検証可能になった。
  - **型安全性 (Type Safety) の保証**: 無効な例外ハンドラを誤って登録することがコンパイル時に防止される。
  - **DIパターンの標準化**: ASP.NET Core の標準的な Builder パターン (`IMvcBuilder` 等) と設計が統一された。
- **Negative**:
  - 既存のコンシューマ API (`AddExceptionHandling(options => options.Handlers.Add(...))`) に破壊的変更が生じる。
- **Mitigation**:
  - `VK.Blocks` を利用する各アプリケーションの `Program.cs` において、流暢な API `.AddExceptionHandling().AddHandler<...>()` への移行ガイドを提供する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**:
  `ProblemDetailsFactory` 内で `IOptions<ExceptionHandlingOptions>` を注入・評価するように変更し、本番環境でスタックトレース等の内部情報が漏洩しないようマスクするロジックをファクトリ内に一元化した。
- **セキュリティ重点**:
  これにより、各ハンドラが誤って例外の生メッセージを露呈させてしまう情報漏洩リスク (Information Disclosure) を根絶した。
