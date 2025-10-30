using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyShop.Api.Data;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Products.Queries;

/// <summary>
/// Handler for GetProductsQuery - implements read side of CQRS pattern.
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly StudyShopDbContext _context;

    public GetProductsQueryHandler(StudyShopDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search));
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CreatedUtc = p.CreatedUtc
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}

