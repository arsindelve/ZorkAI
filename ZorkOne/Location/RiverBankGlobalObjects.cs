using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Location;

/// <summary>
///     Shared "local-global" river/water responses for the river-bank rooms whose descriptions
///     expose the river directly. In the original ZIL, both Shore and Sandy Beach declare
///     <c>(GLOBAL GLOBAL-WATER RIVER)</c>, which makes the water/river objects respond to commands
///     there even though neither object is ever actually carried or placed in the room's inventory.
///     Without this, "swim"/"drink water"/"take water" fell through to the engine's generic no-op
///     responses instead of the original water/river handlers (issue #337).
/// </summary>
internal static class RiverBankGlobalObjects
{
    public const string SwimmingMessage = "Swimming isn't usually allowed in the dungeon. ";

    /// <summary>
    ///     Prose-only "river" scenery: examinable and un-takeable, like the ZIL RIVER object
    ///     (FLAGS NDESCBIT, no TAKEBIT).
    /// </summary>
    public static readonly SceneryItem River = new(
        ["river"],
        "The river is wide, swift, and cold. ",
        "You can't take the river. ");

    /// <summary>
    ///     True for "swim", "swim in river", "swim across river", "bathe", "wade", etc. Checked
    ///     against the raw input (rather than a parsed noun) because, like "pray" at the Altar, swim
    ///     is meaningful with or without an object.
    /// </summary>
    public static bool IsSwimAttempt(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var trimmed = input.Trim();
        return Verbs.SwimVerbs.Any(verb => trimmed.StartsWith(verb, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Delegates any "water" noun interaction (drink, take, examine, ...) to the single global
    ///     <see cref="Water" /> item, without ever placing it in this room's <c>Items</c> - doing so
    ///     would fight over its singleton <c>CurrentLocation</c> with wherever else it's seeded (e.g.
    ///     inside the Bottle). Returns null when the action's noun isn't water at all, so the caller
    ///     can fall through to its normal handling.
    /// </summary>
    public static async Task<InteractionResult?> RespondToWater(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        var result = await Repository.GetItem<Water>()
            .RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        return result is NoNounMatchInteractionResult ? null : result;
    }
}
