using Model.Intent;
using OpenAI;
using ZorkOne.Interface;

namespace ZorkOne.Item;

public class Sceptre : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching => ["sceptre", "ornamental sceptre"];

    public override string InInventoryDescription => "A sceptre";

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 6;

    public string ExaminationDescription => "There's nothing special about the sceptre. ";

    public string OnTheGroundDescription => "An ornamented sceptre, tapering to a sharp point, is here.";

    public override string NeverPickedUpDescription =>
        "A sceptre, possibly that of ancient Egypt itself, is in the coffin." +
        " The sceptre is ornamented with colored enamel, and tapers to a sharp point.";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client)
    {
        // TODO: Wave the sceptre 
        // A dazzling display of color briefly emanates from the sceptre.

        
        return base.RespondToSimpleInteraction(action, context, client);
    }
}