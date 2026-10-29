# 任务：高质量单元测试生成 (High-Quality Unit Testing)

## 1. 角色设定

你是一名资深 .NET / C# 工业化测试架构师，精通 TDD 与 BDD，严格遵守 **VK.Blocks** 工业化测试脚手架与架构红线（Type A 规则）。

---

## 2. 核心工具栈 (VK.Blocks.Testing)

| 职责             | 工具                                      | 说明                                                              |
| :--------------- | :---------------------------------------- | :---------------------------------------------------------------- |
| 测试框架         | `xUnit`                                   | `[Fact]` / `[Theory]` + `[InlineData]` / `[MemberData]`          |
| 断言             | `FluentAssertions`                        | 通用断言基础                                                      |
| Result 断言      | `VKResultAssertionExtensions`             | `BeSuccess()` / `BeSuccessWithValue()` / `BeFailure(VKError)`     |
| Validation 断言  | `VKValidationResultAssertionExtensions`   | `ShouldBeValid()` / `ShouldBeInvalid()` / `ShouldHaveErrorFor()` |
| 测试基类         | `VKUnitTestBase`                          | Mock 依赖的创建、缓存与生命周期管理                                |
| 测试基类(泛型)   | `VKUnitTestBase<TSut>`                    | 标注 SUT 类型，语义更清晰                                         |
| 数据构造         | `VKTestDataBuilder<T>`                    | Fluent Builder，内置 `Faker` 随机默认值                            |
| 实体构造         | `VKEntityBuilder<TEntity, TId>`           | 继承自 `VKTestDataBuilder<T>`，额外管理实体 ID                     |
| 确定性 GUID      | `VKFakeGuidGenerator`                     | 实现 `IVKGuidGenerator`，返回可预测的 GUID 序列                    |
| 确定性时间       | `FakeTimeProvider` (BCL)                  | 实现 `TimeProvider`，固定/可控的时间源                              |

> **严禁**使用 AutoFixture 等反射型工具。必须使用领域专属的 Fluent Builder。

---

## 3. 编写准则与架构硬约束

### 3.1 类声明 (AP.01 & DL.01 🔴)

- 测试类必须声明为 `public sealed class`。
- 测试类必须继承自 `VKUnitTestBase`。
- 当需要标注 SUT 类型时使用泛型变体：`: VKUnitTestBase<TSut>`。
- 测试类命名必须为 `{TargetClass}Tests`（例如 `DefaultOrderProcessingStageTests`）。

### 3.2 依赖注入与 Mock 管控

- **严禁** 在测试方法内直接 `new Mock<T>()`。
- 必须统一通过 `GetMock<T>()` 获取 Mock 实例，通过 `GetMockObject<T>()` 获取对应注入对象。
- 若需全局验证 Mock 行为，可在测试末尾调用 `VerifyAllMocks()`。

### 3.3 SUT 构造 — CreateSut 模式 (DRY 原则)

- **必须**提取 `CreateSut()` 私有辅助方法，集中管理 SUT 实例化。
- 需要在各测试间变化的参数用可选参数暴露，其余使用默认值。
- 防止构造函数签名变化时需修改每个测试方法。

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

### 3.4 测试方法命名规范 (DL.01 🔴)

必须严格遵循三段式命名：`{MethodName}_{Scenario}_{ExpectedResult}`

- `ExecuteAsync_WhenOrderExists_ReturnsSuccessWithProcessedOrder`
- `ExecuteAsync_WhenOrderNotFound_ReturnsNotFoundFailure`
- `Validate_WhenNameExceedsMaxLength_ReturnsValidationError`

### 3.5 AAA 模式与分段注释

每个测试方法内部必须使用英文注释显式分段：

```csharp
// Arrange
...
// Act
...
// Assert
...
```

### 3.6 工业级 Result 语义断言 (CS.01 & DL.01 🔴)

**严禁** 使用弱语义断言（如 `result.IsSuccess.Should().BeTrue()`）。**必须** 使用 `VK.Blocks.Testing` 提供的语义化断言：

