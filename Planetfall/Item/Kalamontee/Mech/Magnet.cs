using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Lawanda.Lab;

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
    /// I-MAGNET (compone.zil:1595-1612): while held, the magnet corrupts the first not-yet-scrambled
    /// carried access card, checked each turn in the original's priority order - one card per turn,
    /// so carrying the magnet long enough eventually scrambles them all. Scrambling is permanent (no
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

        foreach (var tryScramble in ScramblePriorityOrder)
            if (tryScramble(context))
                break;

        return Task.FromResult(string.Empty);
    }

    /// <summary>
    /// I-MAGNET's priority order (compone.zil:1595-1612): checked in this order each turn, the first
    /// not-yet-scrambled carried card is the one that gets corrupted.
    /// </summary>
    private static readonly Func<IContext, bool>[] ScramblePriorityOrder =
    [
        TryScramble<KitchenAccessCard>,
        TryScramble<ShuttleAccessCard>,
        TryScramble<TeleportationAccessCard>,
        TryScramble<UpperElevatorAccessCard>,
        TryScramble<LowerElevatorAccessCard>,
        TryScramble<MiniaturizationAccessCard>
    ];

    private static bool TryScramble<TCard>(IContext context) where TCard : AccessCard, new()
    {
        if (!context.HasItem<TCard>())
            return false;

        var card = Repository.GetItem<TCard>();
        if (card.Scrambled)
            return false;

        card.Scrambled = true;
        return true;
    }
}
