## Step Overview

| Step | English                 | 中文            | 日本語                   |
| ---- | ----------------------- | --------------- | ------------------------ |
| 1    | Project Setup           | 项目设置        | プロジェクトセットアップ |
| 2    | Database Layer          | 数据库层        | データベース層           |
| 3    | Models                  | 模型            | モデル                   |
| 4    | Repository Layer        | 仓储层          | リポジトリ層             |
| 5    | Service Layer           | 服务层          | サービス層               |
| 6    | Controllers             | 控制器          | コントローラー           |
| 7    | Middleware & Extensions | 中间件和扩展    | ミドルウェアと拡張機能   |
| 8    | Constants               | 常量            | 定数                     |
| 9    | GraphQL (Optional)      | GraphQL（可选） | GraphQL（オプション）    |
| 10   | Program.cs              | Program.cs      | Program.cs               |

## Common Commands

```bash
dotnet restore
dotnet build
dotnet run

dotnet ef migrations add [MigrationName]
dotnet ef database update

dotnet test
```

## Notes

你现在的角色是Senior Principal Fullstack Engineer

---

创建一个新的 Cosmosdb的Repository

---

通过 MediatR 将 Controller 里的逻辑拆分

---

添加 AutoMapper 进行 Entity 和 DTO 之间的映射

---

为 DTOs 添加更多验证特性（FluentValidation）

---

加一个filter的中间件 用来处理异常

---

provide a brief summary of the architectural logic in Japanese for the project's documentation folder

---

我正在遵循 Clean Architecture 开发。请根据下方的 [Domain Entity] 生成对应的 [DTO 类] 以及 [映射代码]。

要求：

DTO 定义： 所有的属性应为只读（使用 init 关键字或 record）。

隐私处理： 自动忽略敏感字段（如 Password, InternalId）。

映射实现： 请提供一个扩展方法 ToDto() 或 AutoMapper 的 Profile 配置。

类型转换： 自动处理枚举到字符串的转换，以及日期格式化（ISO 8601）。

---

请以 Database Administrator (DBA) 的视角审核以下 EF Core / LINQ 查询。

诊断重点：

内存泄漏： 检查是否有过早调用 .ToList() 或 .ToArray() 导致在内存中过滤（Client-side evaluation）。

查询效率： 是否缺少必要的 .AsNoTracking()（针对只读查询）？

预加载： 是否存在 N+1 问题？建议哪些关联属性应该使用 .Include() 或 .ThenInclude()。

索引建议： 根据 Where 子句，推测数据库可能需要建立哪些复合索引。

---

Design Principles
Design Patterns
Architectural Principles
Architectural Styles
Architectural Patterns
Enterprise Patterns
