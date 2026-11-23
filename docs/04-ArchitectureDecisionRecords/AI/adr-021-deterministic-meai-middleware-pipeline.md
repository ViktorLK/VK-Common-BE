# ADR-021: Deterministic MEAI Middleware Pipeline

- **Status**: Accepted
- **Date**: 2026-11-11
- **Area**: AI / Middleware & Pipeline Architecture
- **Related ADRs**: [ADR-019 (MEAI Migration)](/docs/04-ArchitectureDecisionRecords/AI/adr-019-migration-from-semantic-kernel-to-meai-and-maf.md), [ADR-020 (Pure Isolation of Ports)](/docs/04-ArchitectureDecisionRecords/AI/adr-020-pure-isolation-of-vk-blocks-ai-core-ports.md)

---

## Context

Semantic Kernel から Microsoft.Extensions.AI (MEAI) への移行（ADR-019）に伴い、中間処理は `DelegatingChatClient` によるオニオン型（洋葱型）ミドルウェアチェーンとして再構成された。  
しかし、ミドルウェアの登録順序が DI 解決順序や動的な呼び出し順序に依存する場合、以下の重大な課題が生じる：

1. **セキュリティとプライバシーの順序逆転リスク**:
   PII マスキングやプロンプトインジェクション検知がトークン計算やキャッシュの後段に配置されると、未サニタイズの機密情報がログやキャッシュに永続化されたり、攻撃プロンプトに対してトークン消費や外部 API 呼び出しが行われてしまう。
2. **非決定的な挙動（ARCH-05 違反）**:
   サービスの登録順序によって実行結果が変動する「順序依存性」は VK.Blocks の設計原則に反する。
3. **マルチモーダルコンテンツの欠落**:
   ミドルウェア内でテキストのみを操作する簡易マッピングを行うと、画像（`DataContent`/`UriContent`）やツール呼び出し（`FunctionCallContent`/`FunctionResultContent`）などの非テキスト `AIContent` が消失する。

## Options Considered

1. **Option 1: 自由順序の DI コレクション登録 (Ad-hoc Registration)**
   - Pros: 利用者が任意の順序でミドルウェアをスタックできる。
   - Cons: セキュリティガードレールのすり抜け、ログ漏洩、トークン計算狂いなどの事故が不可避であり、`ARCH-05` および `ARCH-12` に違反する。

2. **Option 2: 7層の決定論的固定パイプライン (Deterministic 7-Stage Pipeline — 採用)**
   - Pros:
     - `AIExtensionsClientFactory` において厳密に定義された 7 段階の決定論的オニオン構造を適用。
     - オプション（`VKAIPipelineOptions`）の Boolean トグルによる明示的な有効化・無効化（`AP.06`）。
     - 各ミドルウェアがマルチモーダル・ツール呼び出しを安全に透過・保持。
   - Cons:
     - パイプライン層の順序そのものは静的に固定され、利用者が順序を入れ替えることはできない（これは安全性の観点から望ましい制約）。

## Decision & Rationale

**Option 2 を採用する。**  
`ARCH-12`（Perimeter Cross-Cutting）および `ARCH-05`（Co-Located Defaults, Deterministic Overrides）に基づき、MEAI `ChatClientBuilder` による以下の厳密な実行パイプライン順序を強制する：

```
[Inbound Request]
  │
  ▼
1. Prompt Injection Guard (`PromptInjectionChatClient`)
   - 攻撃プロンプトを最外周で遮断（後続のトークン消費や機密ログ漏洩を未然に防止、PromptInjectionDetected 監査ログ出力）
  │
  ▼
2. Privacy PII Masking (`PrivacyChatClient`)
   - 機密情報をマスキング。非テキスト AIContent (Data/Uri/FunctionCall/FunctionResult) は完全保持
  │
  ▼
3. OpenTelemetry (`UseOpenTelemetry` with EnableSensitiveData = false)
   - サニタイズ済みメッセージに対して呼び出し全体のメトリクス・トレースを計測 (OR.01, OR.02)
  │
  ▼
4. Audit Logging (`AuditChatClient`)
   - サニタイズ済みペイロードに基づくトレースIDと実行時間の外周監査記録 (FS-11)
  │
  ▼
5. Idempotency & Caching (`IdempotencyChatClient`)
   - キャッシュヒット時はツール呼び出しおよびレート制限トークン消費を完全にバイパス
   - TurnId / IdempotencyKey 優先の決定論的キャッシュキー生成 (FS-06)
  │
  ▼
6. Function Invocation (`UseFunctionInvocation`)
   - ツール実行ループ。各ラウンドで発生する再帰的な LLM 呼び出しに対して最内周ガバナンスを適用
  │
  ▼
7. Token Governance & Pruning (`TokenGovernanceChatClient`)
   - 各 LLM ラウンドの TPM/RPM レート制限 (`IVKTokenRateLimiter`) の事前検証
   - スライディングウィンドウ履歴縮退 (`IVKAIContextPruner`) による System メッセージ保護
   - トークン使用量の集計 (`IVKTokenUsageAggregator`) と適用ログ (`TokenGovernanceApplied`)
  │
  ▼
[Target LLM Provider (OpenAI / Azure / Ollama / Google Gemini via Resilient HttpClient)]
```

### Embedding パイプライン

Embedding についても同様に `DelegatingEmbeddingGenerator` による決定論的パイプラインを強制する：

```
[Inbound Embedding Request]
  │
  ▼
1. Privacy PII Masking (`PrivacyEmbeddingGenerator`)
   - 入力テキストの機密情報を事前マスキング
  │
  ▼
2. OpenTelemetry (`UseOpenTelemetry`)
   - 埋め込み全体のメトリクス・トレースを計測 (OR.01)
  │
  ▼
3. Audit Logging (`AuditEmbeddingGenerator`)
   - `TimeProvider` による生成所要時間およびベクトル次元数・件数の構造化ログ記録
  │
  ▼
[Target Embedding Provider (OpenAI / Azure / Ollama / Google Gemini)]
```

### 技術的要点

- **レジリエント HTTP トランスポート**:
  OpenAI, Azure OpenAI, Ollama に加え、Google Gemini クライアントに対しても `IHttpClientFactory` 経由の標準レジリエンスハンドラー（Polly: Retry 3x, Circuit Breaker, Timeout; `OR.03`）を適用した `HttpClient` を注入する。
- **高精度トークン計測**:
  `Microsoft.ML.Tokenizers` による `TiktokenTokenCounter` を導入し、モデル別キャッシュ（`ConcurrentDictionary`）により高速かつ正確なトークン計算を実現。
- **System プロンプトの不可侵性**:
  `SlidingWindowContextPruner` は System ロールを持つメッセージを無条件で保護し、直近の対話履歴を時系列順に維持したままコンテキスト制限内に収める。

## Consequences & Trade-offs

- **プラスの影響**:
  - セキュリティ、プライバシー、トークンガバナンスが常に正しい順序で実行され、人為的設定ミスによるセキュリティホールを根絶。
  - マルチモーダル対話や関数呼び出しを含む複雑なチャットセッションでもデータ欠落が発生しない。
  - プロバイダー通信の耐障害性が向上。
- **マイナスの影響 / リスク**:
  - 順序が固定されるため、特殊なユースケースで順序を変更したい場合はカスタムファクトリまたは DelegatingChatClient の独自構成が必要。
