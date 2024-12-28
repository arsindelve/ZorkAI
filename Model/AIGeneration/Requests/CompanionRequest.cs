namespace Model.AIGeneration.Requests;

public sealed class CompanionRequest : Request
{
    public CompanionRequest(string userMessage, string systemMessage)
    {
        UserMessage = userMessage;
        SystemMessage = systemMessage;
    }

    public string SystemMessage { get; }
}