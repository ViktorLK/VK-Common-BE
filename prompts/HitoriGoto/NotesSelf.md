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

Design Principles
Design Patterns
Architectural Principles
Architectural Styles
Architectural Patterns
Enterprise Patterns
