# CQRS Refactor - StudyShop API

## Overview

The StudyShop API has been successfully refactored from a traditional controller-based architecture to a **CQRS (Command Query Responsibility Segregation)** pattern using:

- **MediatR** - Mediator pattern for handling commands and queries
- **Minimal APIs** - .NET 6+ endpoint registration
- **FluentValidation** - Validation rules for commands

## Architecture

### CQRS Pattern

**Commands (Write Operations)**
- Modify state
- Return data as confirmation
- Examples: CreateProduct, UpdateProduct, DeleteProduct, CreateOrder

**Queries (Read Operations)**
- Return data
- Do not modify state
- Examples: GetProducts, GetProductById, GetOrders, GetOrderById

### Folder Structure

```
StudyShop.Api/
├── Features/                           # Feature-based organization
│   ├── Products/
│   │   ├── Commands/                   # Write operations
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── CreateProductCommandHandler.cs
│   │   │   ├── CreateProductCommandValidator.cs
│   │   │   ├── UpdateProductCommand.cs
│   │   │   ├── UpdateProductCommandHandler.cs
│   │   │   ├── DeleteProductCommand.cs
│   │   │   └── DeleteProductCommandHandler.cs
│   │   └── Queries/                    # Read operations
│   │       ├── GetProductsQuery.cs
│   │       ├── GetProductsQueryHandler.cs
│   │       ├── GetProductByIdQuery.cs
│   │       └── GetProductByIdQueryHandler.cs
│   └── Orders/
│       ├── Commands/
│       │   ├── CreateOrderCommand.cs
│       │   └── CreateOrderCommandHandler.cs
│       └── Queries/
│           ├── GetOrdersQuery.cs
│           ├── GetOrdersQueryHandler.cs
│           ├── GetOrderByIdQuery.cs
│           └── GetOrderByIdQueryHandler.cs
├── Controllers/                        # DEPRECATED - kept for reference
│   ├── ProductsController.cs
│   └── OrdersController.cs
└── Program.cs                         # Minimal API endpoints
```

## Key Components

### 1. Commands & Command Handlers

**Example: CreateProductCommand**

```csharp
public record CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}
```

**Command Handler:**
- Validates input using FluentValidation
- Creates entity
- Saves to database
- Returns DTO

```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validation, business logic, persistence
    }
}
```

### 2. Queries & Query Handlers

**Example: GetProductsQuery**

```csharp
public record GetProductsQuery : IRequest<IEnumerable<ProductDto>>
{
    public string? Search { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 100;
}
```

**Query Handler:**
- No state modifications
- Optimized for read operations
- Returns DTOs

### 3. Minimal API Endpoints

**Replaced:** `ProductsController.cs` with endpoints in `Program.cs`

```csharp
var productsGroup = apiGroup.MapGroup("products");

// GET /api/products
productsGroup.MapGet("/", async (IMediator mediator, string? search, int skip = 0, int take = 100) =>
{
    var query = new GetProductsQuery { Search = search, Skip = skip, Take = take };
    var products = await mediator.Send(query);
    return Results.Ok(products);
})
.WithName("GetProducts")
.WithDescription("Get all products with optional search and pagination")
.Produces<List<ProductDto>>(200);

// POST /api/products
productsGroup.MapPost("/", async (IMediator mediator, CreateProductCommand command) =>
{
    try
    {
        var product = await mediator.Send(command);
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (CommandValidationException ex)
    {
        return Results.BadRequest(ex.Errors);
    }
})
.WithName("CreateProduct");
```

## Benefits of CQRS + Minimal API

### 1. **Separation of Concerns**
- Commands handle writes
- Queries handle reads
- Clear responsibility boundaries

### 2. **Scalability**
- Read and write operations can be scaled independently
- Different optimizations for each side

### 3. **Testability**
- Handlers are easily unit tested
- Commands/Queries are simple POCOs (Plain Old CLR Objects)

### 4. **Maintainability**
- Feature-based folder structure
- Each feature is self-contained
- Easy to locate related code

### 5. **Type Safety**
- MediatR provides compile-time checking
- IRequest<TResponse> ensures correct return types

### 6. **Minimal API Benefits**
- Less boilerplate than controllers
- Better performance (fewer abstractions)
- Cleaner code organization
- Built-in Swagger support

## Validation

### FluentValidation Integration

Validators are registered automatically:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

        RuleFor(c => c.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");
    }
}
```

### Custom Validation Exception

```csharp
public class CommandValidationException : Exception
{
    public IEnumerable<ValidationError> Errors { get; }
}
```

## Error Handling

### Exception Types

1. **CommandValidationException** - Validation failures
2. **NotFoundException** - Resource not found

### Minimal API Error Handling

```csharp
try
{
    var product = await mediator.Send(command);
    return Results.Created($"/api/products/{product.Id}", product);
}
catch (CommandValidationException ex)
{
    return Results.BadRequest(ex.Errors);
}
catch (NotFoundException ex)
{
    return Results.NotFound(new { message = ex.Message });
}
```

## API Endpoints (Same as before!)

All endpoints remain the same for backward compatibility:

```
GET    /api/products           - Get all products (with search & pagination)
GET    /api/products/{id}      - Get product by ID
POST   /api/products           - Create product
PUT    /api/products/{id}      - Update product
DELETE /api/products/{id}      - Delete product

GET    /api/orders             - Get all orders
GET    /api/orders/{id}        - Get order by ID
POST   /api/orders             - Create order
```

## Migration from Controllers

### Before (Controller-based)
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly StudyShopDbContext _context;
    
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
    {
        // Logic here
    }
}
```

### After (CQRS + Minimal API)
```csharp
// Command
public record CreateProductCommand : IRequest<ProductDto> { /* ... */ }

// Handler
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    // Logic here
}

// Endpoint
productsGroup.MapPost("/", async (IMediator mediator, CreateProductCommand command) =>
{
    var product = await mediator.Send(command);
    return Results.Created($"/api/products/{product.Id}", product);
});
```

## Testing the Refactored API

### Run the API

```bash
dotnet run --project StudyShop.Api
```

### Test Endpoints

1. **Get Products**
```bash
curl http://localhost:5170/api/products
```

2. **Create Product**
```bash
curl -X POST http://localhost:5170/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"New Product","price":99.99,"stock":10}'
```

3. **Swagger UI**
```
http://localhost:5170/swagger
```

## Next Steps

1. **Add more features** - Each new feature follows the same pattern
2. **Add caching** - Query handlers can implement caching strategies
3. **Add event sourcing** - Commands can publish events
4. **Add authorization** - Apply policies to commands/queries
5. **Performance monitoring** - Track query/command performance separately

## Code Comparison

| Aspect | Controller-based | CQRS + Minimal API |
|--------|-----------------|-------------------|
| Endpoints | Controllers | Minimal API |
| Business Logic | In controller actions | In handlers |
| Validation | FluentValidation | FluentValidation |
| Request/Response | DTOs | Commands/Queries + DTOs |
| Folder Structure | By layer (Controllers, Models, etc.) | By feature (Features/Products, etc.) |
| Testability | Mock repositories | Mock handlers |
| Complexity | Medium | Higher initially, better long-term |

## Summary

✅ Successfully migrated from controllers to CQRS pattern  
✅ Implemented Minimal API endpoints  
✅ Maintained backward compatibility with existing API contract  
✅ Improved code organization and separation of concerns  
✅ Enhanced testability and maintainability  

The API is now more scalable and maintainable while preserving all existing functionality!

