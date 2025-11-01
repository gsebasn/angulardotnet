using MediatR;
using Microsoft.Extensions.Logging;
using StudyShop.Application.Data;
using StudyShop.Application.Common;

namespace StudyShop.Application.Features.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IAppDbContext context, ILogger<DeleteProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found");
        }
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product deleted: {ProductId}", request.Id);
        return Unit.Value;
    }
}


