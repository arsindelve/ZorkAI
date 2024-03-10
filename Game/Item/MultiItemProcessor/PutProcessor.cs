using Model.Item;

namespace Game.Item.MultiItemProcessor;

public class PutProcessor : IMultiNounVerbProcessor
{
    public InteractionResult? Process(MultiNounIntent action, IContext context, IInteractionTarget itemOne,
        IInteractionTarget itemTwo)
    {
        if (itemOne is not IItem castItemOne)
            throw new Exception("Cast Error");

        // If the receiver (item two) is not an item that can hold
        // other items, this interaction is never going to work
        if (itemTwo is not ICanHoldItems itemReceiver)
            return null;

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "put":
            case "place":
            case "shove":
            case "push":

                switch (action.Preposition.ToLowerInvariant().Trim())
                {
                    case "in":
                    case "into":
                    case "inside":

                        return PutTheThingIntoTheThing(castItemOne, itemReceiver);

                    default:
                        return null;
                }
        }

        return null;
    }

    private static InteractionResult PutTheThingIntoTheThing(IItem item, ICanHoldItems itemReceiver)
    {
        // Do I have the thing? 
        if (item.CurrentLocation is not IContext)
            return new PositiveInteractionResult($"You don't have the {item.NounsForMatching.First()}");

        // Is the recipient open?
        if (itemReceiver is IOpenAndClose { IsOpen: false })
            return new PositiveInteractionResult("It's closed. ");

        // TODO: Case where the item cannot be put there. 
        if (!itemReceiver.HaveRoomForItem(item))
            return new PositiveInteractionResult("There's no room. ");

        item.CurrentLocation?.RemoveItem(item);
        item.CurrentLocation = itemReceiver;
        itemReceiver.ItemPlacedHere(item);
        return new PositiveInteractionResult("Done.");
    }
}