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
using ZorkAI.OpenAI;
using ZorkConsole;
using ZorkOne;

// Guard the required game argument before touching AWS or the game engine: an empty args used to throw
// IndexOutOfRangeException on args[0], and an unrecognized game threw an uncaught exception. Give the
// user actionable feedback and a non-zero exit code instead. Flags after the game name are ignored
// here — GameArgumentResolver only inspects args[0].
var gameSelection = GameArgumentResolver.Resolve(args);
if (!gameSelection.IsValid)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(gameSelection.Feedback);
    Console.ResetColor();
    Environment.Exit(1);
}

var gameName = gameSelection.GameName!;

// Optional flags after the game name map onto the self-hosted environment variables (issue #383),
// so "ZorkOne --provider ollama --model llama3.1" works without exporting anything first. Must run
// before the settings are read, and before any OpenAI client is constructed.
ApplySelfHostFlags(args);

var settings = OpenAIEndpointSettings.FromEnvironment();

// A custom endpoint on the console means "play entirely locally", so publish that as the explicit
// ZORKAI_SELF_HOSTED opt-in the rest of the engine reads. Set here, in the composition root, rather
// than having each consumer infer "no AWS" from the endpoint variables on its own — see
// SelfHostedMode for why that inference is unsafe anywhere that gets deployed.
if (settings.IsSelfHosted)
    Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, "true");

// Self-hosted mode swaps every cloud dependency for a local one: file-based saves instead of
// DynamoDB, a built-in narrator prompt instead of Secrets Manager, and a local conversation
// classifier instead of the Lambda. Cloud mode is untouched.
ISessionRepository database = settings.IsSelfHosted
    ? new FileSessionRepository()
    : new DynamoDbSessionRepository();

var sessionId = Environment.MachineName + "8";

Console.ForegroundColor = ConsoleColor.DarkCyan;

if (settings.IsSelfHosted)
{
    Console.WriteLine(
        $"Self-hosted AI mode: {settings.Endpoint} (model: {settings.ModelOverride ?? "server default"})");
    await WarnIfEndpointUnreachable(settings.Endpoint!);
}

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

    ISecretsManager secretsManager = settings.IsSelfHosted
        ? new LocalSecretsManager()
        : new AmazonSecretsManager();

    IParseConversation parseConversation = settings.IsSelfHosted
        ? new LocalParseConversation { Logger = parseLogger }
        : new ParseConversation(null, parseLogger);

    var gameEngine = new GameEngine<TGame, TContext>(logger, secretsManager, parseConversation)
    {
        Runtime = Runtime.Console,
        NoGeneratedResponses = false,
        // This is the only place that knows self-hosted play means "no AWS at all"; the engine never
        // infers it from the environment. Everything else keeps CloudWatch telemetry on by default.
        CloudLoggingEnabled = !settings.IsSelfHosted
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

// Maps --provider/--endpoint/--model flags to the ZORKAI_PROVIDER/OPENAI_BASE_URL/OPENAI_MODEL
// environment variables read by OpenAIEndpointSettings. Flags win over pre-existing variables.
static void ApplySelfHostFlags(string[] arguments)
{
    for (var i = 1; i < arguments.Length - 1; i++)
    {
        var value = arguments[i + 1];
        switch (arguments[i].ToLowerInvariant())
        {
            case "--provider":
                Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", value);
                break;
            case "--endpoint":
                Environment.SetEnvironmentVariable("OPENAI_BASE_URL", value);
                break;
            case "--model":
                Environment.SetEnvironmentVariable("OPENAI_MODEL", value);
                break;
        }
    }
}

// Cheap fail-fast: ping the local server's /models endpoint so a player whose LM Studio/Ollama
// isn't running gets a clear warning up front instead of a mid-game timeout. Warning only - the
// game still starts, since the server may come up later.
static async Task WarnIfEndpointUnreachable(Uri endpoint)
{
    try
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        var modelsUrl = endpoint.ToString().TrimEnd('/') + "/models";
        using var response = await httpClient.GetAsync(modelsUrl);
    }
    catch (Exception)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(
            $"Warning: nothing answered at {endpoint}. Is your local AI server running? " +
            "The game will start, but AI commands will fail until it is reachable.");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
    }
}
