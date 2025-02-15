using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

/// <summary>
///     Represents a processor for eating an item.
/// </summary>
public class EatAndDrinkInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not IItem baseItem)
            throw new Exception("Cast Error");

        var message = "";

        if (string.IsNullOrEmpty(action.Verb))
            return null;

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "swallow":
            case "sip":
            case "drink":
            case "consume":
            case "gobble":
            case "snack on":
            case "eat":
            case "devour":
            case "chew":

                if (context is { HasLightSource: false, CurrentLocation: DarkLocation })
                    return new PositiveInteractionResult("It's too dark to see!");

                message = item switch
                {
                    ICanBeEaten food => EatIt(baseItem, message, food, context),
                    IAmADrink drink => DrinkIt(baseItem, drink, context),
                    _ => throw new ArgumentException()
                };

                return new PositiveInteractionResult(message);
        }

        return null;
    }

    private string DrinkIt(IItem item, IAmADrink drink, IContext context)
    {
        // Drinking is a little different, because liquid usually has to be inside a container
        // right up until the moment we drink it. We cannot "hold" water in inventory without
        // a bottle, the way we can "hold" a sandwich. 

        var container = item.CurrentLocation as IItem;

        if (container != null && container.CurrentLocation != context)
            return $"You would have to be holding the {container.Name}. ";

        if (container is IOpenAndClose { IsOpen: false })
            return $"The {container.Name} is not open. ";

        DestroyIt(item);
        return drink.OnDrinking(context);
    }

    private static string EatIt(IItem item, string message, ICanBeEaten food, IContext context)
    {
        // We know the item is in this location, but it might be inside something else. If so
        // we need to take it first. This will have no practical effect because we are just
        // about to destroy it, but we do need to say "(Taken)" first. 

        var container = item.CurrentLocation;

        // If it's in a container, is the container open? We might be able to see it but not 
        // take it, if it's in the trophy case for example
        if (container is IOpenAndClose { IsOpen: false })
            return $"The {container.Name} is not open. ";

        if (container is not IContext)
            message = "(Taken)\n";

        DestroyIt(item);

        message += food.OnEating(context);
        return message;
    }

    private static void DestroyIt(IItem item)
    {
        Repository.DestroyItem(item);
    }
}