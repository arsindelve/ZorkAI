namespace Model.AIGeneration.Requests;

public class MissingSecondNounWithPersonMultiNounOperationRequest : MultiNounRequest
{
    public override string UserMessage =>
        $"The player is in this location: \"{Location}\". " +
        $"They wrote \"{Verb} {NounOne} {Preposition} the {NounTwo}\", and while " +
        $"there is a person named  \"{NounOne}\" here, (who is described as {PersonOneDescription}) there is no \"{NounTwo}\" here " +
        $"so that action has no effect on the story. Provide the narrator's response";
}