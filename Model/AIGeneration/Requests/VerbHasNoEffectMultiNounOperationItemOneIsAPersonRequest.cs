namespace Model.AIGeneration.Requests;

public class VerbHasNoEffectMultiNounOperationItemOneIsAPersonRequest : Request
{
    public VerbHasNoEffectMultiNounOperationItemOneIsAPersonRequest(string location, string? nounOne, string? nounTwo,
        string? preposition, string verb, string personDescription)
    {
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} {nounOne} {preposition} the {nounTwo}\", and while " +
            $"there is a person named \"{nounOne}\" here (described as \"{personDescription}\"), and a {nounTwo} here, " +
            $"that action has no effect on the story. Provide the narrator's response indicating that their " +
            $"action is silly or pointless and so " +
            $"they change their mind and don't bother";
    }
}