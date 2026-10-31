# ADR 005: Use Static Abstract Interface for Configuration Section Resolution

- **Date**: 2026-04-16
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks Core Architecture

## Context (背景)

VK.Blocks では、各 BuildingBlock の設定を `appsettings.json` から読み込む際、Options クラス（例：`VKCorsOptions`）に定義された `SectionName` 定数を使用して構成セクションを特定しています。これまでの実装では、汎用的な `AddVKBlockOptions<T>` メソッド内で **Reflection（反射）** を使用してこの定数を取得していましたが、これはランタイムのオーバーヘッドを伴い、コンパイル時の型安全性も欠如していました。

## Problem Statement (問題定義)

1. **実行時のパフォーマンス**: Reflection によるフィールド取得は、スタートアップ時にのみ発生するものの、高頻度で呼び出される場合や大規模プロジェクトにおいて微細な遅延の原因となります。
2. **型安全性の欠如**: `TOptions` 型に `SectionName` が存在することをコンパイル時に保証できず、実行時に例外が発生するリスクがありました。
3. **モダンな標準への非準拠**: .NET 10 をターゲットとする本プロジェクトにおいて、C# 11 で導入された「インターフェースの静的抽象メンバー（Static Abstract Members in Interfaces）」を活用しない手はありません。

## Decision (決定事項)

Reflection による動的な定数取得を廃止し、**Static Abstract Interface** パターンを採用します。

### 1. インターフェースの定義
`VK.Blocks.Core.Abstractions` に `IVKBlockOptions` インターフェースを定義します。

```csharp
public interface IVKBlockOptions
{
    static abstract string SectionName { get; }
}
```

### 2. 精緻化されたオーバーロード戦略 (Overload Strategy)
コンパイル時の曖昧さを回避し、型安全性を高めるために以下の 2 層構造を採用します。

- **[WRAPPER] 自动解決版**: `IConfiguration` を受け取り、`SectionName` を自動解決してコア実装へ委譲します。
- **[CORE] 手動指定版**: `IConfigurationSection` を受け取り、実際のバインドと登録（Dual-Registration）を行います。

### 3. セマンティックな冪等性チェック
登録済みの判定には `services.Any()` を直接記述せず、統一された拡張メソッド `IsVKBlockRegistered<TOptions>()` を使用します。これにより、フレームワーク全体の DSL の一貫性を保ちます。

## Alternatives Considered (代替案の検討)

### Option 1: Source Generator
- **Approach**: SG で各 Options クラスの DI 登録コードを自動生成する。
- **Rejected Reason**: 実装が複雑になり、ビルド時間が増加する。本件のような単純なメタデータ解決にはインターフェースの方が軽量。

### Option 2: 属性（Attribute）による指定
- **Approach**: `[VKBlockOptions("Section:Name")]` のような属性を付与する。
- **Rejected Reason**: 依然として Reflection が必要であり、型安全性の向上には寄与しない。

## Consequences & Mitigation (結果と緩和策)

### Positive
- **ゼロ・リフレクション**: 実行時のオーバーヘッドが皆無になります。
- **コンパイル時の強制**: `IVKBlockOptions` を実装していないクラスは、自動解決版の `AddVKBlockOptions` を使用できず、設計ミスを未然に防げます。
- **Dual-Registration の保証**: 常に `IOptions<T>` と `Singleton` の両方で利用可能であることを保証します。

### Negative / Caution
- **カスタムバリデータの注意点**: `AddVKBlockOptions` 内部で `ValidateDataAnnotations` が実行されるため、後続のカスタムバリデータ登録には必ず `TryAddEnumerable` を使用する必要があります。通常の `TryAddSingleton` では無視されるリスクがあります。

## Implementation & Security (実装詳細とセキュリティ考察)

この変更により、設定情報の読み取りパスが静的に確定されるため、不正な Reflection による予期せぬフィールドへのアクセスリスクが排除され、セキュリティ面でも堅牢性が向上します。また、`IsVKBlockRegistered` による冪等性担保により、マルチモジュール構成での二重登録やバリデーションの重複実行を安全に回避します。
