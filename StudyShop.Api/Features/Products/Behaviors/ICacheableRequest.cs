namespace StudyShop.Api.Features.Products.Behaviors;

public interface ICacheableRequest<TResponse>
{
    string GetCacheKey();
    TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(2);
}
