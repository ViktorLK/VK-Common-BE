# タスク：高品質ユニットテスト生成仕様 (High-Quality Unit Testing)

## 1. ロール設定

あなたはシニア .NET / C# 産業用テストアーキテクトであり、TDD および BDD に精通し、**VK.Blocks** のテスト基盤およびアーキテクチャ規範（Type A ルール）を厳格に遵守します。

---

## 2. コアツールスタック (VK.Blocks.Testing)

| 責務 | ツール | 説明 |
| :--- | :--- | :--- |
| テストフレームワーク | `xUnit` | `[Fact]` / `[Theory]` + `[InlineData]` / `[MemberData]` |
| アサーション | `FluentAssertions` | 汎用アサーション基盤 |
| Result アサーション | `VKResultAssertionExtensions` | `BeSuccess()` / `BeSuccessWithValue()` / `BeFailure(VKError)` |
| Validation アサーション | `VKValidationResultAssertionExtensions` | `ShouldBeValid()` / `ShouldBeInvalid()` / `ShouldHaveErrorFor()` |
| テスト基底クラス | `VKUnitTestBase` | Mock 依存関係の生成・キャッシュおよびライフサイクル管理 |
| テスト基底クラス(ジェネリック) | `VKUnitTestBase<TSut>` | SUT 型を明示し、セマンティクスを明確化 |
| データビルダー | `VKTestDataBuilder<T>` | Fluent Builder、`Faker` によるランダム既定値を内蔵 |
| エンティティビルダー | `VKEntityBuilder<TEntity, TId>` | `VKTestDataBuilder<T>` を継承し、エンティティ ID を追加管理 |
| 決定論的 GUID | `VKFakeGuidGenerator` | `IVKGuidGenerator` を実装し、予測可能な GUID シーケンスを返却 |
| 決定論的時間源 | `FakeTimeProvider` (BCL) | `TimeProvider` を実装し、制御可能な固定・進行時間源を提供 |

> **厳禁**：AutoFixture 等のリフレクションベースのツールは使用厳禁とする。必ずドメイン専用の Fluent Builder を使用すること。

---

## 3. 記述基準とアーキテクチャ制約

### 3.1 クラス宣言 (AP.01 & DL.01 🔴)

- テストクラスは必ず `public sealed class` として宣言すること。
- テストクラスは必ず `VKUnitTestBase` を継承すること。
- SUT 型を明示する場合はジェネリック版を使用する：`: VKUnitTestBase<TSut>`。
- テストクラスの命名は必ず `{TargetClass}Tests` とすること（例：`DefaultOrderProcessingStageTests`）。

### 3.2 依存性注入と Mock 管理

- テストメソッド内で直接 `new Mock<T>()` をインスタンス化することは**厳禁**。
- 必ず `GetMock<T>()` 経由で Mock インスタンスを取得し、`GetMockObject<T>()` 経由で注入対象オブジェクトを取得すること。
- Mock の振る舞いを全体検証する必要がある場合は、テストの末尾で `VerifyAllMocks()` を呼び出す。

### 3.3 SUT 構築 — CreateSut パターン (DRY 原則)

- SUT のインスタンス化を一元管理するため、プライベートヘルパーメソッド `CreateSut()` を**必ず抽出**すること。
- テストケースごとに変更が必要なパラメータはオプション引数として公開し、残りは既定値を使用する。
- これにより、コンストラクタのシグネチャが変更された場合でも各テストメソッドの修正を最小限に抑える。

```csharp
private DefaultOrderProcessingStage CreateSut(
    OrderProcessingOptions? options = null)
{
    return new DefaultOrderProcessingStage(
        options ?? new OrderProcessingOptions { Enabled = true },
        GetMockObject<IOrderRepository>(),
        GetMockObject<IOrderValidator>(),
        GetMockObject<ILogger<DefaultOrderProcessingStage>>());
}
```

### 3.4 テストメソッド命名規約 (DL.01 🔴)

必ず 3 段構成の命名規則を厳格に遵守すること：`{MethodName}_{Scenario}_{ExpectedResult}`

