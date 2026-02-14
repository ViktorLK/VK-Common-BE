# 任务：生产级 Azure DevOps Pipeline 生成 (Enterprise YAML Pipeline)

# 角色设定

你是一名 Azure Solution Architect 和 Senior DevOps Engineer。你擅长利用 Azure DevOps 的特性（如 Environments, Variable Groups, Service Connections）构建安全且高效的 CI/CD 流水线。

# 核心环境

- 平台：Azure Pipelines (YAML)
- 目标框架：.NET 9 / .NET 8
- Pool：ubuntu-latest

# Pipeline 阶段要求 (Stages)

## 1. Build 阶段：

    - 包使用 DotNetCoreCLI@2 任务进行 Restore 和 Build。
    - 缓存优化：配置 Cache@2 任务，针对 NuGet 依赖包进行缓存。
    - 设置 projects 路径模式，支持多项目方案。

## 2. Test & Coverage 阶段 (Stage: Test)

    - 执行测试：执行 dotnet test，并集成 coverlet.collector。
    - 覆盖率收集：生成 cobertura 格式的 XML 结果。
    - 报告生成 (HTML)：使用 reportgenerator 任务（或 Script 安装并运行工具）将 XML 转换为 HTML 报告。
    - 使用 PublishCodeCoverageResults@1 任务，将结果发布到 Azure DevOps 的 Code Coverage 选项卡。
    - 发布测试结果：使用 PublishTestResults@2 确保测试详情出现在 Azure DevOps 的 Tests 选项卡。
    - 阈值检查：如果覆盖率低于 [80%]，则 Pipeline 运行失败。

## 3. Deploy 阶段 (Stage: Deploy)

    - 依赖性：此阶段必须在 Build/Test 成功后运行。
    - 环境绑定：使用 Azure DevOps 的 environment 特性，支持审批（Approvals）。
    - 部署目标：[请选择：App Service / Azure Kubernetes Service (AKS) / Docker Container]。
    - 变量管理：展示如何通过 variable groups 或 Library 安全引用机密。

# 进阶规范

    - Artifacts：将编译输出和 HTML 报告打包为 Pipeline Artifacts。
    - 命名空间：采用清晰的 Stage 和 Job 命名规范。
    - 错误处理：确保测试失败时 Pipeline 能够正确中断并标记为失败。

# 性能与安全优化

    - Caching：配置 NuGet 包缓存以加速构建。
    - Permissions：遵循最小权限原则配置 GITHUB_TOKEN。
    - Concurrency：配置并发限制，确保同一分支的新推送会取消旧的运行。

# 输出要求

    - 提供完整的 azure-pipelines.yml 代码。
    - 简要列出实现覆盖率报告所需的 NuGet 包（如 coverlet.collector）。
    - 说明如何在 Azure DevOps 界面查看生成的 HTML 覆盖率报告。

# 输入代码文件夹路径 (Input Code)

# 输出代码文件夹路径 (Output Code)
