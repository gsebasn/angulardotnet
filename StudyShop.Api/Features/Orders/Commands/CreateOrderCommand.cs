using MediatR;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Orders.Commands;

/// <summary>
/// Command to create a new order.
/// </summary>
public record CreateOrderCommand : IRequest<OrderDto>
{
    public string OrderNumber { get; init; } = string.Empty;
    public List<OrderItemDto> Items { get; init; } = new();
}

public record OrderItemDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

