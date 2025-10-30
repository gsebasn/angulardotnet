using MediatR;
using StudyShop.Api.DTOs;
using StudyShop.Api.Features.Products.Behaviors;

namespace StudyShop.Api.Features.Products.Queries;

/// <summary>
/// Query to get all products with optional filtering and pagination.
/// </summary>
public record GetProductsQuery : IRequest<IEnumerable<ProductDto>>, ICacheableRequest<IEnumerable<ProductDto>>
{
    public string? Search { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 100;

    public string GetCacheKey() => CacheKeyHelper.GetProductsListKey(Search, Skip, Take);
}