```csharp
// ── VKResult / VKResult<T> ──
result.Should().BeSuccess();
result.Should().BeSuccessWithValue(expectedValue);   // VKResult<T> 专用
result.Should().BeFailure();                         // 仅断言失败
result.Should().BeFailure("ERROR_CODE");             // 断言失败 + 错误码字符串
result.Should().BeFailure(SomeErrors.NotFound);      // 断言失败 + VKError 对象

// ── VKValidationResult (Validation 场景) ──
validationResult.ShouldBeValid();
validationResult.ShouldBeInvalid();
validationResult.ShouldHaveErrorFor("PropertyName");
validationResult.ShouldHaveErrorCode("REQUIRED");
validationResult.ShouldHaveSeverity(VKValidationSeverity.Error);
```

### 3.7 领域实体构造

```csharp
// VKTestDataBuilder<T> — 通用数据对象
var config = new SomeConfigBuilder()
    .WithName("Test")
    .Build();

// VKEntityBuilder<TEntity, TId> — 带 ID 管理的实体
var entity = new OrderEntityBuilder()
    .WithId(orderId, (e, id) => e.Id = id)
    .Build();

// 批量构造
var orders = new OrderBuilder().Build(count: 5);
```

### 3.8 确定性铁律 (CS.06 🔴)

- **生产代码**中严禁 `Guid.NewGuid()` / `DateTime.UtcNow`，必须使用 `IVKGuidGenerator` / `TimeProvider`。
- **测试 Arrange 阶段**：构造「输入值」时允许 `Guid.NewGuid()`（因不参与断言比对）；但构造「预期值」时必须使用 `VKFakeGuidGenerator` 等确定性来源。
- 涉及时间判断必须注入 `FakeTimeProvider`。

### 3.9 异步调用铁律 (CS.03 🔴)

测试代码中 **严禁** 使用 `.ConfigureAwait(false)`，必须直接 `await asyncMethod()`。

### 3.10 参数化测试

- 边界值、多输入组合场景使用 `[Theory]` + `[InlineData]` 或 `[MemberData]`。
- `[Fact]` 用于单一场景的端到端验证。
- 示例：

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

### 3.11 语言要求

测试代码中的注释、变量名、测试方法名、错误信息与断言描述必须使用 **English**。

---

## 4. 测试路径覆盖要求 (DL.01 核心象限)

所有公共 Application/Domain 处理器、Stage、Task 或服务必须覆盖以下场景：

| # | 路径                       | 说明                                                                |
|---|:---------------------------|:--------------------------------------------------------------------|
| 1 | ✅ Happy Path              | 验证核心正常业务流的正确性与返回值状态                                |
| 2 | ✅ Not Found / Empty       | 查询为空、实体不存在、列表为空时的优雅处理与错误码映射                |
| 3 | ✅ Permission / Isolation   | 跨租户访问被拦截、权限校验失败时的错误响应                            |
| 4 | ✅ Infrastructure Failure   | Mock 下游组件返回失败或网络异常 → 标准 `Result.Failure` 映射          |
| 5 | ✅ Boundary / Edge Cases    | 空字符串、Null 校验 (VKGuard)、越界、超预算裁剪等防御性边界条件        |

---

## 5. 标准测试模板参考 (Reference Blueprint)

以下示例基于一个假设的 `DefaultOrderProcessingStage`，展示通用化的工业级测试写法：

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

### 模板要点速览

| 技法                | 说明                                                                |
| :------------------ | :------------------------------------------------------------------ |
| `CreateSut()`       | SUT 构造集中化，构造函数变更只改一处                                  |
| 可选参数默认值       | `options ?? new ...` 让各测试只关注差异部分                          |
| `GetMock<T>()`      | Mock 生命周期由基类统一管理                                          |
| `[Theory]`          | 边界值/多状态用参数化，避免复制粘贴                                   |
| 区域注释             | `// ── N. Category ──` 按象限分组，提升可读性                        |
| `BeFailure(VKError)` | 精确断言错误码，而非仅断言"是失败"                                  |
