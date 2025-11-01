# StudyShop - Full-Stack Learning Project

A modern full-stack e-commerce demo showcasing **Clean Architecture** with **Vertical Slices**, **CQRS**, AI-powered semantic search (RAG), and automated code generation.

## 🎯 Project Goals

- **Backend**: .NET 9 Web API with Clean Architecture (Domain/Application/Infrastructure) + Vertical Slices + CQRS
- **AI/RAG**: Local LLM (Ollama) with semantic product search using pgvector
- **Frontend**: Angular 17+ with Material UI and auto-generated typed services
- **Architecture**: Clean Architecture principles with Vertical Slices for maintainability
- **Code Generation**: One-command script to regenerate TypeScript client from Swagger

## 🏗️ Architecture Overview

### Clean Architecture + Vertical Slices

The project follows **Clean Architecture** principles with **Vertical Slices** pattern:

```
StudyShop/
├── StudyShop.Domain/              # Core Business Entities (unchanged)
│   └── Models/
│       ├── Product.cs
│       ├── Order.cs
│       └── OrderItem.cs
│
├── StudyShop.Application/          # Business Logic & Use Cases
│   ├── Features/                   # Vertical Slices
│   │   ├── Products/
│   │   │   ├── Commands/          # CreateProduct, UpdateProduct, DeleteProduct
│   │   │   ├── Queries/           # GetProducts, GetProductById
│   │   │   └── Behaviors/         # Caching, Validation
│   │   └── Orders/
│   │       ├── Commands/          # CreateOrder
│   │       └── Queries/           # GetOrders, GetOrderById
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Validators/                # FluentValidation rules
│   ├── Common/                    # Exceptions, shared logic
│   └── Data/                      # IAppDbContext interface
│
├── StudyShop.Infrastructure/      # External Dependencies (unchanged)
│   ├── Data/
│   │   └── StudyShopDbContext.cs  # EF Core DbContext (implements IAppDbContext)
│   ├── Ai/
│   │   └── OllamaAndVector.cs     # Ollama client, pgvector store
│   └── DependencyInjection.cs     # Service registration
│
├── StudyShop.Api/                 # Presentation Layer (Minimal API)
│   ├── Endpoints/                 # API endpoint mappings
│   │   ├── ProductsEndpoints.cs
│   │   ├── OrdersEndpoints.cs
│   │   └── AiEndpoints.cs        # /ai/search, /ai/answer
│   └── Program.cs                 # Startup & configuration
│
├── StudyShop.Api.Tests/           # Integration & Unit Tests
│   ├── Integration/
│   └── Unit/
│
└── studyshop-ui/                  # Angular 17+ Frontend
    ├── src/app/
    │   ├── pages/                  # ProductsPage, OrdersPage
    │   └── api/                     # Generated TypeScript client
    └── package.json
```

### Architecture Diagram

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[Angular UI<br/>Port 4200]
        API[StudyShop.Api<br/>Minimal API Endpoints<br/>Port 5170]
    end
    
    subgraph "Application Layer (Vertical Slices)"
        APP[StudyShop.Application]
        PROD[Products Slice<br/>Commands/Queries/Handlers]
        ORD[Orders Slice<br/>Commands/Queries/Handlers]
        DTO[DTOs & Validators]
        COMMON[Common Exceptions]
    end
    
    subgraph "Domain Layer"
        DOMAIN[StudyShop.Domain]
        ENTITIES[Entities<br/>Product, Order, OrderItem]
    end
    
    subgraph "Infrastructure Layer"
        INFRA[StudyShop.Infrastructure]
        DB[(SQL Server<br/>Port 1433)]
        PG[(PostgreSQL + pgvector<br/>Port 5432)]
        EF[EF Core DbContext]
        OLLAMA[Ollama LLM<br/>Port 11434]
        VECTOR[Vector Store<br/>Embeddings]
    end
    
    UI -->|HTTP| API
    API -->|MediatR| PROD
    API -->|MediatR| ORD
    API -->|Direct| APP
    
    PROD -->|IAppDbContext| EF
    ORD -->|IAppDbContext| EF
    PROD -->|Dependencies| DTO
    ORD -->|Dependencies| DTO
    
    EF -->|Implements| APP
    EF -->|Query/Write| DB
    
    PROD -.->|AI Search| VECTOR
    VECTOR -->|Stores| PG
    VECTOR -->|Generates| OLLAMA
    
    EF -->|Uses| ENTITIES
    APP -->|References| ENTITIES
    
    style UI fill:#42b983
    style API fill:#42b983
    style APP fill:#3498db
    style PROD fill:#3498db
    style ORD fill:#3498db
    style DOMAIN fill:#e74c3c
    style INFRA fill:#f39c12
    style DB fill:#95a5a6
    style PG fill:#95a5a6
    style OLLAMA fill:#9b59b6
