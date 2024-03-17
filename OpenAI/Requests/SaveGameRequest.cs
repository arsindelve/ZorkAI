namespace OpenAI.Requests;

public class SaveGameRequest : Request
{
    public SaveGameRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage = "The adventurer wants to save their game. Ask them in funny, rhetorical sentence or two " +
                      "if they are about to do something risky or dangerous.";
    }
}