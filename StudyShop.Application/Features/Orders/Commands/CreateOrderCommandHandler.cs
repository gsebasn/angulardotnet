using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyShop.Application.Common;
using StudyShop.Application.Data;
using StudyShop.Application.DTOs;
using StudyShop.Domain.Models;

namespace StudyShop.Application.Features.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(IAppDbContext context, ILogger<CreateOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
            if (product == null)
            {
                throw new CommandValidationException(new[] { new ValidationError
                {
                    Property = nameof(item.ProductId),
                    Message = $"Product with ID {item.ProductId} not found"
                }});
            }
            if (product.Stock < item.Quantity)
            {
                throw new CommandValidationException(new[] { new ValidationError
                {
                    Property = nameof(item.Quantity),
                    Message = $"Insufficient stock for {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}"
                }});
            }
        }

        var order = new Order
        {
            OrderNumber = request.OrderNumber,
            CreatedUtc = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product!.Price,
                Order = order
            };
            order.Items.Add(orderItem);
            product.Stock -= item.Quantity;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        _logger.LogInformation("Order created: {OrderNumber} (ID: {OrderId}) with total {Total}", order.OrderNumber, order.Id, order.Total);

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Items = order.Items.Select(i => new OrderItemDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList(),
            Total = order.Total,
            CreatedUtc = order.CreatedUtc
        };
    }
}


