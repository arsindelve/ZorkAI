using GameEngine;
using GameEngine.Item;
using GameEngine.Location;
using Model;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.Location;

namespace ZorkOne.Item;

public class Bat : ItemBase
{
    public override string[] NounsForMatching => ["bat", "vampire bat", "vampire"];

    public override string CannotBeTakenDescription => "You can't reach him; he's on the ceiling. ";

    /// <summary>
    /// Provoking the bat (attacking or trying to take it) without garlic carries you off, same as
    /// just walking into the room without it - see BatRoom.CarryPlayerOff.
    /// </summary>
    private static readonly string[] ProvokingVerbs =
        Verbs.KillVerbs.Concat(["hold", "take", "pick up", "grab", "get", "acquire", "snatch"]).ToArray();

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return GenericDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "In the corner of the room on the ceiling is a large vampire bat who is obviously deranged and holding his nose.";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.MatchNoun(NounsForMatching))
        {
            var batRoom = Repository.GetLocation<BatRoom>();

            if (context.CurrentLocation == batRoom && action.MatchVerb(ProvokingVerbs) &&
                !batRoom.IsSafeFromBat(context))
            {
                var destination = batRoom.CarryPlayerOff(context);
                var destinationDescription = context.ItIsDarkHere
                    ? ((DarkLocation)destination).DarkDescription
                    : destination.GetDescription(context);

                return new PositiveInteractionResult($"{BatRoom.CarryOffText}\n\n{destinationDescription} ");
            }

            return new PositiveInteractionResult(CannotBeTakenDescription);
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}