namespace Model.AIGeneration.Requests;

public class MissingSecondNounMultiNounOperationRequest : MultiNounRequest
{
    public MissingSecondNounMultiNounOperationRequest()
    {
        SystemMessage = SystemPrompt;
    }

    public override string UserMessage =>
        $"The player is in this location: \"{Location}\". " +
        $"They wrote \"{Verb} the {NounOne} {Preposition} the {NounTwo}\", and while " +
        $"there is a \"{NounOne}\" here, there is no \"{NounTwo}\" here " +
        $"so that action has no effect on the story. Provide the narrator's response";
}