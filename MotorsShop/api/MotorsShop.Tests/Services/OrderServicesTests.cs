using Castle.Core.Resource;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MotorsShop.Data;
using MotorsShop.Models;
using MotorsShop.Services;
using Xunit;

namespace MotorsShop.Tests.Services;

public class OrderServiceTests
{
    // Helper: builds a fresh in-memory DbContext with seed data, per test
    private static MotorsShopDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MotorsShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new MotorsShopDbContext(options);

        db.Brands.Add(new Brand { Id = 1, Name = "Honda", Country = "Japan" });
        db.Categories.Add(new Category { Id = 1, Name = "Sport" });
        db.Motorcycles.Add(new Motorcycle
        {
            Id = 1, Name = "CBR600RR", Description = "Sport bike",
            Price = 12000m, Year = 2024, Stock = 5, EngineCc = 599,
            BrandId = 1, CategoryId = 1
        });
        db.Customers.Add(new Customer
        {
            Id = 1, FullName = "Alice", Email = "alice@example.com",
            ApplicationUserId = "user-abc"
        });
        db.SaveChanges();

        return db;
    }


    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoOrders()
    {
        // Arrange — set up the world this test runs in
        using var db = CreateDb();
        var service = new OrderService(db);

        // Act — exercise the thing being tested
        var result = await service.GetAllAsync();

        // Assert — check the outcome
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_DecrementsStock_WhenOrderSucceeds()
    {
        // Arrange
        using var db = CreateDb();
        var service = new OrderService(db);
        var dto = new MotorsShop.Dtos.OrderCreateDto(new List<MotorsShop.Dtos.OrderItemCreateDto>
    {
        new(MotorcycleId: 1, Quantity: 2)
    });

        // Act
        var order = await service.CreateAsync("user-abc", dto);

        // Assert
        order.Should().NotBeNull();
        order.Items.Should().HaveCount(1);
        order.Total.Should().Be(24000m);     // 2 * 12000

        var motorcycle = await db.Motorcycles.FindAsync(1);
        motorcycle!.Stock.Should().Be(3);     // 5 - 2
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCustomerNotFound()
    {
        using var db = CreateDb();
        var service = new OrderService(db);
        var dto = new MotorsShop.Dtos.OrderCreateDto(new List<MotorsShop.Dtos.OrderItemCreateDto>
    {
        new(MotorcycleId: 1, Quantity: 1)
    });

        Func<Task> act = () => service.CreateAsync("nonexistent-user", dto);

        await act.Should().ThrowAsync<MotorsShop.Exceptions.NotFoundException>()
            .WithMessage("*No customer profile*");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenMotorcycleMissing()
    {
        using var db = CreateDb();
        var service = new OrderService(db);
        var dto = new MotorsShop.Dtos.OrderCreateDto(new List<MotorsShop.Dtos.OrderItemCreateDto>
    {
        new(MotorcycleId: 999, Quantity: 1)
    });

        Func<Task> act = () => service.CreateAsync("user-abc", dto);

        await act.Should().ThrowAsync<MotorsShop.Exceptions.NotFoundException>()
            .WithMessage("*Motorcycle(s) not found*");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenInsufficientStock()
    {
        using var db = CreateDb();
        var service = new OrderService(db);
        var dto = new MotorsShop.Dtos.OrderCreateDto(new List<MotorsShop.Dtos.OrderItemCreateDto>
    {
        new(MotorcycleId: 1, Quantity: 999)
    });

        Func<Task> act = () => service.CreateAsync("user-abc", dto);

        await act.Should().ThrowAsync<MotorsShop.Exceptions.ConflictException>()
            .WithMessage("*Insufficient stock*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]   // exactly stock available
    public async Task CreateAsync_Succeeds_WhenQuantityWithinStock(int quantity)
    {
        using var db = CreateDb();
        var service = new OrderService(db);
        var dto = new MotorsShop.Dtos.OrderCreateDto(new List<MotorsShop.Dtos.OrderItemCreateDto>
    {
        new(MotorcycleId: 1, Quantity: quantity)
    });

        var order = await service.CreateAsync("user-abc", dto);

        order.Items[0].Quantity.Should().Be(quantity);
    }
}