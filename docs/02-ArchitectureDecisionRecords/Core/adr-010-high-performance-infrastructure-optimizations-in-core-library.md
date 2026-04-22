# ADR 010: High-Performance Infrastructure Optimizations in Core Library

- **Date**: 2026-04-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: BuildingBlocks.Core Performance and Standard Compliance

## 1. Context (背景)

`BuildingBlocks.Core` ライブラリは、システムのあらゆるリクエストにおいて基盤となる型（`VKResult`, `VKGuard`, `VKExpressionCache` 等）を提供します。これらのコンポーネントは、システム全体の実行パスにおいて極めて高い頻度で呼び出されるため、微小なオーバーヘッドの累積が全体の総スループットに大きな影響を与えます。

また、静的ジェネリックキャッシュを採用した設計において、コード分析警告 `CA1000` (Do not declare static members on generic types) が発生しており、性能と保守性（警告ゼロのビルド）の両立が課題となっていました。

## 2. Problem Statement (問題定義)

1. **呼出オーバーヘッド**: `VKGuard` や `VKResult` のプロパティのように、極めて軽量なメソッドがインライン化されない場合、メソッド呼び出しのスタックフレーム構築コストが実処理コストを上回る。
2. **メモリ割り当て (GC 負荷)**: `VKResult.Success()` が呼び出されるたびに新しいインスタンスが生成され、短寿命オブジェクトの GC 負荷を増大させている。
3. **JIT 最適化の阻害**: コンストラクタ内に複雑な例外スローロジックが含まれると、メソッドサイズが大きくなり、JIT によるインライン化の対象から外れる。
4. **CA1000 警告**: 静的ジェネリックキャッシュを実現するためにジェネリッククラスに静的メンバを定義すると、`CA1000` 警告が発生する。これを単に `SuppressMessage` で抑制することは、根本的な設計上のクリーンさを欠く。

## 3. Decision (決定事項)

Core ライブラリのホットパスに対し、以下の包括的なパフォーマンス最適化を適用しました。

### 3.1. AggressiveInlining の適用
高頻度で呼び出される軽量メソッドおよびプロパティゲッターに `[MethodImpl(MethodImplOptions.AggressiveInlining)]` を付与しました。

```csharp
public virtual bool IsSuccess { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
```

対象：`VKGuard`, `VKResult`, `VKResultExtensions` (同期版), `VKEntityMetadata`, `VKError` ファクトリ。

### 3.2. 成功インスタンスのキャッシュ
`VKResult` の成功結果について、`static readonly` な共有インスタンスを返却するように変更しました。

```csharp
private static readonly VKResult _success = new(true, []);
public static VKResult Success() => _success;
```

### 3.3. Throw ヘルパーと [DoesNotReturn] の導入
コンストラクタ等のメインパスから例外スローロジックを分離し、`[DoesNotReturn]` 属性を付与した静的ヘルパーに委譲しました。これにより、呼び出し元メソッドの IL サイズを最小化し、インライン化を促進しました。

### 3.4. Static Generic Caching (InnerCache) パターンの採用
CA1000 警告を回避しつつ、型固有の高速な静的キャッシュを維持するため、「非ジェネリックな外部クラス + プライベートなジェネリック内部クラス」の構成を採用しました。

```csharp
public sealed class VKTypeMetadataCache 
{
    public static bool IsAuditable<T>() => InnerCache<T>.IsAuditable;

    private static class InnerCache<T> 
    {
        public static readonly bool IsAuditable = typeof(IVKAuditable).IsAssignableFrom(typeof(T));
    }
}
```

### 3.5. VKGuard の Throw Helper 化
例外スローロジックを `[DoesNotReturn]` 属性付きのヘルパーメソッドに分離しました。これにより、ガード節自体のメソッドサイズを極限まで小さくし、呼び出し元へのインライン化率を向上させました。

### 3.6. コンストラクタおよび DI 登録における LINQ 排除
`VKResult` のコンストラクタや `IsVKBlockRegistered` 等の DI 関連メソッドにおいて、LINQ (`ToArray`, `Any`, `First`) を `foreach` ループや直接的な配列アクセスに置き換えました。これにより、ランタイムにおけるデリゲートの生成やイテレータの割り当てを排除しました。

### 3.7. 算術演算の最適化
`VKPagedResult` の `TotalPages` 計算において、`Math.Ceiling` (浮動小数点演算) を整数演算 `(count + size - 1) / size` に置き換え、CPU サイクルを微調整しました。

## 4. Alternatives Considered (代替案の検討)

### Option 1: `SuppressMessage` による警告抑制のみ
- **Approach**: 既存の `VKTypeMetadataCache<T>` のまま警告を抑制する。
- **Rejected Reason**: 警告は消えるが、呼び出し側で `VKTypeMetadataCache<T>.IsAuditable` のように常に型引数を明示する必要があり、利便性が低い。また、設計上の根本解決にならない。

### Option 2: `ConcurrentDictionary` への完全移行
- **Approach**: すべてのキャッシュを `ConcurrentDictionary<Type, T>` で管理する。
- **Rejected Reason**: ハッシュルックアップのコストが発生し、本質的な「静的フィールドアクセス」によるゼロコストの恩恵が得られない。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **スループット向上**: メソッド呼び出しオーバーヘッドの削減とインライン化の促進により、マイクロベンチマークレベルでの性能が向上。
- **GC 負荷の低減**: 成功結果のキャッシュにより、ヒープ割り当てが劇的に減少。
- **ビルド品質の向上**: CA1000 警告を属性による抑制なしで根本解決し、クリーンなビルドを実現。

### Negative
- **コードの複雑性**: `InnerCache` パターンの導入により、クラス構造が一段階複雑になる。

### Mitigation
- **命名規則の統一**: すべてのキャッシュクラスで `InnerCache<T>` という一貫した命名を採用し、パターンを明文化することでメンテナンス性を維持。

## 6. Implementation Detail (実装詳細)

- **.NET 10 対応**: C# 12+ の機能を活用し、`static` メソッド内での型推論を最大限利用できるように API を設計。
- **スレッドセーフティ**: `InnerCache` の静的フィールド初期化はランタイムによってスレッドセーフに行われるため、ロックフリーで高性能なアクセスを実現。

**Last Updated**: 2026-04-22
