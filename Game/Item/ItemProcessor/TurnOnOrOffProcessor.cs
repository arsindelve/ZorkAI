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
    /// <param name="client"></param>
    /// <returns>An InteractionResult indicating the result of the action.</returns>
    public InteractionResult? Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (string.IsNullOrEmpty(action.Verb))
            return null;

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "turn on":
            case "activate":
                return ProcessOn(context, item, client);

            case "turn off":
            case "extinguish":
                return ProcessOff(context, item, client);

            // Sometimes, the parser can't determine the adverb "on" or "off". It that case, hopefully,
            // the parser gave us the adverb. 
            case "turn":
                return ProcessTurn(action, context, item, client);
        }

        return null;
    }

    private static InteractionResult? ProcessTurn(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        var adverb = action.Adverb?.ToLowerInvariant().Trim();
        if (string.IsNullOrEmpty(action.Adverb?.ToLowerInvariant().Trim()))
            return null;

        switch (item)
        {
            case ICanBeTurnedOnAndOff onAndOff:

                switch (adverb)
                {
                    case "on":
                        return TurnItOn(onAndOff, context, client).Result;
                    case "off":
                        return TurnItOff(onAndOff, context, client).Result;

                    default:
                        return null;
                }

            case ICannotBeTurnedOff noOnAndOff:

                switch (adverb)
                {
                    case "off":
                        return new PositiveInteractionResult(noOnAndOff.CannotBeTurnedOffMessage);

                    default:
                        return null;
                }

            default:
                return null;
        }
    }

    private static InteractionResult? ProcessOn(IContext context, IInteractionTarget item, IGenerationClient client)
    {
        switch (item)
        {
            case ICanBeTurnedOnAndOff onAndOff:
                return TurnItOn(onAndOff, context, client).Result;

            default:
                return null;
        }
    }

    private static InteractionResult? ProcessOff(IContext context, IInteractionTarget item, IGenerationClient client)
    {
        switch (item)
        {
            case ICanBeTurnedOnAndOff onAndOff:
                return TurnItOff(onAndOff, context, client).Result;

            case ICannotBeTurnedOff cannotBeTurnedOff:
                return new PositiveInteractionResult(cannotBeTurnedOff.CannotBeTurnedOffMessage);

            default:
                return null;
        }
    }

    private static async Task<InteractionResult> TurnItOff(ICanBeTurnedOnAndOff item, IContext context,
        IGenerationClient client)
    {
        if (!item.IsOn)
            return new PositiveInteractionResult(item.AlreadyOffText);

        item.IsOn = false;

        if (item is IAmALightSource && context.CurrentLocation is IDarkLocation)
            return new PositiveInteractionResult(await new LookProcessor().Process(null, context, client));

        return new PositiveInteractionResult(item.NowOffText);
    }

    private static async Task<InteractionResult> TurnItOn(ICanBeTurnedOnAndOff item, IContext context,
        IGenerationClient client)
    {
        if (item.IsOn)
            return new PositiveInteractionResult(item.AlreadyOnText);

        item.IsOn = true;

        if (item is IAmALightSource && context.CurrentLocation is IDarkLocation)
            return new PositiveInteractionResult(await new LookProcessor().Process(null, context, client));

        return new PositiveInteractionResult(item.NowOnText);
    }
}