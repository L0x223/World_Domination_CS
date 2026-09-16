using Microsoft.EntityFrameworkCore;
using WorldDominationSignalR.Context;
using WorldDominationSignalR.Hubs;
using WorldDominationSignalR.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddSingleton<IGameStateService, GameStateService>();
builder.Services.AddSingleton<ICountryDataService, CountryDataService>();
builder.Services.AddScoped<IGameRoundService, GameRoundService>();


var useTestDb = builder.Configuration.GetValue<bool>("UseTestDatabase");
if (useTestDb)
{
    builder.Services.AddSingleton<IDataBaseService, TestDataBaseService>();
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<IDataBaseService, DataBaseService>();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); //WebSocket handshake
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueClient");

app.MapHub<WdGameHub>("/WorldDominationGame"); // URL
app.MapHub<WdChatHub>("/WorldDominationChat"); // URL

app.Run();