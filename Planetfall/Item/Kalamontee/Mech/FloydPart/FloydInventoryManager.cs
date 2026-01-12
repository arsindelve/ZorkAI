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
        if (!IsFloydInRoom(context) ||
            !floyd.IsOn ||
            !floyd.Items.Any() ||
            !chooser.RollDiceSuccess(3))
            return null;

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
