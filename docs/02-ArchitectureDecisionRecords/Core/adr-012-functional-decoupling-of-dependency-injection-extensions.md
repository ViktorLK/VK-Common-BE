# ADR 012: Functional Decoupling of Dependency Injection Extensions

## 1. Meta Data

- **Date**: 2026-04-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core Dependency Injection Refactoring

## 2. Context (背景)

`BuildingBlocks.Core` における依存性注入（DI）用の拡張メソッドは、VK.Blocks 全体の起動基盤として機能します。しかし、開発が進むにつれ、以下の機能が 1 つのクラス（旧 `VKBlockServiceExtensions` 等）に混在し、メンテナンスが困難になっていました。

1. **基本登録**: サービスの実装登録や `IVKBlockMarker` の注入。
2. **依存関係照会**: 他のモジュールが登録済みかを確認する `IsVKBlockRegistered` 等のロジック。
3. **Builder パターン**: `AddVKBlock` から始まり、流れるようなインターフェースで追加機能を設定するロジック。

特に、ADR-008 で導入された「再帰的な依存関係検証」のロジックが追加されたことで、ファイルサイズが肥大化し、責務の境界が曖昧になっていました。

## 3. Problem Statement (問題定義)

- **責務の混在**: 「サービスを登録するメソッド」と「登録状況を調べるメソッド」が同じファイルにあるため、インテリセンスの候補が整理されず、開発者が適切なメソッドを見つけにくい。
- **拡張性の制限**: すべてが単一の静的クラスにあると、将来的に Builder パターンを拡張したり、条件付き登録ロジックを複雑化したりする際に、コードの結合度が高くなりすぎる。
- **循環参照の複雑さ**: DI コンテナ自体の登録状況を参照しながら登録を行うため、ロジックが複雑になりやすく、テストやデバッグが困難。

## 4. Decision (決定事項)

DI 関連の拡張メソッドを、その役割に応じて明確に 3 つのファイル/クラスに分離し、名前空間を整理します。

### 4.1. VKBlockRegistrationExtensions
- **役割**: サービスの実装を `IServiceCollection` に追加する「書き込み」の責務。
- **主な内容**: `AddVKBlockMarker`, `AddVKBlockOptions`, および各モジュールの `AddVKXxxBlock` 本体。
- **特徴**: Idempotency（冪等性）の保証と、実際のインスタンス登録を担当。

### 4.2. VKBlockQueryExtensions
- **役割**: `IServiceCollection` の現在の状態を確認する「読み取り」の責務。
- **主な内容**: `IsVKBlockRegistered<T>`, `GetVKBlockMarker`, 依存関係の再帰的チェック（Pre-order Traversal）。
- **特徴**: 状態変更を行わず、検証と照会に特化。

### 4.3. VKBlockBuilderExtensions
- **役割**: 流れるような構成インターフェースの提供。
- **主な内容**: `IVKBlockBuilder` に対する拡張メソッド（例: `EnableXxx`, `ConfigureYyy`）。
- **特徴**: 開発者エクスペリエンス（DX）の向上と、モジュールのオプション機能の有効化を担当。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 機能ごとに別々の名前空間に分ける**
    - Approach: `VK.Blocks.Core.DependencyInjection.Registration` 等。
    - Rejected Reason: 利用者が複数の `using` を書く必要があり、利便性が低下する。
- **Option 2: 巨大な Partial Class にする**
    - Approach: `public static partial class VKBlockExtensions` としてファイルを分ける。
    - Rejected Reason: 物理ファイルは分かれるが、型としては同一であるため、インテリセンス上のノイズ（登録メソッドと照会メソッドの混在）が解消されない。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - **DX の向上**: `IServiceCollection` に対する拡張メソッドと、`IVKBlockBuilder` に対する拡張メソッドが明確に分離され、インテリセンスが使いやすくなる。
    - **保守性の向上**: 依存関係検証のアルゴリズムを `QueryExtensions` に集約できるため、ロジックの修正が他に影響を与えにくい。
    - **テストの容易性**: 照会ロジックのみを対象としたユニットテストが書きやすくなる。
- **Negative**:
    - 内部的な共通ヘルパー（リフレクション等）を各クラスからアクセスできるようにするため、一部のロジックを内部的なユーティリティクラスへ抽出する必要がある。
- **Mitigation**:
    - `Internal/` フォルダ内に `DIUtils` 等の共有クラスを用意し、DRY 原則を維持する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Visibility**: これらの拡張メソッドは `public` で提供されるが、複雑な検証ロジックの詳細は `internal` または `private` メソッドとして隠蔽し、API サーフェスをクリーンに保つ。
- **Performance**: `QueryExtensions` における照会は高速である必要があるため、ADR-010 で定義されたキャッシュパターンを積極的に活用する。

**Last Updated**: 2026-04-22
