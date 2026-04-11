# ADR 010: Standardization of Authorization Vertical Slice Structure and Delegated Registration

## 1. Meta Data

- **ADR 编号与标题**: ADR 010: Standardization of Authorization Vertical Slice Structure and Delegated Registration
- **Date**: 2026-04-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Authorization Module Maintenance & Vertical Slice Architecture Enforcement

## 2. Context (背景)

Authorization ビルディングブロックの開発が進むにつれ、多くの機能（Roles, Permissions, MinimumRank 等）が追加された。しかし、以下の課題が浮き彫りになっていた：

1.  **中央集約的な DI 登録**: `AuthorizationBlockExtensions.cs` が全機能の内部クラス（`DefaultRoleProvider` 等）を直接参照しており、機能を追加するたびに名前空間と依存関係が肥大化していた。
2.  **フォルダ構造の不一致**: 機能によってサブフォルダ（Metadata, Internal）の有無や属性の配置場所が異なり、開発者がどこにファイルを置くべきか迷う状況であった。
3.  **Source Generator の保守性**: SG のトリガー属性が `Abstractions/` に散らばっており、機能単位でのカプセル化（Vertical Slice）が不完全であった。

## 3. Problem Statement (問題定義)

- **カプセル化の欠如**: 中央の DI 登録コードが `Internal` 名前空間に深く依存しており、内部実装の変更が広範囲に影響する。
- **予測可能性の低下**: SG トリガーが特定のフォルダにまとまっていないため、新しい機能を追加する際のパターンが不明確。
- **結合度の増大**: `Abstractions` フォルダが機能固有の属性で汚染され、真に共通のインターフェースが埋もれてしまっている。

## 4. Decision (決定事項)

垂直スライスの独立性を高め、保守性を向上させるために以下の構造を標準として採用した。

### 1. 垂直スライスの階層標準化
各機能フォルダ (`Features/{FeatureName}/`) を以下の 4 層で構成する：

- **[Root]**: 公開 API（Handler, Requirement, Evaluator インインターフェース、Registration 拡張メソッド）。
- **Metadata/**: Source Generator のトリガーとなる属性、および走査対象となる Enum やモデル。
- **Internal/**: 外部に公開しない内部実装クラス、定数（Constants）、ログ（Log）。
- **Persistence/**: データストアの抽象化インターフェース（必要な場合のみ）。

### 2. DI 登録の委譲 (Delegated Registration)
中央の `AddVKAuthorizationBlock` から各機能の詳細な登録を排除し、各機能フォルダ内の拡張メソッドに委譲する。

```csharp
// 中央ファイル
public static IVKBlockBuilder<AuthorizationBlock> AddVKAuthorizationBlock(...)
{
    // 各機能の公開メソッドを呼ぶだけ
    services.AddRolesFeature();
    services.AddPermissionsFeature();
    // ...
}
```

### 3. SG トリガーの配置
SG トリガー属性は常に `Metadata/` フォルダに配置し、名前空間に `.Metadata` を付加する。

## 5. Alternatives Considered (代替案の検討)

### Option 1: 現状維持 (Centralized Registration)
- **Approach**: 中央ファイルですべての `TryAdd` を行う。
- **Rejected Reason**: 機能が増えるたびにファイルが巨大化し、全機能の内部クラスを公開（あるいは internal 参照）し続ける必要があるため。

### Option 2: Abstractions への属性集約
- **Approach**: 全てのトリガー属性を `Abstractions/` に配置する。
- **Rejected Reason**: 機能ごとの「垂直スライス」という設計思想に反し、Abstractions が特定の機能の詳細に汚染されるため。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 名前空間の依存が整理され、コードの読みやすさが劇的に向上した。
    - 各機能が自己完結型（Self-contained）になり、機能の追加・削除が容易になった。
    - 「どこに何を置くか」のルールが明確になり、コードレビューの負荷が下がった。
- **Negative**:
    - ネーム空間が 1 階層深くなる（`.Metadata`）。
    - 既存の SG のスキャンロジックを更新する必要があった。
- **Mitigation**:
    - Source Generator 側で `AttributeFullName` 定数を一括管理し、パスの変更に対応。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **アクセス修飾子**: 機能ごとの Registration クラスは `internal` とし、同一アセンブリ内の中央 DI コードからのみ見えるようにした。
- **Namespace 分離**: 内部実装（Internal）やメタデータを分離することで、IntelliSense の汚染を防ぎ、開発者の誤用（内部クラスの直接使用）を抑制する。

---
**Last Updated**: 2026-04-10
