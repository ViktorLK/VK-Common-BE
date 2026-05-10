# ADR 001: Deferred Plugin Registration Pattern

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI.SemanticKernel Plugin Infrastructure

## 1. Context (背景)

Semantic Kernel (SK) のプラグインは、ロガー、HttpClient、あるいはカスタムのドメインサービスなど、DI コンテナ内の他のサービスに依存することが一般的です。通常、SK の `Kernel` オブジェクトとそのプラグインは DI 登録フェーズ（`IServiceCollection` の構成時）に設定されますが、この時点では依存するサービスが完全に使用可能でなかったり、実行時のスコープに基づいて解決する必要があったりする場合があります。

## 2. Problem Statement (問題定義)

DI 登録フェーズでプラグインを直接インスタンス化して登録する手法には、以下の課題があります。

- **Resolution Issues**: プラグインが依存するサービスがまだ DI に登録されていない、あるいはスコープが異なる（Singleton から Scoped を参照しようとする等）場合に実行時エラーが発生する。
- **Brittle Configuration**: プラグインの追加順序や DI の登録順序に強く依存した設計になり、保守性が低下する。
- **Lack of Context**: `Kernel` インスタンスが実際に作成されるまで、どのようなサービスプロバイダーのコンテキストでプラグインが動作するかが確定しない。

## 3. Decision (決定事項)

`IAISKPluginProvider` を用いた「遅延プラグイン登録（Deferred Plugin Registration）」パターンを導入します。

1. **Abstraction**:
   - `KernelBuilder` に対してプラグインを追加するロジックをカプセル化する内部インターフェース `IAISKPluginProvider` を定義します。
2. **Registration**:
   - `AddPlugin<T>()` 等の拡張メソッドが呼び出された際、プラグインそのものではなく、そのプラグインを登録するための `IAISKPluginProvider` 実装を DI に登録します。
3. **Execution**:
   - `Kernel` インスタンスが DI から解決される際、DI 内のすべての `IAISKPluginProvider` を取得し、それらの `Register` メソッドを `KernelBuilder` に対して実行します。
4. **Context Access**:
   - これにより、プラグインのインスタンス化と構成は `Kernel` が必要になったタイミングで行われ、その時点の `IServiceProvider` にフルアクセスできるようになります。

### 核心的なインターフェースと DI 登録ロジック

```csharp
internal interface IAISKPluginProvider
{
    void Register(IKernelBuilder builder, IServiceProvider serviceProvider);
}

// DI 登録時の処理
services.AddSingleton<IAISKPluginProvider>(new AISKDelegatePluginProvider((builder, sp) => {
    // 実際に Kernel をビルドする際に実行される
    builder.Plugins.Add(KernelPluginFactory.CreateFromType<T>(null, sp));
}));
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Direct Registration in KernelBuilder
- **Approach**: `AddVKAIBlock` の中で直接 `builder.Plugins.AddFromType<T>()` を呼び出す。
- **Rejected Reason**: コンパイル時の型情報には依存できるが、実行時に DI から解決されたインスタンスをプラグインに渡すことが難しいため。

### Option 2: Custom Factory for Kernel
- **Approach**: `Kernel` を作成する独自のファクトリクラスを作成し、その中で手動でプラグインを管理する。
- **Rejected Reason**: VK.Blocks の標準的な Options パターンや DI 拡張メソッドの流儀に合わず、利用側のコードが複雑になるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Clean Dependency Resolution**: プラグインが DI 内のあらゆるサービス（Scoped 含む）を安全に参照できるようになる。
- **Decoupled Registration**: プラグインの登録と `Kernel` の構築が分離され、柔軟な構成が可能になる。
- **Standardized Infrastructure**: すべてのプラグインが同じインターフェースを介して管理されるため、一貫したログ出力や検証が可能。

### Negative
- **Indirection**: 直接登録する場合に比べ、内部的な抽象化レイヤーが一段増える。

### Mitigation
- 開発者向けに `AddPlugin` 拡張メソッドを提供し、内部の `IAISKPluginProvider` の存在を意識させないシームレスな API を維持する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Service Lifetime**: `IAISKPluginProvider` 自体は Singleton または Scoped として登録され、`Kernel` のライフサイクル（通常は Scoped）に合わせて実行される。
- **Error Handling**: プラグインの登録中に発生した例外はキャッチされ、`Diagnostics` を通じて記録されることで、DI 解決時の不透明なエラーを防止する。

## 7. Status
✅ Accepted
