using ChatApp.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(o => 
    o.AddPolicy
    ("ng", p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    )
);


var app = builder.Build();

app.UseCors("ng");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();