```

### Dependency Flow

```
Presentation (API)
    ↓ depends on
Application (Business Logic)
    ↓ depends on
Domain (Entities)
    ↑ implements
Infrastructure (EF, AI, External Services)
```

**Key Principles:**
- ✅ **Domain** has no dependencies (pure business logic)
- ✅ **Application** depends only on Domain (defines interfaces)
- ✅ **Infrastructure** implements Application interfaces
- ✅ **Presentation** depends on Application (uses MediatR, DTOs)
- ✅ **Vertical Slices** organize features by business capability (Products, Orders, AI)

## 🔌 Development Ports

All ports used in development mode:

| Service | Port | Purpose | URL |
|---------|------|---------|-----|
| **Angular UI** | `4200` | Frontend application | http://localhost:4200 |
| **.NET API** | `5170` | REST API backend | http://localhost:5170 |
| **Swagger UI** | `5170/swagger` | API documentation | http://localhost:5170/swagger |
| **SQL Server** | `1433` | Primary database | `localhost,1433` |
| **PostgreSQL** | `5432` | Vector database (pgvector) | `localhost:5432` |
| **Ollama LLM** | `11434` | Local LLM service | http://localhost:11434 |

### Docker Container Ports

When running with Docker Compose, internal container ports are mapped to host ports:

| Container | Internal Port | Host Port | Access |
|-----------|--------------|-----------|-------|
| `studyshop-ui` | 80 | 4200 | http://localhost:4200 |
| `studyshop-api` | 8080 | 5170 | http://localhost:5170 |
| `studyshop-sqlserver` | 1433 | 1433 | `localhost,1433` |
| `studyshop-postgres` | 5432 | 5432 | `localhost:5432` |
| `studyshop-ollama` | 11434 | 11434 | http://localhost:11434 |

### Port Conflicts

If any ports are already in use, you can modify them in:

- **Docker**: Edit `docker-compose.yml` port mappings
- **Local API**: Edit `StudyShop.Api/Program.cs` (line ~171) - change `app.Urls.Add("http://0.0.0.0:5170")`
- **Local UI**: Edit `studyshop-ui/angular.json` or use `ng serve --port <PORT>`

### Quick Port Checks

```bash
# Check if ports are available
lsof -i :4200  # Angular UI
lsof -i :5170  # .NET API
lsof -i :1433  # SQL Server
lsof -i :5432  # PostgreSQL
lsof -i :11434 # Ollama
```

## 🚀 Quick Start

### Prerequisites

- **.NET 9 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+**: [Download](https://nodejs.org/)
- **Angular CLI**: `npm install -g @angular/cli`
- **Docker Desktop** (for AI/RAG features)

### Step 1: Start with Docker (Recommended)

```bash
# From project root
docker compose up -d --build

# Pull Ollama models (first time only)
curl -X POST http://localhost:11434/api/pull -d '{"name":"llama3.2:3b"}'
curl -X POST http://localhost:11434/api/pull -d '{"name":"bge-m3"}'
```

This starts all services with ports mapped as listed above.

### Step 2: Start Locally (Alternative)

#### Backend
```bash
cd StudyShop.Api
dotnet restore
dotnet run
# API: http://localhost:5170
# Swagger: http://localhost:5170/swagger
```

> **Note**: When running locally, ensure SQL Server is running on port 1433, or the API will use an in-memory database.

#### Frontend
```bash
cd studyshop-ui
npm install
npm run gen:api  # Generate TypeScript client
npm start
# UI: http://localhost:4200
```

> **Note**: The frontend expects the API at `http://localhost:5170`. Update `studyshop-ui/src/app/environments/environment.ts` if using a different port.

## 🧠 AI/RAG Features

### Semantic Search

