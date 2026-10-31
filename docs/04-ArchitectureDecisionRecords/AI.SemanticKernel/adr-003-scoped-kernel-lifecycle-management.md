# ADR 003: Scoped Kernel Lifecycle Management

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI.SemanticKernel Infrastructure Lifecycle

## 1. Context (背景)

Semantic Kernel (SK) のインスタンスおよびそのプラグインは、実行時のコンテキスト（CorrelationId、UserContext、TenantId 等）を意識した動作が求められることが多々あります。これらの情報を安全かつ効率的にプラグインへ伝播させ、リソースの適切な破棄を保証するためには、DI コンテナにおける `Kernel` インスタンスのライフサイクルを慎重に決定する必要があります。

## 2. Problem Statement (問題定義)

`Kernel` のライフサイクル選択には以下のトレードオフがあります。

- **Singleton Risk**: `Kernel` を Singleton として登録すると、リクエスト固有の情報（スレッドローカルでないユーザー情報等）をプラグインに安全に持たせることが困難になり、スレッドセーフティの問題が発生する。
- **Transient Overhead**: `Kernel` を Transient として登録すると、サービスが解決されるたびに新しいインスタンスが作成され、特にプラグインの数が多い場合にメモリ割り当てと初期化のオーバーヘッドが増大する。
- **Resource Cleanup**: `Kernel` やプラグインが `IDisposable` を実装している場合、明示的な破棄ロジックがないとリソースリークの原因となる。

## 3. Decision (決定事項)

Semantic Kernel インスタンス（`Kernel`）およびそれに関連するエンジン（`AISKChatEngine`, `AISKEmbeddingEngine`）を **Scoped** ライフサイクルとして登録します。

1. **DI Configuration**:
   - `services.TryAddScoped<Kernel>(...)` を使用して DI 登録を行います。
2. **Reuse within Scope**:
   - これにより、1つの論理的なリクエスト（例：HTTP リクエストやメッセージキューの処理）の間、同じ `Kernel` インスタンスが再利用されます。
3. **Contextual Integration**:
   - Scoped ライフサイクルにより、プラグインはコンストラクタ注入を通じて他の Scoped サービス（`IVKCorrelationContext` や `ITenantProvider` 等）を直接受け取ることができます。
4. **Deterministic Disposal**:
   - スコープの終了時に `IServiceProvider` によって `Kernel` およびプラグインのリソースが適切に管理・破棄されることを保証します。

### 核心的な DI 登録コード

```csharp
// AISKBlockRegistration.cs 内部での登録例
services.TryAddScoped<Kernel>(sp =>
{
    var builder = Kernel.CreateBuilder();
    // ... 設定ロジック
    return builder.Build();
});

services.TryAddScoped<IVKChatEngine, AISKChatEngine>();
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Singleton Kernel
- **Approach**: アプリケーション全体で1つの `Kernel` を共有する。
- **Rejected Reason**: ステートフルなプラグインや、ユーザーコンテキストに依存する処理において、データの混入（Data Contamination）のリスクを排除できないため。

### Option 2: Transient Kernel
- **Approach**: 解決されるたびに新しい `Kernel` インスタンスを作成する。
- **Rejected Reason**: 1つのリクエスト内で複数のサービスが `IVKChatEngine` を必要とする場合、無駄な再生成コストが発生するため。Scoped ライフサイクルの方がパフォーマンスとコンテキスト共有のバランスが取れていると判断した。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Thread Safety**: 同一スレッド（リクエストスコープ）内でのみインスタンスが共有されるため、競合状態を避けつつ安全にコンテキストを共有できる。
- **Seamless DI Integration**: 他の Scoped サービスとの連携が自然になり、複雑なファクトリロジックを排除できる。
- **Efficient Resource Management**: リクエスト終了時に一括してクリーンアップが行われる。

### Negative
- **Scope Dependency**: Scoped サービスを解決できない環境（例：バックグラウンドタスクの初期化時など）では、明示的にスコープを作成する必要がある。

### Mitigation
- `IServiceScopeFactory` を使用して、バックグラウンド処理においても適切なスコープ管理が行われるようにドキュメントとサンプルコードでガイダンスを提供する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **HttpClient Lifecycle**: `Kernel` は Scoped ですが、内部の `HttpClient` は `IHttpClientFactory` を介して Singleton 的に管理されるように構成し、ソケットの枯渇（Socket Exhaustion）を防止します。
- **Tenant Isolation**: `ITenantProvider` などの Scoped サービスを注入することで、AI 操作が現在のテナント境界を越えないように厳密に制御します。

## 7. Status
✅ Accepted
