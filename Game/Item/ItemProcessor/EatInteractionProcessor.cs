using Model.Item;

namespace Game.Item.ItemProcessor;

/// <summary>
///     Represents a processor for eating an item.
/// </summary>
public class EatInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item)
    {
        if (item is not ICanBeEaten castItem)
            throw new Exception("Cast Error");

        if (item is not IItem baseItem)
            throw new Exception("Cast Error");

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "consume":
            case "gobble":
            case "snack on":
            case "eat":
            case "devour":

                var message = "";
                // We know the item is in this location, but it might be inside something else. If so
                // we need to take it first. This will have no practical effect because we are just
                // about to destroy it, but we do need to say "(Taken)" first. 

                if (baseItem.CurrentLocation is not IContext)
                    message = "(Taken)\n";

                // It's destroyed now. 
                baseItem.CurrentLocation?.RemoveItem(baseItem);
                baseItem.CurrentLocation = null; // It's destroyed.

                return new PositiveInteractionResult(message + castItem.EatenDescription);
        }

        return null;
    }
}