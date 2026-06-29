# ADR 002: Relocation of Mcp Server to Tools Directory

- **Date**: 2026-06-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/Tools/McpServer

## 1. Context (背景)

VK.Blocks 体系では、再利用可能なビジネスロジックの最小単位を「BuildingBlocks（ビルディングブロック）」として `src/BuildingBlocks/` 配下に配置している。一方で、開発支援ツールや開発時にのみ動作するサーバー（Model Context Protocol / MCP サーバーなど）は、当初 `src/McpServer` のように `src/` ルート配下に直接配置されていた。このようにコアライブラリと同格の場所にツールプロジェクトが配置されていると、コードベース全体の物理的な境界が曖昧になり、製品コードと開発時専用ツールの区別が付きにくくなる。

## 2. Problem Statement (問題定義)

開発用補助プロジェクトが `src/` 直下に乱立することには、以下の問題がある：
1. **ディレクトリ構造の汚染**: 業務アプリケーションや BuildingBlock を開発・参照する開発者にとって、`src/` 配下が「何が製品ライブラリで、何がツールなのか」判別しにくくなり、認知負荷が高くなる。
2. **ビルドおよび依存関係の混乱**: ツールプロジェクトが参照する外部依存（例: McpServer が使用するホスティングパッケージや診断ライブラリなど）が、クリーンな再利用ブロックである `BuildingBlocks` と混ざり合い、依存関係管理（`Directory.Packages.props` など）が煩雑になる。
3. **名前空間の不整合**: クラスライブラリとして流通させるべき名前空間（`VK.Blocks...`）と、内部ツール用名前空間（`VK.Tools...`）の分類ルールが明確に区別されない。

## 3. Decision (決定事項)

プロジェクト物理構成の整理と名前空間の標準化を進めるため、**「Relocation to Tools Subdirectory (Tools サブディレクトリへの再配置)」**を決定する。

1. **`src/Tools/` 共通フォルダの定義**:
   - 開発者向け CLI ツール、ソースジェネレーター、およびローカル開発用サーバー等を一元管理する親ディレクトリ `src/Tools/` を定義する。
2. **McpServer の移設**:
   - 原型プロジェクト `src/McpServer` を、`src/Tools/McpServer` へ物理移動する。
   - プロジェクト名およびデフォルト名前空間を、ツール専用の `VK.Tools.McpServer` に変更・統一する。
3. **他の開発ツールの統合**:
   - ソースジェネレーター（`SourceGenerators`）等も同様に `src/Tools/SourceGenerators` 配下に配置し、製品としてのクラスライブラリと開発ツールの境界線を厳格にする。

### 変更後の物理ツリー構成例

```
e:\code\github\VK-Common-BE\
 ├── src\
 │    ├── BuildingBlocks\      <-- 製品用クラスライブラリ群 (Core, AI 等)
 │    └── Tools\
 │         ├── McpServer\       <-- [移設完了] 本開発支援MCPサーバー
 │         └── SourceGenerators <-- 開発時ソースジェネレータ
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Keep at src/ root with specific prefixes
- **Approach**: `src/VK.Blocks.McpServer` や `src/VK.Tool.McpServer` のように、プレフィックスで名前空間を表現しつつ `src/` 直下のフラットな配置を維持する。
- **Rejected Reason**: フォルダ数が膨大になった際、エクスプローラーでの視認性が著しく低下し、物理レイヤーでの製品コードとツールコードの物理分離（ビルドターゲット除外の指定など）が難しくなるため。

### Option 2: Move to e.g. a global `/tools` root directory
- **Approach**: リポジトリのルートディレクトリに `src/` と並列で `/tools` ディレクトリを掘り、そこに移設する。
- **Rejected Reason**: `Directory.Build.props` や共通の NuGet `Directory.Packages.props` による依存管理は `src/` 配下の共通コンテキストで動作しているため、`src/` の外に出すとビルド構成ファイルの適用ルールが複雑化するため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **ディレクトリセマンティクスの健全化**: `src/BuildingBlocks` は顧客にクラスライブラリとして提供されるピュアなモジュール群であり、`src/Tools` は開発補助用のクローズドなアプリ群である、という物理境界が 100% 成立。
- **依存管理のクリーン化**: ツール類が必要とする特有のパッケージ記述が `Tools/` 名前空間配下に閉じるため、製品ライブラリ側が不要なパッケージを引きずるリスクを抑止。

### Negative
- **既存のパス変更に伴う CI/CD への影響**: 各種ビルドスクリプトや、MCP サーバー起動時に設定されているエージェント構成パスの変更が必要になる。

### Mitigation
- 移設に伴う `Program.cs` や `McpTools.cs` の内部パス（`FindProjectRoot` での相対パス解決など）を適切に修正し、動作確認の上、CI パイプライン定義も同時に書き換える。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Namespace Restriction**: 移設後のツールクラスには、製品コードが誤って依存しないよう、パブリッククラスを最小限にし、名前空間を `VK.Tools` プレフィックスに縛ることで、依存の逆流（製品コード -> ツールコード）をアーキテクチャ監査ツール（ArchUnit等）で防止できるようにする。

## 7. Status
✅ Accepted
