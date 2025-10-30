using MediatR;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Products.Commands;

/// <summary>
/// Command to update an existing product.
/// </summary>
public record UpdateProductCommand : IRequest<ProductDto>
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public decimal? Price { get; init; }
    public int? Stock { get; init; }
}

