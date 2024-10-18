using System.Text;
using Microsoft.AspNetCore.Mvc;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Web;

namespace Lambda.Controllers;

[ApiController]
[Route("[controller]")]
public class PlanetfallController(
    ILogger<PlanetfallController> logger,
    IGameEngine engine,
    ISessionRepository sessionRepository,
    ISavedGameRepository savedGameRepository
)
    : ControllerBase
{
    private const string SaveGameTableName = "planetfall_savegame";
    private const string SessionTableName = "planetfall_session";

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

    [HttpPost]
    [Route("restoreGame")]
    public async Task<GameResponse> RestoreGame([FromBody] RestoreGameRequest request)
    {
        var gameData = await savedGameRepository.GetSavedGame(request.Id, request.ClientId, SaveGameTableName);

        if (string.IsNullOrEmpty(gameData))
            throw new ArgumentException($"Saved gamed {request.Id} had empty game data");

        RestoreSession(gameData);

        var sb = new StringBuilder();
        sb.AppendLine(
            await engine.GenerationClient.CompleteChat(new AfterRestoreGameRequest(engine.LocationDescription)));
        sb.AppendLine();
        sb.AppendLine(await engine.GetResponse("look"));

        await WriteSession(request.SessionId);
        return new GameResponse(sb.ToString(), engine.LocationName, engine.Moves, engine.Score);
    }

    [HttpPost]
    [Route("saveGame")]
    public async Task<string> SaveGame([FromBody] SaveGameRequest request)
    {
        var savedSession = await GetSavedSession(request.SessionId);

        if (string.IsNullOrEmpty(savedSession))
            throw new ArgumentException("Session had empty game data before attempting save game.");

        RestoreSession(savedSession);
        var encodedText = GetGameData();
        await savedGameRepository.SaveGame(request.Id, request.ClientId, request.Name, encodedText, SaveGameTableName);
        return await engine.GenerationClient.CompleteChat(new AfterSaveGameRequest(engine.LocationDescription));
    }

    [HttpGet]
    [Route("saveGame")]
    public async Task<List<SavedGame>> GetAllSavedGames([FromQuery] string sessionId)
    {
        var results = await savedGameRepository.GetSavedGames(sessionId, SaveGameTableName);
        return results
            .OrderByDescending(s => s.SavedOn)
            .Select(s => new SavedGame(s.Id, s.Name, s.SavedOn))
            .ToList();
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
        var encodedText = GetGameData();
        await sessionRepository.WriteSession(sessionId, encodedText, SessionTableName);
    }

    private string GetGameData()
    {
        var bytesToEncode = Encoding.UTF8.GetBytes(engine.SaveGame());
        var encodedText = Convert.ToBase64String(bytesToEncode);
        return encodedText;
    }

    private void RestoreSession(string savedGame)
    {
        var decodedBytes = Convert.FromBase64String(savedGame);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);
        engine.RestoreGame(decodedText);
    }

    private async Task<string?> GetSavedSession(string sessionId)
    {
        var savedGame = await sessionRepository.GetSession(sessionId, SessionTableName);
        return savedGame;
    }
}