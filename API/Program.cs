var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


GameEngine<ZorkI, ZorkIContext> CreateEngine()
{
    return new GameEngine<ZorkI, ZorkIContext>();
}

app.MapGet("/zorkOne/respond", async () =>
{
    var database = new SessionRepository();
    var sessionId = Environment.MachineName;
    var savedGame = await database.GetSession(sessionId);
    var engine = CreateEngine();

    if (!string.IsNullOrEmpty(savedGame))
    {
        var decodedBytes = Convert.FromBase64String(savedGame);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);
        engine.RestoreGame(decodedText);
    }

    return await engine.GetResponse("look");
    
})
    .WithName("GetGameResponse")
    .WithOpenApi();

app.Run();