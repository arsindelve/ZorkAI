using GameEngine;
using Model.Intent;
using Model.Interface;
using Model.Location;

namespace ZorkOne.Location;

internal static class RainbowInteraction
{
    private static readonly string[] RainbowNouns = ["rainbow"];
    private static readonly string[] CrossRainbowVerbs = ["cross", "through", "traverse", "go through"];

    public static InteractionResult? TryHandle(SimpleIntent action, IContext context, Func<ILocation?> destinationWhenSolid)
    {
        if (action.Match(["look under"], RainbowNouns))
            return new PositiveInteractionResult("The Frigid River flows under the rainbow. ");

        if (!action.Match(CrossRainbowVerbs, RainbowNouns))
            return null;

        // Zork treats RAINBOW as a local-global object; keep every rainbow room on one action table.
        if (!Repository.GetLocation<EndOfRainbow>().RainbowIsSolid)
            return new PositiveInteractionResult("Can you walk on water vapor? ");

        var destination = destinationWhenSolid();
        if (destination is null)
            return new PositiveInteractionResult("You'll have to say which way... ");

        context.CurrentLocation = destination;
        return new PositiveInteractionResult(context.CurrentLocation.GetDescription(context));
    }
}
