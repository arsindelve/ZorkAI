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
                baseItem.CurrentLocation = null; // It's destroyed.
                context.Items.Remove(baseItem);
                return new PositiveInteractionResult(castItem.EatenDescription);
        }

        return null;
    }
}