# 💥 Failure Modes & Resilience Handbook: VK.Blocks.Core

- **System ID / Package ID**: `VK.Blocks.Core`
- **Domain / Area**: `Core / Foundation`
- **Target Audience**: SRE、プラットフォームエンジニア、下流サービス開発者、AI ペアプログラミング
- **Governing Rules**: `CS.01`, `BASE-05`, `FS-02`, `BB.03`
- **Status**: 🏛️ Accepted

---

## 1. 🎯 目的と概要 (Purpose & Overview)

`VK.Blocks.Core` はシステム最下層の基盤であり、Core 内部の不具合や契約違反は上位のすべての BuildingBlock およびアプリケーション全体に波及します。

本文書は、`VK.Blocks.Core` において発生し得る主要な **障害モード（Failure Modes）**、DI コンテナ起動時およびランタイムにおける **縮退・遮断挙動（Degraded / Fail-Fast Behavior）**、下流ハンドラーへ返却される **`VKResult` エラーコード（`CS.01`）**、および推奨される **復旧手順（Recovery Runbook）** を定義する運用ハンドブックです。

---

## 2. 📊 障害モードマトリクス (Failure Modes Matrix)

| 障害モード (Failure Mode) | トリガー条件 | システムの挙動 (Behavior) | 返却エラーコード / 送出例外 | 推奨回復アクション |
| :--- | :--- | :--- | :--- | :--- |
| **F-01: DI 循環依存の検出** | モジュール間で相互参照（A ➡️ B ➡️ A）が存在する場合（`ADR-008`）。 | **Fail-Fast 即時停止**。Pre-order 走査時に `HashSet` が検知し、起動プロセスを即座にクラッシュさせる。 | `VKBlockDependencyException`<br/>`"Circular dependency detected: A -> B -> A"` | 依存関係グラフを確認し、共有インターフェースを Core または独立スライスへ切り出す。 |
| **F-02: 必須依存ブロックの未登録** | 子ブロック（例: `OpenIdConnect`）の登録時に親ブロック（`Authentication`）が欠落している場合（`ADR-003`）。 | **Fail-Fast 即時停止**。`EnsureDependenciesRegistered` がマーカー欠落を検知して起動を遮断。 | `VKBlockDependencyException`<br/>`"Missing required block: VKAuthenticationBlock"` | `Program.cs` の DI 構成で、親ブロック（例: `AddVKAuthenticationBlock`）を追加する。 |
| **F-03: 防御的ガード（VKGuard）違反** | メソッド境界で null や空文字列、不正な範囲の値が渡された場合。 | **即時例外送出**。不変条件破壊（Programming Error）として即座に中断。 | `VKValidationException`<br/>`VKCoreErrors.ArgumentNull` | 呼び出し元の入力値検証（FluentValidation / DTO）を追加し、不正データを境界で拒絶する。 |
| **F-04: テナントコンテキスト欠落** | マルチテナント要求スコープで `VKTenantId` が初期化されていない場合。 | **フェイルクローズ（Fail-Closed）**。未設定の空テナントを拒否し、処理を中断。 | `VKCoreErrors.TenantMissing`<br/>`"Core.Tenancy.MissingContext"` (403/401) | HTTP 境界のテナント解決ミドルウェアが正しく設定されているか確認。 |
| **F-05: 成功 Result に対するエラー参照** | `result.IsSuccess == true` なのに `result.Error` を参照した場合。 | **InvalidOperationException 送出**。ROP の誤用として開発時エラーを通知。 | `InvalidOperationException`<br/>`"Cannot access Error on a successful Result."` | `result.IsFailure` を確認してから `result.Error` にアクセスするようコードを修正。 |
| **F-06: 失敗 Result に対する値参照** | `result.IsFailure == true` なのに `result.Value` を参照した場合。 | **InvalidOperationException 送出**。未検証の値アンラップを防止。 | `InvalidOperationException`<br/>`"Cannot access Value on a failed Result."` | `if (result.IsSuccess)` ガードまたは流暢な `.Match()` / `.Map()` 拡張を使用。 |
| **F-07: 分散ロック取得タイムアウト** | 同時実行インスタンスの競合により、制限時間内にロックを獲得できなかった場合。 | **グレースフル失敗**。例外を投げず、`VKResult.Failure` を返却。 | `VKCoreErrors.LockTimeout`<br/>`"Core.Sync.LockTimeout"` (409/503) | 指数バックオフによる再試行（Polly）またはロック保持時間の短縮。 |

---

## 3. 🛡️ Fail-Fast 原則とプログラミングエラーの分離 (`BASE-05`)

Core では、障害を以下の 2 つに明確に分離して処理します：

1. **予見可能な業務・運用エラー (Foreseeable Operational Failures)**:
   - 例: 入力バリデーション違反、テナント未検出、ロック競合タイムアウト。
   - **対処**: **例外を一切スローせず、`VKResult<T>.Failure(VKError)` を返却**。
2. **回復不能なプログラミングエラー・不変条件破壊 (Bugs & Broken Invariants)**:
   - 例: DI コンテナの循環参照、引数の null 違反（`VKGuard`）、`Result.Value` の不正アクセス。
   - **対処**: **即座に例外（`VKBaseException` または `InvalidOperationException`）を送出し、プロセスまたはリクエストを即座に Fail-Fast 停止**。

---

## 4. 📋 SRE 運用トラブルシューティング (On-Call Runbook)

### 4.1 起動時クラッシュ: `VKBlockDependencyException`
- **症状**: アプリケーション起動直後にコンテナが終了し、エラーログに `VKBlockDependencyException` が出力される。
- **初動対応**:
  1. ログメッセージの `"Circular dependency detected"` または `"Missing required block"` を確認。
  2. 欠落しているブロックのマーカー型を特定。
  3. `Program.cs` の `services.AddVK...` 呼び出しに不足しているブロックを追加する。

### 4.2 実行時エラー: `Core.Tenancy.MissingContext`
- **症状**: API クライアントに HTTP 403 / 401 が返却され、ProblemDetails に `Core.Tenancy.MissingContext` が記録される。
- **初動対応**:
  1. API ゲートウェイまたはリバースプロキシが `X-Tenant-Id` ヘッダーを正しく転送しているか確認。
  2. JWT トークン内に `tenant_id` クレームが含まれているか検証。

