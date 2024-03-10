using Game.StaticCommand.Implementation;
using Model.Item;

namespace Game.Item.ItemProcessor;

/// <summary>
///     A class that handles the processing of turning an object on or off.
/// </summary>
public class TurnOnOrOffProcessor : IVerbProcessor
{
    /// <summary>
    ///     Processes the action of turning on or off an object.
    /// </summary>
    /// <param name="action">The simple intent representing the action to be performed.</param>
    /// <param name="context">The context in which the action is being performed.</param>
    /// <param name="item">The item to be turned on or off.</param>
    /// <returns>An InteractionResult indicating the result of the action.</returns>
    public InteractionResult? Process(SimpleIntent action, IContext context, IInteractionTarget item)
    {
        if (item is not ICanBeTurnedOnAndOff castItem)
            throw new Exception("Cast error");

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "turn on":
            case "activate":
                return TurnItOn(castItem, context);

            case "turn off":
                return TurnItOff(castItem, context);
        }

        return null;
    }

    private static InteractionResult TurnItOff(ICanBeTurnedOnAndOff item, IContext context)
    {
        if (!item.IsOn)
            return new PositiveInteractionResult(item.AlreadyOffText);

        item.IsOn = false;

        if (item is IAmALightSource && context.CurrentLocation is IDarkLocation)
            return new PositiveInteractionResult(new LookProcessor().Process(null, context));

        return new PositiveInteractionResult(item.NowOffText);
    }

    private static InteractionResult TurnItOn(ICanBeTurnedOnAndOff item, IContext context)
    {
        if (item.IsOn)
            return new PositiveInteractionResult(item.AlreadyOnText);

        item.IsOn = true;

        if (item is IAmALightSource && context.CurrentLocation is IDarkLocation)
            return new PositiveInteractionResult(new LookProcessor().Process(null, context));

        return new PositiveInteractionResult(item.NowOnText);
    }
}