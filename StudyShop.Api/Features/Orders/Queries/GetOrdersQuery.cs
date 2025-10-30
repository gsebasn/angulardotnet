using MediatR;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Orders.Queries;

/// <summary>
/// Query to get all orders.
/// </summary>
public record GetOrdersQuery : IRequest<IEnumerable<OrderDto>>;

