namespace Model.AIGeneration.Requests;

public class AfterRestoreGameRequest : Request
{
    public AfterRestoreGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The adventurer has restored their game from a previous saved game and is now in this location: \"{location}.\"" +
            " Tell them in a funny sentence or two " +
            "that their game restored successfully and wish them better luck this time. ";
    }
}