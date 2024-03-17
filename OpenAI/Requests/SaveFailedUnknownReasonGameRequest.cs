namespace OpenAI.Requests;

public class SaveFailedUnknownReasonGameRequest : Request
{
    public SaveFailedUnknownReasonGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The adventurer has attempted to save their game but " +
            $" there was an unknown error and it did not succeed. Let them know.";
    }
}