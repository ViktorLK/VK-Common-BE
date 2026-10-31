# ADR 013: Autonomous Lifecycle Management for Cleanup Tasks in Multi-Storage Environments

**Date**: 2026-03-31  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Authentication / Caching

## 2. Context (背景)

VK.Blocks では、メモリ内キャッシュ（In-Memory）と分散キャッシュ（Cloud/Redis）を切り替えて利用可能です。メモリ内キャッシュを使用する場合、期限切れデータの掃除を行うバックグラウンドタスク（Cleanup Task）が必要ですが、分散キャッシュ使用時にはこれが不要となります。

## 3. Problem Statement (問題定義)

分散ストレージ（Redis等）へ移行した後も、起動設定に残っているメモリクリーンアップ用のバックグラウンドサービスが実行され続ける問題がありました：
- **リソースの浪費**: 使用されていないメモリ空間を走査するスレッドがCPUを消費する。
- **ノイズログ**: 実装が存在しないプロバイダーに対してクリーンアップを試行し、警告ログを出力し続ける。
- **手動設定の負担**: 開発者がインフラ構成に合わせて、クリーンアップタスクの有効無効を手動で管理する必要があった。

## 4. Decision (決定事項)

インフラ構成に合わせて「自律的に実行を判断する」ロジックを導入しました：

1.  **関連プロバイダーの紐付け**: `IInMemoryCacheCleanup` に `AssociatedServiceType` を追加。
2.  **DIコンテナの自己診断**: `InMemoryCleanupBackgroundService` は開始時に、自身に登録されたクリーンアップタスクが現在「有効な（Primaryな）プロバイダー」に紐付いているかをチェックする。
3.  **自律的ハードエグジット**: すべての関連サービスが分散型に置換されている場合、バックグラウンドタスクは自身を停止（Self-Termination）させる。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 設定ファイルでの明示的なフラグ管理**
  - **Rejected Reason**: 設定ミス（Storage=Redis なのに Cleanup=True）などの不整合を完全に防ぐことができず、管理コストが高いため、「実装の存在有無」で自律判断する方式を採用した。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: 設定不要で、Redis等への移行時にリソース消費とログノイズが自動的にゼロになる。
- **Negative**: 起動時のDI走査に極めて僅かなオーバーヘッドが発生する。
- **Mitigation**: 走査は起動時の一回のみとし、判定結果をキャッシュすることで実行時のパフォーマンスには影響を与えない。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- この仕組みにより、開発環境（In-Memory）から本番環境（Redis）への移行がよりシームレスかつ効率的になります。
