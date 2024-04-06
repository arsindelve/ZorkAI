using Model;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Services.AddLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IGameEngine, GameEngine<ZorkI, ZorkIContext>>();

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load("../.env");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using var serviceScope = app.Services.CreateScope();
var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
logger.LogWarning("Ready. Let's play some Zork!");

app.Run();

