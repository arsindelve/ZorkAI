using Model.AIGeneration;
using Model.Intent;

namespace ZorkOne.Item;

public class Sceptre : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["sceptre", "ornamental sceptre"];

    public override string InInventoryDescription => "A sceptre";

    public string ExaminationDescription => "There's nothing special about the sceptre. ";

    public string OnTheGroundDescription => "An ornamented sceptre, tapering to a sharp point, is here.";

    public override string NeverPickedUpDescription =>
        "A sceptre, possibly that of ancient Egypt itself, is in the coffin." +
        " The sceptre is ornamented with colored enamel, and tapers to a sharp point.";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 4;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 6;

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Verb.ToLowerInvariant() != "wave")
            return base.RespondToSimpleInteraction(action, context, client);

        if (!action.MatchNoun(NounsForMatching))
            return base.RespondToSimpleInteraction(action, context, client);

        return new PositiveInteractionResult("A dazzling display of color briefly emanates from the sceptre.");
    }
}