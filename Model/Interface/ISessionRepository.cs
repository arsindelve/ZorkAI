namespace Model.Interface;

public interface ISessionRepository
{
    Task<string?> GetSession(string sessionId, string tableName);

    Task WriteSession(string sessionId, string gameData, string tableName);
}