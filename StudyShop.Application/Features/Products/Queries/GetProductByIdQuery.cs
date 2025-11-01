using MediatR;
using StudyShop.Application.DTOs;

namespace StudyShop.Application.Features.Products.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;


