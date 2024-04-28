using Azure;
using DotNetEnv;
using Model;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Services.AddLogging();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISessionRepository, DynamoDbSessionRepository>();
builder.Services.AddScoped<IGameEngine, GameEngine<ZorkI, ZorkIContext>>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Env.Load("../.env");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(b => b
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapFallbackToFile("/index.html");

using var serviceScope = app.Services.CreateScope();
var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
logger.LogWarning("Ready. Let's play some Zork!");

app.Run();