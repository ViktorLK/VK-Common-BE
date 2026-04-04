# ADR 007: Optimize High-Frequency Auth Interfaces with ValueTask

**Date**: 2026-03-29
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: Authentication Module Performance Tuning

## 2. Context (背景)

Authentication モジュールの下位プロバイダー（API Keyのレートリミッターや、Tokenの失効確認プロバイダーなど）は、APIのあらゆるエンドポイントへのリクエストの度・高頻度に呼び出されます。
前回の ADR-006 にて、コアパッケージは単一ノードにおいて `InMemory` 実装で動作するようになりました。この `InMemory` 実装では、DB通信やネットワークI/Oを伴わないため、すべての認証チェックが同期的（Synchronous）に完了します。

## 3. Problem Statement (問題定義)

現在の各プロバイダー・インターフェイスのシグネチャ（例: `Task<bool> IsAllowedAsync(...)`）は `Task` ベースの非同期モデルを強制しています。この状態では、内部処理が完全に同期的にブロックなく完了したとしても、呼び出しごとに `Task` オブジェクトがヒープ（Heap）上にアロケーションされます。
（`Task.CompletedTask` や、内部的に静的キャッシュされる `Task.FromResult(true)` など一部の最適化は機能しますが、汎用的な返り値・動的な値の返却時には常に割り当てが生じます。）

高トラフィック・高並行のシナリオ下において、毎回のHTTPリクエストで発生するこれらの無駄な `Task` オブジェクトのメモリリアロケーションは、ガベージコレクション（GC）の圧力と遅延を増加させる深刻なボトルネックになり得ます。

## 4. Decision (決定事項)

高頻度に呼び出され、かつ「同期的に完了する可能性が非常に高い」下位インターフェイスの戻り値を、すべて `ValueTask` / `ValueTask<T>` に最適化します。

**変更されるインターフェイス（最適化対象）**:
- `IApiKeyRateLimiter`
- `IApiKeyRevocationProvider`
- `ITokenRevocationProvider`
- `IRefreshTokenValidator`

**非対称最適化（最適化対象外）**:
すべての非同期処理を `ValueTask` にするわけではありません。以下のような明確なI/Oバウンドな操作や、高度なオーケストレーションを行うサービスレイヤーは、状態機械のオーバーヘッドや利用制約を考慮し、従来の `Task` のまま維持します。
- `IApiKeyStore` (データベースへの問い合わせ等、必ず非同期I/Oが発生するインフラ層)
- `IJwtAuthenticationService` や `ITokenRevocationService` (高レベルのビジネスユースケース層)

## 5. Alternatives Considered (代替案の検討)

- **Option 1**: キャッシュされた Task (e.g. `Task.FromResult`) に依存し、シグネチャは `Task` のままにする。
  - **Rejected Reason**: 真・偽等のブーリアン値についてはキャッシュが効きますが、`Result<T>` のようなカスタムオブジェクトを返す場合は必ずアロケーションが発生してしまうため不十分です。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
  - `InMemory` 構成時のヒープメモリのアロケーションがゼロになり、GCプレッシャーとレイテンシを大幅に改善します（Zero-Allocation 同期実行）。
- **Negative**:
  - `ValueTask` には「複数回の `await` ができない」「`.AsTask()` しないと並列実行できない」といった、`Task` よりも厳格な利用上の制約があり、実装者に正しい知識が求められます。
- **Mitigation**:
  - プロバイダ層であるため、最上位層の AuthenticationHandler 内で一度だけ `await` されるような直線的な利用パターンのみに限定されており、複雑な Task の再利用リスクは極めて低いです。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- コアパッケージの `InMemory` 実装群においては、同期的に値が確定した場合直ちに `new ValueTask<bool>(result)` として返すことで、アロケーションを完全に回避します。
- 一方、`StackExchangeRedis` パッケージなどの実際の分散インフラにおいては本物の非同期I/Oが発生しますが、`async ValueTask` を使用することで、C#の非同期ステートマシンが自動的にI/O完了を待機する設計となっており、インターフェイスの契約にシームレスに適合しつつ安全性も担保されます。
