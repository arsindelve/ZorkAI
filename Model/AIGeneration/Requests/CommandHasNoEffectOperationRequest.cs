namespace Model.AIGeneration.Requests;

public class CommandHasNoEffectOperationRequest : Request
{
    public CommandHasNoEffectOperationRequest(string location, string? command)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". They wrote \"{command}\". If the player references any object of " +
            $"any kind, other than what is described above, your response " +
            $"will indicate that no such object can be found here. Provide the narrator's response " +
            $"Provide the narrator's response indicating that their action is silly or pointless and so " +
            $"they change their mind and don't bother";
    }
}