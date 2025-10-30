using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyShop.Api.Data;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Orders.Queries;

/// <summary>
/// Handler for GetOrdersQuery.
/// </summary>
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly StudyShopDbContext _context;

    public GetOrdersQueryHandler(StudyShopDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(item => item.Product)
            .OrderByDescending(o => o.CreatedUtc)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList(),
                Total = o.Total,
                CreatedUtc = o.CreatedUtc
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}

