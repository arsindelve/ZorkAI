namespace Model.Web;

public record SaveGameRequest(string SessionId, string ClientId, string Name, string? Id);