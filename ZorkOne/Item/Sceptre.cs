using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Sceptre : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp, IAmPointyAndPunctureThings
{
    public override string[] NounsForMatching => ["sceptre", "ornamental sceptre"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "An ornamented sceptre, tapering to a sharp point, is here.";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A sceptre, possibly that of ancient Egypt itself, is in the coffin." +
               " The sceptre is ornamented with colored enamel, and tapers to a sharp point. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 4;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 6;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A sceptre";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Verb.ToLowerInvariant() != "wave")
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (!action.MatchNoun(NounsForMatching))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        return new PositiveInteractionResult("A dazzling display of color briefly emanates from the sceptre. ");
    }
}