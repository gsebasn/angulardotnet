using MediatR;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Orders.Queries;

/// <summary>
/// Query to get an order by ID.
/// </summary>
public record GetOrderByIdQuery(int Id) : IRequest<OrderDto>;

