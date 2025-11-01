using MediatR;
using StudyShop.Application.DTOs;
using StudyShop.Application.Features.Products.Commands;
using StudyShop.Application.Features.Products.Queries;
using StudyShop.Application.Common;

namespace StudyShop.Api.Endpoints;

/// <summary>
/// Extension methods for registering product endpoints.
/// </summary>
public static class ProductsEndpoints
{
    /// <summary>
    /// Maps all product-related endpoints to the specified route group.
    /// </summary>
    public static void MapProductsEndpoints(this WebApplication app)
    {
        var productsGroup = app.MapGroup("api/products");

        // Query: Get all products
        productsGroup.MapGet("/", async (IMediator mediator, string? search, int skip = 0, int take = 100) =>
        {
            var query = new GetProductsQuery { Search = search, Skip = skip, Take = take };
            var products = await mediator.Send(query);
            return Results.Ok(products);
        })
        .WithName("GetProducts")
        .WithDescription("Get all products with optional search and pagination")
        .Produces<List<ProductDto>>(200)
        .Produces(500);

        // Query: Get product by ID
        productsGroup.MapGet("/{id:int}", async (IMediator mediator, int id) =>
        {
            try
            {
                var query = new GetProductByIdQuery(id);
                var product = await mediator.Send(query);
                return Results.Ok(product);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GetProduct")
        .WithDescription("Get a product by ID")
        .Produces<ProductDto>(200)
        .Produces(404)
        .Produces(500);

        // Command: Create product
        productsGroup.MapPost("/", async (IMediator mediator, CreateProductCommand command) =>
        {
            try
            {
                var product = await mediator.Send(command);
                return Results.Created($"/api/products/{product.Id}", product);
            }
            catch (CommandValidationException ex)
            {
                return Results.BadRequest(ex.Errors);
            }
        })
        .WithName("CreateProduct")
        .WithDescription("Create a new product")
        .Accepts<CreateProductCommand>("application/json")
        .Produces<ProductDto>(201)
        .Produces(400)
        .Produces(500);

        // Command: Update product
        productsGroup.MapPut("/{id:int}", async (IMediator mediator, int id, UpdateProductDto input) =>
        {
            try
            {
                var command = new UpdateProductCommand
                {
                    Id = id,
                    Name = input.Name,
                    Price = input.Price,
                    Stock = input.Stock
                };
                var product = await mediator.Send(command);
                return Results.Ok(product);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (CommandValidationException ex)
            {
                return Results.BadRequest(ex.Errors);
            }
        })
        .WithName("UpdateProduct")
        .WithDescription("Update an existing product")
        .Produces<ProductDto>(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);

        // Command: Delete product
        productsGroup.MapDelete("/{id:int}", async (IMediator mediator, int id) =>
        {
            try
            {
                var command = new DeleteProductCommand(id);
                await mediator.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("DeleteProduct")
        .WithDescription("Delete a product")
        .Produces(204)
        .Produces(404)
        .Produces(500);
    }
}

