using MediatR;
using StudyShop.Application.DTOs;

namespace StudyShop.Application.Features.Products.Commands;

public record UpdateProductCommand : IRequest<ProductDto>
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public decimal? Price { get; init; }
    public int? Stock { get; init; }
}


