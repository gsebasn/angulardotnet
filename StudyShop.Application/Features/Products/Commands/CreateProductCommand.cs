using MediatR;
using StudyShop.Application.DTOs;

namespace StudyShop.Application.Features.Products.Commands;

public record CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}


