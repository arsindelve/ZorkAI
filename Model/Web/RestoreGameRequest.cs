namespace Model.Web;

public record RestoreGameRequest(string SessionId, string ClientId, string Id);