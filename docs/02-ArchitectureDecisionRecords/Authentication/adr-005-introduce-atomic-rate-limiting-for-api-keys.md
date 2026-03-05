# ADR 005: Introduce Atomic Rate Limiting for API Keys

**Date**: 2026-03-05  
**Status**: 📝 Draft  
**Deciders**: Architecture Team  
**Technical Story**: `src/BuildingBlocks/Authentication`  
**Depends on**: ADR 004 (Rate Limiter の初期導入)

## 1. Context (背景)

ADR 004 のフェーズ3において、`IApiKeyRateLimiter` インターフェースと `DistributedCacheApiKeyRateLimiter` 実装が導入された。Fixed Window アルゴリズムを採用し、`IDistributedCache` を通じて分散環境でのレート制限を実現している。

しかし、2026年3月のアーキテクチャ監査において、現在の実装に **競合状態 (Race Condition)** が存在し、高スループット環境ではレートリミットが正確に機能しないことが判明した。

## 2. Problem Statement (問題定義)

### 競合状態の詳細

```csharp
// ApiKeys/DistributedCacheApiKeyRateLimiter.cs (L14-43)
public async Task<bool> IsAllowedAsync(Guid keyId, int limitPerMinute, CancellationToken ct = default)
{
    var windowKey = GetWindowKey(keyId);

    // ❌ Step 1: Read — 複数リクエストが同時にカウンターを読み取り
    var countStr = await cache.GetStringAsync(windowKey, ct).ConfigureAwait(false);
    var count = countStr != null && int.TryParse(countStr, out var c) ? c : 0;

    if (count >= limitPerMinute) return false;

    // ❌ Step 2: Increment + Write — 各リクエストが独立にインクリメント
    count++;
    await cache.SetStringAsync(windowKey, count.ToString(), options, ct).ConfigureAwait(false);

    return true;
}
```

### 競合シナリオ

```
時刻 T=0, limit=60, 現在のカウント=59

Thread A: GET windowKey → count=59 → 59 < 60 → count=60 → SET 60
Thread B: GET windowKey → count=59 → 59 < 60 → count=60 → SET 60  ← 同時読み取り
Thread C: GET windowKey → count=59 → 59 < 60 → count=60 → SET 60  ← 同時読み取り

結果: 3リクエストが通過 (limit=60 のはずが、実質 count=60 として記録)
実際の処理数: 62 (制限超過)
```

**影響**: DDoS やブルートフォース攻撃の緩和策としてレートリミッターが機能せず、APIキーの不正使用を防げない。

## 3. Decision (決定事項)

**`IDistributedCache` の抽象化を超えて、Redis 固有のアトミック操作 (`INCR` + `EXPIRE`) を利用するレートリミッター実装に変更する。**

### 3.1 新しいインターフェースの維持

既存の `IApiKeyRateLimiter` インターフェースは変更なし。実装クラスのみ差し替える。

```csharp
public interface IApiKeyRateLimiter
{
    Task<bool> IsAllowedAsync(Guid keyId, int limitPerMinute, CancellationToken ct = default);
}
```

### 3.2 Redis アトミック実装

```csharp
using StackExchange.Redis;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// Atomic fixed-window rate limiter using Redis INCR + EXPIRE commands.
/// Guarantees correct behavior under concurrent access.
/// </summary>
public sealed class RedisAtomicApiKeyRateLimiter(IConnectionMultiplexer redis) : IApiKeyRateLimiter
{
    // Lua script for atomic increment + expire
    private const string LuaScript = """
        local current = redis.call('INCR', KEYS[1])
        if current == 1 then
            redis.call('EXPIRE', KEYS[1], ARGV[1])
        end
        return current
        """;

    private static readonly LuaScript _preparedScript = LuaScript.Prepare(LuaScript);

    public async Task<bool> IsAllowedAsync(Guid keyId, int limitPerMinute, CancellationToken ct = default)
    {
        if (limitPerMinute <= 0) return false;

        var db = redis.GetDatabase();
        var windowKey = GetWindowKey(keyId);

        // Atomic: INCR + conditional EXPIRE in a single round-trip
        var currentCount = (long)await db.ScriptEvaluateAsync(
            _preparedScript,
            new RedisKey[] { windowKey },
            new RedisValue[] { 60 } // TTL in seconds (1 minute window)
        ).ConfigureAwait(false);

        return currentCount <= limitPerMinute;
    }

    private static string GetWindowKey(Guid keyId)
    {
        var now = DateTimeOffset.UtcNow;
        var minuteTimestamp = new DateTimeOffset(
            now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeSpan.Zero
        ).ToUnixTimeSeconds();

        return $"ratelimit:apikey:{keyId}:{minuteTimestamp}";
    }
}
```

### 3.3 DI 登録の変更

```csharp
// AuthenticationBlockExtensions.cs
- services.AddScoped<IApiKeyRateLimiter, DistributedCacheApiKeyRateLimiter>();
+ services.AddScoped<IApiKeyRateLimiter, RedisAtomicApiKeyRateLimiter>();
```

