using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyShop.Infrastructure.Data;
using StudyShop.Infrastructure.Ai;
using StudyShop.Application;
using StudyShop.Application.Ai;
using StudyShop.Application.Data;

namespace StudyShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Db
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<StudyShopDbContext>(options =>
        {
            if (!string.IsNullOrEmpty(connectionString))
                options.UseSqlServer(connectionString);
            else
                options.UseInMemoryDatabase("StudyShopDb");
        });
        
        // Register IAppDbContext to resolve to StudyShopDbContext
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<StudyShopDbContext>());

        // AI / Vector store bindings
        services.Configure<AiOptions>(opts => configuration.GetSection("LLM").Bind(opts));
        services.Configure<VectorStoreOptions>(opts => configuration.GetSection("VectorStore").Bind(opts));
        services.AddHttpClient<IOllamaClient, OllamaClient>();
        services.AddSingleton<IVectorStore>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<VectorStoreOptions>>().Value;
            // Pgvector type mapping is automatically registered via NpgsqlDataSource
            return string.IsNullOrWhiteSpace(opts.ConnectionString)
                ? new NoopVectorStore()
                : new PostgresVectorStore(opts.ConnectionString);
        });
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<ILlmService, LlmService>();

        return services;
    }
}


