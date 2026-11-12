using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Item.Lawanda;

public class FusedBedistor : BedistorBase, ICanBeTakenAndDropped
{
    private static readonly VerbFamily TakeRemoveVerbs = new("take", "remove");
    private static readonly VerbFamily UseVerb = new("use");

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

        var match = action.Match<Pliers>(TakeRemoveVerbs, NounsForMatching, ["with", "using"]);

        // Also support "use pliers on bedistor"
        match |= action.Match<FusedBedistor>(UseVerb, Repository.GetItem<Pliers>().NounsForMatching, ["on"]);

        if (match)
        {
            // Container-aware possession (issue #503): the pliers are small enough to pocket, and the
            // flat context.HasItem<T>() would refuse the whole action if they were. Checked after the
            // match, since the pliers are resolved by type - this is what scopes them to the player.
            if (!context.IsCarrying<Pliers>())
                return new NoNounMatchInteractionResult();

            // Remove from the cube and place in the player's inventory
            cube.RemoveItem(this);
            context.ItemPlacedHere(this);
            return new PositiveInteractionResult("With a tug, you manage to remove the fused bedistor. ");
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }
}
