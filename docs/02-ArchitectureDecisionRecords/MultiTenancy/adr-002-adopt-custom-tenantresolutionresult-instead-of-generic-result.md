# ADR 002: Adopt Custom TenantResolutionResult Instead of Generic Result

## 1. Meta Data

- **Date**: 2026-03-12
- **Status**: ❌ Superseded
- **Deciders**: Architecture Team
- **Technical Story**: MultiTenancy Module Refactoring

## 2. Context (背景)

（参照: ADR 001）
新しい Tenant Resolution Pipeline において、各リゾルバは解決の成否（TenantId、またはエラーメッセージ）を返す必要がある。VK.Blocks の標準アーキテクチャールール（Rule 1: Result Pattern）では汎用の `Result<T>` を使用することが義務付けられているが、パイプラインの初期実装においては、結果をより明示的かつドメイン固有に表現するため、専用の `TenantResolutionResult` が導入された。
しかし、その後のアーキテクチャ監査において、生の文字列（Raw Error Strings）によるエラーハンドリングと、標準の `Result<T>` トラクトからの逸脱が重大なアーキテクチャの匂い（Critical Architectural Smell）として指摘された。

## 3. Problem Statement (問題定義)

現在の `TenantResolutionResult` の使用には以下の問題がある。

1. **一貫性の欠如**: システム全体で統一されている `Result<T>` エラーハンドリングパターンから逸脱している。
2. **生文字列のエラー (Raw String Errors)**: エラーメッセージがハードコードされた文字列として定義されており、エラーコード（Constants）を通じた国際化や集中的な追跡が困難である。

```csharp
// 現在の違反例
public static TenantResolutionResult Fail(string error) => new(false, null, error);

// Resolver内での使用例
return TenantResolutionResult.Fail("Request host is empty.");
```

## 4. Decision (決定事項)

**現在の決定は将来的に Superseded（廃止）とし、VK.Blocks 標準の `Result<T>` へ統合するリファクタリングを実施する。**

- 独自の `TenantResolutionResult` は非推奨とし、削除する。
- 解決結果は `Result<TenantInfo>` または `Result<string>` として伝搬させる。
- 生の文字列でのエラーメッセージ指定を禁止し、`MultiTenancyConstants.Errors` に定義された静的エラーオブジェクトを返す。

## 5. Alternatives Considered (代替案の検討)

### Option 1: `TenantResolutionResult` を維持し、内部構造のみ変更
- **Approach**: 独自の `TenantResolutionResult` クラスを残しつつ、エラー生成メソッドのみ VK.Blocks の Error オブジェクトを受け取るように変更する。
- **Rejected Reason**: クラス自体が冗長であり、他の BuildingBlocks との間で結果処理の共通基盤（ROP: Railway Oriented Programming）を利用できなくなるため却下。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- コアモジュール全体の ROP（Result Pattern）と完全に統合され、ミドルウェアでのエラーハンドリングが一貫する。
- 標準の `Error` オブジェクトにより、問題追跡と RFC 7807 へのマッピングが簡素化される。

### Negative
- `ITenantResolver`, `TenantResolutionPipeline`, および関連するミドルウェアのシグネチャ変更を伴うため、一時的なリファクタリングコストが発生する。

### Mitigation
- コンパイルエラーを順番に解消し、既存の単体テストを活用してリファクタリングの安全性を担保する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- エラー定数は `MultiTenancyConstants.Errors` に静的読み取り専用（`static readonly Error`）として定義する。
- `TenantResolutionMiddleware` にて、パイプラインから返された `Result<T>.Failure` のエラーオブジェクトから、統一された Error Code/Message を使用して Problem Details を生成する。これにより、クライアントへの適切なエラー構造の提示とロギングのセキュアな標準化が保証される。
