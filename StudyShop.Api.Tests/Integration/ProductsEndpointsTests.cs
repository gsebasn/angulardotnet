using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using StudyShop.Application.DTOs;
using StudyShop.Api.Tests.Infrastructure;

namespace StudyShop.Api.Tests.Integration;

public class ProductsEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public ProductsEndpointsTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsOk_WithSeededItems()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().NotBeNull();
        products!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreated_ProductPersisted()
    {
        var request = new CreateProductDto { Name = "Test From IT", Price = 25.5m, Stock = 3 };

        var response = await _client.PostAsJsonAsync("/api/products", request);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be("Test From IT");

        var check = await _client.GetAsync($"/api/products/{created.Id}");
        check.EnsureSuccessStatusCode();
        var fetched = await check.Content.ReadFromJsonAsync<ProductDto>();
        fetched!.Name.Should().Be("Test From IT");
    }
}


