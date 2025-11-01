using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyShop.Api.Endpoints;
using FluentValidation;
using System.Reflection;
using StudyShop.Infrastructure;
using StudyShop.Infrastructure.Data;
using StudyShop.Domain.Models;
using StudyShop.Application.Ai;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "StudyShop API (Minimal API + CQRS)",
        Version = "v1",
        Description = "A minimal API demonstrating CQRS pattern with MediatR, Swagger + code generation",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "StudyShop",
            Email = "dev@studyshop.example"
        }
    });

    // Include XML comments in Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
var allowedOrigins = builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins(allowedOrigins.Split(';'))
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Add MediatR (CQRS pattern mediator)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(StudyShop.Application.DTOs.ProductDto).Assembly);
});

// Register MediatR pipeline behaviors (order matters - they execute in reverse order after handler)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StudyShop.Application.Features.Products.Behaviors.QueryCachingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StudyShop.Application.Features.Products.Behaviors.CacheInvalidationBehavior<,>));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(StudyShop.Application.Validators.CreateProductDtoValidator).Assembly);

// Add Problem Details for RFC 7807
builder.Services.AddProblemDetails();

// Add memory caching
builder.Services.AddMemoryCache();

// Infrastructure (DbContext, AI services, vector store)
builder.Services.AddInfrastructure(builder.Configuration);

// Register ProductEmbeddingIndexer background service
builder.Services.AddHostedService<ProductEmbeddingIndexer>();

// (AI services are registered via Infrastructure)

var app = builder.Build();

// Seed initial data and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StudyShopDbContext>();
    
    // Apply migrations (for SQL Server) or ensure created (for InMemory/SQLite)
    try
    {
        context.Database.Migrate();
    }
    catch
    {
        // If migrations fail (e.g., using InMemory), use EnsureCreated
        context.Database.EnsureCreated();
    }
    
    if (!context.Products.Any())
    {
        context.Products.AddRange(new Product[]
        {
            new Product
            {
                Name = "Laptop Computer",
                Price = 999.99m,
                Stock = 15,
                CreatedUtc = DateTime.UtcNow.AddDays(-10)
            },
            new Product
            {
                Name = "Wireless Mouse",
                Price = 29.99m,
                Stock = 50,
                CreatedUtc = DateTime.UtcNow.AddDays(-5)
            },
            new Product
            {
                Name = "Mechanical Keyboard",
                Price = 89.99m,
                Stock = 30,
                CreatedUtc = DateTime.UtcNow.AddDays(-3)
            },
            new Product
            {
                Name = "4K Monitor",
                Price = 349.99m,
                Stock = 10,
                CreatedUtc = DateTime.UtcNow.AddDays(-7)
            },
            new Product
            {
                Name = "USB-C Cable",
                Price = 12.99m,
                Stock = 100,
                CreatedUtc = DateTime.UtcNow.AddDays(-1)
            }
        });
        
        context.SaveChanges();
        Console.WriteLine("✓ Seeded initial products");
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyShop API v1");
        c.RoutePrefix = "swagger";
    });
}

// Exception handling
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors("AllowAngular");

// ========================================
// Minimal API Endpoints (CQRS Pattern)
// ========================================

// Register all API endpoints using extension methods
app.MapProductsEndpoints();
app.MapOrdersEndpoints();
app.MapAiEndpoints();

// Run the app
// Listen on all interfaces (0.0.0.0) to accept connections from Docker host
app.Urls.Add("http://0.0.0.0:8080");

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║     StudyShop API - Ready! (CQRS + Minimal API)     ║");
Console.WriteLine("║                                                      ║");
Console.WriteLine("║  API:      http://localhost:5170                     ║");
Console.WriteLine("║  Swagger:  http://localhost:5170/swagger             ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();

app.Run();

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/ai/search", async (string q, IEmbeddingService emb, IVectorStore store, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q)) return Results.BadRequest("Query 'q' is required.");
            var vector = await emb.CreateEmbedding(q, ct);
            var hits = await store.QueryProductsAsync(vector, 5, ct);
            return Results.Ok(new { results = hits.Select(h => new { productId = h.productId, content = h.content, score = h.score }) });
        });

        app.MapPost("/ai/answer", async (AiAnswerRequest req, IEmbeddingService emb, IVectorStore store, ILlmService llm, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Question)) return Results.BadRequest("Question is required.");
            var vector = await emb.CreateEmbedding(req.Question, ct);
            var hits = await store.QueryProductsAsync(vector, 5, ct);
            var context = string.Join("\n\n", hits.Select(h => $"- {h.content}"));
            var prompt = $"You are a helpful assistant. Answer using ONLY the context below. If nothing is relevant, say you don't know.\n\nContext:\n{context}\n\nQuestion: {req.Question}\nAnswer:";
            var chunks = new List<string>();
            await foreach (var piece in llm.Generate(prompt, ct)) chunks.Add(piece);
            return Results.Ok(new { answer = string.Join(string.Empty, chunks), citations = hits.Select(h => new { h.productId, h.score }) });
        });

        return app;
    }
}

public sealed class AiAnswerRequest
{
    public string Question { get; set; } = string.Empty;
}

public sealed class ProductEmbeddingIndexer : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ProductEmbeddingIndexer> _logger;
    public ProductEmbeddingIndexer(IServiceProvider sp, ILogger<ProductEmbeddingIndexer> logger)
    {
        _sp = sp; _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Wait a bit for Ollama to be ready
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            
            using var scope = _sp.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IVectorStore>();
            var emb = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
            var db = scope.ServiceProvider.GetRequiredService<StudyShopDbContext>();

            await store.EnsureSchemaAsync(stoppingToken);

            var products = db.Products.Select(p => new { p.Id, p.Name }).ToList();
            int chunkIndex = 0;
            foreach (var p in products)
            {
                try
                {
                    var content = $"Name: {p.Name}";
                    var vector = await emb.CreateEmbedding(content, stoppingToken);
                    await store.UpsertProductEmbeddingAsync(p.Id, chunkIndex, content, vector, stoppingToken);
                    chunkIndex++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to index product {ProductId}: {Error}", p.Id, ex.Message);
                    // Continue with next product
                }
            }
            _logger.LogInformation("Indexed {Count} products for semantic search", chunkIndex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProductEmbeddingIndexer failed: {Error}. AI features may not be available.", ex.Message);
            // Don't crash the host - just log the error
        }
    }
}

// Make Program class accessible for test projects
public partial class Program { }
