# ADR 004: Automated Strongly Typed ID Generation

- **Date**: 2026-06-05
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/Tools/SourceGenerators

## 1. Context (背景)

VK.Blocks 体系では、ドメインモデルの識別子（例: `SessionId`、`PersonaId`、`DirectiveId`、`TenantId`）に素の `Guid` や `string` を直接使用することを避ける必要がある。型安全性を高め、引数での誤った ID 渡し（例: PersonaId パラメータに誤って SessionId を渡す等）をコンパイル時に防止するためには、個別の専用型（強タイプ ID）を用意することが推奨される。しかし、これらをレコード構造体（`record struct`）として手動で一つずつ定義すると、シリアライズ（System.Text.Json）、型変換（TypeConverter）、データベースマッピング（EF Core ValueConverter）などの膨大な定型コード（ボイラープレート）を手動で書き続ける必要があり、開発能率を大きく損なう。

## 2. Problem Statement (問題定義)

強タイプ ID を手動で作成・保守することには、以下の問題がある：
1. **膨大な定型コードの重複**: 新しい強タイプ ID を作成するたびに、約 100 行に及ぶ JsonConverter、TypeConverter、EF Core ValueConverter / ValueComparer などのコードを手書きする必要がある。
2. **保守漏れと実装不整合**: ある型では JSON シリアライザを実装したが、別の型では実装し忘れるといった記述のばらつきが発生しやすく、実行時の例外原因となる。
3. **リフレクションの回避制限**: 実行時のリフレクションや汎用ラッパーを用いた処理はアロケーションを増加させ、ホットパスでのパフォーマンスボトルネックとなる。

## 3. Decision (決定事項)

開発能率の向上と 100% コンパイルタイム安全を両立するため、**「Incremental Source Generator for Strongly-Typed IDs (インクリメンタルソースジェネレーターによる強タイプ ID の自動生成)」**を採用する。

1. **`VKStronglyTypedIdAttribute` マーカーの導入**:
   - `VK.Blocks.Core` 内に `[VKStronglyTypedId]` 属性を定義し、強タイプ ID 化したい `partial record struct` に付与する。
2. **`VKStronglyTypedIdGenerator` の開発**:
   - Roslyn インクリメンタルソースジェネレーターを実装する。
   - 対象属性で修飾された部分構造体（`partial record struct`）を検出し、以下の定型コードをビルド時に自動出力（AddSource）する：
     - 空の値（`Empty`）および空判定（`IsEmpty`）プロパティ。
     - 比較インターフェース（`IComparable<T>`）、パースインターフェース（`IParsable<T>`）。
     - `System.Text.Json` 用の `JsonConverter<T>` 派生。
     - `System.ComponentModel` 用の `TypeConverter` 派生。
     - **条件付き生成**: コンパイル空間内に EntityFramework Core が存在する場合、自動的に `ValueConverter` と `ValueComparer` を生成する。

### 核心的なジェネレータの入力マーカーと出力構造

```csharp
namespace VK.Blocks.Core;

[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class VKStronglyTypedIdAttribute : Attribute
{
}

// 利用例 (手動宣言は partial を付与するだけで完了する)
[VKStronglyTypedId]
public partial record struct VKKnowledgeId;
```

*自動生成されるコードの抜粋 (例: `VKKnowledgeId.g.cs`)*:
```csharp
[JsonConverter(typeof(VKKnowledgeIdJsonConverter))]
[TypeConverter(typeof(VKKnowledgeIdTypeConverter))]
public partial record struct VKKnowledgeId(Guid Value) : IComparable<VKKnowledgeId>, IParsable<VKKnowledgeId>
{
    public static VKKnowledgeId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public int CompareTo(VKKnowledgeId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
    public static implicit operator VKKnowledgeId(Guid value) => new(value);
    public static VKKnowledgeId New(IVKGuidGenerator generator) => new(generator.Create());
    
    // Parse / TryParse 実装
}

// VKKnowledgeIdJsonConverter および TypeConverter 実装
// EF Core 参照時は VKKnowledgeIdEfCoreConverter および ValueComparer も出力
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Generic Base Record Struct
- **Approach**: `public record struct StronglyTypedId<T>(Guid Value)` のようなジェネリックな共通構造体を作成する。
- **Rejected Reason**: C# の型システム上、`StronglyTypedId<Persona>` と `StronglyTypedId<Session>` は異なる型として区別できるが、暗黙的型キャスト（implicit operator）やシリアライザのバインド設定が非常に複雑になり、EF Core のマッピング設定も各エンティティごとに明示的に登録し続けなければならないため。

### Option 2: Run-time Code Generation (Emit / Reflection)
- **Approach**: アプリケーション起動時にリフレクションでアセンブリをスキャンし、動的に IL を生成してコンバータを差し込む。
- **Rejected Reason**: アプリケーションの起動時間が著しく劣化するうえ、AOT（Ahead-Of-Time）コンパイルやクラウドネイティブな環境（トリミング対応など）との互換性が完全に失われるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **驚異的な記述量削減**: 開発者は `[VKStronglyTypedId]` を構造体に付けるだけで、API、JSON シリアライズ、データベース永続化に対応した強タイプ ID を瞬時に手に入れられる。
- **マルチレイヤー適合**: コントローラーでのルーティングパラメータ解決（`TypeConverter`）から、データベース保存（EF Core）までが完全に自動化される。

### Negative
- **IDE でのビルド待ち**: 属性を付与した直後、ソースジェネレーターがバックグラウンドでコードを生成して反映するまでに、ごく僅かな時間（数百ミリ秒）タイムラグがあり、インテリセンスが即座に認識しない場合がある。

### Mitigation
- パフォーマンスに最適化された Roslyn `IIncrementalGenerator` API を使用し、無駄な AST（抽象構文木）のパースを抑え、エディタ実行時の応答性能への影響を極限まで低減する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **EF Core Dependency Protection**: 生成モジュールはプロジェクトが EF Core を直接参照している場合のみ `ValueConverter` を生成する（`target.HasEfCore` 判定）。これにより、EF Core の依存関係を持たない軽量なアプリケーションや BuildingBlock（例: Core や純粋なドメイン層）でも同じソースジェネレーターを何のエラーも出さずに再利用できる。

## 7. Status
✅ Accepted
