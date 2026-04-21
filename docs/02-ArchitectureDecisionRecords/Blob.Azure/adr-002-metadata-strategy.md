# ADR 002: Metadata-Driven Audit and Cost-Optimization Strategy

- **Date**: 2026-03-24
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Blob.Azure Implementation Optimization

## 2. Context (背景)

Azure Blob Storage には、Blob の検索性を高めるための「Blob Index Tags（キー・値ペア）」と、オブジェクトの付随情報を保持するための「Metadata（キー・値ペア）」の 2 種類が存在します。
*   **Index Tags**: サーバーサイドでの高速なフィルタリング（FindBlobsByTags）が可能ですが、課金対象であり、かつ 1 つの Blob あたり 10 個までの制限があります。
*   **Metadata**: オブジェクトの属性情報として無料で利用可能ですが、これを条件とした検索は SDK レベルではサポートされていませ。

## 3. Problem Statement (問題定義)

「作成者別」「期間別」などの監査要件を満たすために、すべての情報を Index Tags に入れてしまうと、以下の懸念が生じます：
1.  **コスト増**: 数百万件のファイルがある場合、タグの維持コストが無視できなくなる。
2.  **拡張性の限界**: 将来的にビジネスロジック固有のタグを増やしたい場合、10 個の制限に抵触する恐れがある。
3.  **検索シンプリティの欠如**: クラウド固有の API (FindBlobsByTags) に依存しすぎると、アプリケーション層での柔軟なクエリ（JOIN 等）が難しくなる。

## 4. Decision (決定事項)

**「データベース (DB) を主、Blob Metadata を従」** とするハイブリッドなインデックス戦略を採用しました。

1.  **DB によるインデックス管理**: ファイルのメタデータ（作成者、作成日時、ファイル名、ディレクトリ構造、ビジネス属性）は、アプリケーション側の DB (SQL Server 等) に保存し、検索・一覧表示は DB に対して行います。
2.  **Metadata による自己記述**: Blob 自体にも `CreatedBy`, `CreatedAt` を Metadata として付与します。これは DB がない状態での内容確認や、将来的なデータ修復（Re-indexing）のための「信頼できる唯一の情報源（SSOT）」として機能します。
3.  **Index Tags の限定利用**: ライブラリによる「自動注入」は行わず、呼び出し側が真に必要と判断した場合のみ明示的に利用する方針としました。

## 5. Alternatives Considered (代替案の検討)

### Option 1: すべて Index Tags で管理
*   **Approach**: `FoundBlobsByTags` をフル活用して DB なしのストレージを目指す。
*   **Rejected Reason**: コスト高であることと、10 個の制限により複雑なビジネス要件に対応しきれないため。

### Option 2: メタデータを一切付与しない (DB のみ)
*   **Approach**: Blob にはデータのみを置き、属性は DB だけが持つ。
*   **Rejected Reason**: Azure Portal や Storage Explorer で直接 Blob を閲覧した際、誰のファイルか判別できず運用の利便性が低下するため。

## 6. Consequences & Mitigation (結果と緩和策)

*   **Positive**: ストレージコストを最小限に抑えつつ、DB による高速かつ複雑な検索（ファイル名検索、ソート、期間抽出等）を実現できる。
*   **Negative**: DB とストレージ間でのデータの不整合（Orphaned blobs）が発生する可能性がある。
*   **Mitigation**: バックグラウンドでのクリーンアップジョブや、`IDbContextInterceptor` による整合性確保を実施する。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

この決定により、`BlobFileService` 等のコンストラクタから `IUserContext` 等を排除することができ、設計の疎結合化（ADR-003(Blob)）にも寄与しました。