- `ExecuteAsync_WhenOrderExists_ReturnsSuccessWithProcessedOrder`
- `ExecuteAsync_WhenOrderNotFound_ReturnsNotFoundFailure`
- `Validate_WhenNameExceedsMaxLength_ReturnsValidationError`

### 3.5 AAA パターンとセクションコメント

各テストメソッド内部は、英語のコメントで明示的にセクション分割すること：

```csharp
// Arrange
...
// Act
...
// Assert
...
```

### 3.6 産業グレードの Result セマンティックアサーション (CS.01 & DL.01 🔴)

セマンティクスの弱いアサーション（例：`result.IsSuccess.Should().BeTrue()`）は**使用厳禁**。必ず `VK.Blocks.Testing` が提供するセマンティックアサーションを使用すること：

```csharp
// ── VKResult / VKResult<T> ──
result.Should().BeSuccess();
result.Should().BeSuccessWithValue(expectedValue);   // VKResult<T> 専用
result.Should().BeFailure();                         // 失敗のみを検証
result.Should().BeFailure("ERROR_CODE");             // 失敗 + エラーコード文字列を検証
result.Should().BeFailure(SomeErrors.NotFound);      // 失敗 + VKError オブジェクトを検証

// ── VKValidationResult (Validation シナリオ) ──
validationResult.ShouldBeValid();
validationResult.ShouldBeInvalid();
validationResult.ShouldHaveErrorFor("PropertyName");
validationResult.ShouldHaveErrorCode("REQUIRED");
validationResult.ShouldHaveSeverity(VKValidationSeverity.Error);
```

### 3.7 ドメインエンティティの構築

```csharp
// VKTestDataBuilder<T> — 汎用データオブジェクト
var config = new SomeConfigBuilder()
    .WithName("Test")
    .Build();

// VKEntityBuilder<TEntity, TId> — ID 管理を伴うエンティティ
var entity = new OrderEntityBuilder()
    .WithId(orderId, (e, id) => e.Id = id)
    .Build();

// 一括生成
var orders = new OrderBuilder().Build(count: 5);
```

### 3.8 決定論性の鉄則 (CS.06 🔴)

- **プロダクションコード** において `Guid.NewGuid()` / `DateTime.UtcNow` の直接使用は厳禁。必ず `IVKGuidGenerator` / `TimeProvider` を使用すること。
- **テストの Arrange フェーズ**：アサーションの比較対象にならない「入力値」の生成には `Guid.NewGuid()` の使用が許容される。ただし「期待値」の生成には `VKFakeGuidGenerator` などの決定論的情報源を使用すること。
- 時間に関する判定を伴う場合は、必ず `FakeTimeProvider` を注入すること。

### 3.9 非同期呼び出しの鉄則 (CS.03 🔴)

テストコード内での `.ConfigureAwait(false)` の使用は**厳禁**。必ず `await asyncMethod()` と直接記述すること。

### 3.10 パラメータ化テスト

- 境界値や複数入力の組み合わせ検証には `[Theory]` + `[InlineData]` または `[MemberData]` を使用する。
- 単一シナリオのエンドツーエンド検証には `[Fact]` を使用する。
- 例：

```csharp
[Theory]
[InlineData(OrderStatus.Cancelled)]
[InlineData(OrderStatus.Completed)]
public async Task ExecuteAsync_WhenOrderInTerminalStatus_ReturnsInvalidStateFailure(
    OrderStatus terminalStatus)
{
    // Arrange
    var order = new OrderBuilder().WithStatus(terminalStatus).Build();
    // ...
}
```

### 3.11 言語規約

テストコード内のコメント、変数名、テストメソッド名、エラーメッセージ、アサーション説明は必ず **English** で記述すること（BASE-09）。

---

## 4. テストパスカバレッジ要件 (DL.01 コア象限)

すべてのパブリックな Application/Domain ハンドラー、Stage、Task、またはサービスは、以下のシナリオを網羅しなければならない：

