using MediatR;
using StudyShop.Application.DTOs;
using StudyShop.Application.Common;
using StudyShop.Application.Features.Orders.Commands;
using StudyShop.Application.Features.Orders.Queries;

namespace StudyShop.Api.Endpoints;

/// <summary>
/// Extension methods for registering order endpoints.
/// </summary>
public static class OrdersEndpoints
{
    /// <summary>
    /// Maps all order-related endpoints to the specified route group.
    /// </summary>
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        var ordersGroup = app.MapGroup("api/orders");

        // Query: Get all orders
        ordersGroup.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetOrdersQuery();
            var orders = await mediator.Send(query);
            return Results.Ok(orders);
        })
        .WithName("GetOrders")
        .WithDescription("Get all orders")
        .Produces<List<OrderDto>>(200)
        .Produces(500);

        // Query: Get order by ID
        ordersGroup.MapGet("/{id:int}", async (IMediator mediator, int id) =>
        {
            try
            {
                var query = new GetOrderByIdQuery(id);
                var order = await mediator.Send(query);
                return Results.Ok(order);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GetOrder")
        .WithDescription("Get an order by ID")
        .Produces<OrderDto>(200)
        .Produces(404)
        .Produces(500);

        // Command: Create order
        ordersGroup.MapPost("/", async (IMediator mediator, CreateOrderCommand command) =>
        {
            try
            {
                var order = await mediator.Send(command);
                return Results.Created($"/api/orders/{order.Id}", order);
            }
            catch (CommandValidationException ex)
            {
                return Results.BadRequest(ex.Errors);
            }
        })
        .WithName("CreateOrder")
        .WithDescription("Create a new order")
        .Accepts<CreateOrderCommand>("application/json")
        .Produces<OrderDto>(201)
        .Produces(400)
        .Produces(500);
    }
}

