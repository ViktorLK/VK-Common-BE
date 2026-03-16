# ADR 003: Implement Partial-Match Lazy-Evaluated PII Masking Strategy

**Date**: 2026-03-15  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: Observability.Serilog

## Context (背景)

アプリケーションやユーザーのコンテキスト（`ApplicationContext`, `UserContext`）をログに記録する際、パスワード、セキュリティトークン、個人情報 (PII) などの機密データが事故で平文として出力されるリスクがあります。以前のアプローチでは、保護すべきプロパティ名を「完全一致 (Exact Match)」でブラックリスト化していましたが、この手法では命名規約の揺れ（例: "Password" と "UserPassword"）に対して極めて脆弱でした。また、すべてのログイベントに対してマスキング処理を律儀に実行することはパフォマンスオーバーヘッドにも繋がっていました。

## Problem Statement (問題定義)

- **セキュリティリスク**: "Password" の完全一致に依存すると、"NewPassword" や "apiKey" のような亜種がマスキングをすり抜け、ログシステムから情報漏洩（OWASP A09: Security Logging and Monitoring Failures）を引き起こす可能性があります。
- **パフォーマンス劣化**: 高スループットな API では、ログのたびに不必要なコレクション生成や走査が発生することはレイテンシ悪化の要因となります。

## Decision (決定事項)

ログ生成パイプラインにおいて機密データを安全かつ高速に保護するため、`SensitiveDataEnricher` を導入し、部分一致マッチング（`string.Contains` + `StringComparison.OrdinalIgnoreCase`）を採用しました。
さらに、Null合体代入演算子 (`??=`) による遅延評価 (Lazy Evaluation) を組み合わせ、マスキングが必要なときのみヒープアロケーションを行う設計としました。

### 設計詳細 (Design Details)

```csharp
public sealed class SensitiveDataEnricher : ILogEventEnricher
{
    private readonly IEnumerable<string> _sensitiveKeys;

    public SensitiveDataEnricher(IEnumerable<string> sensitiveKeys)
    {
        _sensitiveKeys = sensitiveKeys;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        List<string>? keysToMask = null; // Lazy Allocation

        foreach (var property in logEvent.Properties)
        {
            foreach (var sensitiveKey in _sensitiveKeys)
            {
                // ✅ 完全一致ではなく部分一致（大文字小文字区別なし）
                if (property.Key.Contains(sensitiveKey, StringComparison.OrdinalIgnoreCase))
                {
                    keysToMask ??= [];
                    keysToMask.Add(property.Key);
                    break;
                }
            }
        }

        if (keysToMask is null) return;

        foreach (var key in keysToMask)
        {
            // プロパティ値を "***" で置換
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(key, "***"));
        }
    }
}
```

## Alternatives Considered (代替案の検討)

- **Option 1: Serilog.Exceptions や Destructurama の導入**
    - _Approach_: サードパーティライブラリである `Destructurama.Attributed` などを使い、プロパティに `[NotLogged]` 属性を付与する。
    - _Rejected Reason_: 当該ライブラリはオブジェクトのシリアライズ時（Destructure）には有効ですが、ログのトップレベルスコープに直接 `Enrich.WithProperty` で追加された独立したディクショナリ等の保護には不完全であるため、手動の Enricher を挟むことで全体の安全網（Safety Net）を構築しました。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - "token", "password", "secret" の部分一致を採用したことで、命名の揺れに対する堅牢なマスキングが保証されました。
    - アロケーションゼロベースの遅延評価により、マスキング不要なパスにおける CPU およびヒープの浪費が解消されました。
- **Negative**:
    - 部分一致は意図しない文字列をマスキングしてしまう(False Positive) リスクがあります（例: "isPasswordSet" という単なる Boolean まで隠匿される）。
- **Mitigation**:
    - `Boolean` のようなトラブルシューティング上問題のない単純なプリミティブ型が隠れてしまうリスクは受容し、「万が一でも漏洩させない（Fail-Safe）」ことを最優先とする組織ポリシーを適用します。また、複雑なネスト構造の PII 対応のために、将来的に `Destructure.ByTransforming<T>()` との併用を検討します（Audit Report 2026-03-12 で提言）。

## Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: DI コンテナからの `IEnumerable<string>` によるコンフィグ注入を行い、組織のセキュリティポリシー変更に伴うブラックリストキーワードの追加をテスト容易かつ無停止で実現できる構成としています。
- **セキュリティ重点**: ログは最も狙われやすい情報資産（攻撃者の侵害経路）であるため、この Enricher をパイプラインの最終段で必ず通過させる Must-Run なミドルウェアとして位置づけています。
