using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyShop.Api.Data;
using StudyShop.Api.DTOs;
using StudyShop.Api.Features.Products.Commands;

namespace StudyShop.Api.Features.Orders.Queries;

/// <summary>
/// Handler for GetOrderByIdQuery.
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly StudyShopDbContext _context;

    public GetOrderByIdQueryHandler(StudyShopDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {request.Id} not found");
        }

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList(),
            Total = order.Total,
            CreatedUtc = order.CreatedUtc
        };
    }
}

