namespace Model.Web;

public record GameRequest(string Input, string SessionId, bool NoGeneratedResponses = false);