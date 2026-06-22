using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;

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

        // Move (one match) or disambiguate (two+); null means no adjacent room matched, so we give the
        // "go to"-flavoured refusal.
        return await DestinationNavigation.TryNavigate(goTo.Destination, context, generationClient)
               ?? (null, await CantGetThere(generationClient, context, goTo.Destination));
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
