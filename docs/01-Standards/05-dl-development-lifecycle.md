# 05. Development Lifecycle (`DL.01`〜`DL.05`)

> **対応ルール**: [`.agents/rules/l3-vk-be-dl-development-lifecycle.md`](/.agents/rules/l3-vk-be-dl-development-lifecycle.md) (`DL.01`〜`DL.05`)

本ドキュメントは、単体テスト・結合テストの4経路プロトコル、プレースホルダー禁止原則、ADR 起票トリガー、バックログ同期トリガー、および Source Generator ドキュメントタグの規約と具体例を定義します。

---

## `DL.01` — Testing Protocol (4-Path Coverage) 🔴

### 1. Unit & Integration Test Foundations
- **Unit Tests (`{Project}.UnitTests`)**:
  - `VKUnitTestBase` を継承し、すべての外部依存（DB・キャッシュ・外部サービス）はインターフェース経由でモック化（`GetMock<T>()`）します。
  - ドメインモデルや集約のテストデータ生成には `VKTestDataBuilder<T>`、決定論的な GUID 生成には `VKFakeGuidGenerator` を使用します。
  - `Result<T>` のアサーションには `VKResultAssertionExtensions`（`.Should().BeSuccess()`, `.Should().BeFailure(expectedError)`）を使用します。
  - **Async Rule (`CS.03`)**: テストメソッド内では **`.ConfigureAwait(false)` を使用しません**。
- **Integration Tests (`{Project}.IntegrationTests`)**:
  - 実際の DB やインフラ依存の検証には **Testcontainers** を使用します。
- **Naming Convention**:
  - テストクラス名: `{TargetClass}Tests.cs`
  - テストメソッド名: `{Method}_{Scenario}_{ExpectedResult}`（例: `EvaluateAsync_WhenTenantMismatch_ReturnsForbiddenError`）

### 2. Mandatory 4-Path Coverage
すべてのパブリックなアプリケーションハンドラー／評価サービスは、以下の **4つの経路** を網羅するテストを持つ必要があります：
1. ✅ **Happy Path**: 正常系の期待結果が返ること
2. ✅ **Not Found / Empty Result**: データ不在時に `null` ではなく適切な `Result.Failure` が返ること
3. ✅ **Permission / Tenant Isolation Failure**: 権限不足や別テナントアクセス時に拒否されること
4. ✅ **Infrastructure Failure Mapped to `Result.Failure`**: 下位依存のエラーや例外が境界でキャッチされ、`Result.Failure` に変換されること

---

## `DL.02` — Production-Grade Code Generation 🟡
- **No Placeholders / No `// TODO`**: `// TODO: implement later` のような未完成コードやスタブのコミットは厳禁です。技術的負債マーカーを残す必要がある場合は、解除条件を明記した形式（例: `TODO[remove-after-source-gen]`）とし、同時にバックログタスク（`DL.04`）として登録します。
- **Immediately Compilable**: すべてのコードは `using` ディレクティブや名前空間宣言を省略せず、即座にビルドが通る状態で記述します。

---

## `DL.03` — Architecture Decision Trigger (ADR) 🟡
以下のいずれかのアーキテクチャ変更が発生する場合、実装を進める前に **[`docs/00-Blueprints/04-AdrBlueprint.md`](/docs/00-Blueprints/04-AdrBlueprint.md)** に従って [`docs/04-ArchitectureDecisionRecords/`](/docs/04-ArchitectureDecisionRecords) に ADR を作成します：

- Level 1 Public API インターフェース（`IVK...`）の新規導入またはシグネチャ変更
- 新しいデザインパターンの採用または既存パターンの置き換え
- 横断的関心事（マルチテナントフィルタ、監査ログ、エラーハンドリング、DI シーケンス）のリファクタリング
- 複数の設計選択肢のトレードオフを解決して1つを選択したとき

---

## `DL.04` — Backlog Synchronization Trigger 🟡
以下の事象が発生した際は、コード内や会話内に埋もれさせず、**[`docs/00-Blueprints/07-BacklogItemBlueprint.md`](/docs/00-Blueprints/07-BacklogItemBlueprint.md)**（または MCP ツール `vk_be_add_backlog_item`）を用いて [`docs/07-Backlogs/`](/docs/07-Backlogs) にアトミックなタスク（例: `CORE-001`）として起票し、`Active_Sprint.md` と同期します：

- `README.md` に「今後の展望（Future Roadmap）」を追記したとき
- コードレビューや監査中に既存の技術的負債（`FIXME` / `DEBT`）を発見したとき
- 今すぐには対応しない改善提案（Non-urgent refactoring）を先送りしたとき

---

## `DL.05` — Source Generator Documentation Tags 🟡
Source Generator と連携する手書きのクラスや `partial` メソッドには、どの部分が自動生成と結びついているかを人間とツールの双方に明示するため、以下の標準コメントタグを付与します：

| タグ | 付与対象 | 標準コメントフォーマット |
| :--- | :--- | :--- |
| **`// [SG Marker]`** | `[VKBlockMarker]` や `[VKFeature]` を付与したクラス | `// [SG Marker] - This attribute triggers the Source Generator to generate module metadata and base implementation.` |
| **`// [SG Hook]`** | `RegisterBlockCustom` や `ValidateBlockCustom` 等の手動拡張 partial メソッド | `// [SG Hook] - This method is called by the Source Generator to inject manual [DI/validation] logic.` |
| **`// [SG Diagnostics]`** | `[VKBlockDiagnostics]` を付与した診断クラス | `// [SG Diagnostics] - This attribute triggers the Source Generator to generate ActivitySource and Meter.` |
| **`// [SG Logger]`** | `[LoggerMessage]` メソッドを含む partial クラス | `// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.` |

```csharp
namespace VK.Blocks.Core;

// [SG Marker] - This attribute triggers the Source Generator to generate module metadata and base implementation.
[VKBlockMarker]
public sealed partial class VKCoreBlock
{
    // [SG Hook] - This method is called by the Source Generator to inject manual DI registration logic.
    static partial void RegisterBlockCustom(IVKCoreBuilder builder)
    {
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.TryAddSingleton<IVKGuidGenerator, DefaultGuidGenerator>();
    }
}
```

