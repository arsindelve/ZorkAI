using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydInventoryManager(Floyd floyd)
{
    // Per-day reveal percentages derived from FLOYD-REVEAL-CARD-F (planetfall-source/globals.zil:1440-1468).
    // The ZIL uses INTERNAL-MOVES < 5000 for a within-day split (5%/10% on Day 2, 20%/40% on Day 3).
    // We approximate each day with its average chance; the Day gate and post-Day-3 guarantee are exact.
    private static readonly Dictionary<int, int> RevealPercentByDay = new()
    {
        { 2, 8 },   // ~5% first half, ~10% second half → average ≈ 8%
        { 3, 30 },  // ~20% first half, ~40% second half → average ≈ 30%
    };

    public InteractionResult OfferItem(IItem item, IContext context)
    {
        if (floyd.ItemBeingHeld != null)
        {
            // It ends up on the floor.
            context.Drop(item);
            return new PositiveInteractionResult($"Floyd examines the {item.Name}, shrugs, and drops it.");
        }

        item.CurrentLocation = floyd;
        context.RemoveItem(item);
        floyd.ItemBeingHeld = item;
        floyd.ItemBeingHeld.OnBeingTakenCallback = "Floyd,BeingTakenCallback";

        return new PositiveInteractionResult(FloydConstants.ThanksYouForGivingItem);
    }

    public InteractionResult SearchFloyd(IContext context)
    {
        if (floyd.IsOn)
            return new PositiveInteractionResult(FloydConstants.TickleFloyd);

        if (!floyd.HasItem<LowerElevatorAccessCard>())
            return new PositiveInteractionResult(
                "Your search discovers nothing in the robot's compartments except a single crayon " +
                "which you leave where you found it. ");
        
        context.ItemPlacedHere<LowerElevatorAccessCard>();
        return new PositiveInteractionResult(FloydConstants.FindAndTakeLowerCard);
    }

    public string? OfferLowerElevatorCard(IContext context, IRandomChooser chooser)
    {
        if (!IsFloydInRoom(context) || !floyd.IsOn || !floyd.Items.Any() || floyd.LowerElevatorCardRevealed)
            return null;

        // Day 1: never reveal (globals.zil:1440-1468 has no clause matching Day 1)
        var day = context is PlanetfallContext pfContext ? pfContext.Day : 1;
        if (day <= 1)
            return null;

        // Day > 3: always reveal; Day 2-3: escalating percent chance
        if (RevealPercentByDay.TryGetValue(day, out var percent) && chooser.RollDice(100) > percent)
            return null;

        floyd.LowerElevatorCardRevealed = true;

        // Remove it from inside, put it in his hand.
        floyd.Items.Clear();
        floyd.ItemBeingHeld = Repository.GetItem<LowerElevatorAccessCard>();
        floyd.ItemBeingHeld.OnBeingTakenCallback = "Floyd,BeingTakenCallback";

        return "\n\nFloyd claps his hands with excitement. \"Those cards are really neat, huh? Floyd has one for " +
               "himself--see?\" He reaches behind one of his panels and retrieves a magnetic-striped card. He waves it exuberantly in the air. ";
    }

    public string DescribeItemBeingHeld(ILocation? currentLocation)
    {
        if (floyd.ItemBeingHeld is null)
            return "";

        return "\nThe multiple purpose robot is holding: \n   " + floyd.ItemBeingHeld.GenericDescription(currentLocation);
    }

    private bool IsFloydInRoom(IContext context)
    {
        return floyd.CurrentLocation == context.CurrentLocation;
    }
}
