# ADR 002: Do Not Register Static ActivitySource/Meter in DI Container

**Date**: 2026-03-11  
**Status**: ✅ Accepted  
**Deciders**: Architecture Team  
**Technical Story**: Observability Module Refactoring

## 2. Context (背景)

VK.Blocks の Observability モジュールアーキテクチャ監査において、`ActivitySource` と `Meter` インスタンスが DI コンテナにシングルトン（`services.AddSingleton()`）として登録されていることが発覚しました。
しかし、これらのテレメトリ用コンポーネントは本来 `static readonly` フィールドとして定義されており、DIコンテナのライフサイクル管理の範疇外となります。

## 3. Problem Statement (問題定義)

現在の実装には以下の課題がありました：

1. **Lifecycle Mismatch**: DI コンテナはアプリケーション終了時に `IDisposable` を実装した登録済みサービスの `Dispose()` を試みますが、インスタンスが `static readonly` である場合、DIコンテナからの Dispose 呼び出しは機能せず、リソースのクリーンアップが保証されません。
2. **Performance Overhead**: テレメトリの記録は極めて高頻度で実行されるため、DI経由でインスタンスを解決するよりも、Static フィールドへ直接アクセスする方がパフォーマンス上有利です。
3. **Misleading Design**: DI コンテナに登録することで、開発者に「DIから注入して使うべき」という誤ったメンタルモデルを与えていました。

## 4. Decision (決定事項)

`System.Diagnostics.ActivitySource` および `System.Diagnostics.Metrics.Meter` インスタンスは、`static readonly` フィールドとして定義することを原則とし、**DIコンテナへの登録設定（`services.AddSingleton()`）を行わない**こととします。

### 設計ガイドライン

- テレメトリソースは `Diagnostics` クラスの `static` メンバーとして定義し、直接アクセスして使用します。
- ライフサイクル管理（`Dispose`）が厳密に必要な場合は、DI コンテナに依存せず、`IHostApplicationLifetime` の `ApplicationStopped` イベントフック等を通じて明示的に一元管理します。

## 5. Alternatives Considered (代替案の検討)

- **Option 1: DI への登録を維持し、インスタンス生成を DI に委譲する**
    - **Approach**: `static` をやめ、コンストラクタインジェクションで `ActivitySource` を注入する。
    - **Rejected Reason**: テレメトリの記録はパフォーマンスクリティカルであり、DIによる解決オーバーヘッドやコンストラクタの肥大化を避けるため（既存の `[VKBlockDiagnostics]` ソースジェネレーター設計とも背反する）。

- **Option 2: DI に Action デリゲートとして Dispose を登録する**
    - **Approach**: 起動時に DI に Dispose を強制するラムダを登録する。
    - **Rejected Reason**: システムが複雑化し、責務過多になるため。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**:
    - メモリ割り当てとDI解決のオーバーヘッドが削減され、パフォーマンスが向上します。
    - 開発者が「DIから取得するのではなく、Staticフィールドを参照する」という正しいメンタルモデルを持つようになります。
- **Negative**:
    - ホストアプリケーションの終了時に明示的な `Dispose()` が呼ばれない場合、エクスポート前のバッファにわずかに残ったトレースが欠落する可能性があります。
- **Mitigation**:
    - OpenTelemetry の基盤（TracerProvider, MeterProvider）が自身の Dispose 時にデータをフラッシュする機能を持つため、個別の Source/Meter の Dispose 漏れによる影響は実際には最小限に抑えられます。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

- **Implementation**:  
  `ObservabilityBlockExtensions.cs` から `services.AddSingleton(ObservabilityDiagnostics.Source)` 等の記述を完全に削除しました。
- **Security**:  
  `static` なパブリックフィールドであるため、外部から誤って null 代入や再代入が行われるのを防ぐため、必ず `readonly` キーワードを付与します。
