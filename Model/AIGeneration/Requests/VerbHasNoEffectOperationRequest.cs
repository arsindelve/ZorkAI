namespace Model.AIGeneration.Requests;

public class VerbHasNoEffectOperationRequest : Request
{
    public VerbHasNoEffectOperationRequest(string location, string? noun, string verb)
    {
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} the {noun}\", and while there is a \"{noun}\" here, " +
            $"that action has no effect on the story. Provide the narrator's response " +
            $"indicating that their action is silly or pointless and so they change their mind and don't bother. " +
            // Anti-hallucination guard: don't embellish the deflection with new objects/characters.
            "Do not invent, name, or describe any object, item, scenery, exit, or character that is " +
            "not already explicitly present in the location description above. Do not alter the state " +
            "of the game or add new information.";

        // Deflection responses should not embellish; keep creativity low to avoid invented detail.
        Temperature = 0.4f;
    }
}