### 3.4 フォールバック戦略

`IConnectionMultiplexer` が DI に登録されていない環境（開発環境やインメモリキャッシュ使用時）のために、既存の `DistributedCacheApiKeyRateLimiter` をフォールバックとして残す。DI 登録時に条件分岐を行う。

```csharp
// AuthenticationBlockExtensions.cs
if (services.Any(s => s.ServiceType == typeof(IConnectionMultiplexer)))
{
    services.AddScoped<IApiKeyRateLimiter, RedisAtomicApiKeyRateLimiter>();
}
else
{
    services.AddScoped<IApiKeyRateLimiter, DistributedCacheApiKeyRateLimiter>();
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: `IDistributedCache` の範囲で楽観的ロック

- **Approach**: `GetAsync` → Check → `SetAsync` の間にバージョニング（ETag）を用いて楽観的ロックを実装。競合時はリトライ。
- **Rejected Reason**: `IDistributedCache` インターフェースは ETag / CAS (Compare-And-Swap) 操作をサポートしていない。カスタム拡張を作成する場合、結局 Redis 固有の `IDatabase` にアクセスする必要があり、抽象化のメリットが失われる。

### Option 2: Sliding Window Log アルゴリズム

- **Approach**: Redis の Sorted Set (`ZADD` + `ZRANGEBYSCORE` + `ZREMRANGEBYSCORE`) を使用して、リクエストのタイムスタンプを正確に追跡。
- **Rejected Reason**: Fixed Window で十分な精度を持つ。Sliding Window は実装の複雑さとメモリ使用量が大幅に増加する（各リクエストが個別エントリとして保存される）。API Key のユースケースでは、分単位の粒度で十分。

### Option 3: ASP.NET Core 組み込みの Rate Limiting Middleware

- **Approach**: .NET 7+ の `Microsoft.AspNetCore.RateLimiting` ミドルウェアを使用し、`FixedWindowRateLimiter` をパーティションキーとして API Key ID を利用。
- **Rejected Reason**:
    - 組み込みレートリミッターはインプロセスの `ConcurrentDictionary` に基づくため、分散環境（複数インスタンス）でのカウント共有ができない。
    - API Key 認証はカスタム `AuthenticationHandler` 内で実行されるため、ミドルウェアパイプラインの順序制約が生じる。
    - ビルディングブロックとしてホストアプリのミドルウェア構成に依存を強制するのは適切ではない。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - アトミック操作により、高負荷・高並行リクエスト下でもレートリミットが **確実に** 機能。
    - Lua Script による単一ラウンドトリップで、ネットワーク遅延を最小化。
    - `EXPIRE` 自動設定により、カウンターのライフサイクル管理が簡素化。
- **Negative**:
    - `IDistributedCache` の抽象化を超えた **Redis への明示的依存** が導入される。
    - `StackExchange.Redis` NuGet パッケージへのビルディングブロックの依存追加。
    - Redis が利用不可の環境では、レートリミットの精度が既存の非アトミック実装に退行する。
- **Mitigation**:
    - フォールバック戦略（3.4 節参照）により、Redis 非使用環境でもアプリケーションが動作。
    - `StackExchange.Redis` の依存は、既に `IDistributedCache` の Redis 実装（`Microsoft.Extensions.Caching.StackExchangeRedis`）で間接的に存在するため、追加の依存コストは最小限。
    - 将来的に、`IConnectionMultiplexer` に対して Polly の Circuit Breaker を適用し、Redis 障害時はレートリミットをバイパス（またはフォールバック）するレジリエンス設計を検討（Rule 8）。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

### データ構造

| キー形式                                     | TTL                    | 型                     |
| -------------------------------------------- | ---------------------- | ---------------------- |
| `ratelimit:apikey:{keyId}:{minuteTimestamp}` | 60秒 (Window 終了まで) | Redis String (Integer) |

### Lua Script の安全性

- Lua Script は Redis 内でアトミックに実行される（他のコマンドによるインターリーブなし）。
- `INCR` は存在しないキーに対して `0` から開始し、`1` を返す。明示的な初期化不要。
- `EXPIRE` は `current == 1` の場合にのみ設定するため、既存の TTL を上書きしない。

### 攻撃耐性

- **DDoS**: 正確なカウントにより、設定されたしきい値を超えるリクエストが確実にブロックされる。
- **タイミング攻撃**: Fixed Window の境界をまたぐ「バースト」攻撃に対しては、将来的に Sliding Window への移行を検討（ただし現時点では過剰設計）。
- **Key Enumeration**: キーフォーマットに `keyId` (UUID) を使用しているため、推測困難。

### テスト戦略

- **単体テスト**: `IConnectionMultiplexer` を Mock し、`ScriptEvaluateAsync` の呼び出しと戻り値を検証。
- **統合テスト**: Testcontainers で Redis インスタンスを起動し、並行リクエストのシナリオで正確なカウントを検証。
