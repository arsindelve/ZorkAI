namespace Model.AIGeneration.Requests;

public class VerbHasNoEffectOnAPersonOperationRequest : Request
{
    public VerbHasNoEffectOnAPersonOperationRequest(string location, string? noun, string verb,
        string personDescription)
    {
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} the {noun}\", and while there is a person named \"{noun}\" here, described as \"{personDescription}\" " +
            $"that action has no effect on the story. Provide the narrator's response " +
            $"indicating that their action is silly or pointless and so they change their mind and don't bother. ";
    }
}