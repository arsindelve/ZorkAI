using API.Model;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ZorkOneController : ControllerBase
{
    private readonly string _sessionId = Environment.MachineName;
    private readonly ILogger<ZorkOneController> _logger;
    private readonly ISessionRepository _database;

    public ZorkOneController(ILogger<ZorkOneController> logger)
    {
        _logger = logger;
        _database = new SessionRepository();
    }

    [HttpGet]
    public string Index()
    {
        _logger.LogInformation("Hello!");
        return "Hello!";
    }
    
    [HttpPost]
    public async Task<GameResponse> Index([FromBody] GameRequest request)
    {
        var engine = CreateEngine();

        var savedSession = await GetSavedSession();
        if (!string.IsNullOrEmpty(savedSession))
        {
            RestoreSession(savedSession, engine);
        }

        _logger.LogInformation($"Request: {request.Input}");
        var response = await engine.GetResponse(request.Input);
        _logger.LogInformation($"Response: {response}");

        await WriteSession(engine);
        return new GameResponse(response!, engine.LocationName, engine.Moves, engine.Score);
    }

    private async Task WriteSession(IGameEngine engine)
    {
        var bytesToEncode = Encoding.UTF8.GetBytes(engine.SaveGame());
        var encodedText = Convert.ToBase64String(bytesToEncode);
        await _database.WriteSession(_sessionId, encodedText);
    }

    private void RestoreSession(string savedGame, IGameEngine engine)
    {
        var decodedBytes = Convert.FromBase64String(savedGame);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);
        engine.RestoreGame(decodedText);
    }

    private async Task<string?> GetSavedSession()
    {
        var savedGame = await _database.GetSession(_sessionId);
        return savedGame;
    }

    private IGameEngine CreateEngine()
    {
        return new GameEngine<ZorkI, ZorkIContext>();
    }
}