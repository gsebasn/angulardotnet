using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StudyShop.Api.Features.Products.Commands;

namespace StudyShop.Api.Features.Products.Behaviors;

/// <summary>
/// MediatR pipeline behavior that invalidates product cache when products are modified.
/// Automatically clears cache after CreateProductCommand, UpdateProductCommand, or DeleteProductCommand.
/// </summary>
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
        // Execute the command first
        var response = await next();

        // Invalidate cache if this is a product-modifying command
        if (IsProductCommand(request))
        {
            InvalidateProductCache(request);
        }

        return response;
    }

    private static bool IsProductCommand(TRequest request)
    {
        return request is CreateProductCommand ||
               request is UpdateProductCommand ||
               request is DeleteProductCommand;
    }

    private void InvalidateProductCache(TRequest request)
    {
        // Remove all product-related cache entries
        // Since we can't enumerate memory cache easily, we'll use a prefix-based approach
        // We'll need to track cache keys or just clear everything product-related

        switch (request)
        {
            case CreateProductCommand:
                // Clear all product lists (new product added)
                ClearAllProductCaches();
                _logger.LogInformation("Cache invalidated: Product created");
                break;

            case UpdateProductCommand updateCmd:
                // Clear specific product and all lists
                _cache.Remove(CacheKeyHelper.GetProductByIdKey(updateCmd.Id));
                ClearAllProductListCaches();
                _logger.LogInformation("Cache invalidated: Product {ProductId} updated", updateCmd.Id);
                break;

            case DeleteProductCommand deleteCmd:
                // Clear specific product and all lists
                _cache.Remove(CacheKeyHelper.GetProductByIdKey(deleteCmd.Id));
                ClearAllProductListCaches();
                _logger.LogInformation("Cache invalidated: Product {ProductId} deleted", deleteCmd.Id);
                break;
        }
    }

    private void ClearAllProductListCaches()
    {
        // Clear common product list cache variations
        // Since MemoryCache doesn't support pattern-based removal, we clear known common keys
        // For production, consider using Redis with key patterns or implementing a cache key registry
        
        var commonKeys = new List<string>
        {
            CacheKeyHelper.GetProductsListKey(null, 0, 100),  // Default
            CacheKeyHelper.GetProductsListKey(null, 0, 50),
            CacheKeyHelper.GetProductsListKey(null, 0, 10),
            CacheKeyHelper.GetProductsListKey("", 0, 100),
        };

        // Clear paginated variations
        for (int skip = 0; skip <= 100; skip += 10)
        {
            commonKeys.Add(CacheKeyHelper.GetProductsListKey(null, skip, 100));
            commonKeys.Add(CacheKeyHelper.GetProductsListKey(null, skip, 50));
        }

        foreach (var key in commonKeys)
        {
            if (_cache.TryGetValue(key, out _))
            {
                _cache.Remove(key);
                _logger.LogDebug("Removed cache key: {CacheKey}", key);
            }
        }

        // Note: This doesn't clear ALL possible cache keys (e.g., custom search terms, pagination)
        // For a complete solution in production, consider:
        // 1. Using Redis with key pattern matching
        // 2. Maintaining a registry of active cache keys
        // 3. Using a cache tag/invalidation system like EasyCaching
    }

    private void ClearAllProductCaches()
    {
        ClearAllProductListCaches();
        
        // Note: We can't clear all product-by-id caches without tracking them
        // In production, maintain a registry or use Redis with patterns
        _logger.LogDebug("Cleared all product list caches");
    }
}
