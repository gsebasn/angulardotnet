# StudyShop System Architecture Diagrams

## Complete System Overview

```mermaid
graph TB
    subgraph "Client Tier"
        USER[User]
        BROWSER[Web Browser]
    end
    
    subgraph "Presentation Tier"
        ANGULAR[Angular 17 UI<br/>Port 4200<br/>Material UI]
    end
    
    subgraph "API Tier"
        API[ASP.NET Core 9<br/>Minimal API<br/>Port 5170]
        SWAGGER[Swagger UI<br/>/swagger]
    end
    
    subgraph "Application Tier - StudyShop.Application"
        MEDIATR[MediatR Pipeline]
        
        subgraph "Products Vertical Slice"
            PROD_CMD[Commands<br/>Create/Update/Delete]
            PROD_QRY[Queries<br/>Get/List]
            PROD_HANDLER[Handlers]
            PROD_BEHAVIOR[Caching Behaviors]
        end
        
        subgraph "Orders Vertical Slice"
            ORD_CMD[Commands<br/>Create]
            ORD_QRY[Queries<br/>Get/List]
            ORD_HANDLER[Handlers]
        end
        
        subgraph "AI Services"
            AI_INTERFACE[IVectorStore<br/>IEmbeddingService<br/>ILlmService]
        end
        
        DTO[DTOs]
        VALIDATORS[FluentValidation]
    end
    
    subgraph "Domain Tier - StudyShop.Domain"
        PRODUCT[Product Entity]
        ORDER[Order Entity]
        ORDERITEM[OrderItem Entity]
    end
    
    subgraph "Infrastructure Tier - StudyShop.Infrastructure"
        EF[EF Core<br/>StudyShopDbContext]
        
        subgraph "AI Implementation"
            OLLAMA_CLIENT[OllamaClient]
            EMBEDDING_SVC[EmbeddingService]
            LLM_SVC[LlmService]
            VECTOR_STORE[PostgresVectorStore]
            INDEXER[ProductEmbeddingIndexer<br/>Background Service]
        end
    end
    
    subgraph "Data Tier"
        SQLSERVER[(SQL Server<br/>Port 1433<br/>Products & Orders)]
        POSTGRES[(PostgreSQL<br/>Port 5432<br/>pgvector<br/>Embeddings)]
    end
    
    subgraph "External Services"
        OLLAMA[Ollama LLM<br/>Port 11434<br/>Models:<br/>llama3.2:3b<br/>bge-m3]
    end
    
    USER --> BROWSER
    BROWSER --> ANGULAR
    ANGULAR -->|REST API<br/>HTTP/JSON| API
    API --> SWAGGER
    
    API -->|Routes| MEDIATR
    MEDIATR --> PROD_CMD
    MEDIATR --> PROD_QRY
    MEDIATR --> ORD_CMD
    MEDIATR --> ORD_QRY
    
    PROD_CMD --> PROD_HANDLER
    PROD_QRY --> PROD_HANDLER
    ORD_CMD --> ORD_HANDLER
    ORD_QRY --> ORD_HANDLER
    
    PROD_HANDLER --> DTO
    PROD_HANDLER --> VALIDATORS
    PROD_HANDLER -->|IAppDbContext| EF
    
    ORD_HANDLER --> DTO
    ORD_HANDLER --> VALIDATORS
    ORD_HANDLER -->|IAppDbContext| EF
    
    PROD_BEHAVIOR -.->|Pipeline| MEDIATR
    
    API -->|Direct| AI_INTERFACE
    AI_INTERFACE -->|Implemented by| EMBEDDING_SVC
    AI_INTERFACE -->|Implemented by| VECTOR_STORE
    
    EMBEDDING_SVC --> OLLAMA_CLIENT
    LLM_SVC --> OLLAMA_CLIENT
    OLLAMA_CLIENT -->|HTTP| OLLAMA
    
    INDEXER --> EMBEDDING_SVC
    INDEXER --> VECTOR_STORE
    VECTOR_STORE --> POSTGRES
    
    EF -->|Maps| PRODUCT
    EF -->|Maps| ORDER
    EF -->|Maps| ORDERITEM
    EF -->|CRUD| SQLSERVER
    
    style ANGULAR fill:#42b983
    style API fill:#42b983
    style MEDIATR fill:#3498db
    style PROD_CMD fill:#3498db
    style PROD_QRY fill:#3498db
    style ORD_CMD fill:#3498db
    style ORD_QRY fill:#3498db
    style PRODUCT fill:#e74c3c
    style ORDER fill:#e74c3c
    style ORDERITEM fill:#e74c3c
    style EF fill:#f39c12
    style OLLAMA fill:#9b59b6
    style VECTOR_STORE fill:#9b59b6
    style SQLSERVER fill:#95a5a6
    style POSTGRES fill:#95a5a6
```

