using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyShop.Api.Data;

namespace StudyShop.Api.Tests.Infrastructure;

public class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with InMemory for isolation
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StudyShopDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<StudyShopDbContext>(options =>
            {
                options.UseInMemoryDatabase("StudyShopTests");
            });

            // Build the service provider and ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StudyShopDbContext>();
            db.Database.EnsureCreated();
        });
    }
}


