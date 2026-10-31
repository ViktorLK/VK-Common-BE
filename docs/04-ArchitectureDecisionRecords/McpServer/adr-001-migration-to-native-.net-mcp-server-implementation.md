# ADR 001: Migration to Native .NET MCP Server Implementation

**Date**: 2026-05-05
**Status**: ✅ Accepted
**Deciders**: Architecture Team
**Technical Story**: McpServer (Developer Ecosystem)

## 1. Context (背景)

当初、MCP (Model Context Protocol) サーバーは TypeScript/Node.js のプロトタイプとして実装されていました。これは迅速な検証には適していましたが、プロジェクトが成長するにつれて、以下の課題が顕在化しました。

- **技術スタックの乖離**: プロジェクト本体は .NET 10 ですが、ツール層だけが Node.js というハイブリッド構成になり、開発者の学習コストや環境構築の複雑さが増大していました。
- **コード再利用の困難性**: `VK.Blocks.Core` や `Diagnostics` などの既存の BuildingBlock ロジックをツール側で直接利用できず、ロジックの二重管理や同期の遅れが発生していました。
- **CI/CD の複雑化**: Node.js と .NET の両方のランタイムを維持・管理する必要があり、デプロイやパイプラインの保守コストが高まっていました。

## 2. Problem Statement (問題定義)

TypeScript 版の MCP サーバー実装には、以下の設計・運用上のボトルネックが存在しました：

- **保守性**: `VK.Blocks` の設計原則（Resultパターン、Guard等）を TypeScript 側で手動で再現する必要があり、実装の不一致が生じやすかった。
- **パフォーマンスと連携**: リポジトリのソースコード解析や EF Core メタデータの取得など、.NET 固有のメタプログラミングが必要なシーンで、外部プロセス（Node.js）からの連携が非効率であった。
- **開発体験 (DX)**: Windows 環境において `.exe` バイナリとして単体実行できる方が、AI エージェント（Claude Desktop 等）との統合においてセットアップが容易である。

## 3. Decision (決定事項)

MCP サーバーの実装を、TypeScript から **ネイティブ .NET 10 アプリケーション** へ移行することを決定しました。

### 主要な実装方針：

1. **ModelContextProtocol ライブラリの採用**:
   .NET 向けの MCP SDK である `ModelContextProtocol` を採用し、プロトコル準拠を保証します。
2. **Partial Classes によるモジュール化**:
   `McpTools` クラスを `partial class`（例: `McpTools.Adr.cs`, `McpTools.Codebase.cs`）として定義し、機能（Tool）ごとの関心の分離（SoC）を徹底します。
3. **Static-First & Zero-Reflection**:
   CA1822 (Mark members as static) に準拠し、インスタンス状態を持たないツールメソッドは `static` として定義することで、オーバーヘッドを最小化します。
4. **Lifecycle 管理ツールの導入**:
   開発中の「ビルド → 実行 → ファイルロック」のサイクルをスムーズにするため、自律的にプロセスを終了させる `vk_mcp_shutdown` ツールを標準搭載します。
5. **Naming Strategy**:
   C# 内部では `PascalCase` で実装し、MCP プロトコルとして公開する際は SDK の自動変換を利用して `snake_case` に統一します。

## 4. Alternatives Considered (代替案の検討)

### Option 1: TypeScript 実装の継続
- **Approach**: 現行の Node.js サーバーを維持し、gRPC 等で .NET 側と通信させる。
- **Rejected Reason**: 通信オーバーヘッドと二重管理のコストが、再開発のコストを上回ると判断。

### Option 2: 共有 DLL を C++/CLI でラップする
- **Approach**: C# のロジックを DLL 化し、Node.js から `node-gyp` 等で呼び出す。
- **Rejected Reason**: ネイティブ連携の複雑さと保守性が極めて低く、現代的な .NET 開発には不向き。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive (長期收益)
- **Single Stack**: 全てのコードが .NET 10 / C# 12+ で統一され、リポジトリ全体の整合性が向上。
- **Direct Logic Reuse**: `VKGuard` や `Result<T>`、診断ログなどの BuildingBlock 資産をそのままツールに注入可能。
- **Easier Distribution**: 単一の `.exe` バイナリとして配布可能になり、環境構築が簡略化。

### Negative (リスク)
- **Build Cycle**: スクリプト言語（TS）と比較し、変更のたびにコンパイルが必要になる。
- **Process Locking**: 開発中にバイナリが使用中（ロック）になり、再ビルドを妨げる可能性がある。

### Mitigation (緩和策)
- **`vk_mcp_shutdown`**: プロセスを自発的に終了させるツールを提供し、ビルド前のアンロックを容易にする。
- **`dotnet watch`**: 開発時に自動リビルド設定を活用できるよう、`Program.cs` に設定例をドキュメント化。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Input Validation**: 全てのツール引数は `VKGuard` (Core Block) によって境界チェックが行われます。
- **Error Handling**: 例外は境界で捕捉され、VK.Blocks 標準の `Result<T>` パターンにマッピングして MCP エラーレスポンスとして返却します。
- **Stdout Purity**: MCP プロトコル（JSON-RPC over stdio）を破壊しないよう、`builder.Logging.ClearProviders()` を使用して標準出力をクリーンに保ちます。

---
**Last Updated**: 2026-05-05
