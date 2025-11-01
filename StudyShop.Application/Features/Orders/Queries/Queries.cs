using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyShop.Application.Data;
using StudyShop.Application.DTOs;
using StudyShop.Application.Common;

namespace StudyShop.Application.Features.Orders.Queries;

public record GetOrdersQuery() : IRequest<IEnumerable<OrderDto>>;
public record GetOrderByIdQuery(int Id) : IRequest<OrderDto>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IAppDbContext _context;
    public GetOrdersQueryHandler(IAppDbContext context) => _context = context;

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedUtc)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedUtc = o.CreatedUtc,
                Total = o.Total,
                Items = o.Items.Select(i => new OrderItemDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
            })
            .ToListAsync(cancellationToken);
        return orders;
    }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IAppDbContext _context;
    public GetOrderByIdQueryHandler(IAppDbContext context) => _context = context;

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order == null) throw new NotFoundException($"Order with ID {request.Id} not found");
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CreatedUtc = order.CreatedUtc,
            Total = order.Total,
            Items = order.Items.Select(i => new OrderItemDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
        };
    }
}


