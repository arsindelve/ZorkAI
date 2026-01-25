using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Item.Lawanda;

public class FusedBedistor : BedistorBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["fused ninety-ohm bedistor", "ninety-ohm bedistor", "bedistor", "fused", "fused bedistor"];

    public override string? CannotBeTakenDescription =>
        CurrentLocation is LargeMetalCube ? "It seems to be fused to its socket. " : null;

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a fused ninety-ohm bedistor here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A fused ninety-ohm bedistor";
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        // Only allow removal with pliers if bedistor is in the cube
        if (CurrentLocation is not LargeMetalCube cube)
            return await base.RespondToMultiNounInteraction(action, context);

        if (!context.HasItem<Pliers>())
            return new NoNounMatchInteractionResult();

        var match = action.Match<Pliers>(["take", "remove"], NounsForMatching, ["with", "using"]);

        // Also support "use pliers on bedistor"
        match |= action.Match<FusedBedistor>(["use"], Repository.GetItem<Pliers>().NounsForMatching, ["on"]);

        if (match)
        {
            // Remove from the cube and place in the player's inventory
            cube.RemoveItem(this);
            context.ItemPlacedHere(this);
            return new PositiveInteractionResult("With a tug, you manage to remove the fused bedistor. ");
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }
}
