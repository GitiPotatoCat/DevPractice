using FoodShop.Data;
using FoodShop.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<FoodShopDbContext>(opt =>
    opt.UseInMemoryDatabase("FoodShopDb"));

// Add services to the container.

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();

var app = builder.Build();

// Make sure the in-memory DB is seeded
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<FoodShopDbContext>();
    db.Database.EnsureCreated();
}


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
