namespace Model.AIGeneration.Requests;

public class BeforeSaveGameRequest : Request
{
    public BeforeSaveGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The adventurer is in this location: \"{location}\" and wants to save their game. Ask them in funny, rhetorical sentence or two " +
            "if they are about to do something risky or dangerous.";
    }
}