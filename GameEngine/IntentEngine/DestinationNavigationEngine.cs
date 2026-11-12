using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace GameEngine.IntentEngine;

/// <summary>
/// Handles <see cref="GoToDestinationIntent"/> (issue #268): "go to / walk to / head to ... a NAMED
/// room". Resolves the name against the current room's own exits and, for a single match, delegates
/// to <see cref="MoveEngine"/> so all gating (closed/locked doors, weight squeezes), leave/enter
/// hooks, and per-turn side-effects are reused unchanged. Two or more matches ask "which one?";
/// none gives a sensible refusal with no teleport.
/// </summary>
internal class DestinationNavigationEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(
        IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not GoToDestinationIntent goTo)
            throw new ArgumentException("Cast error");

        // Move (one match) or disambiguate (two+); null means no adjacent room matched.
        var navigated = await DestinationNavigation.TryNavigate(goTo.Destination, context, generationClient);
        if (navigated is not null)
            return navigated.Value;

        // No room of that name - but the "place" may be a direction. A model files "go to the aft" and
        // "head to starboard" as a trip to somewhere CALLED aft, and that used to be refused as an
        // unknown place. Rooms are tried first, so a room that really is named for a direction still
        // wins; only when none is does the word mean the way to walk. The move engine then gives the
        // move, or its own refusal for a way that has no exit.
        var direction = DirectionParser.ParseDirection(WithoutArticle(goTo.Destination));
        if (direction != Direction.Unknown)
            return await new MoveEngine().Process(
                new MoveIntent { Direction = direction, Message = goTo.Message }, context, generationClient);

        return (null, await CantGetThere(generationClient, context, goTo.Destination));
    }

    private static string WithoutArticle(string destination)
    {
        var words = destination.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 1 && words[0].ToLowerInvariant() is "the" or "a" or "an"
            ? string.Join(' ', words.Skip(1))
            : destination.Trim();
    }

    private static async Task<string> CantGetThere(
        IGenerationClient client, IContext context, string destination)
    {
        if (client.IsDisabled)
            return "You can't get there from here. ";

        var request = new CannotReachLocationRequest(
            context.CurrentLocation.GetDescriptionForGeneration(context), destination);
        return await client.GenerateNarration(request, context.SystemPromptAddendum);
    }
}
