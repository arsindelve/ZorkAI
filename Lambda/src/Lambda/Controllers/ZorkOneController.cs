using System.Text;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Web;

namespace Lambda.Controllers;

[ApiController]
[Route("[controller]")]
public class ZorkOneController(
    ILogger<ZorkOneController> logger,
    IGameEngine engine,
    ISessionRepository sessionRepository,
    ISavedGameRepository savedGameRepository,
    IUserPreferencesService userPreferencesService
)
    : ControllerBase
{
    private const string SaveGameTableName = "zork1_savegame";
    private const string SessionTableName = "zork1_session";
    private const string SessionStepsTableName = "zork1_session_steps";

    [HttpPost]
    public async Task<GameResponse> Index([FromBody] GameRequest request)
    {
        await engine.InitializeEngine();
        var savedSession = await GetSavedSession(request.SessionId);
        if (!string.IsNullOrEmpty(savedSession)) RestoreSession(savedSession);

        logger.LogInformation($"Request: {request.Input}");
        var response = await engine.GetResponse(request.Input);
        logger.LogInformation($"Response: {response}");

        await WriteSession(request.SessionId, request.Input, response!);
        return new GameResponse(response!, engine);
    }

    [HttpPost]
    [Route("restoreGame")]
    public async Task<GameResponse> RestoreGame([FromBody] RestoreGameRequest request)
    {
        await engine.InitializeEngine();
        var gameData = await savedGameRepository.GetSavedGame(request.Id, request.ClientId, SaveGameTableName);

        if (string.IsNullOrEmpty(gameData))
            throw new ArgumentException($"Saved gamed {request.Id} had empty game data");

        RestoreSession(gameData);

        var sb = new StringBuilder();
        sb.AppendLine(
            await engine.GenerationClient.GenerateNarration(new AfterRestoreGameRequest(engine.LocationDescription), String.Empty));
        sb.AppendLine();
        sb.AppendLine(await engine.GetResponse("look"));

        await WriteSession(request.SessionId, "restore", sb.ToString());
        return new GameResponse(sb.ToString(), engine);
    }

    [HttpPost]
    [Route("saveGame")]
    public async Task<string> SaveGame([FromBody] SaveGameRequest request)
    {
        await engine.InitializeEngine();
        var (savedSession, _) = await GetSavedSessionWithHistory(request.SessionId);

        if (string.IsNullOrEmpty(savedSession))
            throw new ArgumentException("Session had empty game data before attempting save game.");

        RestoreSession(savedSession);
        var encodedText = GetGameData();
        await savedGameRepository.SaveGame(request.Id, request.ClientId, request.Name, encodedText, SaveGameTableName);
        return await engine.GenerationClient.GenerateNarration(new AfterSaveGameRequest(engine.LocationDescription), String.Empty);
    }

    [HttpGet]
    [Route("saveGame")]
    public async Task<List<SavedGame>> GetAllSavedGames([FromQuery] string sessionId)
    {
        await engine.InitializeEngine();
        var results = await savedGameRepository.GetSavedGames(sessionId, SaveGameTableName);
        return results
            .OrderByDescending(s => s.SavedOn)
            .Select(s => new SavedGame(s.Id, s.Name, s.SavedOn))
            .ToList();
    }

    [HttpDelete]
    [Route("saveGame/{id}")]
    public async Task<IActionResult> DeleteSavedGame(string id, [FromQuery] string sessionId)
    {
        await engine.InitializeEngine();
        await savedGameRepository.DeleteSavedGameAsync(id, sessionId, SaveGameTableName);
        return Ok();
    }

    [HttpGet]
    public async Task<GameResponse> Index([FromQuery] string sessionId)
    {
        await engine.InitializeEngine();
        var (savedSession, history) = await GetSavedSessionWithHistory(sessionId);
        
        if (!string.IsNullOrEmpty(savedSession))
        {
            RestoreSession(savedSession);
            var response = string.IsNullOrEmpty(history) ? await engine.GetResponse("look") : history;
            return new GameResponse(response!, engine);
        }

        return new GameResponse(engine.IntroText, engine);
    }

    private async Task WriteSession(string sessionId, string input, string response)
    {
        await sessionRepository.WriteSessionState(sessionId, GetGameData(), SessionTableName);
        await sessionRepository.WriteSessionStep(sessionId, engine.Moves, input, response, SessionStepsTableName);
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

    private async Task<(string?, string?)> GetSavedSessionWithHistory(string sessionId)
    {
        var savedGame = await sessionRepository.GetSessionState(sessionId, SessionTableName);
        string history = await sessionRepository.GetSessionStepsAsText(sessionId, SessionStepsTableName);
        return (savedGame, history);
    }
    
    private async Task<string?> GetSavedSession(string sessionId)
    {
        var savedGame = await sessionRepository.GetSessionState(sessionId, SessionTableName);
        return savedGame;
    }

    [HttpGet]
    [Route("preferences")]
    public async Task<UserPreferences> GetUserPreferences([FromQuery] string? userId = null)
    {
        return await userPreferencesService.GetPreferencesAsync(userId);
    }

    [HttpPost]
    [Route("preferences")]
    public async Task<IActionResult> SaveUserPreferences([FromBody] SaveUserPreferencesRequest request)
    {
        try
        {
            await userPreferencesService.SavePreferencesAsync(request.Preferences, request.UserId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving user preferences");
            return BadRequest("Failed to save user preferences");
        }
    }

    [HttpPost]
    [Route("preferences/reset")]
    public async Task<IActionResult> ResetUserPreferences([FromBody] ResetUserPreferencesRequest request)
    {
        try
        {
            await userPreferencesService.ResetToDefaultsAsync(request.UserId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting user preferences");
            return BadRequest("Failed to reset user preferences");
        }
    }

    [HttpGet]
    [Route("preferences/export")]
    public async Task<string> ExportUserPreferences([FromQuery] string? userId = null)
    {
        return await userPreferencesService.ExportPreferencesAsync(userId);
    }

    [HttpPost]
    [Route("preferences/import")]
    public async Task<IActionResult> ImportUserPreferences([FromBody] ImportUserPreferencesRequest request)
    {
        try
        {
            await userPreferencesService.ImportPreferencesAsync(request.PreferencesJson, request.UserId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error importing user preferences");
            return BadRequest("Failed to import user preferences");
        }
    }
}