namespace OpenAI.Requests;

public class RestoreFailedFileNotFoundGameRequest : Request
{
    public RestoreFailedFileNotFoundGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The adventurer has attempted to restored their game from a previous saved game but the file they " +
            $"typed does not exist. Let them know.";
    }
}