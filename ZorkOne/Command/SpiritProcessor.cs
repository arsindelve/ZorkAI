using GameEngine;
using Model.Movement;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Command;

/// <summary>
///     The "spirit" command handler — the C# equivalent of the original ZIL DEAD-FUNCTION
///     (zork1/1actions.zil). Once the player has died as a spirit (<see cref="ZorkIContext.IsDead" />),
///     they wander the dungeon as a ghost and almost every verb is overridden with a canned response.
///     The two things a spirit can still do are <b>move</b> (to walk back toward the altar) and
///     <b>pray</b> at the altar (to be resurrected). See issue #17.
/// </summary>
public class SpiritProcessor
{
    /// <summary>
    ///     Handle the raw command for a player who is currently a spirit.
    /// </summary>
    /// <param name="input">The raw, unparsed player input.</param>
    /// <param name="context">The Zork game context (already known to be in the spirit/DEAD state).</param>
    /// <returns>
    ///     The ghost-world response, or <c>null</c> to let the engine process the command normally —
    ///     used for movement so the player can walk back to the altar.
    /// </returns>
    public string? Process(string? input, ZorkIContext context)
    {
        var command = input?.Trim().ToLowerInvariant() ?? string.Empty;

        // Movement still works — a spirit walks the dungeon back to the altar. Returning null lets the
        // engine's normal movement processing run (the player is always-lit while dead, so no grue).
        if (DirectionParser.IsDirection(command, out _))
            return null;

        var firstWord = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        // Praying is how a spirit is resurrected — but only at the altar.
        if (firstWord is "pray")
            return Pray(context);

        // Lighting a lamp: a spirit needs no light to guide it.
        if (Matches(command, "turn on", "light", "activate") && MentionsLamp(command))
            return "You need no light to guide you. ";

        return firstWord switch
        {
            "wait" or "z" => "Might as well. You've got an eternity. ",
            "score" => "You're dead! How can you think of your score? ",
            "inventory" or "i" or "drop" or "throw" => "You have no possessions. ",
            "take" or "get" or "grab" or "rub" or "touch" => "Your hand passes through its object. ",
            "diagnose" => "You are dead. ",
            "look" or "l" => "The room looks strange and unearthly, and objects appear indistinct. ",
            "attack" or "kill" or "mung" or "swing" or "hit" or "destroy" or "fight" =>
                "All such attacks are vain in your condition. ",
            "open" or "close" or "eat" or "drink" or "turn" or "burn" or "move" or "push"
                or "pull" or "read" or "examine" or "put" or "give" =>
                "Even such an action is beyond your capabilities. ",
            _ => "You can't even do that. "
        };
    }

    /// <summary>
    ///     Praying at the altar resurrects the spirit: it clears the DEAD state, lifts the player out of
    ///     the underworld and deposits them, unencumbered, back in the forest. Anywhere else, the prayer
    ///     goes unanswered.
    /// </summary>
    private static string Pray(ZorkIContext context)
    {
        if (context.CurrentLocation is not Altar)
            return "Your prayers are not heard. ";

        context.IsDead = false;
        var forest = Repository.GetLocation<ForestOne>();
        context.CurrentLocation = forest;

        return "From the distance the sound of a lone trumpet is heard. The world seems to dissolve " +
               "around you, and you feel yourself rising as if from a long sleep, deep in the woods. " +
               "You are unencumbered by any earthly possessions.\n\n" +
               forest.GetDescription(context);
    }

    private static bool Matches(string command, params string[] verbs)
    {
        return verbs.Any(v => command.StartsWith(v, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MentionsLamp(string command)
    {
        return command.Contains("lamp") || command.Contains("lantern") || command.Contains("light");
    }
}
