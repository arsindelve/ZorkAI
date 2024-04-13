using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Model;
using ZorkWeb.Server.Model;

namespace ZorkWeb.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ZorkOneController(
    ILogger<ZorkOneController> logger,
    IGameEngine engine,
    ISessionRepository sessionRepository)
    : ControllerBase
{
    private readonly string _sessionId = Environment.MachineName;

    [HttpPost]
    public async Task<GameResponse> Index([FromBody] GameRequest request)
    {
        var savedSession = await GetSavedSession();
        if (!string.IsNullOrEmpty(savedSession)) RestoreSession(savedSession);

        logger.LogInformation($"Request: {request.Input}");
        var response = await engine.GetResponse(request.Input);
        logger.LogInformation($"Response: {response}");

        await WriteSession();
        return new GameResponse(response!, engine.LocationName, engine.Moves, engine.Score);
    }

    private async Task WriteSession()
    {
        var bytesToEncode = Encoding.UTF8.GetBytes(engine.SaveGame());
        var encodedText = Convert.ToBase64String(bytesToEncode);
        await sessionRepository.WriteSession(_sessionId, encodedText);
    }

    private void RestoreSession(string savedGame)
    {
        var decodedBytes = Convert.FromBase64String(savedGame);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);
        engine.RestoreGame(decodedText);
    }

    private async Task<string?> GetSavedSession()
    {
        var savedGame = await sessionRepository.GetSession(_sessionId);
        return savedGame;
    }
}