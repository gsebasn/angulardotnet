using Microsoft.EntityFrameworkCore;
using StudyShop.Domain.Models;

namespace StudyShop.Application.Data;

public interface IAppDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


