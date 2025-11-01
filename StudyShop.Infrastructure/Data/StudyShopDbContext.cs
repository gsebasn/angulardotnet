using Microsoft.EntityFrameworkCore;
using StudyShop.Domain.Models;
using StudyShop.Application.Data;

namespace StudyShop.Infrastructure.Data;

public class StudyShopDbContext : DbContext, IAppDbContext
{
    public StudyShopDbContext(DbContextOptions<StudyShopDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}


