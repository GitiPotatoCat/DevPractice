using FoodShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodShop.Data;


public class FoodShopDbContext : DbContext
{
    public FoodShopDbContext(DbContextOptions<FoodShopDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fruits" },
            new Category { Id = 2, Name = "Bakery" },
            new Category { Id = 3, Name = "Drinks" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Apple", Description = "Crisp red apple", Price = 0.50m, Stock = 100, CategoryID = 1 },
            new Product { Id = 2, Name = "Croissant", Description = "Buttery and flaky", Price = 2.20m, Stock = 30, CategoryID = 2 },
            new Product { Id = 3, Name = "Orange Juice", Description = "1L fresh", Price = 3.50m, Stock = 25, CategoryID = 3 }
        );
    }
}
