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

        var matches = DestinationNavigation.ResolveAllAdjacent(goTo.Destination, context);

        switch (matches.Count)
        {
            case 0:
                return (null, await CantGetThere(generationClient, context, goTo.Destination));

            case 1:
                // Single hop: reuse the full movement pipeline so a gated exit yields its own message
                // ("The kitchen window is closed.") instead of a teleport, and the leave/enter hooks
                // and turn side-effects all fire normally.
                return await new MoveEngine().Process(
                    new MoveIntent { Direction = matches[0].Direction }, context, generationClient);

            default:
                // Ambiguous → hand the engine's existing disambiguation flow a prompt. The
                // InteractionMessage must ALSO be the ResultMessage so the player sees the question
                // this turn (mirrors SimpleInteractionEngine's disambiguation return).
                var disambiguation = DestinationNavigation.BuildDisambiguation(matches);
                return (disambiguation, disambiguation.InteractionMessage);
        }
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
