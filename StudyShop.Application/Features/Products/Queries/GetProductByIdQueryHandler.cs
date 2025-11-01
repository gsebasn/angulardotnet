using MediatR;
using StudyShop.Application.Data;
using StudyShop.Application.DTOs;
using StudyShop.Application.Common;

namespace StudyShop.Application.Features.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IAppDbContext _context;
    public GetProductByIdQueryHandler(IAppDbContext context) => _context = context;

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


