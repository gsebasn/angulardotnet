# Input Validation in StudyShop API

## Overview

The StudyShop API uses **FluentValidation** for server-side input validation combined with **handlers** that perform business logic validation. This provides clean, reusable validation with clear error messages.

## Validation Architecture

### Three Layers of Validation

1. **FluentValidation** - Declarative rules on DTOs/Commands
2. **Handler Validation** - Business logic checks in handlers
3. **Exception Mapping** - Custom exceptions for proper error responses

### Validation Flow

```
HTTP Request
    ↓
Minimal API Endpoint
    ↓
CQRS Command/Query
    ↓
FluentValidation Rules → Validate DTOs
    ↓
Handler → Business Logic Validation
    ↓
Database Save
```

## FluentValidation Setup

### Registration (Program.cs)

```csharp
// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
```

This registers all validators from the assembly and integrates them with MediatR.

## Validator Examples

### 1. Create Product Validator

**Location:** `Features/Products/Commands/CreateProductCommandValidator.cs`

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

        RuleFor(c => c.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be >= 0");
    }
}
```

**Validation Rules:**
- ✅ `Name` - Required, 2-100 characters
- ✅ `Price` - Must be >= 0
- ✅ `Stock` - Must be >= 0

### 2. Update Product Validator (Handler-based)

**Location:** `Features/Products/Commands/UpdateProductCommandHandler.cs`

Since UpdateProductCommand uses optional properties, validation is done in the handler:

```csharp
// Partial update validation
if (request.Name != null)
{
    if (request.Name.Length < 2 || request.Name.Length > 100)
    {
        throw new CommandValidationException(new[] { new ValidationError
        {
            Property = nameof(request.Name),
            Message = "Name must be between 2 and 100 characters"
        }});
    }
    product.Name = request.Name;
}

if (request.Price.HasValue)
{
    if (request.Price.Value < 0)
    {
        throw new CommandValidationException(new[] { new ValidationError
        {
            Property = nameof(request.Price),
            Message = "Price must be >= 0"
        }});
    }
    product.Price = request.Price.Value;
}
```

**Validation Rules:**
- ✅ `Name` - If provided, must be 2-100 characters
- ✅ `Price` - If provided, must be >= 0
- ✅ `Stock` - If provided, must be >= 0

### 3. Create Order Validator

**Location:** `Validators/CreateOrderDtoValidator.cs`

```csharp
public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(o => o.OrderNumber)
            .NotEmpty().WithMessage("Order number is required")
            .MaximumLength(50).WithMessage("Order number must be <= 50 characters");

        RuleFor(o => o.Items)
            .NotEmpty().WithMessage("Order must have at least one item")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("Order must have at least one item");

        RuleForEach(o => o.Items).SetValidator(new OrderItemDtoValidator());
    }
}

public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(item => item.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");

        RuleFor(item => item.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1");
    }
}
```

**Validation Rules:**
- ✅ `OrderNumber` - Required, max 50 characters
- ✅ `Items` - Required, at least one item
- ✅ `ProductId` - Must be > 0
- ✅ `Quantity` - Must be >= 1

### 4. Business Logic Validation (Orders)

**Location:** `Features/Orders/Commands/CreateOrderCommandHandler.cs`

Beyond FluentValidation, handlers also perform business logic checks:

```csharp
// Validate stock availability
foreach (var item in request.Items)
{
    var product = await _context.Products.FindAsync(new object[] { item.ProductId });
    
    if (product == null)
    {
        throw new CommandValidationException(new[] { new ValidationError
        {
            Property = nameof(item.ProductId),
            Message = $"Product with ID {item.ProductId} not found"
        }});
    }

    if (product.Stock < item.Quantity)
    {
        throw new CommandValidationException(new[] { new ValidationError
        {
            Property = nameof(item.Quantity),
            Message = $"Insufficient stock for {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}"
        }});
    }
}
```

**Business Rules:**
- ✅ Product must exist
- ✅ Stock must be sufficient
- ✅ Automatically decrements stock on successful order

## Exception Handling

### CommandValidationException

**Location:** `Features/Products/Commands/CreateProductCommandHandler.cs`

```csharp
public class CommandValidationException : Exception
{
    public CommandValidationException(IEnumerable<ValidationError> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }

