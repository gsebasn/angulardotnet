using MediatR;
using StudyShop.Application.DTOs;

namespace StudyShop.Application.Features.Products.Queries;

public record GetProductsQuery : IRequest<IEnumerable<ProductDto>>
{
    public string? Search { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 100;
}


