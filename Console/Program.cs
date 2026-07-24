using System.Diagnostics;
using System.Text;
using ChatLambda;
using DynamoDb;
using EscapeRoom;
using GameEngine;
using Microsoft.Extensions.Logging;
using Model;
using Model.Interface;
using Planetfall;
using SecretsManager;
using Stationfall;
using ZorkConsole;
using ZorkOne;

// Guard the required game argument before touching AWS or the game engine: an empty args used to throw
// IndexOutOfRangeException on args[0], and an unrecognized game threw an uncaught exception. Give the
// user actionable feedback and a non-zero exit code instead.
var gameSelection = GameArgumentResolver.Resolve(args);
if (!gameSelection.IsValid)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(gameSelection.Feedback);
    Console.ResetColor();
    Environment.Exit(1);
}

var gameName = gameSelection.GameName!;

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
        Runtime = Runtime.Console,
        NoGeneratedResponses = false
    };
    await gameEngine.InitializeEngine();
    return gameEngine;
}

async Task<IGameEngine> GetEngine()
{
    IGameEngine newEngine = gameName switch
    {
        "Planetfall" => await CreateEngine<PlanetfallGame, PlanetfallContext>(),
        "Stationfall" => await CreateEngine<StationfallGame, StationfallContext>(),
        "ZorkOne" => await CreateEngine<ZorkI, ZorkIContext>(),
        "EscapeRoom" => await CreateEngine<EscapeRoomGame, EscapeRoomContext>(),
        //"ZorkTwo" => CreateEngine<ZorkII, ZorkIIContext>(),

        // Defense-in-depth: GameArgumentResolver already guaranteed a supported name up front.
        _ => throw new InvalidOperationException($"Unsupported engine type: {gameName}")
    };

    return newEngine;
}