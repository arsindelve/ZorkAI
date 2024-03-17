namespace OpenAI.Requests;

public class MissingBothNounsMultiNounOperationRequest : MultiNounRequest
{
    public MissingBothNounsMultiNounOperationRequest()
    {
        SystemMessage = SystemPrompt;
    }

    public override string UserMessage =>
        $"The player is in this location: \"{Location}\". " +
        $"They wrote \"{Verb} the {NounOne} {Preposition} the {NounTwo}\", but  " +
        $"there is no \"{NounOne}\" here, and no {NounTwo} here, " +
        "Provide the narrator's response indicating that their " +
        "action is impossible. ";
}
