using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydInventoryManager(Floyd floyd)
{
    public InteractionResult OfferItem(IItem item, IContext context)
    {
        if (floyd.ItemBeingHeld != null)
        {
            // It ends up on the floor.
            context.Drop(item);
            return new PositiveInteractionResult($"Floyd examines the {item.Name}, shrugs, and drops it.");
        }

        // Trap: RemoveItem must run before CurrentLocation is reassigned - it needs the item's
        // *current* location to detach it from its real container (e.g. a card nested inside a worn
        // uniform pocket), not from wherever it's about to be reassigned to. Swapping this order once
        // orphaned the item in its original container while Floyd also appeared to hold it.
        context.RemoveItem(item);
        item.CurrentLocation = floyd;
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
        if (!IsFloydInRoom(context) ||
            !floyd.IsOn ||
            !floyd.Items.Any() ||
            floyd.HasRevealedLowerElevatorCard ||
            !RevealChanceSucceeds(context, chooser))
            return null;

        // Shared one-time flag (CARD-REVEALED): also set when the player shows Floyd a lower-elevator
        // card (Floyd.RespondToShow, #203), so that path and this daemon never both reveal the card.
        floyd.HasRevealedLowerElevatorCard = true;

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

    // Based on FLOYD-REVEAL-CARD-F (globals.zil:1440-1455): the chance Floyd reveals his lower-elevator
    // card escalates by day and is guaranteed after Day 3 (the divergence #222 fixes — the old code used a
    // flat ~33% every turn). The original gives Day 1 a 0% chance and splits Days 2-3 by an absolute
    // INTERNAL-MOVES boundary (5%/10% on Day 2, 20%/40% on Day 3); that move counter has no clean C# analog
    // (the chronometer resets to a morning base each day with no fixed day length), so we use one
    // representative chance per day. We deliberately keep a small NON-ZERO Day-1 chance rather than the
    // original's strict 0%, so the card stays obtainable on Day 1 to match the engine's existing early-game
    // flow. The day escalation and the post-Day-3 guarantee are the load-bearing behavior.
    private static bool RevealChanceSucceeds(IContext context, IRandomChooser chooser)
    {
        var day = (context as PlanetfallContext)?.Day ?? 1;
        var percent = day switch
        {
            <= 1 => 5,   // Day 1: small but non-zero (pragmatic divergence from the original's 0%)
            2 => 10,     // Day 2: ~5-10% in the original
            3 => 30,     // Day 3: ~20-40% in the original
            _ => 100     // Day > 3: guaranteed
        };

        // RollDice(100) returns 1-100; succeed when the roll lands in the percent window. Short-circuit the
        // guaranteed (>=100%) case so the roll is only consulted when it actually matters.
        return percent >= 100 || chooser.RollDice(100) <= percent;
    }
}
