using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Bat : ItemBase
{
    public override string[] NounsForMatching => ["bat", "vampire bat", "vampire"];

    public override string CannotBeTakenDescription => "You can't reach him; he's on the ceiling. ";

    public override string NeverPickedUpDescription =>
        "In the corner of the room on the ceiling is a large vampire bat who is obviously deranged and holding his nose.";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.MatchNoun(NounsForMatching))
            return new PositiveInteractionResult(CannotBeTakenDescription);

        return base.RespondToSimpleInteraction(action, context, client);
    }
}