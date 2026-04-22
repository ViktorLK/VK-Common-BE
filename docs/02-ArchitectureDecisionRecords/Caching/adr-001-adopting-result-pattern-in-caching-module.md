# ADR 001: Adopting Result Pattern in Caching Module

- **Date**: 2026-03-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Caching

## 2. Context (背景)

VK.Blocks.Caching モジュールのアーキテクチャ監査において、戻り値が `T?` であり、プロバイダー障害やシリアライゼーションエラーなどのインフラレベルの異常が呼び出し側に適切に伝達されない問題が指摘されました。

## 3. Problem Statement (問題定義)

現在の実装には以下の課題があります：
- **不透明なエラー処理**: エラー発生時に `null` を返すか例外をスローするかが混在しており、呼び出し側が原因（キャッシュミスなのかプロバイダー障害なのか）を特定できません。
- **一貫性の欠如**: Application Layer (Handlers) では `Result<T>` が強制されていますが、BuildingBlock レベルで `null` を許容しているため、レイヤー境界でのマッピングが煩雑になっています。
- **テスト容易性の低下**: 異常系のテストにおいて、例外を期待するか `null` を期待するかのセマンティクスが曖昧です。

## 4. Decision (決定事項)

`ICacheBlock` および内部の `ICacheProvider` のすべてのパブリックメソッドの戻り値を `Result<T>` または `Result` パターンへ移行します。

### 接口定義の変更案

```csharp
public interface ICacheBlock
{
    Task<Result<T?>> GetAsync<T>(string key, CancellationToken ct = default);
    Task<Result> SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken ct = default);
}
```

## 5. Alternatives Considered (代替案の検討)

### Option 1: 現状維持 (T? + Exceptions)
- **Approach**: キャッシュミスは `null`、異常事態は例外スロー。
- **Rejected Reason**: VK.Blocks Rule 1 (Result Pattern) に反し、フロー制御に例外を使用することになるため。

### Option 2: TryGet パターン (bool TryGet(out T value))
- **Approach**: 標準的な Try パターン。
- **Rejected Reason**: 非同期メソッド (`async`) では `out` パラメータが使用できず、またエラー詳細を保持できないため。

## 6. Consequences & Mitigation (結果と緩和策)

### Positive
- **堅牢性**: キャッシュ障害時に「失敗」として明示的に処理でき、フォールバック（DBへの直接参照など）を安全に実行可能。
- **一貫性**: サービス全体で統一されたエラーハンドリング体系（RFC 7807 準拠）を実現。

### Negative
- **コードの冗長化**: 呼び出し側で `if (result.IsFailure)` のチェックが必要になる。

### Mitigation
- **拡張メソッドの提供**: `GetValueOrThrow()`, `Match()` などの高階関数を提供し、記述を簡潔に保つ。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **階層化エラー**: `CachingErrors` を定義し、原因（ProviderError, SerializationError 等）を特定可能にします。
- **セキュリティ**: キャッシュキーや値に機密情報が含まれる場合、エラーメッセージに具体的なデータを含めないよう、構造化ログと連携します。