## Clean Architecture Layers

```mermaid
graph TB
    subgraph "Presentation Layer<br/>StudyShop.Api"
        API[Minimal API Endpoints]
        EP[Endpoint Mappers]
    end
    
    subgraph "Application Layer<br/>StudyShop.Application"
        FEATURES[Vertical Slices]
        INTERFACES[Application Interfaces<br/>IAppDbContext<br/>IVectorStore]
        DTO[DTOs]
        VAL[Validators]
    end
    
    subgraph "Domain Layer<br/>StudyShop.Domain"
        ENTITIES[Business Entities<br/>Product<br/>Order<br/>OrderItem]
    end
    
    subgraph "Infrastructure Layer<br/>StudyShop.Infrastructure"
        IMPL[Implementations<br/>StudyShopDbContext<br/>PostgresVectorStore]
        EXTERNAL[External Services<br/>EF Core<br/>Ollama Client]
    end
    
    API --> EP
    EP -->|Uses| FEATURES
    FEATURES -->|Uses| INTERFACES
    FEATURES -->|Uses| DTO
    FEATURES -->|Uses| VAL
    FEATURES -->|Depends on| ENTITIES
    IMPL -->|Implements| INTERFACES
    IMPL -->|Uses| ENTITIES
    IMPL -->|Uses| EXTERNAL
    
    style API fill:#42b983
    style FEATURES fill:#3498db
    style ENTITIES fill:#e74c3c
    style IMPL fill:#f39c12
```

## Vertical Slices Pattern

```mermaid
graph LR
    subgraph "Products Slice"
        PROD_CMD[Commands]
        PROD_QRY[Queries]
        PROD_HANDLER[Handlers]
        PROD_DTO[ProductDto]
        PROD_VAL[Validators]
        PROD_BEHAVIOR[Behaviors]
    end
    
    subgraph "Orders Slice"
        ORD_CMD[Commands]
        ORD_QRY[Queries]
        ORD_HANDLER[Handlers]
        ORD_DTO[OrderDto]
        ORD_VAL[Validators]
    end
    
    subgraph "Shared Application"
        SHARED[Common<br/>Exceptions<br/>Interfaces]
    end
    
    PROD_CMD --> PROD_HANDLER
    PROD_QRY --> PROD_HANDLER
    PROD_HANDLER --> PROD_DTO
    PROD_HANDLER --> PROD_VAL
    PROD_BEHAVIOR -.-> PROD_HANDLER
    
    ORD_CMD --> ORD_HANDLER
    ORD_QRY --> ORD_HANDLER
    ORD_HANDLER --> ORD_DTO
    ORD_HANDLER --> ORD_VAL
    
    PROD_HANDLER --> SHARED
    ORD_HANDLER --> SHARED
    
    style PROD_CMD fill:#3498db
    style PROD_QRY fill:#3498db
    style PROD_HANDLER fill:#3498db
    style ORD_CMD fill:#3498db
    style ORD_QRY fill:#3498db
    style ORD_HANDLER fill:#3498db
    style SHARED fill:#95a5a6
```

## CQRS Flow

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant MediatR
    participant Handler
    participant DbContext
    participant Database
    
    Note over Client,Database: Write Operation (Command)
    Client->>API: POST /api/products
    API->>MediatR: Send(CreateProductCommand)
    MediatR->>Handler: CreateProductCommandHandler
    Handler->>Handler: Validate
    Handler->>DbContext: Products.Add(product)
    Handler->>DbContext: SaveChangesAsync()
    DbContext->>Database: INSERT INTO Products
    Database-->>DbContext: Success
    DbContext-->>Handler: Product created
    Handler-->>MediatR: ProductDto
    MediatR-->>API: ProductDto
    API-->>Client: 201 Created
    
    Note over Client,Database: Read Operation (Query)
    Client->>API: GET /api/products
    API->>MediatR: Send(GetProductsQuery)
    MediatR->>Handler: GetProductsQueryHandler
    Handler->>DbContext: Products.AsQueryable()
    DbContext->>Database: SELECT * FROM Products
    Database-->>DbContext: Results
    DbContext-->>Handler: IQueryable<Product>
    Handler->>Handler: Map to DTOs
    Handler-->>MediatR: IEnumerable<ProductDto>
    MediatR-->>API: ProductDto[]
    API-->>Client: 200 OK
