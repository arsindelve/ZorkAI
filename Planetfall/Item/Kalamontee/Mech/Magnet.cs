using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee.Mech;

public class Magnet : ItemBase, ICanBeTakenAndDropped, ITurnBasedActor
{
    public override string[] NounsForMatching => ["curved metal bar", "metal bar", "bar", "magnet", "metal bar, curved into a U-shape"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a curved metal bar here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On an upper shelf is a metal bar, curved into a U-shape. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A curved metal bar";
    }

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        context.RegisterActor(this);
        return base.OnBeingTaken(context, previousLocation);
    }

    /// <summary>
    /// I-MAGNET (compone.zil:1595-1612): while held, the magnet corrupts one not-yet-scrambled carried
    /// access card per turn until none are left. The original walks a fixed type-priority order; this
    /// port instead scrambles whichever unscrambled AccessCard the player happens to be holding first -
    /// an acceptable simplification per issue #211 that also lets any future AccessCard subtype
    /// participate automatically, with no hardcoded list to extend. Scrambling is permanent (no
    /// un-scramble, matching the original). Dropping the magnet (CurrentLocation no longer the player)
    /// deregisters it so no further cards are scrambled.
    /// </summary>
    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (CurrentLocation != context)
        {
            context.RemoveActor(this);
            return Task.FromResult(string.Empty);
        }

        var card = context.Items.OfType<AccessCard>().FirstOrDefault(c => !c.Scrambled);
        if (card is not null)
            card.Scrambled = true;

        return Task.FromResult(string.Empty);
    }
}
