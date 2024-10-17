using System.Diagnostics;
using System.Text;
using DynamoDb;
using GameEngine;
using Microsoft.Extensions.Logging;
using Model;
using Model.Interface;
using Planetfall;
using ZorkOne;

var database = new DynamoDbSessionRepository();
var sessionId = Environment.MachineName;
var savedGame = await database.GetSession(sessionId);

Console.ForegroundColor = ConsoleColor.DarkCyan;

var engine = CreateEngine<Planetfall.Planetfall, PlanetfallContext>();
Console.WriteLine(engine.IntroText + Environment.NewLine);

if (!string.IsNullOrEmpty(savedGame))
{
    var decodedBytes = Convert.FromBase64String(savedGame);
    var decodedText = Encoding.UTF8.GetString(decodedBytes);
    engine.RestoreGame(decodedText);
}

var result = string.Empty;

while (result != "-1")
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("> ");

    var command = Console.ReadLine();
    result = await engine.GetResponse(command);

    string json = engine.SaveGame();
    var bytesToEncode = Encoding.UTF8.GetBytes(json);
    var encodedText = Convert.ToBase64String(bytesToEncode);
    await database.WriteSession(sessionId, encodedText);

    if (result?.Trim().StartsWith("-2") ?? false)
    {
        engine = CreateEngine<Planetfall.Planetfall, PlanetfallContext>();
        Console.WriteLine(engine.IntroText);
        continue;
    }

    if (result?.Trim().StartsWith("-1") ?? false) Environment.Exit(0);

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Utilities.Utilities.WriteLineWordWrap(result);
}


GameEngine<TGame, TContext> CreateEngine<TGame, TContext>() 
    where TContext : Context<TGame>, new()
    where TGame : class, IInfocomGame, new()
{
    ILoggerFactory loggerFactory;
    
    if (Debugger.IsAttached)
    {
        loggerFactory = LoggerFactory.Create(builder =>
            builder
                .AddConsole()
                .AddDebug()
                .AddFilter((category, level) =>
                {
                    if (category!.Contains("GameEngine.GameEngine"))
                        return true;

                    return false;
                })
                .SetMinimumLevel(LogLevel.Debug)
        );
    }
    else
    {
        loggerFactory = LoggerFactory.Create(builder =>
        
            builder
                .AddDebug()
                .AddFilter((category, level) =>
                {
                    if (category!.Contains("GameEngine.GameEngine"))
                        return true;

                    return false;
                })
                .SetMinimumLevel(LogLevel.Warning)
        );
    }

    var logger = loggerFactory.CreateLogger<GameEngine<TGame, TContext>>();
    var gameEngine = new GameEngine<TGame, TContext>(logger)
    {
        Runtime = Runtime.Console
    };
    return gameEngine;
}