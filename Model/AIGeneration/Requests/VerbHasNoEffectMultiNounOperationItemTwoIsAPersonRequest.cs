namespace Model.AIGeneration.Requests;

public class VerbHasNoEffectMultiNounOperationItemTwoIsAPersonRequest : Request
{
    public VerbHasNoEffectMultiNounOperationItemTwoIsAPersonRequest(string location, string? nounOne, string? nounTwo,
        string? preposition, string verb, string personDescription)
    {
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} the {nounOne} {preposition} {nounTwo}\", and while " +
            $"there is a \"{nounOne}\" here, and a person named {nounTwo} here (described as \"{personDescription}\"), " +
            $"that action has no effect on the story. Provide the narrator's response indicating that their " +
            $"action is silly or pointless and so " +
            $"they change their mind and don't bother";
    }
}