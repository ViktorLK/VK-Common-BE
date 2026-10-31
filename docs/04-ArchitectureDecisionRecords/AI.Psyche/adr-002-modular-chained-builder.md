# ADR 002: Modular Chained Builder

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psycheモジュールは、Directive、Echo、Knowledge、Persona、Pipeline、Weavingなど、それぞれ独自のオプションや依存関係を持つ複数のサブ機能（Features）から構成されている。これらすべての機能がモノリシックに強制ロードされる設計は、一部の機能のみを利用したい軽量なアプリケーションにおいて余分な依存関係や不要なオブジェクト初期化コストを発生させる。

## 2. Problem Statement (問題定義)

モノリシックな登録設計や拡張性の低い登録方法には、以下の問題がある：
1. **設定のオーバーヘッド**: 特定の機能（例：Echoのみ）が必要な場合でも、KnowledgeやPersona関連のデフォルト設定やバリデーションロジックまで強制的に登録・検証されてしまう。
2. **カスタマイズの複雑さ**: 特定のサブ機能に対して個別にカスタムのOptions設定（Options Transform）を差し挟むインターフェースが不足する。
3. **可読性の欠如**: DIコンテナの登録ロジックが肥大化し、どの機能が有効化されているのかが直感的に理解しにくい。

## 3. Decision (決定事項)

可插拔（Pluggable）なモジュール設計を実現するため、**「Modular Chained Builder」パターン**を採用する。

1. **`IVKAIPsycheBuilder` インターフェースの導入**:
   - `AddVKAIPsycheBlock` の戻り値として `IVKAIPsycheBuilder` を返却する。
2. **流式（Fluent）拡張メソッドの定義**:
   - `VKPsycheBuilderExtensions` 内に、各機能を個別に登録する拡張メソッド（例：`AddVKDirective()`, `AddVKEcho()`, `AddVKKnowledge()`）を定義する。
   - 各メソッドは `Func<TOptions, TOptions>? transform = null` 引数を受け入れ、個別の上書きカスタマイズを可能にする。
3. **一善式デフォルト機能群の提供**:
   - `AddVKDefaultFeatures()` メソッドを提供し、標準的なユースケースに対応するすべてのコア機能を一括で有効化するパスを用意する。

### 核心的なDIビルダー設計

```csharp
namespace VK.Blocks.AI.Psyche;

public interface IVKAIPsycheBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}

public static class VKPsycheBuilderExtensions
{
    public static IVKAIPsycheBuilder AddVKEcho(
        this IVKAIPsycheBuilder builder,
        Func<VKEchoOptions, VKEchoOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        EchoFeature.Register(builder, transform);
        return builder;
    }

    public static IVKAIPsycheBuilder AddVKDefaultFeatures(this IVKAIPsycheBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKDirective()
            .AddVKEcho()
            .AddVKKnowledge()
            .AddVKPersona()
            .AddVKPipeline()
            .AddVKWeaving();
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Parameter-based Configuration in AddVKBlock
- **Approach**: `AddVKAIPsycheBlock(this IServiceCollection services, Action<PsycheBlockConfig> configure)` のように、設定デリゲートに機能の有効/無効フラグを持たせる。
- **Rejected Reason**: 設定クラス自体が肥大化し、将来的な新機能の追加時にインターフェースを変更せざるを得ないため。

### Option 2: Individual DI Register Methods on ServiceCollection
- **Approach**: `services.AddVKAIPsycheEcho()`, `services.AddVKAIPsycheKnowledge()` のように個別メソッドを直接 `IServiceCollection` に生やす。
- **Rejected Reason**: 関連のないDI登録順序の混乱を招きやすく、Psycheの文脈外（Builderスコープ外）で無制限にパーツが登録されてしまうため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **高い柔軟性**: クライアントは必要な機能のみを選択してロードし、不要なステージの実行をスキップ可能。
- **カプセル化**: DIの登録ロジックが各Featureの `Internal/` 下の Feature クラス（例：`EchoFeature`）に隠蔽されるため、モジュール境界が極めて強固になる。

### Negative
- **学習コスト**: 新しい開発者は、デフォルト設定を使うために `AddVKDefaultFeatures()` を呼び出す必要があることを理解する必要がある。

### Mitigation
- READMEやサンプルコードに、基本的には `AddVKDefaultFeatures()` を呼び出すコードテンプレートを掲載し、必要に応じて個別カスタマイズを行う手順を明記する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Idempotency**: 各 `Feature.Register` 実装内では `TryAdd` パターンを使用し、誤って同じ機能が二重に登録された場合でも、DIコンテナが不安定にならないように制御する。
- **Marker Check**: `IVKAIPsycheBuilder` は親ブロックが正常に初期化（Mark-Self）されている前提で動作するため、不正な順序でのDI構築をビルド時に防ぐ。

## 7. Status
✅ Accepted