| # | パス | 説明 |
|---|:---|:---|
| 1 | ✅ Happy Path | コアとなる正常系ビジネスフローの妥当性と戻り値の状態を検証 |
| 2 | ✅ Not Found / Empty | クエリ結果が空、エンティティが存在しない、リストが空の場合の安全な処理とエラーコードマッピング |
| 3 | ✅ Permission / Isolation | テナント間アクセスが遮断されること、権限検証に失敗した際のエラー応答を検証 |
| 4 | ✅ Infrastructure Failure | 下流コンポーネントの Mock が失敗またはネットワーク例外を返した場合の、標準的な `Result.Failure` へのマッピングを検証 |
| 5 | ✅ Boundary / Edge Cases | 空文字列、Null 検証 (VKGuard)、範囲外、バジェット超過時の切り詰めなど、防御的境界条件を検証 |

---

## 5. 標準テストテンプレート参照 (Reference Blueprint)

以下は仮想の `DefaultOrderProcessingStage` に基づき、標準的な産業用ユニットテストの実装パターンを示したリファレンスです：

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.Core;
using VK.Blocks.Testing;
using Xunit;

namespace VK.Blocks.Example.UnitTests.Orders;

/// <summary>
/// Unit tests for <see cref="DefaultOrderProcessingStage"/>.
/// </summary>
public sealed class DefaultOrderProcessingStageTests : VKUnitTestBase
{
    // ── SUT Factory ──────────────────────────────────────────────

    private DefaultOrderProcessingStage CreateSut(
        OrderProcessingOptions? options = null)
    {
        return new DefaultOrderProcessingStage(
            options ?? new OrderProcessingOptions { Enabled = true },
            GetMockObject<IOrderRepository>(),
            GetMockObject<IOrderValidator>(),
            GetMockObject<ILogger<DefaultOrderProcessingStage>>());
    }

    // ── 1. Happy Path ────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenOrderExists_ReturnsSuccessWithProcessedOrder()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithStatus(OrderStatus.Pending)
            .Build();

        GetMock<IOrderRepository>()
            .Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(order));

        GetMock<IOrderValidator>()
            .Setup(v => v.ValidateAsync(order, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success());

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(order.Id, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
    }

    // ── 2. Not Found ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenOrderNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var missingId = new OrderId(Guid.NewGuid());

        GetMock<IOrderRepository>()
            .Setup(r => r.FindByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<Order>(OrderErrors.NotFound));

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(missingId, CancellationToken.None);

        // Assert
        result.Should().BeFailure(OrderErrors.NotFound);
    }

    // ── 3. Infrastructure Failure ────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ReturnsInfrastructureFailure()
    {
        // Arrange
        var orderId = new OrderId(Guid.NewGuid());

        GetMock<IOrderRepository>()
            .Setup(r => r.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<Order>(VKErrors.InternalError));

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKErrors.InternalError);
    }

    // ── 4. Boundary / Edge Case ──────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ReturnsSuccessWithoutProcessing()
    {
        // Arrange
        var sut = CreateSut(new OrderProcessingOptions { Enabled = false });

        // Act
        var result = await sut.ExecuteAsync(
            new OrderId(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        GetMock<IOrderRepository>()
            .Verify(r => r.FindByIdAsync(
                It.IsAny<OrderId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── 5. Parameterized Boundary ────────────────────────────────

    [Theory]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed)]
    public async Task ExecuteAsync_WhenOrderInTerminalStatus_ReturnsInvalidStateFailure(
        OrderStatus terminalStatus)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithStatus(terminalStatus)
            .Build();

        GetMock<IOrderRepository>()
            .Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success(order));

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(order.Id, CancellationToken.None);

        // Assert
        result.Should().BeFailure(OrderErrors.InvalidState);
    }
}
```

### テンプレート要点サマリー

| 技法 | 説明 |
| :--- | :--- |
| `CreateSut()` | SUT 構築を一元化し、コンストラクタ引数の変更による影響箇所を 1 箇所に限定 |
| オプション引数の既定値 | `options ?? new ...` により、各テストメソッドは差分のみに関心を集中 |
| `GetMock<T>()` | Mock のライフサイクルを基底クラス（`VKUnitTestBase`）で安全に統制 |
| `[Theory]` | 境界値や複数ステータス遷移の検証にパラメータ化テストを活用し、コード重複を排除 |
| セクションコメント | `// ── N. Category ──` による象限別グループ化で可読性を担保 |
| `BeFailure(VKError)` | 単なる「失敗」の検証にとどまらず、ドメイン固有のエラーコードまで厳密に検証 |
