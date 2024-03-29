namespace Model.AIGeneration.Requests;

public class RestoreFailedUnknownReasonGameRequest : Request
{
    public RestoreFailedUnknownReasonGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The adventurer has attempted to restored their game from a previous saved game but " +
            $" there was an unknown error and it did not succeed. Let them know.";
    }
}