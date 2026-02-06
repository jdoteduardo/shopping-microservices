using Catalog.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Catalog.API.Data;

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
            new Category { Id = 2, Name = "Clothing", Description = "Clothing and fashion items" },
            new Category { Id = 3, Name = "Books", Description = "Books and publications" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Stock = 10,
                CategoryId = 1,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "T-Shirt",
                Description = "Cotton t-shirt",
                Price = 19.99m,
                Stock = 50,
                CategoryId = 2,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "C# Programming Book",
                Description = "Learn C# from scratch",
                Price = 39.99m,
                Stock = 25,
                CategoryId = 3,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
