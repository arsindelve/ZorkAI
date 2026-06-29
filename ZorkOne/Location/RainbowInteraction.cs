using System.Text.RegularExpressions;
using GameEngine;
using Model.Intent;
using Model.Interface;
using Model.Location;

namespace ZorkOne.Location;

internal static class RainbowInteraction
{
    private static readonly string[] RainbowNouns = ["rainbow"];
    private static readonly string[] CrossRainbowVerbs = ["cross", "through", "traverse"];

    public static InteractionResult? TryHandle(SimpleIntent action, IContext context, Func<ILocation?> destinationWhenSolid)
    {
        if (!action.MatchNounAndAdjective(RainbowNouns))
            return null;

        if (IsLookUnderRainbow(action))
            return new PositiveInteractionResult("The Frigid River flows under the rainbow. ");

        if (!IsCrossingRainbow(action))
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

    private static bool IsLookUnderRainbow(SimpleIntent action)
    {
        return action.MatchVerb(["look"]) && IsAdverbOrOriginalInput(action, "under");
    }

    private static bool IsCrossingRainbow(SimpleIntent action)
    {
        return action.MatchVerb(CrossRainbowVerbs) ||
               action.MatchVerb(["go", "walk", "move"]) && IsAdverbOrOriginalInput(action, "through");
    }

    private static bool IsAdverbOrOriginalInput(SimpleIntent action, string preposition)
    {
        if (action.Adverb?.Equals(preposition, StringComparison.OrdinalIgnoreCase) == true)
            return true;

        // The production parser often keeps single-noun prepositions only in OriginalInput.
        var original = action.OriginalInput;
        return !string.IsNullOrWhiteSpace(original) &&
               Regex.IsMatch(original, $@"\b{Regex.Escape(preposition)}\s+(the\s+)?rainbow\b",
                   RegexOptions.IgnoreCase);
    }
}
