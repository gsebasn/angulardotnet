# StudyShop Architecture Documentation

## Overview

StudyShop follows **Clean Architecture** principles combined with the **Vertical Slices** pattern, providing a maintainable, testable, and scalable codebase.

## Architecture Diagram

```mermaid
graph TB
    subgraph "Client Layer"
        BROWSER[Web Browser]
        ANGULAR[Angular 17 UI<br/>Port 4200]
    end
    
    subgraph "Presentation Layer - StudyShop.Api"
        API[ASP.NET Core Minimal API<br/>Port 5170]
        EP_PROD[ProductsEndpoints]
        EP_ORD[OrdersEndpoints]
        EP_AI[AiEndpoints]
        SWAGGER[Swagger UI<br/>/swagger]
    end
    
    subgraph "Application Layer - StudyShop.Application"
        MEDIATR[MediatR Pipeline]
        
        subgraph "Products Slice"
            PROD_CMD[CreateProductCommand<br/>UpdateProductCommand<br/>DeleteProductCommand]
            PROD_QRY[GetProductsQuery<br/>GetProductByIdQuery]
            PROD_HANDLER[CommandHandlers<br/>QueryHandlers]
            PROD_BEHAVIOR[CachingBehavior<br/>InvalidationBehavior]
        end
        
        subgraph "Orders Slice"
            ORD_CMD[CreateOrderCommand]
            ORD_QRY[GetOrdersQuery<br/>GetOrderByIdQuery]
            ORD_HANDLER[CommandHandlers<br/>QueryHandlers]
        end
        
        DTO[DTOs<br/>ProductDto<br/>OrderDto]
        VALIDATORS[Validators<br/>FluentValidation]
        EXCEPTIONS[Common Exceptions<br/>NotFoundException<br/>CommandValidationException]
        INTERFACES[IAppDbContext<br/>IVectorStore<br/>IEmbeddingService]
    end
    
    subgraph "Domain Layer - StudyShop.Domain"
        PRODUCT[Product Entity]
        ORDER[Order Entity]
        ORDERITEM[OrderItem Entity]
    end
    
    subgraph "Infrastructure Layer - StudyShop.Infrastructure"
        DBCONTEXT[StudyShopDbContext<br/>implements IAppDbContext]
        MIGRATIONS[EF Core Migrations]
        
        subgraph "AI/RAG Services"
            OLLAMA_CLIENT[OllamaClient<br/>HTTP Client]
            EMBEDDING[EmbeddingService]
            LLM[LlmService]
            VECTOR_STORE[PostgresVectorStore<br/>implements IVectorStore]
            INDEXER[ProductEmbeddingIndexer<br/>Background Service]
        end
        
        DI[DependencyInjection<br/>AddInfrastructure]
    end
    
    subgraph "Data Layer"
        SQLSERVER[(SQL Server<br/>Port 1433<br/>Products, Orders)]
        POSTGRES[(PostgreSQL + pgvector<br/>Port 5432<br/>Vector Embeddings)]
    end
    
    subgraph "External Services"
        OLLAMA_SVC[Ollama LLM Service<br/>Port 11434<br/>llama3.2:3b, bge-m3]
    end
    
    BROWSER -->|HTTP| ANGULAR
    ANGULAR -->|REST API| API
    
    API --> EP_PROD
    API --> EP_ORD
    API --> EP_AI
    API --> SWAGGER
    
    EP_PROD -->|MediatR| MEDIATR
    EP_ORD -->|MediatR| MEDIATR
    EP_AI -->|Direct DI| EMBEDDING
    
    MEDIATR --> PROD_CMD
    MEDIATR --> PROD_QRY
    MEDIATR --> ORD_CMD
    MEDIATR --> ORD_QRY
    
    PROD_CMD --> PROD_HANDLER
    PROD_QRY --> PROD_HANDLER
    ORD_CMD --> ORD_HANDLER
    ORD_QRY --> ORD_HANDLER
    
    PROD_HANDLER -->|Uses| DTO
    PROD_HANDLER -->|Uses| VALIDATORS
    ORD_HANDLER -->|Uses| DTO
    ORD_HANDLER -->|Uses| VALIDATORS
    
    PROD_HANDLER -->|IAppDbContext| DBCONTEXT
    ORD_HANDLER -->|IAppDbContext| DBCONTEXT
    
    PROD_BEHAVIOR -.->|Pipeline| MEDIATR
    
    DBCONTEXT -->|Maps to| PRODUCT
    DBCONTEXT -->|Maps to| ORDER
    DBCONTEXT -->|Maps to| ORDERITEM
    
    DBCONTEXT -->|Query/Write| SQLSERVER
    DBCONTEXT -->|Migrations| MIGRATIONS
    
    INDEXER -->|Embeds| EMBEDDING
    EMBEDDING -->|HTTP| OLLAMA_CLIENT
    OLLAMA_CLIENT -->|API Calls| OLLAMA_SVC
    
    INDEXER -->|Stores| VECTOR_STORE
    VECTOR_STORE -->|Vector Data| POSTGRES
    
    EP_AI -->|Queries| VECTOR_STORE
    EP_AI -->|Generates| LLM
    LLM -->|HTTP| OLLAMA_CLIENT
    
    API -->|Registers| DI
    DI -->|Configures| DBCONTEXT
    DI -->|Configures| OLLAMA_CLIENT
    DI -->|Configures| VECTOR_STORE
    DI -->|Configures| INDEXER
    
    style API fill:#42b983
    style EP_PROD fill:#42b983
    style EP_ORD fill:#42b983
    style EP_AI fill:#42b983
    style MEDIATR fill:#3498db
    style PROD_CMD fill:#3498db
    style PROD_QRY fill:#3498db
    style ORD_CMD fill:#3498db
    style ORD_QRY fill:#3498db
    style PRODUCT fill:#e74c3c
    style ORDER fill:#e74c3c
    style ORDERITEM fill:#e74c3c
    style DBCONTEXT fill:#f39c12
    style VECTOR_STORE fill:#9b59b6
    style OLLAMA_SVC fill:#9b59b6
    style SQLSERVER fill:#95a5a6
    style POSTGRES fill:#95a5a6
```

