using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StudyShop.Application.Features.Products.Commands;

namespace StudyShop.Application.Features.Products.Behaviors;

public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(IMemoryCache cache, ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        if (request is CreateProductCommand || request is UpdateProductCommand || request is DeleteProductCommand)
        {
            // Clear common product list cache keys
            var keys = new[]
            {
                CacheKeyHelper.GetProductsListKey(null, 0, 100),
                CacheKeyHelper.GetProductsListKey(null, 0, 50)
            };
            foreach (var k in keys) _cache.Remove(k);
            _logger.LogInformation("Product caches invalidated");
        }
        return response;
    }
}


