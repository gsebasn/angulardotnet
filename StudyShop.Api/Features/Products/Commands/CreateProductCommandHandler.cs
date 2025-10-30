using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using StudyShop.Api.Data;
using StudyShop.Api.DTOs;
using StudyShop.Api.Models;

namespace StudyShop.Api.Features.Products.Commands;

/// <summary>
/// Handler for CreateProductCommand - implements CQRS pattern.
/// Handles the write operation (Command) for creating products.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly StudyShopDbContext _context;
    private readonly IValidator<CreateProductCommand> _validator;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        StudyShopDbContext context,
        IValidator<CreateProductCommand> validator,
        ILogger<CreateProductCommandHandler> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validation is handled automatically by FluentValidation when using MediatR
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            throw new CommandValidationException(validationResult.Errors.Select(e => new ValidationError
            {
                Property = e.PropertyName,
                Message = e.ErrorMessage
            }));
        }

        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            CreatedUtc = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created: {ProductName} (ID: {ProductId})", product.Name, product.Id);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedUtc = product.CreatedUtc
        };
    }
}

/// <summary>
/// Validation exception for CQRS commands.
/// </summary>
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

