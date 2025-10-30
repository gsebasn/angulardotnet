using MediatR;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Products.Commands;

/// <summary>
/// Command to create a new product (part of CQRS pattern).
/// </summary>
public record CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}

