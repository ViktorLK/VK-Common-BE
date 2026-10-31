# ADR 004: Semantic Memory Abstraction & Integration

- **Date**: 2026-05-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI.SemanticKernel Knowledge & Retrieval

## 1. Context (背景)

現代の AI エージェントには、学習データ以外の知識にアクセスするための検索能力（RAG: Retrieval-Augmented Generation）が不可欠です。Microsoft の Semantic Kernel は、メモリの抽象化（`ISemanticTextMemory`, `IMemoryStore`）を提供していますが、これらは比較的低レベルであり、特定のプロバイダーや SDK の仕様に強く依存しています。VK.Blocks の「プロバイダー非依存」のコア原則を維持しつつ、SK の強力なメモリ機能を活用するための設計が必要です。

## 2. Problem Statement (問題定義)

SK のメモリ機能を直接公開することには、以下の課題があります。

- **Leaky Abstraction**: 利用側のコードに `Microsoft.SemanticKernel.Memory` への直接的な依存が発生し、将来的な移行や差し替えが困難になる。
- **Complexity**: ベクターデータベースのコネクタ（Qdrant, Chroma, Volatile 等）の設定が複雑で、BuildingBlock としての「使いやすさ（Low friction）」を損なう可能性がある。
- **Inconsistency**: チャット機能と埋め込み（Embedding）機能がバラバラに管理されると、検索と生成の整合性を保つのが難しくなる。

## 3. Decision (決定事項)

VK.Blocks.AI と Semantic Kernel を橋渡しする「セマンティックメモリ抽象化レイヤー」を実装します。

1. **Unified Interface**:
   - `IVKEmbeddingEngine` をベクター表現生成のコアインターフェースとして使用します。
2. **Internal Implementation**:
   - SK の `ITextEmbeddingGenerationService` をラップした内部実装を提供し、VK.Blocks のデータ型に変換します。
3. **Storage Abstraction**:
   - ストレージには SK の `IMemoryStore` を活用しつつ、構成（Configuration）に基づいて適切なコネクタを動的に登録します。
4. **Provider-Agnostic Setup**:
   - 利用側からは `appsettings.json` の設定だけで、背後のベクターデータベース（Qdrant, Volatile 等）を切り替えられるようにし、L1 公開 API には特定のデータベース依存を漏洩させないようにします。

### 核心的な設計と DI 登録

```csharp
// VK.Blocks.AI の抽象インターフェースを SK 実装で埋める
internal sealed class AISKEmbeddingEngine : IVKEmbeddingEngine
{
    private readonly ITextEmbeddingGenerationService _skEmbedding;
    // ... 実装
}

// コネクタの隠蔽登録
if (options.Memory.StoreType == "Qdrant") {
    builder.WithQdrantMemoryStore(options.Memory.Endpoint);
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Direct Export of SK Memory
- **Approach**: SK の `ISemanticTextMemory` インターフェースをそのまま DI に登録して公開する。
- **Rejected Reason**: VK.Blocks の他モジュール（SK を使わないプロバイダー）との一貫性が失われ、ベンダーロックインが発生するため。

### Option 2: Custom Vector Store Implementation
- **Approach**: SK に頼らず、独自にベクターデータベースのクライアントを実装する。
- **Rejected Reason**: 既に成熟している SK のエコシステム（Qdrant, Redis, Pinecone コネクタ等）を再開発するのは非効率であり、SK ブロックの利点を活かしきれないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Vendor Agnostic RAG**: アプリケーションコードを変更せずに、検索エンジンの仕組みやプロバイダーを変更可能になる。
- **Reduced Complexity**: 複雑なベクター DB の初期化コードを BuildingBlock 内部に封じ込めることができる。
- **Integrated Experience**: チャットとメモリが同じ DI ライフサイクル（Scoped）で管理され、シームレスな連携が可能になる。

### Negative
- **Abstraction Gap**: SK が持つ非常に高度なメモリ検索パラメータ（スコアのしきい値等）をすべて抽象化層で表現しきれない場合がある。

### Mitigation
- 共通インターフェースで対応できない高度な要件に対しては、`IVKArgs` 経由でのメタデータ渡しを許容し、柔軟性を確保する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Data Privacy**: メモリへの保存・検索時にテナント ID によるフィルタリングを強制し、マルチテナント環境におけるデータ漏洩を防止します。
- **Resilience**: ベクター DB への接続には Polly によるリトライ戦略を適用し、インフラの瞬断に対する耐性を高めます。

## 7. Status
✅ Accepted
