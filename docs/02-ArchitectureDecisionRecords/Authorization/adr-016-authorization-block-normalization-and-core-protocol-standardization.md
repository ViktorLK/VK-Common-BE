# ADR 016: Authorization Block Normalization and Core Protocol Standardization

- **Date**: 2026-05-05
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: Authorization Block & Core Protocols

## 2. Context (背景)

Authorization ブロックのノーマライゼーションを進める過程で、以下の課題が浮き彫りになった。

1.  **エバリュエータのシグネチャ**: 汎用的な `IVKEvaluator<T>.EvaluateAsync` だけでは、開発者が各機能（例：WorkingHours, Roles）をプログラムから呼び出す際に直感的ではなく（DX の低下）、ドメイン特有の意味論（IsWithinWorkingHoursAsync 等）が失われていた。
2.  **プロトコルの配置**: `IVKArgs` や `IVKEvaluator` は認可だけでなく AI ブロックなど横断的に使われる「工業規格」であるべきだが、配置場所が `Core/Abstractions` と曖昧であり、規格としての重みが不足していた。
3.  **Rule 21 の実装負荷**: 「グローバル設定 + ローカル上書き」のロジックが `args?.Prop ?? options.Prop` という記述の繰り返しになり、可読性と記述効率の課題があった。

## 3. Problem Statement (問題定義)

- **セマンティクスの欠如**: 汎用インターフェースに依存しすぎると、ドメイン固有の認可ロジックが「ただの Evaluate」に埋もれ、コードの意図が伝わりにくくなる。
- **配置の曖昧さ**: `Abstractions` という汎用すぎるフォルダ名は、フレームワークの核心となるプロトコルの重要性を低下させていた。
- **記述スタイルの不一致**: null 合体演算子が至る所に現れることは、Rule 12 が目指す「洗練された簡潔な C# スタイル」に逆行していた。

## 4. Decision (決定事項)

以下の 3 点を VK.Blocks の新しい標準として採用する。

### 4.1 Semantic-Priority パターン
エバリュエータのインターフェース設計において、ドメイン固有のメソッドをパブリックとし、汎用インターフェースは **明示的インターフェース実装 (Explicit Interface Implementation)** を採用する。

```csharp
public interface IVKWorkingHoursEvaluator : IVKEvaluator<VKWorkingHoursArgs>
{
    // ドメイン固有のパブリックAPI
    ValueTask<VKResult<bool>> IsWithinWorkingHoursAsync(ClaimsPrincipal user, ...);

    // 汎用APIは隠蔽（IVKEvaluator 経由でのみアクセス可能）
    ValueTask<VKResult<bool>> IVKEvaluator<VKWorkingHoursArgs>.EvaluateAsync(...) 
        => IsWithinWorkingHoursAsync(...);
}
```

### 4.2 Core Protocols の確立
`src/BuildingBlocks/Core/Protocols` フォルダを創設し、フレームワークの核となる規格を集約する。
- `IVKArgs.cs`: 非ジェネリック・マーカー。
- `IVKArgsT.cs`: ジェネリック・インターフェース。
- 名前空間は `VK.Blocks.Core` を維持し、既存コードへの影響を最小限に抑える。

### 4.3 MergeWith 拡張メソッドの導入
Rule 21 の実装を簡潔かつ宣言的に行うための `MergeWith` パターンを採用する。

```csharp
// VKMergeExtensions.cs
public static T MergeWith<T>(this T? local, T global) where T : struct => local ?? global;

// 使用例
var roles = args.MergeWith(VKRoleArgs.Empty).Roles.MergeWith([]);
```

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 汎用 EvaluateAsync のみを公開する**
    - **Rejected Reason**: 各機能の特有の振る舞いが隠れてしまい、ユニットテストやプログラムからの直接利用時に「何を評価しているのか」が不明確になる。
- **Option 2: 従来の ?? 演算子を使い続ける**
    - **Rejected Reason**: 重複した記述が増え、Rule 12 の「簡潔さ」を追求できない。また、将来的にマージロジックを変更する際の柔軟性に欠ける。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
    - 強固な型定義とドメインセマンティクスの両立。
    - プロジェクト全体で一貫した「設定マージ」の記述スタイル。
    - `Protocols` フォルダによるフレームワーク構造の視覚的理解の向上。
- **Negative**: 
    - `MergeWith` はメソッド呼び出しであるため、短絡評価（Short-circuiting）が行われない。
- **Mitigation**: 
    - ADR にて「右辺には静的な Empty インスタンスや定数のみを渡す」ことをガイドライン化し、パフォーマンスへの影響を排除する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **実装**: `IVKArgs` インターフェースをマーカーとして提供し、Source Generator による将来的なメタデータ自動収集を容易にした。
- **安全性**: `VKRoleArgs.Empty` 等を自動プロパティ初期化子（静的シングルトン）で実装することで、不必要なオブジェクト生成とガベージコレクション（GC）の負荷を最小化している。
