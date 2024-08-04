namespace Model.AIGeneration.Requests;

public class MissingFirstNounMultiNounOperationRequest : MultiNounRequest
{
    public override string UserMessage =>
        $"The player is in this location: \"{Location}\". " +
        $"They wrote \"{Verb} the {NounOne} {Preposition} the {NounTwo}\", and while " +
        $"there is a \"{NounTwo}\" here, there is no \"{NounOne}\" here " +
        $"so that action has no effect on the story. Provide the narrator's response";
}