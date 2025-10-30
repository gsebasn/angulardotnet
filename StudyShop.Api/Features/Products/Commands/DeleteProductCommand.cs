using MediatR;

namespace StudyShop.Api.Features.Products.Commands;

/// <summary>
/// Command to delete a product.
/// </summary>
public record DeleteProductCommand(int Id) : IRequest<bool>;

