# StudyShop API - Clean Architecture + Vertical Slices

Backend API for the StudyShop learning project demonstrating **Clean Architecture** with **Vertical Slices**, **CQRS**, and **AI/RAG** capabilities.

## Quick Start

```bash
# Restore packages
dotnet restore

# Run the application
dotnet run

# API will be available at:
# - http://localhost:5170
# - Swagger UI: http://localhost:5170/swagger
# - Swagger JSON: http://localhost:5170/swagger/v1/swagger.json
```

## Architecture Overview

The solution follows **Clean Architecture** with **Vertical Slices**:

```
StudyShop.Domain          # Pure business entities (no dependencies)
StudyShop.Application     # Business logic & use cases (Vertical Slices)
StudyShop.Infrastructure  # External dependencies (EF Core, AI, DB)
StudyShop.Api            # Presentation layer (Minimal API endpoints)
```

### Project Structure

```
StudyShop.Api/                    # Presentation Layer
├── Endpoints/                    # API endpoint mappings
│   ├── ProductsEndpoints.cs
│   ├── OrdersEndpoints.cs
│   └── AiEndpoints.cs           # AI/RAG endpoints
└── Program.cs                    # Startup & DI configuration

StudyShop.Application/            # Application Layer
├── Features/                     # Vertical Slices
│   ├── Products/
│   │   ├── Commands/            # CreateProduct, UpdateProduct, DeleteProduct
│   │   ├── Queries/             # GetProducts, GetProductById
│   │   └── Behaviors/           # Caching, Invalidation
│   └── Orders/
│       ├── Commands/             # CreateOrder
│       └── Queries/              # GetOrders, GetOrderById
├── DTOs/                         # Data Transfer Objects
├── Validators/                   # FluentValidation rules
├── Common/                       # Exceptions
├── Data/                         # IAppDbContext interface
└── Ai/                           # AI service interfaces

StudyShop.Domain/                 # Domain Layer
└── Models/                       # Product, Order, OrderItem entities

StudyShop.Infrastructure/         # Infrastructure Layer
├── Data/
│   └── StudyShopDbContext.cs     # EF Core (implements IAppDbContext)
├── Ai/
│   └── OllamaAndVector.cs        # Ollama client, pgvector store
└── DependencyInjection.cs         # Service registration
```

## Features

- ✅ **Clean Architecture** - Domain/Application/Infrastructure separation
- ✅ **Vertical Slices** - Feature-based organization
- ✅ **CQRS Pattern** - Commands (writes) and Queries (reads) separated
- ✅ **MediatR** - Mediator pattern for CQRS
- ✅ **Minimal API** - Modern .NET 9 endpoint registration
- ✅ **Swagger/OpenAPI v3** - Auto-generated API docs
- ✅ **EF Core 9.0** - SQL Server with migrations
- ✅ **FluentValidation** - Server-side validation
- ✅ **Caching** - Memory cache with pipeline behaviors
- ✅ **AI/RAG** - Semantic search with Ollama + pgvector

## API Endpoints

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/products | List with search & pagination |
| GET | /api/products/{id} | Get single product |
| POST | /api/products | Create (validates with FluentValidation) |
| PUT | /api/products/{id} | Update (partial update) |
| DELETE | /api/products/{id} | Delete |

**Query Parameters:**
- `?search=keyword` - Filter by name
- `?skip=0` - Skip N records
- `?take=100` - Take N records

### Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/orders | List all orders |
| GET | /api/orders/{id} | Get single order |
| POST | /api/orders | Create (validates stock, computes total) |

### AI/RAG

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/ai/search?q=... | Semantic product search |
| POST | /api/ai/answer | RAG-based Q&A |

## Database Configuration

### SQL Server (Default in Docker)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver,1433;Database=StudyShopDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  }
}
```

### Vector Store (PostgreSQL + pgvector)

```json
{
  "VectorStore": {
    "ConnectionString": "Host=postgres;Port=5432;Database=studyshop;Username=postgres;Password=postgres"
  }
}
```

## AI/RAG Configuration

Edit `appsettings.json`:

```json
{
  "LLM": {
    "BaseUrl": "http://ollama:11434",
    "ChatModel": "llama3.2:3b",
    "EmbeddingModel": "bge-m3"
  }
}
```

## Validation Rules

### Product
- Name: Required, 2-100 characters
- Price: Required, >= 0
- Stock: Required, >= 0

### Order
- OrderNumber: Required
- Items: At least one item required
- Each Item:
  - ProductId: Required, must exist
  - Quantity: Required, >= 1, must not exceed stock

## Error Handling

All errors return RFC 7807 ProblemDetails format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred",
  "status": 400,
  "detail": "Error message"
}
```

Validation errors return:
```json
{
  "Property": "Name",
  "Message": "Name is required"
}
```

## Development

### Adding a New Feature (Vertical Slice)

1. **Create Domain Entity** (if needed):
   ```csharp
   // StudyShop.Domain/Models/Category.cs
   public class Category
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
   }
   ```

2. **Create Application Slice**:
   ```
   StudyShop.Application/Features/Categories/
   ├── Commands/
   │   ├── CreateCategoryCommand.cs
   │   └── CreateCategoryCommandHandler.cs
   ├── Queries/
   │   ├── GetCategoriesQuery.cs
   │   └── GetCategoriesQueryHandler.cs
   ```

3. **Add DTO**:
   ```csharp
   // StudyShop.Application/DTOs/CategoryDto.cs
   public class CategoryDto { ... }
   ```

4. **Add Validator**:
   ```csharp
   // StudyShop.Application/Validators/CreateCategoryDtoValidator.cs
   public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto> { ... }
   ```

5. **Create Endpoint**:
   ```csharp
   // StudyShop.Api/Endpoints/CategoriesEndpoints.cs
   public static class CategoriesEndpoints
   {
       public static void MapCategoriesEndpoints(this WebApplication app)
       {
           var group = app.MapGroup("api/categories");
           group.MapGet("/", async (IMediator mediator) => { ... });
           group.MapPost("/", async (IMediator mediator, CreateCategoryCommand cmd) => { ... });
       }
   }
   ```

6. **Register in Program.cs**:
   ```csharp
   app.MapCategoriesEndpoints();
   ```

### Dependency Injection

Infrastructure services are registered via `AddInfrastructure()`:

```csharp
// Program.cs
builder.Services.AddInfrastructure(builder.Configuration);
```

This registers:
- EF Core DbContext
- AI services (Ollama, Vector Store)
- Background services (Indexer)

## Caching

Product queries are cached using MediatR pipeline behaviors:

- **QueryCachingBehavior**: Caches query results (5 min absolute, 2 min sliding)
- **CacheInvalidationBehavior**: Clears cache on product mutations

## Testing

See `StudyShop.Api.Tests/` for:
- **Integration Tests**: Test API endpoints end-to-end
- **Unit Tests**: Test handlers in isolation

## Dependencies

- **MediatR** ^12.4.0 - CQRS mediator
- **FluentValidation** ^11.9.2 - Validation
- **Swashbuckle.AspNetCore** ^6.8.1 - Swagger
- **EF Core 9.0** - Database access
- **Npgsql** ^8.0.3 - PostgreSQL driver
- **Pgvector** ^0.2.1 - Vector similarity search

## See Also

- **[ARCHITECTURE.md](../ARCHITECTURE.md)** - Detailed architecture documentation
- **[README.md](../README.md)** - Project overview
