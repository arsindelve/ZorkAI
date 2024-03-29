namespace Model.AIGeneration.Requests;

public class AfterSaveGameRequest : Request
{
    public AfterSaveGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The adventurer is in this location: \"{location}\" and has successfully saved " +
            $"their game. Let them know it succeeded. ";
    }
}