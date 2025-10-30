using MediatR;
using StudyShop.Api.Data;

namespace StudyShop.Api.Features.Products.Commands;

/// <summary>
/// Handler for DeleteProductCommand.
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly StudyShopDbContext _context;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(StudyShopDbContext context, ILogger<DeleteProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted: {ProductId}", request.Id);

        return true;
    }
}

