namespace OpenAI.Requests;

public class MultiNounOperationBothMissingRequest : Request
{
    public MultiNounOperationBothMissingRequest(string location, string? nounOne, string? nounTwo,
        string? preposition, string verb)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} the {nounOne} {preposition} the {nounTwo}\", but  " +
            $"there is no \"{nounOne}\" here, and no {nounTwo} here, " +
            "Provide the narrator's response indicating that their " +
            "action impossible. ";
    }
}