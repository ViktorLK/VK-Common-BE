# Code Generation Prompts

This folder contains step-by-step prompts for generating ASP.NET Core Web API code similar to this project.

本文件夹包含用于生成类似于此项目的 ASP.NET Core Web API 代码的分步提示词。

このフォルダには、このプロジェクトに類似した ASP.NET Core Web API コードを生成するための段階的なプロンプトが含まれています。

## Architecture

```
┌─────────────────────────────────────┐
│         Controllers (API)           │
│  - ProductsController               │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Service Layer               │
│  - IProductService                  │
│  - ProductService                   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│       Repository Layer              │
│  - IProductRepository               │
│  - ProductRepository                │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Data Layer                  │
│  - ApplicationDbContext             │
│  - Models (Product, etc.)           │
└─────────────────────────────────────┘

         Middleware Pipeline
┌─────────────────────────────────────┐
│  - Authentication (Azure B2C/API)   │
│  - CORS                             │
│  - Swagger                          │
│  - Health Checks                    │
│  - Application Insights             │
└─────────────────────────────────────┘
```

## Step Overview / 步骤概览 / ステップ概要

| Step | English | 中文 | 日本語 | 
|------|---------|------|--------|
| 1 | Project Setup | 项目设置 | プロジェクトセットアップ | 
| 2 | Database Layer | 数据库层 | データベース層 | 
| 3 | Models | 模型 | モデル | 
| 4 | Repository Layer | 仓储层 | リポジトリ層 | 
| 5 | Service Layer | 服务层 | サービス層 | 
| 6 | Controllers | 控制器 | コントローラー | 
| 7 | Middleware & Extensions | 中间件和扩展 | ミドルウェアと拡張機能 | 
| 8 | Constants | 常量 | 定数 | 
| 9 | GraphQL (Optional) | GraphQL（可选） | GraphQL（オプション） | 
| 10 | Program.cs | Program.cs | Program.cs | 

## Common Commands

```bash
dotnet restore
dotnet build
dotnet run

dotnet ef migrations add [MigrationName]
dotnet ef database update

dotnet test
```