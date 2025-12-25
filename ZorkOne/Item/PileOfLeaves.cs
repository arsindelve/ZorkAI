using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.Command;

namespace ZorkOne.Item;

public class PileOfLeaves : ItemBase, ICanBeTakenAndDropped, IPluralNoun
{
    public override string[] NounsForMatching => ["leaves", "pile", "pile of leaves"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "On the ground is a pile of leaves. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        Repository.GetLocation<Clearing>().ItemPlacedHere(Repository.GetItem<Grating>());
        return "In disturbing the pile of leaves, a grating is revealed. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A pile of leaves";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(
        SimpleIntent action,
        IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory
        )
    {
        if (action.Match(["count"], NounsForMatching))
            return new PositiveInteractionResult("There are 69,105 leaves here. ");

        if (action.Match(["move", "search"], NounsForMatching))
        {
            HasEverBeenPickedUp = true;
            Repository.GetLocation<Clearing>().ItemPlacedHere(Repository.GetItem<Grating>());
            return new PositiveInteractionResult(
                "In disturbing the pile of leaves, a grating is revealed. "
            );
        }

        if (action.Match(["burn"], NounsForMatching))
        {
            var matches = Repository.GetItem<Matchbook>();
            var candles = Repository.GetItem<Candles>();

            var haveFire = context.HasItem<Torch>();
            haveFire |= context.HasItem<Candles>() && candles.IsOn;
            haveFire |= context.HasItem<Matchbook>() && matches.IsOn;

            if (!haveFire)
                return new PositiveInteractionResult("You don't have any fire to burn the leaves with. ");

            var result = "";

            if (!HasEverBeenPickedUp)
                result += OnBeingTaken(context, null) + Environment.NewLine;

            Repository.DestroyItem<PileOfLeaves>();
            result += "The leaves burn, and so do you. ";
            return new DeathProcessor().Process(result, context);
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}