# ADR-008: Robust BuildingBlock Dependency Validation through Pre-order Traversal and Cycle Detection

- **Date**: 2026-04-20
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks Core / Stability / Identity Management

## 1. Context (背景)

VK.Blocks では、BuildingBlock 間の依存関係を検証する `EnsureDependenciesRegistered` を導入していますが、初期の実装には以下の課題がありました：
1.  **走査順序の不備**: 直接の依存先を確認する前にその子の検証（再帰）を行っていたため、エラーメッセージが根本原因（直接の親）を指さない場合があった。
2.  **循環参照のリスク**: A -> B -> A のような依存関係において無限再帰が発生する。
3.  **型への過度な依存**: `IsVKBlockRegistered` がマーカーの具象型に依存していたため、DI 登録時の型が異なると正しく検知できないリスクがあった。
4.  **API の汚染**: 再帰用の状態管理（`HashSet`）が公開メソッドの引数に露出しており、カプセル化が不十分であった。

## 2. Problem Statement (問題定義)

1.  **デバッグ性の向上**: エラーメッセージを常に「直接の不足している依存先」に向け、トラブルシューティングを容易にする。
2.  **安定性の確保**: 無限ループによる `StackOverflowException` を完全に防止する。
3.  **アイデンティティの分離**: マーカーがどのような型で DI に登録されていても、一意の `Identifier` (Slug) を基に正しく登録状況を判定できるようにする。
4.  **API アーキテクチャの洗練**: 再帰の内部状態を隠蔽し、クリーンな公開 API を提供する。

## 3. Decision (決定事項)

1.  **Pre-order CHECK (先行チェック) とカプセル化**:
    `IVKBlockMarker` の公開 API を `EnsureDependenciesRegistered(services, dependentId)` のみに絞り、内部の再帰はインターフェース内の `private static EnsureCore` メソッドに封じ込めます。
2.  **Identifier 基盤の登録チェック (Runtime Marker)**:
    `VKBlockRuntimeMarker` レコードを導入し、`AddVKBlockMarker` 時に Identifier をキーとして DI に登録します。検証時は型ではなくこの Identifier ベースで検索を行います。
3.  **HashSet による循環検知の徹底**:
    `EnsureCore` 内部で `HashSet<string> visited` を持ちまわり、訪問済みの識別子を検知した場合は即座にリターンすることで循環参照を回避します。

### Final Architecture (Implementation Details)

```csharp
public interface IVKBlockMarker
{
    // ... metadata properties
    
    // 公開 API: カプセル化された入り口
    void EnsureDependenciesRegistered(IServiceCollection services, string dependentId)
    {
        EnsureCore(this, services, dependentId, []);
    }

    // 内部実装: static にすることで状態(visited)を共有しつつ再帰
    private static void EnsureCore(IVKBlockMarker marker, IServiceCollection services, string dependentId, HashSet<string> visited)
    {
        if (!visited.Add(marker.Identifier)) return;

        foreach (var dependency in marker.Dependencies)
        {
            // 1. まず直接の親を確認 (Pre-order) かつ Identifier ベースで検索
            if (!services.IsVKBlockRegistered(dependency.Identifier))
            {
                throw new InvalidOperationException(...);
            }

            // 2. 親が健全であれば子を探索
            EnsureCore(dependency, services, marker.Identifier, visited);
        }
    }
}
```

## 4. Alternatives Considered (代替案の検討)

- **Option 1: 公開メソッドに HashSet? を残す**
    - **Rejected**: API の利用者が内部実装（再帰の仕組み）を意識せざるを得なくなり、DRY 原則やカプセル化に反するため。
- **Option 2: 属性(Attribute)ベースの識別**
    - **Rejected**: リフレクションが必要となり、Rule 15 (Zero-Reflection) に反するため。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - デバッグメッセージの正確性が向上し、依存関係の解決が迅速化。
    - 無限ループが防止され、ランタイムの安全性が向上。
    - マーカーの登録方法（Singleton vs Interface 登録など）に左右されない堅牢な識別を実現。
- **Negative**:
    - `HashSet` の生成と `VKBlockRuntimeMarker` の追加登録により、起動時に極微細なオーバーヘッドが生じます（実用上の影響はなし）。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- `VKBlockRuntimeMarker` は `internal` であり、外部から直接操作されることはありません。
- `Identifier` は Slug であることが保証（ADR-007）されているため、文字列ベースの検索でも高い信頼性が保たれます。

**Last Updated**: 2026-04-20
