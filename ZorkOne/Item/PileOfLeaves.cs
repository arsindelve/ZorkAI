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
        var clearing = Repository.GetLocation<Clearing>();
        var grating = Repository.GetItem<Grating>();

        // Only reveal grating if not already visible
        if (!clearing.Items.Contains(grating))
        {
            clearing.ItemPlacedHere(grating);
            return "In disturbing the pile of leaves, a grating is revealed. ";
        }

        return "Taken. ";
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
            var clearing = Repository.GetLocation<Clearing>();
            var grating = Repository.GetItem<Grating>();

            // Check if grating is already revealed
            if (clearing.Items.Contains(grating))
            {
                return new PositiveInteractionResult(
                    "The leaves have already been moved. The grating is clearly visible. "
                );
            }

            HasEverBeenPickedUp = true;
            clearing.ItemPlacedHere(grating);
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