using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

/// <summary>
///     A class that handles the processing of turning a light source on or off.
/// </summary>
public class TurnLightOnOrOffProcessor : IVerbProcessor
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
            case "light":
            case "strike":
            case "turn on":
            case "activate":
            case "enable":
                return ProcessOn(context, item);

            case "turn off":
            case "extinguish":
            case "blow out":
            case "deactivate":
            case "disable":
                return ProcessOff(context, item, client);

            // Sometimes, the parser can't determine the adverb "on" or "off". It that case, hopefully,
            // the parser gave us the preposition (it's really an adverb, but save it for the semantics dome, E.B. White)  
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
            case IAmALightSourceThatTurnsOnAndOff onAndOff:

                switch (adverb)
                {
                    case "on":
                        return TurnItOn(onAndOff, context).Result;
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

    private static InteractionResult? ProcessOn(IContext context, IInteractionTarget item)
    {
        switch (item)
        {
            case IAmALightSourceThatTurnsOnAndOff onAndOff:
                return TurnItOn(onAndOff, context).Result;

            default:
                return null;
        }
    }

    private static InteractionResult? ProcessOff(IContext context, IInteractionTarget item, IGenerationClient client)
    {
        switch (item)
        {
            case IAmALightSourceThatTurnsOnAndOff onAndOff:
                return TurnItOff(onAndOff, context, client).Result;

            case ICannotBeTurnedOff cannotBeTurnedOff:
                return new PositiveInteractionResult(cannotBeTurnedOff.CannotBeTurnedOffMessage);

            default:
                return null;
        }
    }

    private static async Task<InteractionResult> TurnItOff(IAmALightSourceThatTurnsOnAndOff item, IContext context,
        IGenerationClient client)
    {
        if (!item.IsOn)
            return new PositiveInteractionResult(item.AlreadyOffText);

        item.IsOn = false;
        item.OnBeingTurnedOff(context);

        if (context.ItIsDarkHere)
            return new PositiveInteractionResult(
                await new LookProcessor().Process(null, context, client, Runtime.Unknown));

        return new PositiveInteractionResult(item.NowOffText);
    }

    private static Task<InteractionResult> TurnItOn(IAmALightSourceThatTurnsOnAndOff item, IContext context)
    {
        var result = "";

        if (item.IsOn)
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(item.AlreadyOnText));

        if (!string.IsNullOrEmpty(item.CannotBeTurnedOnText))
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(item.CannotBeTurnedOnText));

        var itWasDark = context.ItIsDarkHere;

        item.IsOn = true;
        result += item.OnBeingTurnedOn(context);

        if (itWasDark)
            result += "\n\n" + LookProcessor.LookAround(context, Verbosity.Verbose);

        return Task.FromResult<InteractionResult>(new PositiveInteractionResult(item.NowOnText + result));
    }
}