```

## AI/RAG Flow

```mermaid
sequenceDiagram
    participant User
    participant UI
    participant API
    participant EmbeddingService
    participant Ollama
    participant VectorStore
    participant PostgreSQL
    participant LLMService
    
    Note over User,LLMService: Semantic Search Flow
    User->>UI: Search "laptops under $1000"
    UI->>API: POST /api/ai/search?q=...
    API->>EmbeddingService: CreateEmbedding(query)
    EmbeddingService->>Ollama: POST /api/embeddings (bge-m3)
    Ollama-->>EmbeddingService: float[1536] vector
    EmbeddingService-->>API: Vector
    API->>VectorStore: QueryProductsAsync(vector, topK:5)
    VectorStore->>PostgreSQL: SELECT ... ORDER BY embedding <=> vector
    PostgreSQL-->>VectorStore: Top 5 matches with scores
    VectorStore-->>API: Results
    API-->>UI: JSON { results: [...] }
    UI-->>User: Display matches
    
    Note over User,LLMService: RAG Q&A Flow
    User->>UI: Ask "What products support USB-C?"
    UI->>API: POST /api/ai/answer { question: "..." }
    API->>EmbeddingService: CreateEmbedding(question)
    EmbeddingService->>Ollama: POST /api/embeddings
    Ollama-->>EmbeddingService: Vector
    EmbeddingService-->>API: Vector
    API->>VectorStore: QueryProductsAsync(vector, topK:5)
    VectorStore->>PostgreSQL: Vector search
    PostgreSQL-->>VectorStore: Context documents
    VectorStore-->>API: Context
    API->>LLMService: Generate(prompt + context)
    LLMService->>Ollama: POST /api/generate (llama3.2:3b)
    Ollama-->>LLMService: Streamed response
    LLMService-->>API: Answer
    API-->>UI: JSON { answer: "...", citations: [...] }
    UI-->>User: Display answer
```

## Data Flow: Create Product

```mermaid
graph TB
    START[HTTP POST /api/products] --> VALIDATE{Validate Request}
    VALIDATE -->|Invalid| ERROR1[400 Bad Request]
    VALIDATE -->|Valid| MEDIATR[MediatR.Send CreateProductCommand]
    
    MEDIATR --> HANDLER[CreateProductCommandHandler]
    HANDLER --> FLUENT{FluentValidation}
    FLUENT -->|Fails| EXCEPTION[CommandValidationException]
    FLUENT -->|Passes| CREATE[Create Product Entity]
    
    CREATE --> ADD[DbContext.Products.Add]
    ADD --> SAVE[DbContext.SaveChangesAsync]
    SAVE --> SQL[(SQL Server INSERT)]
    SQL --> CACHE[CacheInvalidationBehavior]
    CACHE --> CLEAR[Clear Product Cache]
    CLEAR --> DTO[Map to ProductDto]
    DTO --> RESPONSE[201 Created Response]
    
    EXCEPTION --> ERROR2[400 Bad Request]
    
    style START fill:#42b983
    style MEDIATR fill:#3498db
    style HANDLER fill:#3498db
    style SQL fill:#95a5a6
    style RESPONSE fill:#42b983
    style ERROR1 fill:#e74c3c
    style ERROR2 fill:#e74c3c
```

## Dependency Injection Container

```mermaid
graph TB
    subgraph "Startup - Program.cs"
        START[WebApplication.CreateBuilder]
        START --> DI1[AddMediatR<br/>Application + API assemblies]
        START --> DI2[AddInfrastructure<br/>Configuration]
        START --> DI3[AddValidators<br/>Application assembly]
        START --> DI4[AddMemoryCache]
        START --> DI5[AddCors]
        START --> DI6[AddSwaggerGen]
    end
    
    subgraph "Infrastructure Registration"
        DI2 --> INFRA[AddInfrastructure Extension]
        INFRA --> EF[AddDbContext<br/>StudyShopDbContext]
        INFRA --> AI1[AddHttpClient<br/>OllamaClient]
        INFRA --> AI2[AddSingleton<br/>IVectorStore]
        INFRA --> AI3[AddSingleton<br/>IEmbeddingService]
        INFRA --> AI4[AddSingleton<br/>ILlmService]
        INFRA --> AI5[AddHostedService<br/>ProductEmbeddingIndexer]
        INFRA --> CFG[Configure<br/>AiOptions, VectorStoreOptions]
    end
    
    subgraph "Registered Services"
        SVC1[MediatR<br/>Commands/Queries/Handlers]
        SVC2[IAppDbContext<br/>→ StudyShopDbContext]
        SVC3[IOllamaClient<br/>→ OllamaClient]
        SVC4[IVectorStore<br/>→ PostgresVectorStore]
        SVC5[IEmbeddingService<br/>→ EmbeddingService]
        SVC6[ILlmService<br/>→ LlmService]
        SVC7[IValidator<br/>→ FluentValidation Validators]
        SVC8[IMemoryCache<br/>→ MemoryCache]
    end
    
    DI1 --> SVC1
    EF --> SVC2
    AI1 --> SVC3
    AI2 --> SVC4
    AI3 --> SVC5
    AI4 --> SVC6
    DI3 --> SVC7
    DI4 --> SVC8
    
    style START fill:#42b983
    style INFRA fill:#f39c12
    style SVC1 fill:#3498db
    style SVC2 fill:#3498db
    style SVC3 fill:#9b59b6
    style SVC4 fill:#9b59b6
    style SVC5 fill:#9b59b6
    style SVC6 fill:#9b59b6
