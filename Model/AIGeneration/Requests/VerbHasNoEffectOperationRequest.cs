namespace Model.AIGeneration.Requests;

public class VerbHasNoEffectOperationRequest : Request
{
    public VerbHasNoEffectOperationRequest(string location, string? noun, string verb)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} the {noun}\", and while there is a \"{noun}\" here, " +
            $"that action has no effect on the story. Provide the narrator's response " +
            $"indicating that their action is silly or pointless and so they change their mind and don't bother. ";
    }
}