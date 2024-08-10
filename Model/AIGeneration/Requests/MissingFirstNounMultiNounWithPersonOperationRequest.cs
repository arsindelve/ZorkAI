namespace Model.AIGeneration.Requests;

public class MissingFirstNounMultiNounWithPersonOperationRequest : MultiNounRequest
{
    public override string UserMessage =>
        $"The player is in this location: \"{Location}\". " +
        $"They wrote \"{Verb} the {NounOne} {Preposition} {NounTwo}\", and while " +
        $"there is a person named \"{NounTwo}\" here (who is described as: \"{PersonTwoDescription}\"), " +
        $"there is no \"{NounOne}\" here " +
        $"so that action has no effect on the story. Provide the narrator's response";
}