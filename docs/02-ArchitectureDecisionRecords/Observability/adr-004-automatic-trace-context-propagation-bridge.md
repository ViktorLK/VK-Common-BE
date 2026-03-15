# ADR 004: Automatic Trace Context Propagation Bridge (Result to Activity)

**Date**: 2026-03-11  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Observability Integration with Result Pattern

## 2. Context (背景)

VK.Blocks では、アプリケーション層のエラーハンドリングにおいて例外（Exception）のスローを禁止し、`Result<T>` パターン（RFC 7807 準拠のエラーオブジェクト）を採用しています。
一方で、分散トレース（OpenTelemetry）においては、操作の成否やエラー内容を `Activity`（Span）のタグやイベントとして記録し、監視ダッシュボードで異常を検知できるようにする必要があります。

## 3. Problem Statement (問題定義)

ビジネスロジック内で `Result` オブジェクトが返された際、それをどのようにしてアクティブなトレース（Span）に紐づけるかが課題でした。
開発者が手動で `Activity.Current?.SetStatus(ActivityStatusCode.Error, result.FirstError.Description);` などを記述する設計にした場合、以下の問題が発生します：

1. **Boilerplate Code**: すべてのハンドラーやメソッドで同じようなトレース記録コードが散乱し、可読性が低下します。
2. **Missing Telemetry**: 開発者が記録処理を書き忘れた場合、重大なエラーが発生していてもトレース上は「正常終了」として扱われ、インシデントの検知が遅れるリスクがあります。

## 4. Decision (決定事項)

`Activity` に対する拡張メソッド `RecordResult(this Activity? activity, IResult result)` を導入し、ビジネスの実行結果（`Result`）をトレースに自動マッピングする「ブリッジ設計」を採用します。

### マッピングルール

- **Success (`result.IsSuccess == true`)**:
    - `result.success = true` というタグをSpanに付与します。
- **Failure (`result.IsSuccess == false`)**:
    - Spanのステータスを `ActivityStatusCode.Error` に設定します。
    - `result.success = false` を付与します。
    - `result.code`（エラーコード）、`result.message`（エラーメッセージ）、`error.type`（エラーの種類）をSpanのタグとして設定します。
    - 同じくエラーの詳細を `ActivityEvent`（`"result.failure"` イベント）としてSpanに記録します。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: MediatR の Pipeline Behavior で一括ハンドリングする**
    - **Approach**: リクエストパイプラインの最後に共通Behaviorを配置し、レスポンスが `IResult` でありかつ失敗している場合に中集権的にSpanを更新する。
    - **Rejected Reason**: 完全な却下ではありません（併用可能）が、MediatR経由ではない内部の細かいメソッドやドメインサービスの実行単位でのトレースに適用できないため、コアの拡張メソッドとして機能を提供する方が柔軟性が高いと判断しました。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - エラーの記録が統一され、すべてのエラースパンが同じ構造のタグ（`result.code`, `error.type`）を持つため、ログ・トレース分析基盤でのクエリ（「特定のドメインエラーの発生率をグラフ化」等）が極めて容易になります。
    - 開発者はビジネスロジックに集中でき、最後に `Activity.Current?.RecordResult(result);` を呼ぶ（あるいは上位のミドルウェアが呼ぶ）だけで済みます。
- **Negative**:
    - `IResult` の構造にトレース層が依存することになりますが、これらは共にプラットフォームコアに属するため許容範囲です。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Implementation details**:
  `src/BuildingBlocks/Observability/Extensions/ActivityExtensions.cs` に実装を配置します。対象の Activity が `null` の場合（＝トレースが無効な場合）は早期リターンし、無駄なメモリ割り当て（アロケーション）をゼロに抑える最適化を施しています。
- **Security**:
  エラーメッセージ（`result.message`）に内部情報の漏洩（DBの生クエリやスタックトレース等）が含まれないよう、`Result` パターン側の設計（Client-safeなエラーオブジェクトのみを返すポリシー）によってセキュリティを担保します。
