using MediatR;
using StudyShop.Api.DTOs;

namespace StudyShop.Api.Features.Products.Queries;

/// <summary>
/// Query to get a product by ID.
/// </summary>
public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

