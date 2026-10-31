# ADR 003: Adopt Attribute Evaluator Pattern for Extensible Custom Requirements

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Authorization Module

## Context (背景)

高度にカスタマイズされたエンタープライズシステムでは、単なるロールや権限の有無にとどまらず、複雑な認可チェック（例：`MinimumRank`（最低役職ランク）、`WorkingHours`（業務時間内のみ許可）、`ClearanceLevel`（機密レベル））が頻繁に要求されます。

## Problem Statement (問題定義)

- **ロジックの散在と重複**: 開発者がこれらの要件を満たすために、コントローラーのメソッド内に直接 if 文を使ったビジネスロジックを記述したり、各条件ごとに個別の `IAsyncActionFilter` を無数に作成したりすると、認可ロジックがアプリケーション全体に散在してしまいます。
- **アーキテクチャの純粋性の喪失**: ビジネスロジックの凝集度が下がり、一貫した認可パイプラインとしての統制（アーキテクチャの純粋性）が失われます。また、特定の条件を満たすユーザーかどうかを検証する似たようなコードが複数箇所で重複する（DRY違反）原因となります。

## Decision (決定事項)

宣言的（Declarative）でメタデータ駆動の認可チェックを実現するため、統合された `IAttributeEvaluator` インターフェースを軸とする **Attribute Evaluator パターン (属性評価器パターン)** を導入することを決定しました。

「業務時間内か？」といった恣意的な、あるいは小さな条件ごとに個別の `IAuthorizationHandler` を大量に記述するのではなく、カスタム宣言属性（Attribute）によって駆動される汎用的な動的要件（例：`DynamicRequirement`）を定義します。
`AttributeEvaluator` は、単一の静的評価インターセプターとして機能し、これらの宣言的制約をパースし、クレーム（Claims）ベースの実行ロジックを一元的に評価します。

## Alternatives Considered (代替案の検討)

### Option 1: 個別の IAuthorizationHandler の大量作成

- **Approach**: `MinimumRankRequirement` 用のハンドラー、`WorkingHoursRequirement` 用のハンドラーなど、カスタムポリシーごとにクラスを量産する。
- **Rejected Reason**: 要件が増えるたびに新しいクラスと DI 登録が必要になり、横断的な処理（例えば「すべての属性条件を満たすか」といった複合条件の評価）が複雑化するため。

### Option 2: 属性を使わないミドルウェアでの一括判定

- **Approach**: カスタムミドルウェアでリクエストパスや メソッドを解析し、外部のコンフィグ（JSONやDB）から条件を引っ張ってきて動的に判定する。
- **Rejected Reason**: ルーティング情報との結びつきが弱く、個別のエンドポイントに対して「このメソッドは Rank 3 以上」といったきめ細かい設定を直感的に（C# の属性として）表現できず、保守性が下がるため。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - バリデーションのロジックが単一のエンジン（Evaluator）に集約されるため、コントローラーは完全に手続き型コードから解放され、宣言的でクリーンな状態を保ちます。
    - 新しいカスタム検証属性を追加する際の手間が大幅に削減され、システムの拡張性が飛躍的に高まります。
- **Negative**:
    - `AttributeEvaluator` 内でリフレクションを使用して属性情報を抽出する場合、実行時にわずかなパフォーマンスヒットが発生する可能性があります。
- **Mitigation**:
    - 属性情報の抽出においてキャッシュメカニズムを設ける、または頻繁に呼び出されるパスに対するメタデータ評価を最適化することで、パフォーマンスへの影響を最小限に抑えます。

## Implementation & Security (実装詳細とセキュリティ考察)

- **データ構造**: `DynamicRequirement` は評価に必要な演算子（`OperatorEquals`, `OperatorExists` など）や対象となる値を保持し、`AttributeEvaluator` がそれを `ClaimsPrincipal` のクレーム情報と照合します。
- **セキュリティと保守性**: 中央集権的な Evaluator が存在することで、「予期せぬ条件漏れ」や「評価ロジックのバグ」が発生した場合でも、一箇所を修正するだけでシステム全体に適用されるため、セキュリティパッチの適用や監査（Audit）が極めて容易になります。
