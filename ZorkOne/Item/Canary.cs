using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Item;

public class Canary : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase, IGivePointsWhenFirstPickedUp
{
    internal const string DestroyedMessage = "It seems to have recently had a bad experience. The mountings for " +
                                             "its jewel-like eyes are empty, and its silver beak is crumpled. " +
                                             "Through a cracked crystal window below its left wing you can see the " +
                                             "remains of intricate machinery. It is not clear what result winding " +
                                             "it would have, as the mainspring seems sprung. ";

    public bool IsDestroyed { get; set; }

    public bool HasDroppedBauble { get; set; }

    public override string[] NounsForMatching => ["canary", "bird", "wind-up canary"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return IsDestroyed ? "There is a broken clockwork canary here. " : "There is a golden clockwork canary here. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 6;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 4;

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.Match(["wind", "wind up", "crank", "twist", "turn"], NounsForMatching))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (IsDestroyed)
            return new PositiveInteractionResult("There is an unpleasant grinding noise from inside the canary. ");

        if (!HasDroppedBauble && context.CurrentLocation is ForestPath or UpATree)
        {
            HasDroppedBauble = true;
            Repository.GetLocation<ForestPath>().ItemPlacedHere(Repository.GetItem<Bauble>());
            return new PositiveInteractionResult(
                "The canary chirps, slightly off-key, an aria from a forgotten opera. From out of the " +
                "greenery flies a lovely songbird. It perches on a limb just over your head and opens its " +
                "beak to sing. As it does so a beautiful brass bauble drops from its mouth, bounces off the top " +
                "of your head, and lands glimmering in the grass. As the canary winds down, the songbird flies away. ");
        }

        return new PositiveInteractionResult(
            "The canary chirps blithely, if somewhat tinnily, for a short time. ");
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return IsDestroyed ? "A broken clockwork canary" : "A golden clockwork canary";
    }
}