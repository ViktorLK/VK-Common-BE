# ADR 007: Standardize Tenant Resolution Result Pattern and Error Handling

**Date**: 2026-03-13  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: MultiTenancy Module Standardization

## 1. Context (背景)

VK.Blocks.MultiTenancy モジュールは、当初独自の `TenantResolutionResult` 型を使用していました。しかし、これはコアプロジェクトで定義されている標準の `Result<T>` パターンと重複しており、プロジェクト全体での一貫性を欠いていました。また、ミドルウェアでのエラーレスポンスが匿名型やマジックストリングに依存しており、RFC 7807 (Problem Details) への準拠が不完全でした。

## 2. Problem Statement (問題定義)

- **一貫性の欠如**: 独自の `TenantResolutionResult` は、`VK.Blocks.Core` の `Result<T>` と機能が重複しており、学習コストとコードの冗長性を高めていました。
- **エラーハンドリングの非標準化**: 匿名型を使用した HTTP エラーレスポンスは、クライアント側での型安全な処理を困難にし、プロジェクトの可観測性（Rule 6）およびエラーハンドリング（Rule 1）の規定に違反していました。
- **マジックストリングの散在**: ヘッダー名やエラーメッセージがハードコードされている箇所があり、メンテナンス性が低下していました。

## 3. Decision (決定事項)

以下の設計変更を実施し、モジュールを標準化しました：

1.  **Resultパターンの統一**: `TenantResolutionResult` を廃止し、`VK.Blocks.Core.Results.Result<string>` に置き換えました。
2.  **標準エラー定数の導入**: `MultiTenancyErrors` クラスを新設し、`TenantNotFound`, `InvalidTenantId` などの標準的な `Error` オブジェクトを定義しました。
3.  **RFC 7807 準拠の強化**: `TenantResolutionMiddleware` において、`VK.Blocks.ExceptionHandling` の `VKProblemDetails` を使用するように修正しました。これにより、一貫した `errorCode`, `traceId`, `timestamp` を含むレスポンスが可能になりました。
4.  **定数の集約**: すべてのマジックストリングを `MultiTenancyConstants` に集約しました。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: `TenantResolutionResult` を維持し、`Result<T>` を継承する**
    - **Rejected Reason**: 継承による複雑化を避け、可能な限り単純な標準型を利用すべきという方針（Rule 1）に基づき却下。
- **Option 2: 既存の `ProblemDetails` 型を直接使用し、拡張プロパティを使用する**
    - **Rejected Reason**: VK.Blocks 全体で `VKProblemDetails` への統一が進められており、依存関係を追加してでも標準に従うべきと判断。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive

- モジュール間の一貫性が向上し、共通のユーティリティ（ResultExtensions等）が利用可能になった。
- エラーレスポンスが型安全かつ標準化され、デバッグと統合が容易になった。
- ユニットテストの記述が標準型に基づき簡素化された。

### Negative

- `ITenantResolver` を実装する既存コードに破壊的変更が発生する。
- `VK.Blocks.ExceptionHandling` への新しい依存関係が発生する。

### Mitigation

- 破壊的変更については、すべての内蔵リゾルバを一括で修正し、十分なユニットテスト（14件）を追加することで品質を担保した。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装**: `Result<T>` の `Value` プロパティは成功時のみアクセス可能であり、ミドルウェア側で型安全に取得されます。
- **セキュリティ**: テナント ID のバリデーション（長さ制限、文字種チェック）を継続して維持し、不正な ID によるインジェクションや攻撃を Boundary で遮断します。エラーレスポンスには機密情報を含めず、`traceId` を通じて管理ログと紐付けます。
