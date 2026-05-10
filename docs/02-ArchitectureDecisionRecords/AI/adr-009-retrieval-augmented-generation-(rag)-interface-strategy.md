# ADR 009: Retrieval-Augmented Generation (RAG) Interface Strategy

- **Date**: 2026-05-10
- **Status**: 📝 Draft
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI / Retrieval

## 1. Context (背景)

LLM の知識は学習時点に限定されており、最新情報や企業内の非公開データにアクセスできません。これを解決する手法として RAG（検索拡張生成）が標準的ですが、検索（Retrieval）と生成（Generation）をどのようにクリーンに統合するかの設計指針が必要です。

## 2. Problem Statement (問題定義)

1. **密結合の回避**: 特定のベクトルデータベース（Pinecone, Qdrant, Azure AI Search 等）にエンジンが直接依存すると、インフラの変更が困難になる。
2. **コンテキスト注入の不透明性**: 検索結果をどのようにプロンプトに混ぜるか（コンテキスト窓の使い分け）が、実装ごとにバラバラである。
3. **オーケストレーションの複雑さ**: 検索 -> フィルタリング -> ランキング -> 生成 という一連のフローをアプリケーションコードに書くと、ビジネスロジックが埋もれてしまう。

## 3. Decision (決定事項)

1. **`IVKRetrievalEngine` の定義**:
   セマンティック検索やキーワード検索を抽象化したインターフェースを定義し、エンジンが「どこから探すか」を知らなくて済むようにします。

   ```csharp
   public interface IVKRetrievalEngine
   {
       Task<VKResult<IReadOnlyList<VKRetrievalResult>>> SearchAsync(string query, VKRetrievalArgs? args = null, CancellationToken ct = default);
   }
   ```

2. **`VKContextMessage` タイプの導入**:
   `VKChatMessage` のサブタイプとして、検索結果（引用元リンク等を含む）を保持するメッセージ型を導入し、履歴（History）の中で透過的に扱えるようにします。

3. **パラメータの標準化**:
   `TopK`（取得件数）および `MinScore`（最低適合スコア）を `VKRetrievalOptions` として標準化します。

4. **`VKAIRagOrchestrator` の提供**:
   「検索して生成する」という高レベルなアクションをカプセル化したオーケストレーターを提供し、シンプルな API 呼び出しで RAG を実現できるようにします。

## 4. Alternatives Considered (代替案の検討)

- **Option 1: Semantic Kernel の `Memory` を直接使う**
  - **Rejected Reason**: SDK 自体の変更（SK の大幅な API 変更など）の影響を最小限に抑えるため、VK.Blocks 独自の抽象化レイヤーを維持する（ADR-001 準拠）。
- **Option 2: アプリケーション側ですべてを繋ぎ合わせる**
  - **Rejected Reason**: 多くのユースケースで共通のパターン（検索結果の引用など）を再発明することになり、開発効率が低下する。

## 5. Consequences & Mitigation (結果と緩和策)

- **Positive**: 
  - インフラ（DB）を差し替えても、RAG のロジックを一切変更せずに済む。
  - 検索精度（Retrieval）と生成品質（Generation）を個別に評価・改善できる。
- **Negative**:
  - オーケストレーション層が厚くなることで、デバッグ時のスタックトレースが複雑になる可能性がある。
- **Mitigation**:
  - `ActivitySource`（OpenTelemetry）を統合し、検索ステップと生成ステップの実行時間を透過的にトレース可能にする。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **実装詳細**: `VKRetrievalResult` には、チャンク化されたテキストだけでなく、メタデータ（SourceUrl, PageNumber 等）を含め、UI 側での引用表示（Citations）を容易にします。
- **セキュリティ考察**: 検索時にユーザーの認可（ACL）を考慮し、ユーザーがアクセス権を持たないドキュメントがコンテキストに混入しないよう、フィルタリングを厳格に行う必要があります。

**Last Updated**: 2026-05-10