    public IEnumerable<ValidationError> Errors { get; }
}

public class ValidationError
{
    public string Property { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
```

**Error Response Format:**

```json
{
  "Property": "Name",
  "Message": "Name must be between 2 and 100 characters"
}
```

### NotFoundException

Used when resources don't exist:

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
```

**Error Response:** 404 Not Found

## Error Response Examples

### Validation Error (400 Bad Request)

```json
{
  "Property": "Stock",
  "Message": "Stock must be >= 0"
}
```

### Business Logic Error (400 Bad Request)

```json
{
  "Property": "Quantity",
  "Message": "Insufficient stock for 4K Monitor. Available: 10, Requested: 15"
}
```

### Not Found Error (404)

```json
{
  "message": "Product with ID 999 not found"
}
```

## Validation in Endpoints

### Minimal API Endpoints

**Location:** `Endpoints/ProductsEndpoints.cs`

```csharp
// Command: Create product
productsGroup.MapPost("/", async (IMediator mediator, CreateProductCommand command) =>
{
    try
    {
        var product = await mediator.Send(command);
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (CommandValidationException ex)
    {
        return Results.BadRequest(ex.Errors);  // ← Returns validation errors
    }
});
```

**Location:** `Endpoints/OrdersEndpoints.cs`

```csharp
// Command: Create order
ordersGroup.MapPost("/", async (IMediator mediator, CreateOrderCommand command) =>
{
    try
    {
        var order = await mediator.Send(command);
        return Results.Created($"/api/orders/{order.Id}", order);
    }
    catch (CommandValidationException ex)
    {
        return Results.BadRequest(ex.Errors);
    }
});
```

## Validation Files

```
StudyShop.Api/
├── Features/
│   ├── Products/
│   │   └── Commands/
│   │       ├── CreateProductCommandValidator.cs  ← FluentValidation rules
│   │       └── CreateProductCommandHandler.cs    ← Business logic checks
│   └── Orders/
│       └── Commands/
│           └── CreateOrderCommandHandler.cs     ← Business logic checks
├── Validators/
│   ├── CreateProductDtoValidator.cs              ← Legacy validator
│   └── CreateOrderDtoValidator.cs                ← DTO validator
```

## Summary

### Validation Methods Used

1. **FluentValidation** - Declarative rules on DTOs
2. **Handler Validation** - Business logic and database checks
3. **Exception Mapping** - Custom exceptions for proper HTTP responses

### Key Features

- ✅ **Type-safe validation** with FluentValidation
- ✅ **Business logic validation** in handlers
- ✅ **Clear error messages** for each field
- ✅ **RFC 7807 compatible** error responses
- ✅ **Stock validation** prevents over-ordering
- ✅ **Partial updates** supported with validation

### Validation Coverage

| Endpoint | Validator | Business Logic |
|----------|-----------|----------------|
| POST /products | ✅ CreateProductCommandValidator | N/A |
| PUT /products/{id} | ✅ Handler validation | ✅ Partial update checks |
| DELETE /products/{id} | N/A | ✅ Existence check |
| POST /orders | ✅ CreateOrderDtoValidator | ✅ Stock validation |
| GET /orders/{id} | N/A | ✅ Existence check |

## Adding New Validation

### Example: Add Category with Validation

1. **Create Validator:**
```csharp
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required")
            .Length(2, 50).WithMessage("Name must be between 2 and 50 characters");
    }
}
```

2. **Register in Handler:**
```csharp
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IValidator<CreateCategoryCommand> _validator;
    
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new CommandValidationException(...);
        }
        // Business logic
    }
}
```

3. **Add to Endpoint:**
```csharp
categoriesGroup.MapPost("/", async (IMediator mediator, CreateCategoryCommand command) =>
{
    try
    {
        var category = await mediator.Send(command);
        return Results.Created($"/api/categories/{category.Id}", category);
    }
    catch (CommandValidationException ex)
    {
        return Results.BadRequest(ex.Errors);
    }
});
```

## Related Documentation

- **[Program.cs](StudyShop.Api/Program.cs)** - FluentValidation registration
- **[Endpoints/](StudyShop.Api/Endpoints/)** - Error handling in endpoints
- **[CQRS-REFACTOR.md](StudyShop.Api/CQRS-REFACTOR.md)** - Architecture overview

