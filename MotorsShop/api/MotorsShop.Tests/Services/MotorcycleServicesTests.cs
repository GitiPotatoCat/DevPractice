using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MotorsShop.Data;
using MotorsShop.Dtos;
using MotorsShop.Models;
using MotorsShop.Services;
using Xunit;

namespace MotorsShop.Tests.Services;

public class MotorcycleServiceTests
{
    private static MotorsShopDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MotorsShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new MotorsShopDbContext(options);

        db.Brands.AddRange(
            new Brand { Id = 1, Name = "Honda", Country = "Japan" },
            new Brand { Id = 2, Name = "Yamaha", Country = "Japan" },
            new Brand { Id = 3, Name = "BMW", Country = "Germany" });

        db.Categories.AddRange(
            new Category { Id = 1, Name = "Sport" },
            new Category { Id = 2, Name = "Cruiser" },
            new Category { Id = 3, Name = "Adventure" });

        db.Motorcycles.AddRange(
            new Motorcycle { Id = 1, Name = "CBR600RR", Description = "Sport bike", Price = 12000m, Year = 2024, Stock = 5, EngineCc = 599, BrandId = 1, CategoryId = 1 },
            new Motorcycle { Id = 2, Name = "MT-07", Description = "Naked bike", Price = 8500m, Year = 2023, Stock = 8, EngineCc = 689, BrandId = 2, CategoryId = 1 },
            new Motorcycle { Id = 3, Name = "R1250GS", Description = "Adventure tour", Price = 22000m, Year = 2024, Stock = 3, EngineCc = 1254, BrandId = 3, CategoryId = 3 },
            new Motorcycle { Id = 4, Name = "Rebel 500", Description = "Cruiser", Price = 6500m, Year = 2022, Stock = 4, EngineCc = 471, BrandId = 1, CategoryId = 2 },
            new Motorcycle { Id = 5, Name = "MT-09", Description = "Naked bike", Price = 10500m, Year = 2024, Stock = 6, EngineCc = 889, BrandId = 2, CategoryId = 1 }
        );

        db.SaveChanges();
        return db;
    }


    // The Baselise test
    [Fact]
    public async Task GetAllAsync_ReturnsAllMotorcycles_WhenNoFilters()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto();   // all defaults

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    // Filtering test
    [Fact]
    public async Task GetAllAsync_FiltersByBrandId()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { BrandId = 2 };   // Yamaha

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(m => m.BrandName == "Yamaha");
    }

    [Fact]
    public async Task GetAllAsync_FiltersByCategoryId()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { CategoryId = 1 };   // Sport

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(3);
        result.Items.Should().OnlyContain(m => m.CategoryName == "Sport");
    }

    [Fact]
    public async Task GetAllAsync_FiltersByPriceRange()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { MinPrice = 8000m, MaxPrice = 13000m };

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(3);   // MT-07 (8500), MT-09 (10500), CBR600RR (12000)
        result.Items.Should().OnlyContain(m => m.Price >= 8000m && m.Price <= 13000m);
    }

    [Theory]
    [InlineData("naked", 2)]      // matches descriptions of MT-07 and MT-09
    [InlineData("cbr", 1)]      // matches name of CBR600RR
    [InlineData("XYZ", 0)]      // no match
    public async Task GetAllAsync_FiltersBySearch(string search, int expectedCount)
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { Search = search };

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(expectedCount);
    }

    // Combined Filters test
    [Fact]
    public async Task GetAllAsync_CombinesFilters_AsAnd()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto
        {
            BrandId = 2,        // Yamaha
            CategoryId = 1,     // Sport
            MinPrice = 9000m    // Excludes MT-07 (8500)
        };

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle()
                .Which.Name.Should().Be("MT-09");       // Using FluentAssertions
    }

    // Sorting test
    [Fact]
    public async Task GetAllAsync_SortsByPriceAscending_ByDefault()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { SortBy = "price" };   // no order = asc

        var result = await service.GetAllAsync(query);

        result.Items.Select(m => m.Price)
            .Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_SortsByPriceDescending()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { SortBy = "price", SortOrder = "desc" };

        var result = await service.GetAllAsync(query);

        result.Items.Select(m => m.Price)
            .Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_SortsByName_Ascending()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { SortBy = "name" };

        var result = await service.GetAllAsync(query);

        result.Items.Select(m => m.Name)
            .Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_SortsByIdAscending_WhenSortByNotRecognized()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { SortBy = "bogus-field" };

        var result = await service.GetAllAsync(query);

        result.Items.Select(m => m.Id)
            .Should().BeInAscendingOrder();
    }

    // Pagination test
    [Fact]
    public async Task GetAllAsync_ReturnsCorrectPage()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { Page = 2, PageSize = 2, SortBy = "name" };

        var result = await service.GetAllAsync(query);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);     // 5 items / 2 per page = 3 pages
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPartialLastPage()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto { Page = 3, PageSize = 2, SortBy = "name" };

        var result = await service.GetAllAsync(query);

        result.Items.Should().HaveCount(1);   // only 1 item on page 3
        result.TotalCount.Should().Be(5);
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_TotalCountReflectsFilteredCount_NotPagedCount()
    {
        using var db = CreateDb();
        var service = new MotorcycleService(db);
        var query = new MotorcycleQueryDto
        {
            BrandId = 1,        // 2 Hondas
            Page = 1,
            PageSize = 1        // but only return 1 per page
        };

        var result = await service.GetAllAsync(query);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);   // total filtered, NOT paged
    }
}