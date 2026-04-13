# ADR 001: Expanding Global ErrorType for Enterprise-Grade Error Handling

- **Date**: 2026-03-26
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: VK.Blocks.Core / Error System Refactoring

## 2. Context (背景)

目前系统的 `ErrorType` 仅涵盖了基础的 `Failure`, `Validation`, `NotFound`, `Conflict`, Unauthorized`, `Forbidden`。这些基本类型能够满足 CRUD 类应用的需求，但在处理分布式系统、第三方集成以及生产环境的可观测性时，语义粒度严重不足。

具体场景包括：
1. **限流 (Rate Limiting)**: API 密钥超限时，通常需要返回 HTTP 429。
2. **基础设施不可用 (Service Unavailable)**: 当 Redis 或数据库瞬时过载时，需要返回 HTTP 503 以触发前端的退避机制。
3. **第三方服务故障 (External Error)**: 调用 Google OAuth 等服务失败时，需要区分是 internal bug (500) 还是外部依赖 (502/504) 问题。

## 3. Problem Statement (問題定義)

1. **HTTP 映射模糊**：限流等错误目前只能映射到 `Failure (500)` 或 `Validation (400)`，这违反了 RESTful 最佳实践，也不利于客户端精确处理。
2. **可观测性缺失**：在监控面板中，我们无法根据 `ErrorType` 直接统计出由于第三方服务导致的问题比例。
3. **开发者体验 (DX)**：开发者在定义业务报错时，往往需要手动硬编码状态码或在错误码字符串中隐含状态，增加了维护成本。

## 4. Decision (決定事項)

在 `VK.Blocks.Core` 中扩展 `ErrorType` 枚举，新增以下类型：

1. **`TooManyRequests` (HTTP 429)**: 专门用于 API 密钥限流逻辑。
2. **`ServiceUnavailable` (HTTP 503)**: 用于基础设施级或系统临时过载。
3. **`Timeout` (HTTP 504/408)**: 专门用于后端依赖处理超时。
4. **`ExternalError` (HTTP 502/504)**: 用于外部集成（如 OAuth, Blob Storage API）返回的异常。

同时：
- 更新 `VK.Blocks.ExceptionHandling` 的 `ProblemDetailsFactory`，使其支持新状态码对应的标准标题。
- 更新 `VK.Blocks.Web` 的 `ErrorTypeExtensions.ToStatusCode()` 方法，完成 1:1 的映射逻辑。

## 5. Alternatives Considered (代替案の検討)

### Option 1: 在 Error record 中直接定义 StatusCode
- **Approach**: 给 `Error` 增加一个 `int StatusCode` 属性。
- **Rejected Reason**: 这会将 Infrastructure 层（HTTP）的概念泄露到 Core/Domain 层。`ErrorType` 应该是语义化的，至于它映射到 HTTP 还是 gRPC 状态码，应该是表示层决定的。

### Option 2: 保持 ErrorType 不变，在错误 Code 中解析
- **Approach**: 通过前缀识别，如 `ApiKey.TooManyRequests` -> 429。
- **Rejected Reason**: 逻辑过于散乱，且对 `VKApiController` 的自动化映射不友好，增加了反射或字符串匹配的性能开销。

## 6. Consequences & Mitigation (結果と緩和策)

- **Positive**: 极大地提升了系统的生产就绪度 (Production Ready)。API 报错语义达到了国际工业级标准。
- **Negative**: 增加了一点枚举定义的复杂性。
- **Mitigation**: 在 `Error.cs` 中添加了详细的 XML 注释，并在 `VK.Blocks.Web` 中提供了统一转换扩展，屏蔽底层细节。

## 7. Implementation & Security (実装詳細とセキュリティ考察)

### Implementation Note
```csharp
public enum ErrorType
{
    // ... 原有项
    TooManyRequests = 6,     // 429
    ServiceUnavailable = 7,  // 503
    Timeout = 8,             // 504/408
    ExternalError = 9        // 502/504
}
```

### Security Observation
细化的错误类型有助于安全团队在监控中识别由于暴力破解（由 429 统计反映）或外部攻击导致的系统异常。

---
**Last Updated**: 2026-03-26  
**Status**: ✅ Accepted
