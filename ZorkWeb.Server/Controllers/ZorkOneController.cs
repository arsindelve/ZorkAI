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
    [HttpPost]
    public async Task<GameResponse> Index([FromBody] GameRequest request)
    {
        var savedSession = await GetSavedSession(request.SessionId);
        if (!string.IsNullOrEmpty(savedSession)) RestoreSession(savedSession);

        logger.LogInformation($"Request: {request.Input}");
        var response = await engine.GetResponse(request.Input);
        logger.LogInformation($"Response: {response}");

        await WriteSession(request.SessionId);
        return new GameResponse(response!, engine.LocationName, engine.Moves, engine.Score);
    }

    [HttpGet]
    public async Task<GameResponse> Index([FromQuery] string sessionId)
    {
        var savedSession = await GetSavedSession(sessionId);
        if (!string.IsNullOrEmpty(savedSession))
        {
            RestoreSession(savedSession);
            var response = await engine.GetResponse("look");
            return new GameResponse(response!, engine.LocationName, engine.Moves, engine.Score);
        }
        
        return new GameResponse(engine.IntroText, engine.LocationName, engine.Moves, engine.Score);
    }

    private async Task WriteSession(string sessionId)
    {
        var bytesToEncode = Encoding.UTF8.GetBytes(engine.SaveGame());
        var encodedText = Convert.ToBase64String(bytesToEncode);
        await sessionRepository.WriteSession(sessionId, encodedText);
    }

    private void RestoreSession(string savedGame)
    {
        var decodedBytes = Convert.FromBase64String(savedGame);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);
        engine.RestoreGame(decodedText);
    }

    private async Task<string?> GetSavedSession(string sessionId)
    {
        var savedGame = await sessionRepository.GetSession(sessionId);
        return savedGame;
    }
}