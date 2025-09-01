namespace Model.Interface;

public interface ISavedGameRepository
{
    Task<string?> GetSavedGame(string id, string sessionId, string tableName);

    Task<string> SaveGame(string? id, string clientId, string name, string gameData, string tableName);

    Task<List<(string Id, string Name, DateTime SavedOn)>> GetSavedGames(string sessionId, string tableName);

    Task DeleteSavedGame(string id, string sessionId, string tableName);
}