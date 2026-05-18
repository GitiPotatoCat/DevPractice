using Microsoft.EntityFrameworkCore;
using ReinsuranceApi.Data;


var builder = WebApplication.CreateBuilder(args);


// --- Services ---
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = 
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContext<ReinsuranceDbContext>(opt => 
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();


var allowedOrigins = 
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
        ?? ["http://localhost:4200"];


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AngularApp", p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});



var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();
app.UseCors("AngularApp");
app.UseAuthorization();
app.MapControllers();

app.Run();