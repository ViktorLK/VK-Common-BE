# ADR 003: Adoption of Pure Infrastructure Model and Decoupling from Core Context

- **Date**: 2026-03-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Blob Module Optimization

## 2. Context (背景)

`VK.Blocks.Blob` モジュールは当初、自動監査（作成者情報の埋め込み）のために `IUserContext` や `IDateTime` を直接注入する設計を採用していました。しかし、この設計には以下の課題がありました：
1.  **密結合**: Core モジュールの Identity や Context 抽象に依存するため、ライブラリとしての独立性が損なわれる。
2.  **汎用性の低下**: ユーザーコンテキストが存在しないバックグラウンドジョブ、CLI ツール、バッチ処理などでの利用に制約が生じる。
3.  **テストの複雑化**: サービスを利用する全テストでコンテキストのモックが必要になる。

## 3. Problem Statement (問題定義)

インフラ層のライブラリが「誰が操作しているか」というアプリケーション層の関心事を知りすぎている。これは単一責任の原則（SRP）および依存関係の逆転（DIP）の観点から、将来的なメンテナンスコストと再利用の障壁となる。

## 4. Decision (決定事項)

Blob モジュールを純粋なインフラストラクチャ・ラッパー（Pure Infrastructure）として再定義し、以下の変更を実施しました：

1.  **依存関係の削除**: `BlobFileService` および `BlobDirectoryService` から `IUserContext` と `IDateTime` を削除。
2.  **責務の移動**: `CreatedBy` や `CreatedAt` などの監査メタデータの付与責任を、呼び出し側（Application 層 / Handler）に移動。
3.  **標準化**: メタデータのキー名は `BlobConstants` に定義し、インフラとしての整合性は維持。

```csharp
// 変更後：純粋なインフラ操作に専念
public sealed class BlobFileService(
    IBlobContainerProvider containerProvider,
    BlobServiceClient blobServiceClient,
    IOptions<BlobOptions> options,
    ILogger<BlobFileService> logger) : IBlobFileService;
```

## 5. Alternatives Considered (代替案の検討)

### Option 1: 自動注入の継続
*   **Approach**: 現在のまま `IUserContext` を使い続ける。
*   **Rejected Reason**: ライブラリとしての汎用性が低く、非 Web 環境での利用が困難になるため。

### Option 2: メタデータプロバイダーの導入
*   **Approach**: `IBlobMetadataProvider` を定義し、外部から注入する。
*   **Rejected Reason**: 実装が複雑になりすぎる。呼び出し側が `Metadata` 辞書に直接値を入れる方がシンプルで透明性が高い。

## 6. Consequences & Mitigation (結果と緩和策)

*   **Positive**: モジュールの独立性が高まり、ビルド速度の向上、テストの単純化、およびあらゆる .NET 実行環境（Azure Functions, Console App 等）での利用が可能になった。
*   **Negative**: 呼び出し側でメタデータ設定のコードを書く必要が生じる。
*   **Mitigation**: ドキュメントおよび実装サンプルにより、標準的なメタデータ設定方法を開発者にガイドする。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

ライブラリ内部では `X-VK-Type` などのシステム管理に必要なメタデータのみを自動管理します。ユーザー情報の欠落はアプリケーション層のバリデーションまたはインターセプター等で防ぐ方針とします。
