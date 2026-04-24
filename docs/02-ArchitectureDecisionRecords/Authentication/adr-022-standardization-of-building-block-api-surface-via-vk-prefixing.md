# ADR 022: Standardization of Building Block API Surface via VK-Prefixing

- **Date**: 2026-04-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks Naming Convention & Brand Consistency

## 2. Context (背景)

VK.Blocks は大規模なマイクロサービス群で共有されるビルディングブロックライブラリです。複数のモジュール（Authentication, Logging, AI 等）が一つのアプリケーションに導入されることが一般的ですが、それぞれのライブラリが提供するパブリックな型（Attribute, Options, Interface）が標準の .NET ライブラリやサードパーティ製ライブラリと名前が似通っている場合がありました。

## 3. Problem Statement (問題定義)

1. **名前の衝突**: `JwtAuthorize` や `ApiKeyOptions` といった名称は一般的すぎて、他のライブラリやユーザー独自のコードと衝突しやすい。
2. **識別性の欠如**: インテリセンスのリストにおいて、どれが VK.Blocks 提供の機能で、どれが ASP.NET Core 標準の機能か判別しにくい。
3. **ブランドの一貫性**: ライブラリ全体で命名規則が統一されていないと、プロフェッショナルな SDK としての信頼性が損なわれる。

## 4. Decision (決定事項)

VK.Blocks モジュールのパブリック API サーフェス（Level 1）において、全ての公開型に `VK` プレフィックスを付与することを標準化しました。

### 具体的な命名規則
1. **Level 1 (Public)**: ルート名前空間に配置される全てのクラス、レコード、インターフェースに `VK` を付与。
    - 例: `VKAuthenticationBlock`, `VKJwtAuthorizeAttribute`, `IVKApiKeyStore`
2. **Level 2+ (Internal)**: `Internal/` フォルダ配下の実装クラスには `VK` を**付与しない**。
    - 例: `ApiKeyAuthenticationHandler`, `JwtLog`
3. **Options**: 全てのオプションクラスに `VK` を付与し、`IVKBlockOptions` を実装。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 名前空間による分離のみ**: プレフィックスを付けず、`VK.Blocks.Authentication` という名前空間だけで区別する。
    - **Rejected Reason**: C# では `using` ディレクティブによって名前空間がフラット化されるため、異なる名前空間の同名のクラス（例：`JwtOptions`）が存在すると、常に完全修飾名を書く必要があり開発効率が落ちる。
- **Option 2: 異なるプレフィックス**: `VKB` (VK.Blocks) 等。
    - **Rejected Reason**: `VK` が既にプロジェクト全体で認知されており、簡潔で識別性が高いため。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 名前衝突のリスクを物理的にほぼゼロにできる。
    - インテリセンスで `VK` と入力するだけで、利用可能な全コンポーネントがフィルタリングされ、発見性が向上する。
    - ライブラリとしてのプロフェッショナルな一貫性が保たれる。
- **Negative**:
    - 全てのパブリック型が 2 文字分長くなる。
- **Mitigation**:
    - 内部コード（Internal）ではプレフィックスを排除することで、実装コードの冗長性を抑える。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **自動化**: ソースジェネレーター（VK.Blocks.Generators）もこの命名規則を認識し、自動生成されるコードも `VK` プレフィックスを尊重するように設計。

**Last Updated**: 2026-04-24
