using System.ComponentModel;
using System.Text;
using Azure;
using DynamoDb;
using Game;
using Microsoft.Extensions.Logging;
using Model;
using ZorkOne;

var database = new RedisSessionRepository();

var sessionId = Environment.MachineName;
var savedGame = await database.GetSession(sessionId);

Console.ForegroundColor = ConsoleColor.DarkCyan;

var engine = CreateEngine();

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

    var bytesToEncode = Encoding.UTF8.GetBytes(engine.SaveGame());
    var encodedText = Convert.ToBase64String(bytesToEncode);
    await database.WriteSession(sessionId, encodedText);

    if (result?.Trim().StartsWith("-2") ?? false)
    {
        engine = CreateEngine();
        Console.WriteLine(engine.IntroText);
        continue;
    }

    if (result?.Trim().StartsWith("-1") ?? false) Environment.Exit(0);

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Utilities.Utilities.WriteLineWordWrap(result);
}

GameEngine<ZorkI, ZorkIContext> CreateEngine()
{
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    var logger = loggerFactory.CreateLogger<GameEngine<ZorkI, ZorkIContext>>();
    var engine = new GameEngine<ZorkI, ZorkIContext>(logger);
    engine.Runtime = Runtime.Console;
    return engine;
}