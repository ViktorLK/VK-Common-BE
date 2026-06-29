# ADR 004: Strict Overrides Contract

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

プロンプト構築処理（Weaving）は、基本設定（Options）に基づいて動作するが、リクエストレベルで一部の設定を動的に上書きしたいケース（例：特定のセッションだけで最大履歴長 `MaxTurns` を小さくする、特定の知識マッチングを無効化するなど）が多発する。しかし、システム全体の安全性を担保するためには、接続文字列やプロバイダーの種別など、リクエスト単位で決して変更されてはならない「グローバルインフラオプション」が混入することを防止しなければならない。

## 2. Problem Statement (問題定義)

Optionsクラス全体をそのままリクエスト引数として渡す、あるいは動的辞書（Dictionary）に頼る設計には、以下のリスクがある：
1. **設定リークとセキュリティ違反**: 開発者がリクエストを介して内部インフラ設定（例：接続情報、エンドポイントURI）を改ざん可能になり、マルチテナント間でのテナント境界汚染やセキュリティ脆弱性が発生する。
2. **型安全性の喪失**: 自由なキーバリュー（`Dictionary<string, object>`）を用いた上書きは、プロパティ名のミスタイプ（タイポ）をコンパイルタイムに検出できない。
3. **マージロジックの複雑化**: どの値がデフォルトで、どの値が上書き値であるかを判定・マージするロジックが各コードで分散し、バグの温床となる。

## 3. Decision (決定事項)

設定パラメータの安全性と確実なマージをコンパイルタイムで強制するため、**「Strict Overrides Contract (厳格な上書き契約)」**を採用する。

1. **グローバル Options とローカル Overrides インターフェースの分離**:
   - `IVK...Options` はすべての設定パラメータを定義する。
   - `IVK...Overrides` は、**リクエストレベルで変更が許可されたプロパティのみ**を定義する。
2. **`Args` クラスによる実装の制限**:
   - リクエストパラメータとして受け取る `xxxArgs`（例：`VKEchoArgs`）は、対応する `IVKxxxOverrides` インターフェースのみを実装し、他のプロパティに一切アクセスさせない。
3. **空合流演算子によるマージの統一**:
   - マージ時は、安全な null 判定と空合流演算子を用いて、リクエストの値が存在すればそれを使い、存在しなければグローバルオプションにフォールバックするロジックを定型コードとして実装する。
   - 例: `args?.MaxTurns ?? _options.MaxTurns`

### 核心的なオーバーライド設計例

```csharp
namespace VK.Blocks.AI.Psyche;

// 変更が許可されたプロパティのみを公開
public interface IVKEchoOverrides
{
    int? MaxTurns { get; }
    double? TokenBudgetRatio { get; }
}

public interface IVKEchoOptions : IVKEchoOverrides
{
    string StoreType { get; } // これはオーバーライド不可能
}

public sealed record VKEchoArgs : IVKEchoOverrides
{
    public int? MaxTurns { get; init; }
    public double? TokenBudgetRatio { get; init; }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Mutable Deep Clone of Options Object
- **Approach**: グローバル Options オブジェクトをリクエストごとに `MemberwiseClone` 等で複製し、上書きプロパティを直接上書きする。
- **Rejected Reason**: オプションオブジェクトがイミュータブルでなくなるため、スレッドセーフティが著しく低下し、意図せず元のシングルトンオプションまで変更されてしまうリスクがあるため。

### Option 2: Generic Key-Value Overrides Container
- **Approach**: すべての上書き値を `Dictionary<string, object>` で渡し、リフレクションでマージする。
- **Rejected Reason**: プロパティ名の変更に追従できず、リフレクションによる実行時オーバーヘッドが生じ、パフォーマンス要件を満たせないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **強固なセキュリティ境界**: データベース接続情報やストレージキーといった機密性の高いパラメータが、動的リクエストによって上書き・改ざんされるリスクが完全に排除される。
- **コンパイルタイム安全**: どの設定が上書き可能であるかがインターフェースの型情報で明確になり、エディタ上での開発体験が向上する。

### Negative
- **コード量の増加**: 各Featureごとに `Options` インターフェース、`Overrides` インターフェース、`Args` レコードの3つを個別に宣言する必要があり、ボイラープレートコードが増加する。

### Mitigation
- 将来的には、ソースジェネレーターを導入して `IVKxxxOverrides` から `VKxxxArgs` クラスを自動生成する（AP.05）ことで、手動での記述量を削減する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Null Handling**: 上書き指定がない場合（`args` またはそのプロパティが `null` の場合）は、完全にグローバル定義された安全なデフォルト値へ強制的にフォールバックさせ、システムが不安定な設定状態で実行されないように防御する。

## 7. Status
✅ Accepted
