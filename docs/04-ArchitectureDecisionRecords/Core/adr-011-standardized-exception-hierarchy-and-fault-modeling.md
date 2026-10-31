# ADR 011: Standardized Exception Hierarchy and Fault Modeling

## 1. Meta Data

- **Date**: 2026-04-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core Exception Handling and Error Modeling

## 2. Context (背景)

VK.Blocks エコシステムにおいて、インフラ層やドメイン層で発生した異常事態（構成エラー、リソース未検出、ビジネスルール違反など）を報告するための例外処理が統一されていませんでした。

これまでの課題：
1. **標準的な情報不足**: 標準の `Exception` では、エラーコードや詳細なメタデータ（Extensions）を保持する統一されたプロパティがなく、場当たり的なメッセージ作成が行われていた。
2. **RFC 7807 との不整合**: API 境界で RFC 7807 (Problem Details) 形式のレスポンスを生成する際、例外から HTTP ステータスコードや詳細情報を抽出するロジックが各所で重複していた。
3. **情報漏洩の懸念**: 内部的なエラー詳細と、クライアントに公開しても安全なメッセージの区別が曖昧であり、意図しないスタックトレースや機密情報の露出リスクがあった。

## 3. Problem Statement (問題定義)

現状の「悪い見本」：
```csharp
// どこで定義されたか不明な独自例外や、標準例外の混在
throw new InvalidOperationException("Dependency X is missing"); 

// API 境界でのマッピング（煩雑で漏れが発生しやすい）
try { ... }
catch (InvalidOperationException ex) {
    return Result.Failure(Errors.General.InternalError(ex.Message)); // 常に 500 にマッピングされるなど
}
```

## 4. Decision (決定事項)

工業級のエラーハンドリングを実現するため、基底例外クラス `VKBaseException` と、セマンティックな派生例外群を導入します。

### 4.1. 基底クラス: VKBaseException
すべての VK.Blocks 関連例外の親となり、以下のプロパティを標準化します。
- `Code`: 機械判読可能なエラーコード（例: `Core.DependencyError`）。
- `StatusCode`: 推奨される HTTP ステータスコード。
- `IsPublic`: メッセージを外部クライアントに公開してよいかを示すフラグ。
- `Extensions`: 任意のメタデータを保持する `IDictionary<string, object?>`。

### 4.2. 標準派生例外
一般的なシナリオに対応するため、以下の例外を Core モジュールに定義します。
- `VKDependencyException`: DI 構成、循環参照、必須オプションの欠落など、システムのセットアップに関連するエラー。
- `VKNotFoundException`: リソースが見つからない場合。
- `VKConflictException`: データの競合やビジネスルールの不整合。
- `VKForbiddenException` / `VKUnauthorizedException`: 権限・認証エラー。
- `VKValidationException`: 入力値検証エラー。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 標準の Exception の Data 辞書を使用する**
    - Approach: 新しい型を作らず、`ex.Data["ErrorCode"]` のように値を詰める。
    - Rejected Reason: 型安全性がなく、開発者がプロパティの存在を意識しにくいため、一貫性が保てない。
- **Option 2: 例外を一切投げず、全層で Result 型を返す**
    - Approach: コンストラクタや DI 登録フェーズも含め、すべて Result 型で制御する。
    - Rejected Reason: C# の言語仕様上、コンストラクタや一部のライフサイクルメソッドでは例外の使用が避けられない。また、回復不能な構成エラーなどは例外として早期中断（Fail-Fast）させる方が健全である。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - API 境界のミドルウェアやインターセプターで、例外を自動的に RFC 7807 形式の `VKResult` にマッピング可能。
    - `Extensions` により、エラーのコンテキスト（不足しているブロック名、期待される型名など）を構造化して報告できる。
    - `IsPublic` フラグにより、セキュリティを担保しつつ開発効率を向上。
- **Negative**:
    - 既存の例外のスロー箇所を新しい型に置き換えるリファクタリングコストが発生する。
- **Mitigation**:
    - `VKBaseExceptionExtensions` を提供し、既存の例外や Result への変換を容易にするヘルパーを用意する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Fault Isolation**: `VKDependencyException` では `CyclePath` などの詳細な解析情報を含むが、これらはデフォルトで `IsPublic = false` とし、内部ログには記録するが外部 API には露出させない。
- **Immutability**: 例外インスタンス生成後の状態変化を最小限にし、スレッドセーフなエラー報告を実現。
- **Throw Helpers**: パフォーマンス向上のため、例外スローは `VKGuard` や専用のスタティックな `Throw` メソッド経由で行うことを推奨（ADR-010 参照）。

**Last Updated**: 2026-04-22