## Layer Details

### 1. Domain Layer (`StudyShop.Domain`)

**Purpose**: Pure business entities with no dependencies.

**Contents**:
- `Models/Product.cs` - Product entity
- `Models/Order.cs` - Order entity  
- `Models/OrderItem.cs` - Order item entity

**Characteristics**:
- ✅ No dependencies on other layers
- ✅ Pure C# classes (POCOs)
- ✅ Contains business logic only (if needed)
- ✅ No frameworks (no EF Core, no MediatR)

### 2. Application Layer (`StudyShop.Application`)

**Purpose**: Business logic, use cases, and application interfaces.

**Contents**:
- **Features/** (Vertical Slices):
  - `Products/` - All product-related use cases
  - `Orders/` - All order-related use cases
- **DTOs/** - Data Transfer Objects
- **Validators/** - FluentValidation rules
- **Common/** - Shared exceptions and utilities
- **Data/** - `IAppDbContext` interface
- **Ai/** - AI service interfaces (`IVectorStore`, `IEmbeddingService`, `ILlmService`)

**Characteristics**:
- ✅ Depends only on Domain
- ✅ Defines interfaces (not implementations)
- ✅ Contains use cases (Commands/Queries)
- ✅ Handlers implement business logic
- ✅ Validation rules
- ✅ Pipeline behaviors (caching, etc.)

### 3. Infrastructure Layer (`StudyShop.Infrastructure`)

**Purpose**: External dependencies and implementations.

**Contents**:
- **Data/**:
  - `StudyShopDbContext` - EF Core DbContext (implements `IAppDbContext`)
  - EF Core configurations
- **Ai/**:
  - `OllamaClient` - HTTP client for Ollama
  - `EmbeddingService` - Generates embeddings
  - `LlmService` - LLM chat generation
  - `PostgresVectorStore` - pgvector implementation
  - `ProductEmbeddingIndexer` - Background service
- `DependencyInjection.cs` - Service registration

**Characteristics**:
- ✅ Implements Application interfaces
- ✅ Depends on Application (for interfaces)
- ✅ Depends on Domain (for entities)
- ✅ Contains external service clients
- ✅ Database access
- ✅ AI/ML integrations

### 4. Presentation Layer (`StudyShop.Api`)

**Purpose**: API endpoints and HTTP concerns.

**Contents**:
- **Endpoints/**:
  - `ProductsEndpoints.cs` - Product API routes
  - `OrdersEndpoints.cs` - Order API routes
  - `AiEndpoints.cs` - AI/RAG endpoints
- `Program.cs` - Startup and configuration

**Characteristics**:
- ✅ Depends on Application (uses MediatR, DTOs)
- ✅ Minimal API endpoints
- ✅ Swagger/OpenAPI generation
- ✅ CORS configuration
- ✅ Error handling

## Vertical Slices Pattern

Each feature is organized as a complete "slice" containing:

```
Features/
  Products/
    Commands/
      CreateProductCommand.cs
      CreateProductCommandHandler.cs
      UpdateProductCommand.cs
      UpdateProductCommandHandler.cs
      DeleteProductCommand.cs
      DeleteProductCommandHandler.cs
    Queries/
      GetProductsQuery.cs
      GetProductsQueryHandler.cs
      GetProductByIdQuery.cs
      GetProductByIdQueryHandler.cs
    Behaviors/
      QueryCachingBehavior.cs
      CacheInvalidationBehavior.cs
```

**Benefits**:
- ✅ All related code in one place
- ✅ Easy to find and modify
- ✅ Minimal coupling between slices
- ✅ Easy to add new features

## Dependency Flow

```
┌─────────────────┐
│  Presentation   │  (StudyShop.Api)
│   (API Layer)   │
└────────┬────────┘
         │ depends on
         ▼
┌─────────────────┐
│  Application    │  (StudyShop.Application)
│  (Use Cases)    │
└────────┬────────┘
         │ depends on
         ▼
┌─────────────────┐
│     Domain      │  (StudyShop.Domain)
│   (Entities)    │
└─────────────────┘
         ▲
         │ implements
┌────────┴────────┐
│ Infrastructure  │  (StudyShop.Infrastructure)
│  (External)     │
└─────────────────┘
```

## Data Flow Example: Create Product

```
1. HTTP POST /api/products
   ↓
2. ProductsEndpoints.MapPost()
   ↓
3. MediatR.Send(new CreateProductCommand { ... })
   ↓
4. CreateProductCommandHandler.Handle()
   ├─→ Validator.ValidateAsync()
   ├─→ new Product { ... }
   ├─→ IAppDbContext.Products.Add(product)
   └─→ IAppDbContext.SaveChangesAsync()
   ↓
5. StudyShopDbContext.SaveChangesAsync()
   ↓
6. SQL Server INSERT
   ↓
7. CacheInvalidationBehavior (clears cache)
   ↓
8. Returns ProductDto
```

## AI/RAG Flow: Semantic Search

```
1. HTTP POST /api/ai/search?q="laptops under $1000"
   ↓
2. AiEndpoints
   ├─→ IEmbeddingService.CreateEmbedding("laptops under $1000")
   │   └─→ OllamaClient.EmbedAsync()
   │       └─→ HTTP POST to Ollama /api/embeddings
   │           └─→ Returns float[1536] vector
   │
   └─→ IVectorStore.QueryProductsAsync(vector, topK: 5)
       └─→ PostgresVectorStore
           └─→ PostgreSQL query with pgvector cosine similarity
               └─→ Returns top 5 matching products
   ↓
3. Returns JSON with product matches and scores
```

## Testing Strategy

### Unit Tests (`StudyShop.Api.Tests/Unit/`)

Test handlers in isolation:
- Mock `IAppDbContext`
- Test business logic
- Verify validation

### Integration Tests (`StudyShop.Api.Tests/Integration/`)

Test API endpoints end-to-end:
- Use `TestApiFactory` with InMemory database
- Test HTTP requests/responses
- Verify complete flow

## Key Design Decisions

1. **Clean Architecture**: Separation of concerns, testability
2. **Vertical Slices**: Feature organization over technical layers
3. **CQRS**: Separate read/write models for scalability
4. **Interface-Based Design**: Application defines contracts, Infrastructure implements
5. **Minimal API**: Modern .NET 9 endpoint registration
6. **Local AI**: Ollama for embeddings, pgvector for storage

## Future Enhancements

- [ ] GraphQL endpoint
- [ ] Neo4j integration for graph queries
- [ ] Redis for distributed caching
- [ ] Event sourcing for audit trail
- [ ] Microservices extraction (if needed)
- [ ] OpenTelemetry for observability

