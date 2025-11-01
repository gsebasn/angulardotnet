using MediatR;

namespace StudyShop.Application.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<Unit>;


