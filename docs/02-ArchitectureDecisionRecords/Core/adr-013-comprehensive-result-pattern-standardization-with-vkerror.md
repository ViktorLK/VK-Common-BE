# ADR 013: Comprehensive Result Pattern Standardization with VKError

## 1. Meta Data

- **Date**: 2026-04-22
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core Result Pattern and Error Modeling

## 2. Context (背景)

VK.Blocks における中心的な設計思想である「Result パターン」は、例外に頼らずに成功と失敗を明示的に表現し、戻り値としての `null` を排除するために導入されています（Rule 1 参照）。

これまでは `Result` クラスと `Error` クラスが使用されてきましたが、ADR-009 による「VK プレフィックスの義務化」に伴い、これらの基盤型の名称を刷新する必要が生じました。また、単なるリネームに留まらず、大規模なリクエスト処理に耐えうる「工業級（Industrial-grade）」の性能と一貫性を確保するための機能強化が求められていました。

## 3. Problem Statement (問題定義)

- **命名の不一致**: 公開 API である `Result` や `Error` に `VK` プレフィックスがなく、他のライブラリやアプリケーション固有の型と名前衝突を起こしやすい。
- **アロケーションオーバーヘッド**: 成功時に毎回 `new Result(true, ...)` を行うと、高頻度なリクエストにおいて GC 負荷が増大する。
- **エラー定数の散散**: エラー情報（Code と Message）の定義場所が分散しており、再利用性や多言語対応の基盤が整っていない。
- **流れるような操作の欠如**: `Task<Result>` から別の `Result` への変換（Map/Bind）などの操作を簡潔に記述するための拡張メソッドが不足している。

## 4. Decision (決定事項)

Result パターンの完全な標準化のため、以下の設計変更を強制します。

### 4.1. 型の刷新
- `Result` → `VKResult`, `Result<T>` → `VKResult<T>` へリネーム。
- `Error` → `VKError` へリネーム。

### 4.2. パフォーマンス最適化 (ADR-010 との連携)
- **成功インスタンスのキャッシュ**: `VKResult.Success()` メソッドは、常に内部で `static readonly` に保持された単一の成功インスタンスを返却し、ヒープアロケーションをゼロにします。

### 4.3. エラー定数管理の標準化
- マジックストリングを排除するため、エラーは必ず `VKError` オブジェクトとして定義します。
- 各ドメインごとに `Errors` 静的クラス（例: `VKCoreErrors`）を作成し、その中で `static readonly` な定数として管理します。

### 4.4. VKResultExtensions の導入
- 同期および非同期（`Task`/`ValueTask`）の `VKResult` を透過的に扱える拡張メソッドを提供します。
- `BindAsync`, `Map`, `Tap`, `Ensure` 等の関数型プログラミングのエッセンスを取り入れ、ガード節や後続処理の記述を簡略化します。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: `Result` クラスを維持し、`using static` で回避する**
    - Approach: 名前衝突は利用側のエイリアスで解決する。
    - Rejected Reason: フレームワーク全体の統一感を損ない、利用者に余計な負担を強いる。
- **Option 2: 既存の FluentResults 等の外部ライブラリを採用する**
    - Approach: 自前実装をやめ、実績のあるライブラリに依存する。
    - Rejected Reason: VK.Blocks は「ゼロ・デパデンシー（外部依存最小化）」を原則としており、コアとなる Result 型を外部に委ねることは、将来的な拡張性やポータビリティを損なう。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - 全モジュールで一貫したエラーレスポンス形式が保証される。
    - 成功時のアロケーション排除により、システム全体の総スループットが向上。
    - コードがより宣言的になり、エラーチェックの漏れが減少。
- **Negative**:
    - すべてのプロジェクトで大規模な置換作業（`Result` → `VKResult`）が必要。
- **Mitigation**:
    - IDE のグローバル置換機能と、非推奨（Obsolete）警告を一時的に併用することで、段階的な移行をサポートする。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Zero-Null Policy**: `VKResult<T>` は、成功時には必ず `Value` が非 null であることを保証し、失敗時には `Value` へのアクセスで例外を投げるか、デフォルト値を安全に扱う設計とします。
- **Security**: `VKError` に含めるメッセージは、実装の詳細を露呈させないよう、ADR-011 で定義された `IsPublic` フラグと連携して制御します。

**Last Updated**: 2026-04-22
