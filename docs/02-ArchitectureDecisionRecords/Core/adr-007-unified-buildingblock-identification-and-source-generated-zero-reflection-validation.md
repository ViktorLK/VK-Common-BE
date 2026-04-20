# ADR-007: Unified BuildingBlock Identification and Source-Generated Zero-Reflection Validation

- **Date**: 2026-04-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks Core / Automation / Zero-Reflection Refactoring

## 1. Context (背景)

VK.Blocks フレームワークにおいて、各 BuildingBlock は独自のメタデータ（識別子、バージョン、診断ソース名など）を持ち、相互の依存関係を検証する仕組みを必要としています。

以前、ADR-003 で導入された「サービスマーカー」パターンはリフレクションに依存しており、ADR-007 (旧) / ADR-008 で検討された `static abstract` メンバによるアプローチは型安全ではありましたが、以下の課題が残っていました：
1.  **再帰的な依存関係検証の困難さ**: ジェネリック型引数を介した静的メンバのアクセスは、実行時の再帰的な型解決と相性が悪く、ロジックが複雑化しがちでした。
2.  **ボイラープレートの多さ**: 全てのマーカークラスに `Instance` シングルトンやメタデータプロパティを記述するのは開発者の負担となっていました。
3.  **部分実装の混在**: `Identifier` と `BlockName` の役割が重複し、命名規約が曖昧でした。

## 2. Problem Statement (問題定義)

1.  **パフォーマンス**: 依存関係の検証（`EnsureVKBlockRegistered`）をリフレクションなしで、かつコンパイル時に近い速度で行う必要があります。
2.  **保守性**: マーカクラスの実装を極限までシンプルにし、定型コード（シングルトン、インターフェース実装）を自動化する必要があります。
3.  **整合性**: ブロック識別子（Identifier）を唯一の正（Single Source of Truth）とし、診断ソース名やメトリクス名がそれに基づいて自動導出される必要があります。

## 3. Decision (決定事項)

1.  **マーカーの名称変更と整理**:
    - 全ての BuildingBlock 識別用インターフェースを `IVKBlockMarker` に統一します。
    - `BlockName` を廃止し、`Identifier` を唯一の識別用プロパティとします。
2.  **ソースジェネレーターによるコード注入**:
    - `VKBlockMarkerGenerator` を導入し、`IVKBlockMarker` を実装した `partial` クラスに対して以下のコードを自動生成します：
        - `public static IVKBlockMarker Instance { get; } = new T();` (シングルトンの注入)
        - `IVKBlockMarkerProvider<T>` の実装（静的アクセスを容易にするためのブリッジ）
3.  **静的抽象メンバの限定利用**:
    - `IVKBlockMarker` インターフェースからは `static abstract` を排除し、代わりに `IVKBlockMarkerProvider<T>` を介してシングルトン `Instance` にアクセスするパターンを採用します。これにより、再帰的な依存ツリーの走査が容易になります。
4.  **依存関係検証の厳格化**:
    - `EnsureVKBlockRegistered<TRequired, TDependent>` において、`TRequired` に対して `IVKBlockMarker` かつ `IVKBlockMarkerProvider<TRequired>` であることをコンパイル時に制約（Constraint）として課します。

### Interface Definition

```csharp
public interface IVKBlockMarker
{
    string Identifier { get; }
    string Version { get; }
    IReadOnlyList<IVKBlockMarker> Dependencies { get; }
    string ActivitySourceName { get; }
    string MeterName { get; }
}

public interface IVKBlockMarkerProvider<TSelf> where TSelf : IVKBlockMarkerProvider<TSelf>
{
    static abstract IVKBlockMarker Instance { get; }
}
```

### Implementation Pattern (Minimal Boilerplate)

```csharp
// 開発者が書くのはこれだけ (partial が必須)
namespace VK.Blocks.Caching.Contracts;

public sealed partial class CachingBlock : IVKBlockMarker
{
    public string Identifier => "Caching";
    public string Version => "1.0.0";
    public IReadOnlyList<IVKBlockMarker> Dependencies => [CoreBlock.Instance];
    
    public string ActivitySourceName => VKBlocksConstants.VKBlocksPrefix + Identifier;
    public string MeterName => VKBlocksConstants.VKBlocksPrefix + Identifier;
}
```

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 全てを static abstract メンバにする**
    - **Rejected**: シングルトンインスタンスがないと、`GetRequiredService` などの DI コンテナ内での「登録済みマーカー」の再帰的チェックが実装しにくいため。
- **Option 2: 属性（Attribute）による識別**
    - **Rejected**: 実行時のリフレクションコストが発生し、Rule 15 (Zero-Reflection) の精神に反するため。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 全 BuildingBlock の識別子が型安全かつ、リフレクションなしの最速（ポインタアクセスと同等）で取得可能。
    - マーカー作成時のボイラープレートが大幅に削減（ジェネレーターが `Instance` を用意するため）。
    - 依存関係の不整合がコンパイルエラーとして即座に判明。
- **Negative**:
    - 全 BuildingBlock でマーカークラスを `partial` に変更し、名前空間を整理する大規模なマイグレーションが発生しました。
- **Mitigation**:
    - Core モジュールのリファクタリングに合わせて全ブロックを修正済み。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `VKBlockMarkerGenerator` は `_globalModules` 設定により、`VK.Blocks.*` プロジェクトであれば自動的にマーカーを検知してコードを生成します。
- 生成された `Instance.g.cs` は開発者からは隠蔽され、コードナビゲーションの邪魔になりません。
- セキュリティ面では、各ブロックのメタデータは不変（Immutable）であり、外部からの不正な変更は不可能です。

**Last Updated**: 2026-04-20
