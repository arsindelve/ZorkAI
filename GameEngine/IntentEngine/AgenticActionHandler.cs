using GameEngine.Item.ItemProcessor;
using Model.AIParsing;
using Model.Interface;

namespace GameEngine.IntentEngine;

/// <summary>
///     Issue #136: the shared "narrator with hands" fall-through. When a command matched no game
///     mechanic and the acted-upon noun is an item the player is CARRYING, the engines consult the
///     <see cref="IAgenticActionParser" /> seam, which may narrate the outcome and emit DROP/DESTROY
///     tool calls that make the world state match the narration. Anything the game models upstream is
///     intercepted before the fall-through narrators, so by construction there is zero overlap with
///     real mechanics.
/// </summary>
internal static class AgenticActionHandler
{
    /// <summary>
    ///     Consults the agentic narrator for an unhandled action, applying whatever tool calls come
    ///     back. Returns null whenever the caller should keep its existing narration-only behavior:
    ///     the seam is unavailable, the trigger gate fails (the noun is not a held item - checked
    ///     BEFORE any AI call), the seam returned nothing, or it returned no tools and no narration.
    /// </summary>
    /// <param name="playerInput">The player's raw input, passed verbatim to the narrator.</param>
    /// <param name="targetNoun">The acted-upon noun; must resolve to an item in inventory (the gate).</param>
    /// <param name="context">The game context.</param>
    /// <param name="itemProcessorFactory">Carries the seam; null-safe for engines built without one.</param>
    /// <param name="noToolResultObject">
    ///     The result object to surface when the narrator returned snark but no tools - the caller's
    ///     existing no-effect result, so an all-talk reply keeps today's "nothing happened" semantics.
    /// </param>
    internal static async Task<(InteractionResult? resultObject, string ResultMessage)?> TryResolveAgenticAction(
        string? playerInput,
        string? targetNoun,
        IContext context,
        IItemProcessorFactory? itemProcessorFactory,
        InteractionResult? noToolResultObject)
    {
        var parser = itemProcessorFactory?.AgenticActionParser;
        if (parser is null || string.IsNullOrWhiteSpace(playerInput) || string.IsNullOrWhiteSpace(targetNoun))
            return null;

        // The trigger gate, deliberately checked before the (expensive) AI call: the acted-upon noun
        // must be an item currently in the player's top-level inventory. Room items, scenery and
        // absent nouns never consult the seam. Top-level matches DropIt's own reach, so the narration
        // can never claim to dispose of something the drop mechanic would refuse to touch.
        var gateItem = Repository.GetItemInInventory(targetNoun, context);
        if (gateItem is null || !context.Items.Contains(gateItem))
            return null;

        var locationDescription = context.CurrentLocation.GetDescriptionForGeneration(context);
        var inventoryDescription = context.ItemListDescription(string.Empty, null);

        var result = await parser.Resolve(playerInput, inventoryDescription, locationDescription);

        // Defensive: an absent reply means the narrator has nothing to add - keep today's behavior.
        if (result is null)
            return null;

        if (result.ToolCalls is not { Count: > 0 })
        {
            // The narrator was not certain the action is plausible and grounded: no state change,
            // just its deflection. Empty narration (the inert test default) falls through entirely.
            return string.IsNullOrWhiteSpace(result.Narration)
                ? null
                : (noToolResultObject, result.Narration + Environment.NewLine);
        }

        var anythingChanged = false;
        string? refusalMessage = null;

        foreach (var toolCall in result.ToolCalls)
        {
            // Re-resolve every target against the player's actual inventory and ignore any tool
            // whose target isn't really held - the narrator cannot act on things the player lacks.
            var item = Repository.GetItemInInventory(toolCall.TargetNoun, context);
            if (item is null || !context.Items.Contains(item))
                continue;

            switch (toolCall.Tool)
            {
                case AgenticTool.Drop:
                    // Reuse the real drop mechanic so the worn-clothing and IDropSpecialLocation
                    // guards still apply; the AI narration replaces its plain "Dropped" message.
                    var dropResult = TakeOrDropInteractionProcessor.DropIt(context, item);
                    if (context.Items.Contains(item))
                        // A guard refused the drop (e.g. worn clothing). Remember its message so we
                        // don't narrate a disposal that never happened.
                        refusalMessage ??= dropResult.InteractionMessage;
                    else
                        anythingChanged = true;
                    break;

                case AgenticTool.Destroy:
                    Repository.DestroyItem(item);
                    anythingChanged = true;
                    break;
            }
        }

        // Every tool was refused or ignored: surfacing the AI narration would describe a state
        // change that never happened, so surface the guard's own refusal (or fall through).
        if (!anythingChanged)
            return refusalMessage is null
                ? null
                : (noToolResultObject, refusalMessage + Environment.NewLine);

        // State changed, so we must say SOMETHING - a blank line here is the classic force-verb
        // bug (engine anti-pattern: silent responses). The narrator essentially never omits its
        // narration, but guard it anyway.
        var narration = string.IsNullOrWhiteSpace(result.Narration) ? "Done. " : result.Narration;
        return (new PositiveInteractionResult(narration), narration + Environment.NewLine);
    }
}
