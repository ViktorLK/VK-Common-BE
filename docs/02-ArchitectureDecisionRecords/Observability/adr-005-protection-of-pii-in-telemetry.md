# ADR 005: Protection of PII (Personally Identifiable Information) in Telemetry

**Date**: 2026-03-11  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Observability Security and Privacy Compliance

## 2. Context (背景)

システム内で問題が発生した際のトラブルシューティング能力を高めるため、ログやトレース（Span）に対して、操作を実行した「ユーザーのコンテキスト（User Context）」をメタデータとして付与するエンリッチメント（Enrichment）機能が存在します。
しかし、このユーザーメタデータ（ID、氏名、メールアドレス等）の取り扱いにおいて、個人を特定できる情報（PII: Personally Identifiable Information）を無制限にログ収集基盤（Elasticsearch, Datadog等）に送信してしまうと、GDPRやCCPAなどのプライバシーコンプライアンスに違反する重大なリスクが生じます。

## 3. Problem Statement (問題定義)

トラブルシューティングには「誰が引き起こしたエラーか」を知るためのユーザー情報が不可欠ですが、同時にPIIの漏洩を防ぐ必要があります。
すべての環境（Production, Staging, Dev）において、ユーザー名などのPIIが一律にログに記録される設計は、セキュリティ監査において致命的な指摘事項となります。

## 4. Decision (決定事項)

ログやトレースのエンリッチメントにおいて、以下のPII保護ポリシーとプラクティスをフレームワークレベルで強制します。

1. **識別子の記録**: `vk.user.id`（通常はUUID等の非可逆な識別子）は、システム追跡上の必須キーとして常に記録を許可します。
2. **PIIのオプトインによる保護**: `vk.user.name`（氏名やハンドルネーム等のPII）の記録は、構成オプション `ObservabilityOptions.IncludeUserName` によって制御し、**デフォルト値を「`false`（無効）」**とします。
3. **明示的な制御**: 開発環境などで特定のデバッグ目的がある場合にのみ、設定ファイル（`appsettings.Development.json`等）を介して明示的にオプトイン（`true` に設定）しなければ、PIIはテレメトリに一切記録されません。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: ログ収集基盤側（DataDogやSerilog Sink）でマスキングを行う**
    - **Approach**: アプリケーションはすべての情報を送信し、外部のエージェントやSaaS側でマスキング処理（`***`への置換等）を行う。
    - **Rejected Reason**: マスキング設定の漏れがあった場合にデータが流出するリスクが高く、またネットワーク境界を越えてPIIが送信されること自体が規制上問題視されるケースがあるため。データの発生源（アプリケーション実装内）で防ぐ「Shift-Left」のアプローチがより安全です。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - フレームワークの設計として「デフォルトで安全（Secure by Default）」が実現し、開発者がうっかりPIIをログに流出させる事故をシステム的に防ぐことができます。
    - プライバシー保護のコンプライアンス要件に適合します。
- **Negative**:
    - 本番環境で障害が発生した際、ログ上に直接ユーザー名が表示されないため、問題の影響を受けたユーザーを特定するには、`vk.user.id` を使ってデータベースを別途引く（JOIN等の）一手間がかかります。
- **Mitigation**:
    - カスタマーサポート用の管理ツールにおいて、エラーログの `vk.user.id` から該当ユーザー情報を安全なネットワーク内でルックアップできるダッシュボードを整備することで、トレース運用を支援します。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Implementation details**:
  `src/BuildingBlocks/Observability/Enrichment/UserContextEnricher.cs` において、`_options.IncludeUserName` を評価し、`true` の場合のみ `propertyAdder(FieldNames.UserName, _userContext.UserName);` を実行する仕組みとして実装しています。
- **Security**:
  PII出力のフラグ（`IncludeUserName`）の設定は本番環境の `appsettings.json` や Azure Key Vault などで厳密に管理され、CI/CD パイプライン内で「Production環境では `false` であること」をアサートするセキュリティテストを組み込むことが推奨されます。
