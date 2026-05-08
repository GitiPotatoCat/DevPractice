using FootballApp.Api.Data;
using FootballApp.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Services ------------------------------------------------------------

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase property names on the wire (e.g. "clubName" not "ClubName").
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;

        // Be lenient about case when reading inbound JSON.
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

        // Strings for enums make API logs and clients far easier to debug.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// OpenAPI / Swagger UI for quick manual testing in Step 6.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// EF Core - SQL Server provider, connection string from appsettings.json.
builder.Services.AddDbContext<FootballDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("FootballDb")
        ?? throw new InvalidOperationException(
            "Connection string 'FootballDb' is not configured.");

    options.UseSqlServer(connectionString, sql =>
    {
        // Be explicit about retry behavior for transient SQL errors.
        sql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging(); // shows parameter values in logs
    }
});


// DI Services

builder.Services.AddScoped<IFootballClubService, FootballClubService>();


// CORS: allow the Angular dev server (default port 4200) to call us.
const string AngularCorsPolicy = "AngularDevClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---- Pipeline ------------------------------------------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(AngularCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();