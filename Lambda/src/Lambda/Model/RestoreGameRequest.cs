namespace Lambda.Model;

public record RestoreGameRequest(string SessionId, string ClientId, string Id);