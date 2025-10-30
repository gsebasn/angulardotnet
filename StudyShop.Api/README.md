# StudyShop API - .NET 9 Web API (CQRS + Minimal API)

Backend API for the StudyShop learning project with modern CQRS architecture.

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

## Features

- ✅ .NET 9 Web API
- ✅ **CQRS Pattern** - Commands and Queries separated
- ✅ **Minimal API** - Modern endpoint registration
- ✅ **MediatR** - Mediator pattern for CQRS
- ✅ Swagger/OpenAPI v3 documentation
- ✅ EF Core 9.0 with InMemory database (easily switchable to SQLite)
- ✅ FluentValidation for request validation
- ✅ CORS configured for Angular on port 4200
- ✅ Global exception handling (RFC 7807 ProblemDetails)
- ✅ Auto-seeded sample products
- ✅ Feature-based organization

## Database

### InMemory (Default)

No setup required. Data is stored in memory and lost when the app restarts.

### SQLite (Persistent)

1. Edit `Program.cs` to switch from InMemory to SQLite:
   ```csharp
   options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
   ```

2. Run migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## Project Structure

```
StudyShop.Api/
├── Features/                    # CQRS pattern - feature-based organization
│   ├── Products/
│   │   ├── Commands/           # Write operations
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── CreateProductCommandHandler.cs
│   │   │   ├── UpdateProductCommand.cs
│   │   │   ├── UpdateProductCommandHandler.cs
│   │   │   └── DeleteProductCommand.cs
│   │   └── Queries/            # Read operations
│   │       ├── GetProductsQuery.cs
│   │       └── GetProductByIdQuery.cs
│   └── Orders/
│       ├── Commands/
│       │   └── CreateOrderCommand.cs
│       └── Queries/
│           ├── GetOrdersQuery.cs
│           └── GetOrderByIdQuery.cs
├── Models/
│   ├── Product.cs              # Product entity
│   └── Order.cs                # Order and OrderItem entities
├── DTOs/
│   ├── ProductDto.cs           # Data transfer objects
│   └── OrderDto.cs
├── Controllers/                # Legacy controllers (deprecated)
│   ├── ProductsController.cs
│   └── OrdersController.cs
├── Data/
│   └── StudyShopDbContext.cs   # EF Core DbContext
├── appsettings.json
└── Program.cs                  # Minimal API endpoints + CQRS setup
```

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

## Validation Rules

### Product
- Name: Required, 2-100 characters
- Price: Required, >= 0
- Stock: Required, >= 0

### Order
- OrderNumber: Required, max 50 characters
- Items: At least one item required
- Each Item:
  - ProductId: Required, > 0
  - Quantity: Required, >= 1
- Stock: Must be >= requested quantity

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

## Seeded Data

On startup, 5 sample products are automatically created:
1. Laptop Computer - $999.99 - Stock: 15
2. Wireless Mouse - $29.99 - Stock: 50
3. Mechanical Keyboard - $89.99 - Stock: 30
4. 4K Monitor - $349.99 - Stock: 10
5. USB-C Cable - $12.99 - Stock: 100

## Development

### Add New Endpoints (CQRS Pattern)

1. Create Command or Query in `Features/YourFeature/` folder
2. Create Handler implementing `IRequestHandler<TCommand/Query, TResult>`
3. Create Validator for commands using FluentValidation
4. Register endpoint in `Program.cs` using Minimal API
5. Add XML comments for Swagger documentation
6. Mark with `.WithName()` and `.WithDescription()` for OpenAPI docs

**Example:**

```csharp
// Command
public record CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; init; }
}

// Handler
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    // Implementation
}

// Endpoint in Program.cs
apiGroup.MapPost("/categories", async (IMediator mediator, CreateCategoryCommand command) =>
{
    var category = await mediator.Send(command);
    return Results.Created($"/api/categories/{category.Id}", category);
});
```

### Enable SQLite

Edit `Program.cs`:

```csharp
builder.Services.AddDbContext<StudyShopDbContext>(options =>
{
    // Comment this line:
    // options.UseInMemoryDatabase("StudyShopDb");
    
    // Uncomment this line:
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
```

Then run:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Dependencies

- Microsoft.AspNetCore.App (included in .NET 9 SDK)
- MediatR ^12.4.0 - CQRS mediator pattern
- FluentValidation.AspNetCore ^11.3.0
- Swashbuckle.AspNetCore ^6.8.1
- Microsoft.EntityFrameworkCore.InMemory ^9.0.0 (or SQLite)
- Microsoft.EntityFrameworkCore.Sqlite ^9.0.0

## Architecture

### CQRS Pattern

**Commands (Write Operations)**
- `CreateProductCommand` - Create new product
- `UpdateProductCommand` - Update existing product
- `DeleteProductCommand` - Delete product
- `CreateOrderCommand` - Create new order

**Queries (Read Operations)**
- `GetProductsQuery` - List all products (with search & pagination)
- `GetProductByIdQuery` - Get single product
- `GetOrdersQuery` - List all orders
- `GetOrderByIdQuery` - Get single order

### MediatR Integration

All commands and queries are sent through MediatR:

```csharp
var query = new GetProductsQuery { Search = searchTerm, Skip = skip, Take = take };
var products = await mediator.Send(query);
```

### Minimal API Endpoints

Endpoints are registered in `Program.cs` using `.MapGet()`, `.MapPost()`, etc.

See [CQRS-REFACTOR.md](CQRS-REFACTOR.md) for detailed architecture documentation.

