namespace Model.Interface;

public interface ISavedGameRepository
{
    Task<string?> GetSavedGame(string id, string sessionId);
    
    Task<string> SaveGame(string sessionId, string name, string gameData);
    
    Task<List<(string Id, string Name, DateTime SavedOn)>> GetSavedGames(string sessionId);
}