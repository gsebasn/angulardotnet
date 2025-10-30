using MediatR;
using StudyShop.Api.Data;
using StudyShop.Api.DTOs;
using StudyShop.Api.Features.Products.Commands;

namespace StudyShop.Api.Features.Products.Queries;

/// <summary>
/// Handler for GetProductByIdQuery.
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly StudyShopDbContext _context;

    public GetProductByIdQueryHandler(StudyShopDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found");
        }

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

