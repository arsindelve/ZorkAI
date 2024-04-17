namespace Model;

public interface ISessionRepository
{
    Task<string?> GetSession(string sessionId);

    Task WriteSession(string sessionId, string gameData);
}