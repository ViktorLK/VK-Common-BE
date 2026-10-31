# ADR 005: AI Options Architecture: Interface Segregation over Class Inheritance

- **Date**: 2026-05-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.AI Options Pattern Refactoring

## 1. Context (背景)

VK.Blocks.AI モジュールでは、これまで `VKAIGovernanceOptions` を基底クラスとし、`VKAIModelOptions` がそれを継承、さらに具体的な機能（Chat, Embedding 等）の Options がそれらを継承するという、深いクラス継承構造を採用していました。この設計は、共通プロパティの再利用を意図したものでしたが、実務上の柔軟性と保守性に課題が生じていました。

## 2. Problem Statement (問題定義)

クラス継承ベースの Options 設計（Class Inheritance-based Options）には、以下の「工業級（Industrial DNA）」の観点から無視できない問題があります。

- **Tight Coupling (密結合)**: 基底クラス（Governance 等）にプロパティを追加・変更すると、全ての派生 Options に影響が波及し、意図しない破壊的変更を招きやすい。
- **Hidden Configuration Surface (設定の不透明性)**: 開発者が `appsettings.json` の設定項目を確認する際、複数の継承階層を遡る必要があり、設定可能な全項目を俯瞰することが困難である。
- **Record Brittleness (Record の脆弱性)**: C# の `record` 型において継承を使用すると、`with` 式によるコピーや等価性比較のセマンティクスが複雑化し、バグの温床となる。
- **Violation of ISP (インターフェース分離の原則違反)**: すべての AI 機能が必ずしも Governance（リトライ、クォータ等）や Connection（ModelId 等）の全項目を必要とするわけではないが、継承によって不要なプロパティの保持を強制される。

## 3. Decision (決定事項)

Options の設計指針を「クラス継承」から「インターフェースベースの組合せ（Interface-based Composition）」へと転換する **"Plan C"** を採用します。

1. **Abstract Base Records の廃止**:
   - `VKAIGovernanceOptions` および `VKAIModelOptions` を削除する。
2. **粒度の細かいインターフェースの活用**:
   - `IVKAIConnectionOptions`, `IVKAIResilienceOptions`, `IVKAIAuditOptions` 等のインターフェースを契約（Contract）として定義し、維持する。
3. **明示的なプロパティ実装**:
   - 各具象 Options レコード（例：`VKChatOptions`）は、自身が必要とするインターフェースのみを直接実装する。
   - プロパティは各レコード内に明示的に宣言する。これにより、各機能に最適なデフォルト値の設定や、文脈に応じた XML ドキュメントの記述を可能にする。
4. **集約インターフェースの制限**:
   - `IVKAIModelOptions` のような集約インターフェースは、それが真に全モデル共通のセマンティクスを持つ場合に限り許容し、基本的には個別のインターフェース実装を優先する。

### 核心的な設計イメージ

```csharp
// 基底クラスは使わず、必要な契約（Interface）を並べる
public sealed record VKChatOptions : IVKAIConnectionOptions, IVKAIResilienceOptions
{
    // Connection
    public string? ModelId { get; init; }
    public VKAIProviderType? Provider { get; init; }

    // Resilience (Explicit defaults for Chat)
    public int? RetryCount { get; init; } = 3;
    public TimeSpan? Timeout { get; init; } = TimeSpan.FromSeconds(60);
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Maintaining Shallow Inheritance
- **Approach**: 継承階層を 1 段だけに絞り、共通プロパティを維持する。
- **Rejected Reason**: 階層が浅くても「不要なプロパティの強制」という根本的な問題（ISP 違反）は解決せず、`record` の継承に伴う複雑性も残るため。

### Option 2: Source Generator for Property Injection
- **Approach**: 属性（`[GenerateOptions]`）を付与したクラスに対し、インターフェースに基づいたプロパティを SG で自動生成する。
- **Rejected Reason**: 配置設定（Configuration）は「何が設定可能か」がソースコード上で明示的であるべき（Explicit over Implicit）である。SG による隠蔽は、デバッグ時や IDE での定義参照において開発者の認知負荷を高めるリスクがある。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **Flexibility**: 機能ごとに必要なインターフェースだけを選択して実装できるため、ISP に忠実な設計となる。
- **Clarity**: クラスファイルを見れば、その機能で何が設定できるかが一目瞭然となる。
- **Safety**: 継承による Record コピーの副作用を排除できる。

### Negative
- **Boilerplate**: 複数の Options で同じプロパティ（Enabled 等）を繰り返し記述する必要がある。

### Mitigation
- IDE の「インターフェースの実装」機能（Quick Fix）を活用することで、記述のオーバーヘッドを最小限に抑える。
- 共通のプロパティセットが極めて多い場合は、継承ではなく「共通 Record のネスト（Composition）」を検討する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Validation**: 各 Options は引き続き `IValidateOptions` を実装し、`VKGuard` を用いて境界値チェックを行う。
- **Configuration Binding**: プロパティを明示的に宣言することで、.NET の `ConfigurationBinder` のソースジェネレーター（Reflection-free）との相性が向上し、起動パフォーマンスと AOT 耐性が高まる。

## 7. Status
✅ Accepted
