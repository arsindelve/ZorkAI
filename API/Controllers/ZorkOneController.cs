using API.Model;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ZorkOneController : ControllerBase
{
    [HttpGet]
    public async Task<GameResponse> Index()
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

        var response = await engine.GetResponse("look");
        return new GameResponse
        {
            Response = response!,
            LocationName = engine.LocationName,
            Moves = engine.Moves,
            Score = engine.Score
        };
    }

    private IGameEngine CreateEngine()
    {
        return new GameEngine<ZorkI, ZorkIContext>();
    }
}