using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyShop.Api.Data;
using StudyShop.Api.Endpoints;
using StudyShop.Api.Features.Orders.Commands;
using StudyShop.Api.Features.Products.Commands;
using StudyShop.Api.Models;
using FluentValidation;
using System.Reflection;

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
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Register MediatR pipeline behaviors (order matters - they execute in reverse order after handler)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StudyShop.Api.Features.Products.Behaviors.QueryCachingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StudyShop.Api.Features.Products.Behaviors.CacheInvalidationBehavior<,>));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

// Add Problem Details for RFC 7807
builder.Services.AddProblemDetails();

// Add memory caching
builder.Services.AddMemoryCache();

// Configure DbContext
builder.Services.AddDbContext<StudyShopDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Use SQL Server if connection string is provided
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Fallback to InMemory for development/testing
        options.UseInMemoryDatabase("StudyShopDb");
    }
});

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