- **Natural Language Queries**: "Find laptops under $1000"
- **Vector Similarity**: Uses pgvector for semantic matching
- **Embeddings**: Generated by Ollama (bge-m3 model)
- **Real-time Indexing**: Products automatically embedded on create/update

### Endpoints

```
POST /api/ai/search?q=laptops       # Semantic product search
POST /api/ai/answer                 # RAG-based Q&A
```

### Configuration

Edit `appsettings.json`:
```json
{
  "LLM": {
    "BaseUrl": "http://localhost:11434",
    "ChatModel": "llama3.2:3b",
    "EmbeddingModel": "bge-m3"
  },
  "VectorStore": {
    "ConnectionString": "Host=postgres;Port=5432;Database=studyshop;Username=postgres;Password=postgres"
  }
}
```

## 📚 Key Features

### Backend (.NET 9)

- ✅ **Clean Architecture** - Domain/Application/Infrastructure separation
- ✅ **Vertical Slices** - Features organized by business capability
- ✅ **CQRS Pattern** - Commands (writes) and Queries (reads) separated
- ✅ **MediatR** - Mediator pattern for CQRS
- ✅ **Minimal API** - Modern .NET 9 endpoint registration
- ✅ **Swagger/OpenAPI v3** - Auto-generated API docs
- ✅ **EF Core 9.0** - SQL Server with migrations
- ✅ **FluentValidation** - Server-side validation
- ✅ **Caching** - Memory cache with invalidation behaviors
- ✅ **AI/RAG** - Semantic search with Ollama + pgvector

### API Endpoints

```
Products:
GET    /api/products                # List (with search/pagination)
GET    /api/products/{id}           # Get by ID
POST   /api/products                # Create
PUT    /api/products/{id}           # Update
DELETE /api/products/{id}           # Delete

Orders:
GET    /api/orders                  # List all
GET    /api/orders/{id}             # Get by ID
POST   /api/orders                  # Create (validates stock)

AI:
POST   /api/ai/search?q=...         # Semantic search
POST   /api/ai/answer               # RAG Q&A
```

### Frontend (Angular 17)

- ✅ **Standalone Components** - Modern Angular
- ✅ **Angular Material** - UI components
- ✅ **Auto-generated Client** - TypeScript services from Swagger
- ✅ **Semantic Search UI** - AI-powered product discovery
- ✅ **Reactive Forms** - Client-side validation

## 🧪 Testing

### Run Tests

```bash
# From project root
dotnet test StudyShop.Api.Tests/StudyShop.Api.Tests.csproj
```

### Test Structure

- **Integration Tests**: Test API endpoints end-to-end
- **Unit Tests**: Test MediatR handlers in isolation

## 📖 Learning Points

1. **Clean Architecture**
   - Domain layer is pure business logic (no dependencies)
   - Application defines use cases and interfaces
   - Infrastructure implements external concerns
   - Presentation (API) orchestrates requests

2. **Vertical Slices**
   - Features grouped by business capability
   - Each slice contains Commands, Queries, Handlers, Validators
   - Easy to add new features without touching existing code

3. **CQRS Pattern**
   - Separates read (Queries) from write (Commands) operations
   - Enables independent scaling and optimization
   - Clear separation of concerns

4. **AI/RAG Implementation**
   - Local LLM (Ollama) for embeddings and chat
   - Vector database (pgvector) for semantic search
   - Automatic product indexing on changes

## 🔧 Configuration

### Database Connection

Edit `StudyShop.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=StudyShopDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  }
}
```

### Vector Store (PostgreSQL)

```json
{
  "VectorStore": {
    "ConnectionString": "Host=localhost;Port=5432;Database=studyshop;Username=postgres;Password=postgres"
  }
}
```

## 📝 Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Detailed architecture overview
- **[SWAGGER-EXPORT.md](SWAGGER-EXPORT.md)** - API code generation guide
- **[DOCKER.md](DOCKER.md)** - Docker deployment guide
- **[MIGRATION-TO-DOTNET9.md](MIGRATION-TO-DOTNET9.md)** - .NET 9 migration notes

## 🎓 Next Steps

- [ ] Add GraphQL endpoint
- [ ] Implement Neo4j for relationship queries
- [ ] Add JWT authentication
- [ ] Add integration tests for AI endpoints
- [ ] Implement Redis for distributed caching
- [ ] Add OpenTelemetry for observability

## 📄 License

This is a learning project. Feel free to use and modify as needed.
