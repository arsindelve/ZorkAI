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
                return TurnItOn(castItem, context).Result;

            case "turn off":
                return TurnItOff(castItem, context).Result;

            // Sometimes, the parser can't determine the adverb "on" or "off". It that case, hopefully,
            // the parser gave us the adverb. 
            case "turn":

                var adverb = action.Adverb?.ToLowerInvariant().Trim();
                if (string.IsNullOrEmpty(action.Adverb?.ToLowerInvariant().Trim()))
                    return null;

                switch (adverb)
                {
                    case "on":
                        return TurnItOn(castItem, context).Result;
                    case "off":
                        return TurnItOff(castItem, context).Result;

                    default:
                        return null;
                }
        }

        return null;
    }

    private static async Task<InteractionResult> TurnItOff(ICanBeTurnedOnAndOff item, IContext context)
    {
        if (!item.IsOn)
            return new PositiveInteractionResult(item.AlreadyOffText);

        item.IsOn = false;

        if (item is IAmALightSource && context.CurrentLocation is IDarkLocation)
            return new PositiveInteractionResult(await new LookProcessor().Process(null, context, null));

        return new PositiveInteractionResult(item.NowOffText);
    }

    private static async Task<InteractionResult> TurnItOn(ICanBeTurnedOnAndOff item, IContext context)
    {
        if (item.IsOn)
            return new PositiveInteractionResult(item.AlreadyOnText);

        item.IsOn = true;

        if (item is IAmALightSource && context.CurrentLocation is IDarkLocation)
            return new PositiveInteractionResult(await new LookProcessor().Process(null, context, null));

        return new PositiveInteractionResult(item.NowOnText);
    }
}