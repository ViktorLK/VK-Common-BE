# ADR 008: Hash-Based Metadata Synchronization for Startup Performance

**Date**: 2026-04-08  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: VK.Blocks.Authorization - Runtime Performance

## 1. Context (背景)

定義優先（Definition-First）のアプローチにより、開発時に定義された権限メタデータをアプリケーションの起動時にデータベース（DB）へ自動同期することが可能になりました。しかし、権限数が数百から数千に及ぶ大規模なエンタープライズシステムにおいて、起動するたびに全量の比較・更新処理を行うと、IO オーバーヘッドや DB ロックにより起動速度が著しく低下（Spin-up Delay）するという課題が発生しました。

## 2. Problem Statement (問題定義)

不必要な DB 同步処理（Idempotent Syncs）が原因で、特にコンテナ環境等でのスケーリング時におけるプロセスの起動時間が許容範囲を超えてしまうリスクがあります。

## 3. Decision (決定事项)

「ハ希指紋（Metadata Hashing）」による差量検知メカニズムを導入し、起動時の同期処理を $O(N)$ から $O(1)$ へ最適化します。

1. **確定的なハッシュ計算**：
   Source Generator (SG) は、生成された全権限の Name, Module, DisplayName, Description を入力値として、**FNV-1a 64-bit 非暗号化ハッシュ**を算出し、`PermissionsCatalog.MetadataHash` として定数化します。
2. **指纹比較ロジック**：
   起動時の同期プロセスの最優先ステップとして、DB に保存されている「前回の同期指紋」と「現在のコードの指紋」を比較します。
3. **条件付き同期（Conditional Execution）**：
   ハッシュが完全に一致する場合、メタデータに変更がないと判断し、すべての DB 同期処理をスキップして即座に `return` します。不一致の場合のみ、実際の `SyncPermissionsAsync` を実行し、成功後に DB の指紋を更新します。
4. **抽象化の分離**：
   ハッシュ自体は特定の権限レコードではなく、システム全体の「メタデータ・バージョン」として扱います（詳細は Core 側の ADR 参照）。

## 4. Alternatives Considered (代替案の検討)

### Option 1: Full Comparison (毎回全量比較)
- **Approach**: 毎回 DB から全量取得してメモリ上で Diff を取る。
- **Rejected Reason**: DB への問い合わせコストが無視できず、データ量が増えるとメモリ消費も悪化する。

### Option 2: Version Numbering (手動バージョン管理)
- **Approach**: 開発者がコードを修正するたびにバージョン番号を手動でインクリメントする。
- **Rejected Reason**: ヒューマンエラーによる更新忘れが発生しやすく、自動化のメリットが損なわれる。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive (メリット)
- **起動パフォーマンスの劇的な向上**：99% の起動ケースにおいて、DB 同期処理の実行時間はほぼゼロになります。
- **完全自動化**：開発者がハッシュを意識する必要はなく、SG がすべてを管理します。

### Negative (デメリット)
- **微細なハッシュ衝突のリスク**：FNV-1a 64-bit は衝突耐性が高いものの、理論上のリスクはゼロではありません。
- **緩和策**: 同期失敗が許されない極めて特殊な環境では、再同期を強制する「ForceSync」フラグを DI オプションに用意することを検討します。

---
**Last Updated**: 2026-04-08  