```

## Docker Services Architecture

```mermaid
graph TB
    subgraph "Docker Network: studyshop-network"
        subgraph "Backend Services"
            API_CONTAINER[studyshop-api<br/>.NET 9<br/>Port 5170]
        end
        
        subgraph "Frontend Services"
            UI_CONTAINER[studyshop-ui<br/>Angular + Nginx<br/>Port 4200]
        end
        
        subgraph "Database Services"
            SQL_CONTAINER[studyshop-sqlserver<br/>SQL Server 2022<br/>Port 1433]
            PG_CONTAINER[studyshop-postgres<br/>PostgreSQL + pgvector<br/>Port 5432]
        end
        
        subgraph "AI Services"
            OLLAMA_CONTAINER[studyshop-ollama<br/>Ollama LLM<br/>Port 11434]
        end
    end
    
    subgraph "Host Machine"
        USER2[Developer]
        BROWSER2[Browser]
    end
    
    USER2 --> BROWSER2
    BROWSER2 -->|http://localhost:4200| UI_CONTAINER
    BROWSER2 -->|http://localhost:5170| API_CONTAINER
    BROWSER2 -->|http://localhost:5170/swagger| API_CONTAINER
    
    UI_CONTAINER -->|HTTP| API_CONTAINER
    API_CONTAINER -->|EF Core| SQL_CONTAINER
    API_CONTAINER -->|Npgsql| PG_CONTAINER
    API_CONTAINER -->|HTTP| OLLAMA_CONTAINER
    
    style API_CONTAINER fill:#42b983
    style UI_CONTAINER fill:#42b983
    style SQL_CONTAINER fill:#95a5a6
    style PG_CONTAINER fill:#95a5a6
    style OLLAMA_CONTAINER fill:#9b59b6
```

## Component Interaction Matrix

| Component | Depends On | Used By |
|-----------|------------|---------|
| **StudyShop.Domain** | None | Application, Infrastructure |
| **StudyShop.Application** | Domain | API, Tests |
| **StudyShop.Infrastructure** | Domain, Application | API |
| **StudyShop.Api** | Application, Infrastructure | Tests, UI |
| **Angular UI** | API (via HTTP) | User |

## Technology Stack

### Backend
- **.NET 9** - Runtime and SDK
- **ASP.NET Core 9** - Web framework
- **EF Core 9** - ORM
- **MediatR 12.4** - CQRS mediator
- **FluentValidation 11.9** - Validation
- **Swashbuckle 6.8** - Swagger generation

### AI/RAG
- **Ollama** - Local LLM runtime
- **pgvector** - Vector similarity search
- **PostgreSQL 16** - Vector database

### Frontend
- **Angular 17** - Framework
- **Angular Material** - UI components
- **TypeScript** - Language
- **openapi-typescript-codegen** - API client generation

### Infrastructure
- **Docker** - Containerization
- **SQL Server 2022** - Primary database
- **PostgreSQL 16** - Vector database
- **Nginx** - Web server (UI)

## Port Allocation

| Service | Port | Protocol | Purpose |
|---------|------|----------|---------|
| Angular UI | 4200 | HTTP | Frontend application |
| API | 5170 | HTTP | REST API & Swagger |
| SQL Server | 1433 | TCP | Product/Order data |
| PostgreSQL | 5432 | TCP | Vector embeddings |
| Ollama | 11434 | HTTP | LLM service |

## Data Flow Summary

1. **User Request** → Angular UI (Port 4200)
2. **API Call** → StudyShop.Api (Port 5170)
3. **MediatR** → Application Layer (Commands/Queries)
4. **Handler** → Infrastructure (DbContext, AI Services)
5. **Database** → SQL Server (entities) or PostgreSQL (vectors)
6. **Response** → DTOs → JSON → UI

