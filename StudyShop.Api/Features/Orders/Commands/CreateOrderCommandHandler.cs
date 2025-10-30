using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyShop.Api.Data;
using StudyShop.Api.DTOs;
using StudyShop.Api.Models;
using StudyShop.Api.Features.Products.Commands;

namespace StudyShop.Api.Features.Orders.Commands;

/// <summary>
/// Handler for CreateOrderCommand - validates stock and computes total server-side.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly StudyShopDbContext _context;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(StudyShopDbContext context, ILogger<CreateOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate stock availability
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

        // Create order
        var order = new Order
        {
            OrderNumber = request.OrderNumber,
            CreatedUtc = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        // Add items and decrement stock
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

        // Reload to get navigation properties
        order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(item => item.Product)
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        _logger.LogInformation("Order created: {OrderNumber} (ID: {OrderId}) with total {Total}",
            order.OrderNumber, order.Id, order.Total);

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Items = order.Items.Select(i => new DTOs.OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList(),
            Total = order.Total,
            CreatedUtc = order.CreatedUtc
        };
    }
}

