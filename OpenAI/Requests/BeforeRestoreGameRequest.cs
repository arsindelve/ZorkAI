namespace OpenAI.Requests;

public class BeforeRestoreGameRequest : Request
{
    public BeforeRestoreGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage = $"The adventurer is in this location: \"{location}\" and wants to restore their game. Tell them in a funny sentence or two " +
                      "that it's unfortunate that their adventure was not going well. ";
    }
}