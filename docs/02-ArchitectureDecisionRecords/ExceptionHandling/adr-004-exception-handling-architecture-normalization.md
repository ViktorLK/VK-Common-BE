# ADR 004: Exception Handling Architecture Normalization

**Date**: 2026-04-18  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: VK.Blocks.ExceptionHandling Normalization

## 2. Context (背景)

`ExceptionHandling` ビルディングブロックは、VK.Blocks フレームワークにおいて例外処理の共通基盤を提供していますが、現状の構成には以下の課題がありました。
- **Rule 14 (Organization)** の違反: フォルダ構成が技術レイヤー（Abstractions, Pipeline, Helpers 等）に基づいており、垂直スライス（Feature-driven）の原則に適合していない。
- **Rule 6 (Observability)** の不足: `[LoggerMessage]` による構造化ログは実装されているが、OpenTelemetry 標準に準拠したメトリクス（Metrics）およびトレーシング（Tracing）の計装が未完了。
- **Rule 12 (Modern C#)** の不徹底: `ExceptionContext` などのモデルにおいて、プロパティの可変性（Mutability）が残っており、予期せぬ副作用のリスクがある。

## 3. Problem Statement (問題定義)

技術ドリブンなフォルダ構成は、モジュールの関心事が分散し、メンテナンス性の低下を招きます。また、観測性の欠如は本番環境でのトラブルシューティングを困難にします。

## 4. Decision (決定事項)

以下の正規化（Normalization）を実施します。

1.  **フォルダ構成の垂直スライス化**:
    - `Abstractions` および `Pipeline` を `Core` フォルダに集約し、例外処理の「核（Core）」となるロジックを明確化します。
    - 関連する契約（Interface）と実装を近接させ、凝集度を高めます。
2.  **VKBlockDiagnostics の導入**:
    - `[VKBlockDiagnostics]` ソースジェネレーターを利用し、`ActivitySource` と `Meter` を自動生成します。
    - `ExceptionHandlerPipeline` において、例外の処理状況（成功/失敗/未処理）をメトリクスとして記録します。
3.  **モデルの不変性強化**:
    - `ExceptionContext` を `sealed record` とし、プロパティを可能な限り `init` 指定、または非破壊的変更（Non-destructive mutation）を推奨する設計に改善します。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 現状維持 (Status Quo)**
    - Approach: 既存の技術フォルダを維持し、メトリクスのみ追加。
    - Rejected Reason: Rule 14 への違反が継続されるため、長期的には他の Building Block との一貫性が失われる。
- **Option 2: 完全な機能別スライス**
    - Approach: ハンドラーごとにフォルダを分ける。
    - Rejected Reason: ExceptionHandling はそれ自体が単一の「コア機能」であり、過度な分割は逆に複雑さを増すため、`Core` への集約が最適と判断。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - VK.Blocks 標準への完全適合。
    - 統合された観測性（Metrics, Tracing）の提供。
    - コードの一貫性と可読性の向上。
- **Negative**:
    - 名前空間（Namespace）の変更に伴う、既存参照元での修正負荷。
- **Mitigation**:
    - 移行ガイドを提供し、必要に応じてエイリアスや `using` の自動整理を実施。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Metrics**: `vk_blocks_exception_handling_handled_total` (Counter) などのメトリクスを定義。
- **Security**: 設定（`ExceptionHandlingOptions`）により、本番環境では詳細なスタックトレースが漏洩しないよう維持（RFC 7807 準拠）。
