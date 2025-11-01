using MediatR;
using StudyShop.Application.DTOs;

namespace StudyShop.Application.Features.Orders.Commands;

public record CreateOrderCommand : IRequest<OrderDto>
{
    public string OrderNumber { get; init; } = string.Empty;
    public List<OrderItemDto> Items { get; init; } = new();
}


