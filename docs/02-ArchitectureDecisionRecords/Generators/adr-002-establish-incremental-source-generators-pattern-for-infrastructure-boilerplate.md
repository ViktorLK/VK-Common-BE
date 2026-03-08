# ADR 002: Establish Incremental Source Generators Pattern for Infrastructure Boilerplate

**Date**: 2026-03-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: VK.Blocks Architecture Foundations

## Context (背景)

VK.Blocks アーキテクチャ内では、依存性の注入（DI）登録や、強型付けされたカタログ（権限やテナント ID など）、メトリクスなどの可観測性（Observability）といったインフラストラクチャー上の関心事に対して、開発者が多くのボイラープレート（定型コード）を記述する必要がありました。
これまでは、開発者の規律に依存した手動管理（エラーが発生しやすく、DRY原則に違反する）か、あるいはアプリケーション起動時のリフレクション（Runtime Reflection）によるアセンブリ走査に頼っていました。しかし、リフレクションは起動時間（Startup Time）のペナルティが大きく、AOT（Ahead-Of-Time）コンパイルの互換性を損ない、エラーの発見を実行時まで遅らせてしまうという深刻な課題がありました。

## Problem Statement (問題定義)

- **手動管理のリスク**: 横断的関心事に関わる定型コードの手動記述は、登録漏れやタイポを引き起こし、システムの安全性や保守性を損なう原因となります。
- **リフレクションの限界**: 従来よく用いられてきた起動時のリフレクションベースの自動化では、マイクロサービスやサーバーレス環境における「コールドスタート問題」を悪化させ、モダンな .NET の AOT 恩恵を受けられなくなります。
- **エラーの遅延**: DIの登録漏れや静的定数のミスマッチがコンパイル時に検出されず、実行時の例外として発覚するため、テスト容易性や開発体験（DX）が低下します。

## Decision (決定事項)

インフラストラクチャーのボイラープレートをコンパイル時に解決するため、専用の `VK.Blocks.Generators` プロジェクト（`.netstandard 2.0`）を構築し、Roslyn `IIncrementalGenerator` 実装によるコード生成パターンをアーキテクチャの標準として確立することを決定しました。

**Generator 作成における厳格なルール (Critical Rule)**:
すべての Source Generator は**完全な増分処理（Strictly Incremental）**でなければなりません。

- 構文解析（Syntax Node）からの抽出直後に、軽量な純粋 DTO（Record 等）へ変換すること。
- パイプライン内で `WhereNotNull` 等を活用して不要なデータ伝搬をフィルタリングすること。
- プロジェクト全体の再コンパイルを誘発する `CompilationProvider.Combine` のようなアンチパターンの使用を厳格に禁止すること。

## Alternatives Considered (代替案の検討)

### Option 1: 従来のリフレクション (Scrutor 等の活用)

- **Approach**: 起動時に DI コンテナ構築の一環としてアセンブリをスキャンし、特定のインターフェースを実装するクラスを自動登録する。
- **Rejected Reason**: 起動パフォーマンスの悪化と Native AOT への非互換性。また、DI登録以外のメタデータ抽出（例：`[AuthorizePermission]` からの定数クラス生成）には応用できず柔軟性に欠けるため。

### Option 2: Roslyn Analyzer (診断と警告のみ)

- **Approach**: コード生成は行わず、手動で実装すべき箇所が欠落している場合にコンパイル警告を出す Analyzer を作成する。
- **Rejected Reason**: エラーは早期に発見できるが、開発者が大量のボイラープレートを手作業で書く負担は一切軽減されないため。

## Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - **Zero Runtime Overhead**: リフレクションが不要になり、起動時間が劇的に短縮され、全体の実行時パフォーマンスが向上します。
    - **Compile-Time Safety**: すべての自動化処理がコンパイル時に完了するため、エラーの早期発見と型安全性が保証されます。
    - **Native AOT Ready**: リフレクション依存を排除することで、将来的な Native AOT 化への道が開かれます。
- **Negative**:
    - Source Generator は開発・デバッグ手法が特殊であり、アーキテクチャチームに高度な Roslyn API の知識が要求されます。
- **Mitigation**:
    - `VK.Blocks.Generators` モジュール内での Generator 実装に対するテストや、軽量な Incremental Pattern（`WhereNotNull` の活用など）に関するガイドラインとサンプル（すでに Authorization 等で実証済み）を提供し、アーキテクチャの標準化を図ります。

## Implementation & Security (実装詳細とセキュリティ考察)

- `VK.Blocks.Generators` は他の BuildingBlocks から参照されるコアツールとして機能します。
- **セキュリティと安定性**: Generator 自身のクラッシュがコンシューマー（利用側プロジェクト）のコンパイルをブロックしないよう、内部での例外ハンドリングや `CancellationToken` による協調的キャンセルを徹底します。
- 開発者の IDE（Visual Studio / Rider）の入力応答性を一切低下させないため、前述した Incremental Pipeline の最適化を継続的にレビューして維持します。
