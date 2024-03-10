namespace OpenAI.Requests;

public class VerbHasNoEffectMultiNounOperationRequest : Request
{
    public VerbHasNoEffectMultiNounOperationRequest(string location, string? nounOne, string? nounTwo, string? preposition, string verb)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} the {nounOne} {preposition} the {nounTwo}\", and while " +
            $"there is a \"{nounOne}\" here, and a {nounTwo} here, " +
            $"that action has no effect on the story. Provide the narrator's response";
    }
}