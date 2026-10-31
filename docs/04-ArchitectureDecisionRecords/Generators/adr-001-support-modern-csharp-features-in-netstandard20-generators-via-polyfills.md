# ADR 001: Support Modern C# Features in .NET Standard 2.0 Generators via Polyfills

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: VK.Blocks Architecture Foundations

## Context (背景)

Roslyn Source Generators は、Visual Studio や `dotnet build` のプロセス内でシームレスに動作するため、ホスト環境との互換性を保つためにコンパイル時のターゲットフレームワークとして `netstandard2.0` を強制されます。
一方で、Source Generator のパイプラインにおいて、パフォーマンスやスレッドセーフ性を高めるためには、不変（Immutable）な DTO を簡単に定義できる C# 9.0 以降の `record` 型や `init` アクセサーといったモダンな言語機能が不可欠です。しかし、これらは `netstandard2.0` には組み込まれていません。

## Problem Statement (問題定義)

- **言語機能の制限**: `netstandard2.0` をターゲットとすると、そのままでは `record` や `init` プロパティが使用できず、従来の冗長な `class` と明示的な不変フィールド（readonly）を手書きする必要があります。
- **外部依存の禁止**: `IsExternalInit` パッケージなどの NuGet に依存してこれらの機能を解放する方法もありますが、Source Generator は利用先プロジェクトに依存関係を引きずらないように、外部パッケージを持たない「Self-contained（自己完結型）」であることが求められます。

## Decision (決定事項)

`VK.Blocks.Generators` モジュール内において、外部パッケージに依存することなくモダンな C# 機能を有効化するため、コンパイル時ポリフィル（Compile-time Polyfill）のアプローチを採用することを決定しました。

具体的には `System.Runtime.CompilerServices.IsExternalInit` クラスのダミー実装を提供する `Polyfills.cs` をプロジェクトに直接配置します。

```csharp
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
```

## Alternatives Considered (代替案の検討)

### Option 1: 外部パッケージ (PolySharp / IsExternalInit NuGet) の導入

- **Approach**: 依存関係として既存の Polyfill 提供パッケージをインストールし、PrivateAssets 指定で利用する。
- **Rejected Reason**: 他のプロジェクトで利用される Source Generator であるため、依存の連鎖や競合を避けるために可能な限り純粋な C# コードのみで構成するべきだと判断したため。

### Option 2: 従来の C# 言語機能での実装 (C# 7.x 互換)

- **Approach**: `record` を完全に諦め、Generator 内の DTO をすべて `struct` ＋ `readonly` で自力実装する。
- **Rejected Reason**: IEquatable の実装や With 式の代替を手動で記述するのは非常に非生産的であり、メタプログラミングにおける保守性が著しく低下するため。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - `VK.Blocks.Generators` プロジェクトは完全に外部ライブラリ依存（Dependency Free）のまま保たれます。
    - 最新の C# の生産性（Records, Init-only setters）をフル活用して Source Generator の内部パイプラインを構築できるようになります。
- **Negative**:
    - 新しい言語機能のサポートが必要になった場合、都度手動で Polyfill を追加するメンテナンスコストが発生する可能性があります。
- **Mitigation**:
    - 将来、より高度な機能（例: Required properties）が必要になった場合は、必要最小限の Polyfill を `Polyfills.cs` に追記して対応します。
