using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyShop.Application.Data;
using StudyShop.Application.DTOs;
using StudyShop.Application.Common;

namespace StudyShop.Application.Features.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(IAppDbContext context, ILogger<UpdateProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found");
        }

        if (request.Name != null)
        {
            if (request.Name.Length < 2 || request.Name.Length > 100)
            {
                throw new CommandValidationException(new[] { new ValidationError
                {
                    Property = nameof(request.Name),
                    Message = "Name must be between 2 and 100 characters"
                }});
            }
            product.Name = request.Name;
        }

        if (request.Price.HasValue)
        {
            if (request.Price.Value < 0)
            {
                throw new CommandValidationException(new[] { new ValidationError
                {
                    Property = nameof(request.Price),
                    Message = "Price must be >= 0"
                }});
            }
            product.Price = request.Price.Value;
        }

        if (request.Stock.HasValue)
        {
            if (request.Stock.Value < 0)
            {
                throw new CommandValidationException(new[] { new ValidationError
                {
                    Property = nameof(request.Stock),
                    Message = "Stock must be >= 0"
                }});
            }
            product.Stock = request.Stock.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product updated: {ProductId}", product.Id);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedUtc = product.CreatedUtc
        };
    }
}


