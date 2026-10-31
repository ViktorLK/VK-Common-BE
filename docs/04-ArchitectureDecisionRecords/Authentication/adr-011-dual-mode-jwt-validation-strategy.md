# ADR 011: Dual Mode JWT Validation Strategy

**Date**: 2026-03-30  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Authentication

## 2. Context (背景)

The JWT validation logic exclusively supported symmetrically signed tokens (HMAC-SHA256) via a hardcoded `SecretKey`, preventing the integration of third-party Identity Providers (IDPs) that use asymmetric encryption and JWKS endpoints.

## 3. Problem Statement (問題定義)

システム自身の発行するアクセストークンは対称鍵暗号（HMAC）で署名されていましたが、この仕組みをハードコード（`IssuerSigningKey = new SymmetricSecurityKey(...)`）していたため、以下のような拡張のブロックが発生しました。
- Azure AD, Google Cloud, Auth0 といった最新のクラウド IDP は、非対称鍵暗号（RSA / ECDSA）を用い、`.well-known/openid-configuration` を通じて公開鍵を定期的に更新（キーローテーション）する。
- 現在のアーキテクチャでは、これらの第三者トークンを受け入れ検証することが不可能。

## 4. Decision (決定事項)

ハイブリッドな ID 戦略をサポートするため、**JWT 認証を「自社発行（Symmetric）」と「サードパーティ（OIDC Discovery）」の双方向モード (Dual Mode)** に拡張しました。

- `JwtAuthMode` 列挙型を導入。
- OIDC モードの場合は静的な署名鍵 (`IssuerSigningKey`) を設定**せず**、代わりに `Authority` URL のみをミドルウェアに渡し、JWKS(JSON Web Key Set) による鍵の自動フェッチとキャッシュを ASP.NET Core ライブラリに委譲する。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: 署名に関わらず JWKS エンドポイントを自前で実装する**
  - **Approach**: 自社の発行サーバー側にも JWKS 発行機能を実装し、すべて OIDC ディスカバリープロトコルにフル対応させる。
  - **Rejected Reason**: 完全な IdentityServer をゼロから構築するのは車輪の再発明でありスコープ超過。VK モジュールの目的は軽量かつ高度な要件の双方に対応することであるため、シンプルな対称鍵モードも維持するべきと判断した。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: 将来的にシステムの認証基盤を既存の Active Directory やクラウドベースの汎用 IDP にリプレースする際にも、アプリケーションのコード修正なしで対応可能（設定ファイルの URL 変更のみ）。
- **Negative**: OIDC ディスカバリーモードを使用する場合、システム起動時やキーローテーション時に一時的に外部 URL (Idp) への HTTP 要求が発生するため、完全なオフライン/エアギャップ環境では動作しない。
- **Mitigation**: 閉域網での運用が求められるケースのために、従来の `Symmetric` モードを維持し、要件に応じてフォールバックできるようにする。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- 外部依存性が高まる OIDC モードに備え、HTTP タイムアウト保護や SSL 通信プロトコルの要件は基礎となる `MetadataAddress` 仕様に依存しています。バリデーターによって、`AuthMode` に矛盾したセキュリティパラメーター（例: 対称鍵の長さ不足）が渡されないよう厳密な保護が行われます。
