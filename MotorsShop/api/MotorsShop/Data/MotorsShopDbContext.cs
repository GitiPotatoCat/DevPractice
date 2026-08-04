using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotorsShop.Models;

namespace MotorsShop.Data;

public class MotorsShopDbContext : IdentityDbContext
    <ApplicationUser,               // user
    IdentityRole,                   // role
    string,                         // key type
    IdentityUserClaim<string>,
    IdentityUserRole<string>,
    IdentityUserLogin<string>,
    IdentityRoleClaim<string>,
    IdentityUserToken<string>>      // passkey type
{
    public MotorsShopDbContext(DbContextOptions<MotorsShopDbContext> options)
        : base(options) { }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Motorcycle> Motorcycles => Set<Motorcycle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Motorcycle>()
            .HasOne(m => m.Brand)
            .WithMany(b => b.Motorcycles)
            .HasForeignKey(m => m.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        // Stop EF from deleting a Brand if motorcycles reference it
        modelBuilder.Entity<Motorcycle>()
            .HasOne(m => m.Brand)
            .WithMany(b => b.Motorcycles)
            .HasForeignKey(m => m.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Motorcycle>()
            .HasOne(m => m.Category)
            .WithMany(c => c.Motorcycles)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.ApplicationUser)
            .WithOne(u => u.Customer)
            .HasForeignKey<Customer>(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Without this, two customers could share one login
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.ApplicationUserId)
            .IsUnique()
            .HasFilter("[ApplicationUserId] IS NOT NULL");

        // Seed data
        modelBuilder.Entity<Brand>().HasData(
            new Brand { Id = 1, Name = "Honda", Country = "Japan" },
            new Brand { Id = 2, Name = "Yamaha", Country = "Japan" },
            new Brand { Id = 3, Name = "BMW", Country = "Germany" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Sport", Description = "Track-oriented" },
            new Category { Id = 2, Name = "Cruiser", Description = "Long-distance comfort" },
            new Category { Id = 3, Name = "Adventure", Description = "On and off-road" }
        );

        modelBuilder.Entity<Motorcycle>().HasData(
            new Motorcycle { Id = 1, Name = "CBR600RR", Price = 12000m, Year = 2024, Stock = 5, EngineCc = 599, BrandId = 1, CategoryId = 1 },
            new Motorcycle { Id = 2, Name = "MT-07", Price = 8500m, Year = 2024, Stock = 8, EngineCc = 689, BrandId = 2, CategoryId = 1 },
            new Motorcycle { Id = 3, Name = "R 1250 GS", Price = 22000m, Year = 2024, Stock = 3, EngineCc = 1254, BrandId = 3, CategoryId = 3 }
        );
    }
}