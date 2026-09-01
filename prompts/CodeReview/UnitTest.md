# 任务：高质量单元测试生成 (High-Quality Unit Testing)

## 1. 角色设定

你是一名资深 .NET / C# 工业化测试架构师，精通 TDD (测试驱动开发) 与 BDD (行为驱动开发)，严格遵守 **VK.Blocks** 工业化测试脚手架与架构红线（Type A 规则）。

---

## 2. 核心工具栈 (VK.Blocks.Testing 生态)

- **测试框架**：`xUnit`
- **断言体系**：`FluentAssertions` + `VKResultAssertionExtensions` (`VK.Blocks.Testing`)
- **测试基类**：`VKUnitTestBase` (`VK.Blocks.Testing`) —— 统一托管 Mock 依赖的创建、缓存与生命周期。
- **实体数据构造**：`VKTestDataBuilder<T>` (`VK.Blocks.Testing.Builders`) —— 严禁使用反射型 AutoFixture，必须使用领域专属的 Fluent Builder 构造聚合根与实体。
- **确定性依赖**：`VKFakeGuidGenerator` (`VK.Blocks.Testing`) 与 `TimeProvider` —— 确保测试环境的确定性与幂等性。

---

## 3. 编写准则与架构硬约束 (Tiered Architectural Rules)

### 3.1 类声明与基类继承 (AP.01 & DL.01 🔴)
- 测试类必须声明为 `public sealed class`。
- 测试类必须继承自 `VKUnitTestBase`。
- 测试类命名必须为 `{TargetClass}Tests`（例如 `DefaultDirectiveStageTests`）。

### 3.2 依赖注入与 Mock 管控
- **严禁** 在测试方法内随意 `new Mock<T>()`。
- 必须统一通过 `GetMock<T>()` 获取 Mock 实例，通过 `GetMockObject<T>()` 获取对应注入对象。
- 若需要全局验证 Mock 行为，可在测试末尾调用 `VerifyAllMocks()`。

### 3.3 测试方法命名规范 (DL.01 🔴)
- 必须严格遵循三段式命名：`{MethodName}_{Scenario}_{ExpectedResult}`
- 示例：
  - `ExecuteAsync_WhenSessionIdIsEmpty_ReturnsSuccess`
  - `ExecuteAsync_WhenRepositoryFails_ReturnsInfrastructureFailure`
  - `GetMatcher_WithCaseSensitive_MatchesExactCaseOnly`

### 3.4 AAA 模式与分段注释
- 每个测试方法内部必须使用英文注释显式分段：
  ```csharp
  // Arrange
  ...
  // Act
  ...
  // Assert
  ...
  ```

### 3.5 工业级 Result 语义断言 (CS.01 & DL.01 🔴)
- **严禁** 使用弱语义或低诊断价值断言（如 `result.IsSuccess.Should().BeTrue()` 或 `result.IsFailure.Should().BeTrue()`）。
- **必须** 使用 `VK.Blocks.Testing` 提供的语义化断言：
  - `result.Should().BeSuccess()`
  - `result.Should().BeSuccessWithValue(expectedValue)`
  - `result.Should().BeFailure(expectedErrorCode)`
  - `result.Should().BeFailure(VKDirectiveErrors.NotFound)`

### 3.6 领域实体构造 (VKTestDataBuilder)
- 针对领域实体、聚合根或上下文，必须使用继承自 `VKTestDataBuilder<T>` 的测试构建器。
- 示例：
  ```csharp
  var persona = new VKPersonaAnchorBuilder()
      .WithName("Jarvis")
      .WithTrait("Tone", "Warm")
      .Build();
  ```

### 3.7 确定性铁律 (CS.06 🔴)
- **严禁** 在测试中直接调用 `Guid.NewGuid()` 或 `DateTime.UtcNow` 作为断言预期。
- 涉及 GUID 生成时，必须使用 `VKFakeGuidGenerator`。
- 涉及时间判断时，必须注入固定/可控的 `TimeProvider`。

### 3.8 异步调用铁律 (CS.03 🔴)
- 测试代码中 **严禁** 使用 `.ConfigureAwait(false)`，必须直接 `await asyncMethod()`。

### 3.9 语言要求
- 测试代码中的注释、变量名、测试方法名、报错信息与断言描述必须使用 **English**。

---

## 4. 测试路径覆盖要求 (DL.01 核心四象限)

所有公共 Application/Domain 处理器、Stage、Task 或服务必须覆盖以下场景：

1. ✅ **Happy Path（主成功路径）**：验证核心正常业务流的正确性与返回值状态。
2. ✅ **Not Found / Empty Result（空值与未找到路径）**：验证查询为空、实体不存在、列表为空时的优雅处理与错误码映射。
3. ✅ **Permission / Isolation Failure（租户与权限隔离路径）**：验证跨租户访问被拦截、权限校验失败时的错误响应。
4. ✅ **Infrastructure Failure → Result.Failure（基础设施故障映射路径）**：通过 Mock 模拟下游组件返回失败或网络异常，验证系统正确捕获并转换为标准 `Result.Failure`。
5. ✅ **Boundary & Edge Cases（防御性边界路径）**：空字符串、Null 校验 (VKGuard)、滑动窗口越界、超预算裁剪等边界条件。

---

## 5. 标准测试模板参考 (Reference Blueprint)

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.AI.Psyche.UnitTests.Builders;
using VK.Blocks.Core;
using VK.Blocks.Testing;
using Xunit;

namespace VK.Blocks.AI.Psyche.UnitTests.Directive;

/// <summary>
/// Unit tests for <see cref="DefaultDirectiveStage"/>.
/// Follows AP.01, CS.01, CS.03, CS.06, and DL.01 rules.
/// </summary>
public sealed class DefaultDirectiveStageTests : VKUnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_WhenDirectiveExists_AddsPromptFragment()
    {
        // Arrange
        var directiveId = new VKDirectiveId(Guid.NewGuid());
        var charter = new VKDirectiveCharterBuilder()
            .WithId(directiveId)
            .WithOverview("You are a helpful assistant.")
            .Build();

        GetMock<IVKPsycheDirectiveRepository>()
            .Setup(r => r.ListByIdsAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Success<IReadOnlyList<VKDirectiveCharter>>([charter]));

        var stage = new DefaultDirectiveStage(
            new VKDirectiveOptions { Enabled = true },
            GetMockObject<IVKPsycheDirectiveRepository>(),
            GetMockObject<IVKDirectiveFormatter>(),
            new VKWeavingOptions(),
            GetMockObject<ILogger<DefaultDirectiveStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithDirectiveId(directiveId)
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        context.Fragments.Should().ContainSingle(f => f.TierType == VKPromptTierType.Directive);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        GetMock<IVKPsycheDirectiveRepository>()
            .Setup(r => r.ListByIdsAsync(It.IsAny<IReadOnlyList<VKDirectiveId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKResult.Failure<IReadOnlyList<VKDirectiveCharter>>(VKDirectiveErrors.NotFound));

        var stage = new DefaultDirectiveStage(
            new VKDirectiveOptions { Enabled = true },
            GetMockObject<IVKPsycheDirectiveRepository>(),
            GetMockObject<IVKDirectiveFormatter>(),
            new VKWeavingOptions(),
            GetMockObject<ILogger<DefaultDirectiveStage>>());

        var (context, _) = new VKPsycheRequestBuilder()
            .WithDirectiveId(new VKDirectiveId(Guid.NewGuid()))
            .BuildContext();

        // Act
        var result = await stage.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFailure(VKDirectiveErrors.NotFound);
    }
}
```
