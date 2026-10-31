# ADR 010: Multi-Engine Failover and Load Balancing

- **Date**: 2026-05-10
- **Status**: 📝 Draft
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI / Resiliency

## 1. Context (背景)

AI プロバイダー（特に OpenAI や Azure OpenAI のパブリックリージョン）は、しばしばレート制限（429 Too Many Requests）や一時的なサービス停止に見舞われます。ミッションクリティカルなシステムにおいて、単一のプロバイダーに依存することは可用性リスクとなります。

## 2. Problem Statement (問題定義)

1. **シングルポイントオブフェイリング**: OpenAI がダウンした際に、システム全体が停止してしまう。
2. **ベンダーロックイン**: 特定のプロバイダーの API 形式や制限に縛られ、コスト効率の良い別のプロバイダー（Google Gemini や local Ollama）への切り替えが困難。
3. **負荷の集中**: 単一のエンドポイントにリクエストが集中し、スループットが制限される。

## 3. Decision (決定事項)

1. **`CompositeAIBuilder` による複数エンジンの登録**:
   同一のインターフェース（例：`IVKChatEngine`）に対して、複数のプロバイダー実装を「候補」として登録できる仕組みを導入します。

2. **`VKRoutingPolicy` の定義**:
   リクエストの振り分けロジックを抽象化し、以下のポリシーをサポートします。
   - **Priority**: エンジン A を優先し、失敗したら B を使う。
   - **RoundRobin**: 均等に振り分けて負荷を分散する。
   - **Random**: 統計的に分散させる。

3. **自動フェイルオーバー（Failover）の実装**:
   `VKResult.Failure` が一時的なエラー（Transient Fault）であると判定された場合、ルーティング層が自動的に次の候補エンジンにリクエストをリトライします。

4. **「Shadow Mode」のサポート**:
   メインのエンジンでの処理と並行して、バックグラウンドで別のエンジンを走らせ、回答の質やレイテンシを比較測定する機能をオプションで提供します。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: Polly の `Fallback` ポリシーのみを使う**
  - **Rejected Reason**: DI コンテナとの統合や、複雑なルーティングロジック（重み付きランダムなど）を簡潔に記述するには、専用のビルダーストラクチャの方が適している。
- **Option 2: インフラ層（API Gateway 等）で振り分ける**
  - **Rejected Reason**: アプリケーション層で「プロンプトの種類」や「コンテキストサイズ」に応じてモデルを動的に選択するインテリジェントなルーティングができない。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - プロバイダーの障害に対して極めて高い耐性を得られる。
  - モデルのバージョンアップやプロバイダーの変更を、アプリケーションコードの変更なしに安全に実施できる。
- **Negative**:
  - 構成（Configuration）が複雑になり、どのリクエストがどのプロバイダーで処理されたかの追跡が難しくなる。
- **Mitigation**:
  - `VKResult` メタデータおよびログに、実際に使用されたエンジンの ID（ActiveProviderId）を必ず含めるようにします。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: DI 登録において `TryAddEnumerable` ではなく、名前付き登録（Named Registration）またはコレクション登録をサポートするように `IVKAIBuilder` を拡張します。
- **セキュリティ考察**: プロバイダーごとにデータの取り扱いポリシーが異なる場合があるため、機密情報を扱うリクエストを「安全なプロバイダー（例：閉域接続の Azure OpenAI）」に限定するフィルタリング機能を備える必要があります。

**Last Updated**: 2026-05-10
