# System Overview (システム全景)

本ドキュメントでは、`VK.Blocks.Persistence.EFCore` モジュールを中心としたシステムの全体像と、各コンポーネントの責務について記述します。

## System Context Diagram (システムコンテキスト図)

```mermaid
graph TD
    User([User / API Client]) --> API[API Layer]
    API --> Application[Application Layer]
    Application --> Domain[Domain Layer]
    Application --> Infrastructure[Infrastructure Layer]

    subgraph Infrastructure
        EFCore[VK.Blocks.Persistence.EFCore]
        EFCore --> |Uses| DbContext
        EFCore --> |Uses| Interceptors
        EFCore --> |Uses| Caches[Metadata Caches]
        EFCore --> |Uses| Lifecycle[Entity Lifecycle Processor]
    end

    Infrastructure --> Database[(SQL Database)]
```

## Component Architecture (コンポーネントアーキテクチャ)

`VK.Blocks.Persistence.EFCore` 内部の主要コンポーネントとその関係性を示します。

```mermaid
classDiagram
    class IBaseRepository~T~ {
        <<interface>>
        +AddAsync()
        +UpdateAsync()
        +DeleteAsync()
    }

    class IReadRepository~T~ {
        <<interface>>
        +GetListAsync()
        +GetFirstOrDefaultAsync()
    }

    class EfCoreRepository~T~ {
        -DbContext _context
        -IEntityLifecycleProcessor _processor
        +ExecuteUpdateAsync()
        +ExecuteDeleteAsync()
    }

    class UnitOfWork {
        +SaveChangesAsync()
        +BeginTransactionAsync()
    }

    class AuditingInterceptor {
        +SavingChanges()
    }

    class EntityLifecycleProcessor {
        +ProcessAuditing()
        +ProcessSoftDelete()
        +ProcessBulkUpdate()
    }

    IReadRepository <|-- EfCoreRepository
    IBaseRepository <|-- EfCoreRepository
    EfCoreRepository --> EntityLifecycleProcessor : Uses (Bulk Ops)
    AuditingInterceptor --> EntityLifecycleProcessor : Uses (Save Changes)
    UnitOfWork --> DbContext : Manages
```

## モジュールの役割

1.  **Repository Layer**: ドメインオブジェクトのコレクションとして振る舞い、データの永続化と再構築を担当します。
2.  **Unit of Work**: ビジネス・トランザクションの境界を定義し、複数のリポジトリ操作を原子的にコミットします。
3.  **Cross-Cutting Concerns**: 監査ログや論理削除といったインフラストラクチャの関心事を、ビジネスロジックから分離して処理します。
