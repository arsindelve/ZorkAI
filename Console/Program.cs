using System.Diagnostics;
using System.Text;
using ChatLambda;
using DynamoDb;
using GameEngine;
using Microsoft.Extensions.Logging;
using Model;
using Model.Interface;
using Planetfall;
using SecretsManager;
using ZorkOne;

var database = new DynamoDbSessionRepository();
var sessionId = Environment.MachineName + "8";

Console.ForegroundColor = ConsoleColor.DarkCyan;

var engine = await GetEngine();

var savedGame = await database.GetSessionState(sessionId, engine.SessionTableName);
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

    var json = engine.SaveGame();
    var bytesToEncode = Encoding.UTF8.GetBytes(json);
    var encodedText = Convert.ToBase64String(bytesToEncode);
    await database.WriteSessionState(sessionId, encodedText, engine.SessionTableName);

    if (result?.Trim().StartsWith("-2") ?? false)
    {
        engine = await GetEngine();
        Console.WriteLine(engine.IntroText);
        continue;
    }

    if (result?.Trim().StartsWith("-1") ?? false) Environment.Exit(0);

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine(result);
}


async Task<GameEngine<TGame, TContext>> CreateEngine<TGame, TContext>()
    where TContext : Context<TGame>, new()
    where TGame : class, IInfocomGame, new()
{
    ILoggerFactory loggerFactory;

    if (Debugger.IsAttached)
        loggerFactory = LoggerFactory.Create(builder =>
            builder
                .AddConsole()
                .AddDebug()
                .AddFilter((category, _) =>
                {
                    if (category!.Contains("GameEngine.GameEngine"))
                        return true;

                    return false;
                })
                .SetMinimumLevel(LogLevel.Debug)
        );
    else
        loggerFactory = LoggerFactory.Create(builder =>
            builder
                .AddDebug()
                .AddFilter((category, _) =>
                {
                    if (category!.Contains("GameEngine.GameEngine"))
                        return true;

                    return false;
                })
                .SetMinimumLevel(LogLevel.Warning)
        );

    var logger = loggerFactory.CreateLogger<GameEngine<TGame, TContext>>();
    var parseLogger = loggerFactory.CreateLogger<ParseConversation>();

    var gameEngine = new GameEngine<TGame, TContext>(logger, new AmazonSecretsManager(), new ParseConversation(null, parseLogger))
    {
        Runtime = Runtime.Console
    };
    await gameEngine.InitializeEngine();
    return gameEngine;
}

async Task<IGameEngine> GetEngine()
{
    IGameEngine newEngine = args[0] switch
    {
        "Planetfall" => await CreateEngine<PlanetfallGame, PlanetfallContext>(),
        "ZorkOne" => await CreateEngine<ZorkI, ZorkIContext>(),
        //"ZorkTwo" => CreateEngine<ZorkII, ZorkIIContext>(),

        _ => throw new InvalidOperationException($"Unsupported engine type: {args[0]}")
    };

    return newEngine;
}