using FileAPI.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<FileDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<FormOptions>(o => {
    o.MultipartBodyLengthLimit = 5 * 1024 * 1024;   // 5 MB
});

builder.Services.AddCors(options => {
    options.AddPolicy("AllowPortal", policy => {
        policy.WithOrigins("http://localhost:5185")
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();

app.UseCors("AllowPortal");

app.UseAuthorization();

app.MapControllers();

app.Run();
