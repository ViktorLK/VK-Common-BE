# ADR 016: Adopt High-Performance Logging Pattern with Feature-Local Source Generators

- **Date**: 2026-04-01
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authentication / Rule 16 Compliance

## Context (背景)

従来のロギング（`ILogger` への直接的な文字列補間）は、実行時のアロケーションや構造化ログの不整合、および Rule 16（高パフォーマンス・ロギング）への非準拠という問題を抱えていた。また、ログ定義が `Diagnostics` フォルダに集中しており、フィーチャーごとの凝集度を下げていた。

## Problem Statement (問題定義)

1. **パフォーマンス**: 文字列補間 `logger.LogInformation($"User {id} logged in")` は、ログレベルが無効な場合でも文字列生成のアロケーションが発生する。
2. **保守性**: 全モジュールのログ定義が一箇所にあると、機能追加時に複数のディレクトリを跨ぐ必要があり、カプセル化が損なわれる。
3. **ルール違反**: `vk-blocks-checklist.md` の Rule 16 では `[LoggerMessage]` 源生成器の使用を義務付けているが、既存コードの多くがこれに準拠していない。

## Decision (決定事項)

すべてのロギング呼び出しを `[LoggerMessage]` ソースジェネレーターに移行し、ログ定義ファイル（`XxxLog.cs`）をそれぞれの Feature フォルダ内に配置する（Rule 12, 16）。

- **ファイル配置**: `Features/{FeatureName}/{FeatureName}Log.cs`
- **実装形態**: `internal static partial class` として定義し、`ILogger` の拡張メソッドとして提供する。
- **禁止事項**: `logger.LogInformation()` 等の直接呼び出しを禁止し、ソース生成されたメソッドのみを使用する。

## Implementation (実装例)

```csharp
// Features/ApiKeys/ApiKeyLog.cs
internal static partial class ApiKeyLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "API Key {KeyId} validated for Tenant {TenantId}")]
    public static partial void LogApiKeyValidated(this ILogger logger, string keyId, string tenantId);
}
```

## Consequences & Mitigation (結果と緩和策)

- **Positive**: 実行時のアロケーションがゼロになり、構造化ログのキー名が固定されるため、クエリ効率が向上する。
- **Negative**: 定義ファイルが増える。
- **Mitigation**: テンプレート化により、定義のコストを最小限に抑える。
