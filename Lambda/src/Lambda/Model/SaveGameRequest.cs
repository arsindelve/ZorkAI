namespace Lambda.Model;

public record SaveGameRequest(string SessionId, string ClientId, string Name, string? Id);