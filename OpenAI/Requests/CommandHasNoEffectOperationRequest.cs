namespace OpenAI.Requests;

public class CommandHasNoEffectOperationRequest : Request
{
    public CommandHasNoEffectOperationRequest(string location, string? command)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". They wrote \"{command}\". If the player references any object of " +
            $"any kind, your response " +
            $"will indicate that no such object can be found here. Provide the narrator's response";
    }
}