using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StudyShop.Api.Data;
using StudyShop.Api.DTOs;
using StudyShop.Api.Features.Products.Commands;
using StudyShop.Api.Features.Products.Queries;
using StudyShop.Api.Models;
using FluentValidation;
using Xunit;

namespace StudyShop.Api.Tests.Unit;

public class ProductsHandlersTests
{
    private static StudyShopDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudyShopDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new StudyShopDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task CreateProductHandler_Creates_Product()
    {
        using var ctx = CreateContext();
        var validator = new Mock<IValidator<CreateProductCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        var logger = new Mock<ILogger<CreateProductCommandHandler>>();

        var handler = new CreateProductCommandHandler(ctx, validator.Object, logger.Object);

        var result = await handler.Handle(new CreateProductCommand
        {
            Name = "WB Product",
            Price = 10.5m,
            Stock = 2
        }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("WB Product");
        (await ctx.Products.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetProductsQueryHandler_Returns_List()
    {
        using var ctx = CreateContext();
        ctx.Products.AddRange(
            new Product { Name = "A", Price = 1, Stock = 1, CreatedUtc = DateTime.UtcNow },
            new Product { Name = "B", Price = 2, Stock = 2, CreatedUtc = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(ctx);
        var result = await handler.Handle(new GetProductsQuery { Skip = 0, Take = 100 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
}


